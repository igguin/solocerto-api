using SoloCerto.Api.Interfaces;
using SoloCerto.Api.Models;
using SoloCerto.Api.Services;
using SoloCerto.Api.Services.Regras;

namespace SoloCerto.Tests;

/// <summary>Cada cenário do apoio à decisão de irrigação (polimorfismo das regras).</summary>
public class MotorRecomendacaoTests
{
    private static readonly DateTime Agora = new(2026, 11, 19, 12, 0, 0, DateTimeKind.Utc);
    private static readonly ParametrosIrrigacao Parametros = new();
    private static readonly Plantacao Milho = new(1, "Milho", 5, 25, 35);

    private static readonly MotorRecomendacao Motor = new(
    [
        // ordem embaralhada de propósito: o motor deve ordenar pela prioridade
        new RegraUmidadeAdequada(), new RegraChuvaPrevista(), new RegraMedicaoIndisponivel(),
        new RegraUmidadeBaixa(), new RegraUmidadeCritica(),
    ]);

    private static (TipoRecomendacao Tipo, string Regra, string Texto) Avaliar(
        decimal? umidade, decimal? chuva = null, int horasDaMedicao = 1, int horasDaPrevisao = 1)
    {
        var medicao = umidade is null
            ? null
            : new MedicaoSolo(1, umidade.Value, Agora.AddHours(-horasDaMedicao), OrigemMedicao.Sensor, Agora);
        var previsao = chuva is null ? null : new PrevisaoChuva(1, chuva.Value, Agora.AddHours(-horasDaPrevisao), "teste");
        var (resultado, regra) = Motor.Avaliar(new ContextoAvaliacao(Milho, medicao, previsao, Agora, Parametros));
        return (resultado.Tipo, regra, resultado.Justificativa);
    }

    [Fact]
    public void Sem_medicao_pede_reavaliacao()
    {
        var r = Avaliar(null);
        Assert.Equal(TipoRecomendacao.Reavaliar, r.Tipo);
        Assert.Equal(nameof(RegraMedicaoIndisponivel), r.Regra);
    }

    [Fact]
    public void Medicao_antiga_pede_reavaliacao_mesmo_se_estiver_seca() =>
        Assert.Equal(TipoRecomendacao.Reavaliar, Avaliar(10, horasDaMedicao: 30).Tipo);

    [Fact]
    public void Umidade_critica_tem_prioridade_sobre_chuva_prevista()
    {
        var r = Avaliar(20, chuva: 0.95m);
        Assert.Equal(TipoRecomendacao.IrrigacaoPrioritaria, r.Tipo);
        Assert.Contains("95% de chance de chuva", r.Texto);
    }

    [Fact]
    public void Umidade_baixa_com_chuva_provavel_aguarda_e_conta_como_irrigacao_evitada()
    {
        var r = Avaliar(30, chuva: 0.8m);
        Assert.Equal(TipoRecomendacao.AguardarChuva, r.Tipo);
        Assert.Contains("80%", r.Texto);
    }

    [Theory]
    [InlineData(0.6, TipoRecomendacao.AguardarChuva)]   // exatamente no limiar
    [InlineData(0.59, TipoRecomendacao.IrrigarHoje)]    // logo abaixo
    public void Limiar_de_chuva_e_respeitado(decimal chuva, TipoRecomendacao esperado) =>
        Assert.Equal(esperado, Avaliar(30, chuva).Tipo);

    [Fact]
    public void Previsao_expirada_e_ignorada()
    {
        var r = Avaliar(30, chuva: 0.9m, horasDaPrevisao: 30);
        Assert.Equal(TipoRecomendacao.IrrigarHoje, r.Tipo);
        Assert.Contains("não há previsão de chuva", r.Texto);
    }

    [Fact]
    public void Umidade_adequada_nao_irriga() =>
        Assert.Equal(TipoRecomendacao.NaoIrrigar, Avaliar(40, chuva: 0.1m).Tipo);

    [Fact]
    public void Nova_regra_entra_sem_alterar_o_motor()
    {
        var motor = new MotorRecomendacao([new RegraUmidadeAdequada(), new RegraSempreReavaliar()]);
        var medicao = new MedicaoSolo(1, 40, Agora, OrigemMedicao.Sensor, Agora);
        var (resultado, regra) = motor.Avaliar(new ContextoAvaliacao(Milho, medicao, null, Agora, Parametros));
        Assert.Equal(nameof(RegraSempreReavaliar), regra);
        Assert.Equal(TipoRecomendacao.Reavaliar, resultado.Tipo);
    }

    private sealed class RegraSempreReavaliar : RegraIrrigacaoBase
    {
        public override int Prioridade => 1;
        public override bool SeAplica(ContextoAvaliacao contexto) => true;
        public override ResultadoRegra Avaliar(ContextoAvaliacao contexto) =>
            new(TipoRecomendacao.Reavaliar, "Regra de teste.");
    }
}
