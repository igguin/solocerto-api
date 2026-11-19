using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SoloCerto.Api.Data;
using SoloCerto.Api.DTOs;
using SoloCerto.Api.Models;

namespace SoloCerto.Tests;

/// <summary>Sobe a API inteira em memória, com SQLite em memória, sem dados de demonstração.</summary>
public sealed class SoloCertoApiFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _conexao = new("Data Source=:memory:");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        _conexao.Open();
        builder.UseSetting("SoloCerto:CarregarDadosDemonstracao", "false");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<AppDbContext>();
            services.AddDbContext<AppDbContext>(o => o.UseSqlite(_conexao));
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _conexao.Dispose();
    }
}

public class ApiIntegracaoTests(SoloCertoApiFactory fabrica) : IClassFixture<SoloCertoApiFactory>
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly HttpClient _http = fabrica.CreateClient();

    private async Task<int> CriarPlantacaoAsync(string cultura = "Tomate")
    {
        var ag = await _http.PostAsJsonAsync("/api/agricultores", new AgricultorRequest("Pedro Teste", "Campinas", null));
        var agricultor = (await ag.Content.ReadFromJsonAsync<AgricultorResponse>(Json))!;
        var pl = await _http.PostAsJsonAsync("/api/plantacoes",
            new PlantacaoRequest(agricultor.Id, cultura, 1.5m, 30, 45));
        return (await pl.Content.ReadFromJsonAsync<PlantacaoResponse>(Json))!.Id;
    }

    [Fact]
    public async Task Post_agricultor_devolve_201_com_location()
    {
        var resposta = await _http.PostAsJsonAsync("/api/agricultores",
            new AgricultorRequest("Maria Teste", "Campinas", "maria@exemplo.com"));

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        Assert.NotNull(resposta.Headers.Location);
        var criado = await resposta.Content.ReadFromJsonAsync<AgricultorResponse>(Json);
        Assert.Equal("Maria Teste", criado!.Nome);
    }

    [Fact]
    public async Task Fluxo_completo_previsao_medicao_recomendacao()
    {
        var id = await CriarPlantacaoAsync();
        (await _http.PostAsJsonAsync("/api/previsoes-chuva", new PrevisaoChuvaRequest(id, 0.85m, null)))
            .EnsureSuccessStatusCode();

        var resposta = await _http.PostAsJsonAsync("/api/medicoes", new MedicaoRequest(id, 40, null), Json);

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        var r = (await resposta.Content.ReadFromJsonAsync<RegistroMedicaoResponse>(Json))!;
        Assert.Equal(TipoRecomendacao.AguardarChuva, r.Recomendacao.Tipo);
        Assert.StartsWith("Aguarde", r.Recomendacao.Mensagem);

        var situacao = await _http.GetFromJsonAsync<SituacaoPlantacaoResponse>($"/api/plantacoes/{id}/situacao", Json);
        Assert.Equal(r.Recomendacao.Id, situacao!.UltimaRecomendacao!.Id);
        Assert.NotNull(situacao.PrevisaoVigente);
    }

    [Fact]
    public async Task Recurso_inexistente_devolve_404_em_problem_details()
    {
        var resposta = await _http.GetAsync("/api/plantacoes/99999");

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
        Assert.Equal("application/problem+json", resposta.Content.Headers.ContentType?.MediaType);
        Assert.Contains("Plantação 99999", await resposta.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Entrada_invalida_devolve_400_em_portugues()
    {
        var resposta = await _http.PostAsJsonAsync("/api/medicoes", new { plantacaoId = 1, umidadePercentual = 150 });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        var corpo = await resposta.Content.ReadAsStringAsync();
        Assert.Contains("Dados inválidos", corpo);
        Assert.Contains("entre 0 e 100", corpo);
    }

    [Fact]
    public async Task Regra_de_negocio_violada_devolve_400()
    {
        var resposta = await _http.PostAsJsonAsync("/api/plantacoes", new PlantacaoRequest(1, "Feijão", 2, 40, 30));
        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [Fact]
    public async Task Put_e_delete_de_agricultor()
    {
        var criado = await (await _http.PostAsJsonAsync("/api/agricultores",
            new AgricultorRequest("Temporário", "Campinas", null))).Content.ReadFromJsonAsync<AgricultorResponse>(Json);

        var put = await _http.PutAsJsonAsync($"/api/agricultores/{criado!.Id}",
            new AgricultorRequest("Atualizado", "Amparo", null));
        Assert.Equal(HttpStatusCode.NoContent, put.StatusCode);
        var lido = await _http.GetFromJsonAsync<AgricultorResponse>($"/api/agricultores/{criado.Id}", Json);
        Assert.Equal("Amparo", lido!.Cidade);

        Assert.Equal(HttpStatusCode.NoContent, (await _http.DeleteAsync($"/api/agricultores/{criado.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await _http.GetAsync($"/api/agricultores/{criado.Id}")).StatusCode);
    }

    [Fact]
    public async Task Documentacao_openapi_e_scalar_estao_publicadas()
    {
        var doc = await _http.GetStringAsync("/openapi/v1.json");
        Assert.Contains("SoloCerto API", doc);
        Assert.Contains("/api/medicoes", doc);
        Assert.Contains("Registra uma medição", doc); // comentário XML virou documentação

        var scalar = await _http.GetAsync("/scalar/v1");
        Assert.Equal(HttpStatusCode.OK, scalar.StatusCode);
    }
}
