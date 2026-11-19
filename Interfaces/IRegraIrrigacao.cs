using SoloCerto.Api.Models;

namespace SoloCerto.Api.Interfaces;

/// <summary>Parâmetros configuráveis do motor (seção "Irrigacao" do appsettings.json).</summary>
public class ParametrosIrrigacao
{
    /// <summary>A partir desta probabilidade (0 a 1) a chuva é considerada provável.</summary>
    public decimal LimiarProbabilidadeChuva { get; set; } = 0.6m;

    /// <summary>Idade máxima de uma medição para ainda servir de base à decisão.</summary>
    public TimeSpan ValidadeMedicao { get; set; } = TimeSpan.FromHours(24);
}

/// <summary>Tudo o que uma regra precisa para decidir.</summary>
public record ContextoAvaliacao(Plantacao Plantacao, MedicaoSolo? Medicao, PrevisaoChuva? Previsao,
                                DateTime Agora, ParametrosIrrigacao Parametros)
{
    /// <summary>Probabilidade de chuva só conta se a previsão estiver vigente.</summary>
    public decimal? ProbabilidadeChuva => Previsao is not null && Previsao.EstaVigente(Agora)
        ? Previsao.Probabilidade
        : null;
}

public record ResultadoRegra(TipoRecomendacao Tipo, string Justificativa);

/// <summary>Uma regra de decisão de irrigação (Strategy).</summary>
public interface IRegraIrrigacao
{
    int Prioridade { get; }
    string Nome { get; }
    bool SeAplica(ContextoAvaliacao contexto);
    ResultadoRegra Avaliar(ContextoAvaliacao contexto);
}

/// <summary>Escolhe e executa a regra aplicável.</summary>
public interface IMotorRecomendacao
{
    (ResultadoRegra Resultado, string Regra) Avaliar(ContextoAvaliacao contexto);
}
