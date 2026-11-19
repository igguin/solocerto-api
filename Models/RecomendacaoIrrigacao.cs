namespace SoloCerto.Api.Models;

/// <summary>Orientação de irrigação gerada pelo motor de regras a partir de uma medição.</summary>
public class RecomendacaoIrrigacao : EntidadeBase
{
    public int PlantacaoId { get; private set; }
    public Plantacao? Plantacao { get; private set; }

    public int? MedicaoSoloId { get; private set; }
    public MedicaoSolo? MedicaoSolo { get; private set; }

    public TipoRecomendacao Tipo { get; private set; }

    /// <summary>Nome da regra que decidiu (rastreabilidade da decisão).</summary>
    public string RegraAplicada { get; private set; } = string.Empty;

    public string Justificativa { get; private set; } = string.Empty;

    public decimal? UmidadeConsiderada { get; private set; }

    public decimal? ProbabilidadeChuvaConsiderada { get; private set; }

    /// <summary>
    /// Indicador de impacto: a umidade estava abaixo do ideal, mas a chuva prevista
    /// tornou a irrigação desnecessária naquele momento.
    /// </summary>
    public bool IrrigacaoEvitada => Tipo == TipoRecomendacao.AguardarChuva;

    private RecomendacaoIrrigacao() { } // EF Core

    public RecomendacaoIrrigacao(int plantacaoId, int? medicaoSoloId, TipoRecomendacao tipo, string regraAplicada,
                                 string justificativa, decimal? umidade, decimal? probabilidadeChuva, DateTime geradaEm)
    {
        Garantir(plantacaoId > 0, "Plantação inválida.");
        PlantacaoId = plantacaoId;
        MedicaoSoloId = medicaoSoloId;
        Tipo = tipo;
        RegraAplicada = TextoObrigatorio(regraAplicada, "A regra", 60);
        Justificativa = TextoObrigatorio(justificativa, "A justificativa", 400);
        UmidadeConsiderada = umidade;
        ProbabilidadeChuvaConsiderada = probabilidadeChuva;
        CriadoEm = geradaEm;
    }
}
