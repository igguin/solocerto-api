using SoloCerto.Api.Interfaces;

namespace SoloCerto.Api.Services;

/// <summary>
/// Executa as regras injetadas (via DI) em ordem de prioridade e devolve a primeira que se aplica.
/// Novas regras entram só registrando uma nova classe: o motor não muda (aberto/fechado).
/// </summary>
public class MotorRecomendacao(IEnumerable<IRegraIrrigacao> regras) : IMotorRecomendacao
{
    private readonly IReadOnlyList<IRegraIrrigacao> _regras = regras.OrderBy(r => r.Prioridade).ToList();

    public (ResultadoRegra Resultado, string Regra) Avaliar(ContextoAvaliacao contexto)
    {
        var regra = _regras.FirstOrDefault(r => r.SeAplica(contexto))
                    ?? throw new InvalidOperationException("Nenhuma regra de irrigação se aplica ao contexto.");
        return (regra.Avaliar(contexto), regra.Nome);
    }
}
