namespace SoloCerto.Api.Models;

/// <summary>Leitura de umidade do solo de uma plantação (sensor ou registro manual).</summary>
public class MedicaoSolo : EntidadeBase
{
    public int PlantacaoId { get; private set; }
    public Plantacao? Plantacao { get; private set; }

    /// <summary>Umidade volumétrica do solo em %.</summary>
    public decimal UmidadePercentual { get; private set; }

    /// <summary>Momento da leitura (UTC).</summary>
    public DateTime MedidoEm { get; private set; }

    public OrigemMedicao Origem { get; private set; }

    private MedicaoSolo() { } // EF Core

    public MedicaoSolo(int plantacaoId, decimal umidadePercentual, DateTime medidoEm, OrigemMedicao origem,
                       DateTime agoraUtc)
    {
        Garantir(plantacaoId > 0, "Plantação inválida.");
        Garantir(umidadePercentual is >= 0 and <= 100, "A umidade deve estar entre 0% e 100%.");
        Garantir(medidoEm <= agoraUtc.AddMinutes(5), "A data da medição não pode estar no futuro.");
        PlantacaoId = plantacaoId;
        UmidadePercentual = decimal.Round(umidadePercentual, 1);
        MedidoEm = medidoEm;
        Origem = origem;
        CriadoEm = agoraUtc;
    }

    public bool EstaDesatualizada(DateTime agoraUtc, TimeSpan validade) => agoraUtc - MedidoEm > validade;
}
