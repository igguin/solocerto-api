using System.Globalization;
using SoloCerto.Api.Interfaces;
using SoloCerto.Api.Models;

namespace SoloCerto.Api.Services.Regras;

/// <summary>
/// Base das regras de irrigação (padrão Strategy). Cada regra concreta decide
/// se se aplica ao contexto e qual recomendação gera. O motor executa as regras
/// em ordem de <see cref="Prioridade"/> e usa a primeira que se aplica.
/// </summary>
public abstract class RegraIrrigacaoBase : IRegraIrrigacao
{
    public abstract int Prioridade { get; }

    public virtual string Nome => GetType().Name;

    public abstract bool SeAplica(ContextoAvaliacao contexto);

    public abstract ResultadoRegra Avaliar(ContextoAvaliacao contexto);

    protected static string Pct(decimal valor) => valor.ToString("0.#", CultureInfo.InvariantCulture) + "%";

    protected static string Chance(decimal probabilidade) =>
        (probabilidade * 100).ToString("0", CultureInfo.InvariantCulture) + "%";
}

/// <summary>Sem medição, ou medição antiga demais: não dá para decidir com segurança.</summary>
public class RegraMedicaoIndisponivel : RegraIrrigacaoBase
{
    public override int Prioridade => 10;

    public override bool SeAplica(ContextoAvaliacao c) =>
        c.Medicao is null || c.Medicao.EstaDesatualizada(c.Agora, c.Parametros.ValidadeMedicao);

    public override ResultadoRegra Avaliar(ContextoAvaliacao c) => c.Medicao is null
        ? new(TipoRecomendacao.Reavaliar, "Não há medição de umidade registrada para esta plantação.")
        : new(TipoRecomendacao.Reavaliar,
              $"A última medição tem mais de {c.Parametros.ValidadeMedicao.TotalHours:0} horas e pode não refletir o solo atual.");
}

/// <summary>Umidade abaixo do limite crítico: irrigar mesmo com chuva prevista (prioridade do solo).</summary>
public class RegraUmidadeCritica : RegraIrrigacaoBase
{
    public override int Prioridade => 20;

    public override bool SeAplica(ContextoAvaliacao c) =>
        c.Medicao is not null && c.Plantacao.ClassificarUmidade(c.Medicao.UmidadePercentual) == FaixaUmidade.Critica;

    public override ResultadoRegra Avaliar(ContextoAvaliacao c)
    {
        var chuva = c.ProbabilidadeChuva is { } p ? $" Mesmo com {Chance(p)} de chance de chuva," : "";
        return new(TipoRecomendacao.IrrigacaoPrioritaria,
            $"Umidade de {Pct(c.Medicao!.UmidadePercentual)} está abaixo do limite crítico de " +
            $"{Pct(c.Plantacao.UmidadeCritica)} para {c.Plantacao.Cultura}.{chuva} o risco de estresse hídrico é alto.");
    }
}

/// <summary>Umidade abaixo do ideal, mas com chuva provável: aguardar (irrigação evitada).</summary>
public class RegraChuvaPrevista : RegraIrrigacaoBase
{
    public override int Prioridade => 30;

    public override bool SeAplica(ContextoAvaliacao c) =>
        c.Medicao is not null
        && c.Plantacao.ClassificarUmidade(c.Medicao.UmidadePercentual) == FaixaUmidade.Baixa
        && c.ProbabilidadeChuva >= c.Parametros.LimiarProbabilidadeChuva;

    public override ResultadoRegra Avaliar(ContextoAvaliacao c) => new(TipoRecomendacao.AguardarChuva,
        $"Umidade de {Pct(c.Medicao!.UmidadePercentual)} está abaixo do ideal ({Pct(c.Plantacao.UmidadeIdeal)}), " +
        $"mas a previsão indica {Chance(c.ProbabilidadeChuva!.Value)} de chance de chuva nas próximas 24 horas.");
}

/// <summary>Umidade abaixo do ideal e sem chuva relevante prevista: irrigar hoje.</summary>
public class RegraUmidadeBaixa : RegraIrrigacaoBase
{
    public override int Prioridade => 40;

    public override bool SeAplica(ContextoAvaliacao c) =>
        c.Medicao is not null && c.Plantacao.ClassificarUmidade(c.Medicao.UmidadePercentual) == FaixaUmidade.Baixa;

    public override ResultadoRegra Avaliar(ContextoAvaliacao c)
    {
        var chuva = c.ProbabilidadeChuva is { } p
            ? $"a chance de chuva ({Chance(p)}) é baixa"
            : "não há previsão de chuva disponível";
        return new(TipoRecomendacao.IrrigarHoje,
            $"Umidade de {Pct(c.Medicao!.UmidadePercentual)} está abaixo do ideal ({Pct(c.Plantacao.UmidadeIdeal)}) e {chuva}.");
    }
}

/// <summary>Regra padrão: umidade adequada.</summary>
public class RegraUmidadeAdequada : RegraIrrigacaoBase
{
    public override int Prioridade => 50;

    public override bool SeAplica(ContextoAvaliacao c) => c.Medicao is not null;

    public override ResultadoRegra Avaliar(ContextoAvaliacao c) => new(TipoRecomendacao.NaoIrrigar,
        $"Umidade de {Pct(c.Medicao!.UmidadePercentual)} está dentro da faixa adequada para {c.Plantacao.Cultura} " +
        $"(ideal a partir de {Pct(c.Plantacao.UmidadeIdeal)}).");
}
