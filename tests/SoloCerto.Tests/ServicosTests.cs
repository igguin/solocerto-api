using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using SoloCerto.Api.Data;
using SoloCerto.Api.DTOs;
using SoloCerto.Api.Interfaces;
using SoloCerto.Api.Models;
using SoloCerto.Api.Services;
using SoloCerto.Api.Services.Regras;

namespace SoloCerto.Tests;

/// <summary>Serviços com EF Core sobre SQLite em memória (mesmas migrations do banco real).</summary>
public sealed class ServicosTests : IAsyncLifetime
{
    private readonly SqliteConnection _conexao = new("Data Source=:memory:");
    private readonly FakeTimeProvider _relogio = new(new DateTimeOffset(2026, 11, 19, 12, 0, 0, TimeSpan.Zero));
    private AppDbContext _db = null!;

    public async Task InitializeAsync()
    {
        await _conexao.OpenAsync();
        _db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_conexao).Options);
        await _db.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await _db.DisposeAsync();
        await _conexao.DisposeAsync();
    }

    private (MedicaoService Medicoes, PrevisaoChuvaService Previsoes, PlantacaoService Plantacoes) Servicos()
    {
        var previsoes = new PrevisaoChuvaService(_db, _relogio);
        var motor = new MotorRecomendacao([new RegraMedicaoIndisponivel(), new RegraUmidadeCritica(),
                                           new RegraChuvaPrevista(), new RegraUmidadeBaixa(), new RegraUmidadeAdequada()]);
        var recomendacoes = new RecomendacaoService(_db, motor, previsoes, Options.Create(new ParametrosIrrigacao()), _relogio);
        return (new MedicaoService(_db, recomendacoes, _relogio), previsoes, new PlantacaoService(_db, previsoes, _relogio));
    }

    private async Task<int> CriarMilhoAsync()
    {
        var agricultor = await new AgricultorService(_db).CriarAsync(new AgricultorRequest("Ana Teste", "Campinas", null));
        var plantacao = await Servicos().Plantacoes.CriarAsync(new PlantacaoRequest(agricultor.Id, "Milho", 5, 25, 35));
        return plantacao.Id;
    }

    [Fact]
    public async Task Registrar_medicao_persiste_e_gera_recomendacao_com_a_previsao_vigente()
    {
        var id = await CriarMilhoAsync();
        var (medicoes, previsoes, _) = Servicos();
        await previsoes.RegistrarAsync(new PrevisaoChuvaRequest(id, 0.75m, null));

        var r = await medicoes.RegistrarAsync(new MedicaoRequest(id, 30, null, OrigemMedicao.Sensor));

        Assert.Equal(FaixaUmidade.Baixa, r.Medicao.Faixa);
        Assert.Equal(TipoRecomendacao.AguardarChuva, r.Recomendacao.Tipo);
        Assert.True(r.Recomendacao.IrrigacaoEvitada);
        Assert.Equal(1, await _db.Recomendacoes.CountAsync());
    }

    [Fact]
    public async Task Medicao_para_plantacao_inexistente_lanca_404()
    {
        await Assert.ThrowsAsync<RecursoNaoEncontradoException>(() =>
            Servicos().Medicoes.RegistrarAsync(new MedicaoRequest(999, 30, null)));
    }

    [Fact]
    public async Task Historico_calcula_media_minima_e_maxima_no_periodo()
    {
        var id = await CriarMilhoAsync();
        var (medicoes, _, plantacoes) = Servicos();
        await medicoes.RegistrarAsync(new MedicaoRequest(id, 20, _relogio.GetUtcNow().UtcDateTime.AddDays(-2)));
        await medicoes.RegistrarAsync(new MedicaoRequest(id, 40, _relogio.GetUtcNow().UtcDateTime.AddDays(-1)));
        await medicoes.RegistrarAsync(new MedicaoRequest(id, 99, _relogio.GetUtcNow().UtcDateTime.AddDays(-40))); // fora

        var h = await plantacoes.ObterHistoricoAsync(id, dias: 7);

        Assert.Equal(2, h.TotalMedicoes);
        Assert.Equal(30, h.UmidadeMedia);
        Assert.Equal(20, h.UmidadeMinima);
        Assert.Equal(40, h.UmidadeMaxima);
        Assert.Equal(40, h.Medicoes[0].UmidadePercentual); // mais recente primeiro
    }

    [Fact]
    public async Task Indicadores_calculam_percentual_de_irrigacoes_evitadas()
    {
        var id = await CriarMilhoAsync();
        var (medicoes, previsoes, _) = Servicos();
        await medicoes.RegistrarAsync(new MedicaoRequest(id, 30, null));   // IrrigarHoje (sem previsão)
        await previsoes.RegistrarAsync(new PrevisaoChuvaRequest(id, 0.9m, null));
        await medicoes.RegistrarAsync(new MedicaoRequest(id, 31, null));   // AguardarChuva
        await medicoes.RegistrarAsync(new MedicaoRequest(id, 32, null));   // AguardarChuva
        await medicoes.RegistrarAsync(new MedicaoRequest(id, 10, null));   // IrrigacaoPrioritaria

        var ind = await new IndicadorService(_db).ObterAsync();

        Assert.Equal(4, ind.TotalRecomendacoes);
        Assert.Equal(2, ind.IrrigacoesEvitadas);
        Assert.Equal(66.7m, ind.PercentualIrrigacoesEvitadas); // 2 de 3 situações "abaixo do ideal"
        Assert.Single(ind.PlantacoesEmSituacaoCritica);
        Assert.Equal(25.8m, ind.UmidadePorCultura.Single().UmidadeMedia); // (30+31+32+10)/4
    }

    [Fact]
    public async Task Excluir_agricultor_remove_plantacoes_e_historico_em_cascata()
    {
        var id = await CriarMilhoAsync();
        await Servicos().Medicoes.RegistrarAsync(new MedicaoRequest(id, 30, null));
        var agricultorId = (await _db.Plantacoes.SingleAsync()).AgricultorId;

        await new AgricultorService(_db).RemoverAsync(agricultorId);

        Assert.Equal(0, await _db.Plantacoes.CountAsync());
        Assert.Equal(0, await _db.Medicoes.CountAsync());
        Assert.Equal(0, await _db.Recomendacoes.CountAsync());
    }

    [Fact]
    public async Task Filtro_por_cidade_ignora_maiusculas()
    {
        var servico = new AgricultorService(_db);
        await servico.CriarAsync(new AgricultorRequest("Ana", "Campinas", null));
        await servico.CriarAsync(new AgricultorRequest("Bia", "Bragança Paulista", null));

        var lista = await servico.ListarAsync("CAMPINAS");

        Assert.Equal("Ana", Assert.Single(lista).Nome);
    }
}
