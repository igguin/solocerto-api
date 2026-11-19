namespace SoloCerto.Api.Models;

/// <summary>
/// Previsão de chuva (≥ 1 mm nas próximas 24 h) recebida do módulo de IA do SoloCerto
/// (Random Forest treinado com dados do INMET) ou informada manualmente.
/// </summary>
public class PrevisaoChuva : EntidadeBase
{
    public int PlantacaoId { get; private set; }
    public Plantacao? Plantacao { get; private set; }

    /// <summary>Probabilidade de chuva entre 0 e 1.</summary>
    public decimal Probabilidade { get; private set; }

    /// <summary>Início da janela de 24 h a que a previsão se refere (UTC).</summary>
    public DateTime ValidaAPartirDe { get; private set; }

    public string Fonte { get; private set; } = string.Empty;

    private PrevisaoChuva() { } // EF Core

    public PrevisaoChuva(int plantacaoId, decimal probabilidade, DateTime validaAPartirDe, string fonte)
    {
        Garantir(plantacaoId > 0, "Plantação inválida.");
        Garantir(probabilidade is >= 0 and <= 1, "A probabilidade deve estar entre 0 e 1.");
        PlantacaoId = plantacaoId;
        Probabilidade = decimal.Round(probabilidade, 3);
        ValidaAPartirDe = validaAPartirDe;
        Fonte = TextoObrigatorio(fonte, "A fonte", 60);
    }

    public bool EstaVigente(DateTime agoraUtc) =>
        agoraUtc >= ValidaAPartirDe && agoraUtc < ValidaAPartirDe.AddHours(24);
}
