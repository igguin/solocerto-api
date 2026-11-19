namespace SoloCerto.Api.Models;

public enum OrigemMedicao
{
    Sensor = 1,
    Manual = 2,
}

public enum FaixaUmidade
{
    Critica = 1,
    Baixa = 2,
    Adequada = 3,
}

public enum TipoRecomendacao
{
    /// <summary>Umidade abaixo do limite crítico: irrigar o quanto antes, mesmo com chuva prevista.</summary>
    IrrigacaoPrioritaria = 1,

    /// <summary>Umidade abaixo do ideal e sem chuva relevante prevista: irrigar hoje.</summary>
    IrrigarHoje = 2,

    /// <summary>Umidade abaixo do ideal, mas com boa chance de chuva: aguardar e monitorar.</summary>
    AguardarChuva = 3,

    /// <summary>Umidade adequada: não irrigar.</summary>
    NaoIrrigar = 4,

    /// <summary>Dados insuficientes ou desatualizados: fazer nova medição.</summary>
    Reavaliar = 5,
}
