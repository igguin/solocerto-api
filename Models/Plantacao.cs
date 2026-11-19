namespace SoloCerto.Api.Models;

/// <summary>
/// Área de cultivo de um agricultor. Guarda a faixa de umidade do solo
/// adequada para a cultura: abaixo de <see cref="UmidadeCritica"/> a irrigação é prioritária;
/// entre a crítica e a <see cref="UmidadeIdeal"/> a decisão depende da chuva prevista.
/// </summary>
public class Plantacao : EntidadeBase
{
    public int AgricultorId { get; private set; }
    public Agricultor? Agricultor { get; private set; }

    public string Cultura { get; private set; } = string.Empty;

    /// <summary>Área em hectares.</summary>
    public decimal AreaHectares { get; private set; }

    /// <summary>Umidade do solo (%) abaixo da qual a planta entra em estresse hídrico.</summary>
    public decimal UmidadeCritica { get; private set; }

    /// <summary>Umidade do solo (%) considerada adequada para a cultura.</summary>
    public decimal UmidadeIdeal { get; private set; }

    private readonly List<MedicaoSolo> _medicoes = [];
    public IReadOnlyCollection<MedicaoSolo> Medicoes => _medicoes;

    private readonly List<RecomendacaoIrrigacao> _recomendacoes = [];
    public IReadOnlyCollection<RecomendacaoIrrigacao> Recomendacoes => _recomendacoes;

    private Plantacao() { } // EF Core

    public Plantacao(int agricultorId, string cultura, decimal areaHectares,
                     decimal umidadeCritica, decimal umidadeIdeal)
    {
        Garantir(agricultorId > 0, "Agricultor inválido.");
        AgricultorId = agricultorId;
        Atualizar(cultura, areaHectares, umidadeCritica, umidadeIdeal);
    }

    public void Atualizar(string cultura, decimal areaHectares, decimal umidadeCritica, decimal umidadeIdeal)
    {
        Cultura = TextoObrigatorio(cultura, "A cultura", 60);
        Garantir(areaHectares > 0, "A área deve ser maior que zero.");
        Garantir(umidadeCritica is >= 0 and <= 100 && umidadeIdeal is >= 0 and <= 100,
                 "As umidades devem estar entre 0% e 100%.");
        Garantir(umidadeCritica < umidadeIdeal, "A umidade crítica deve ser menor que a umidade ideal.");
        AreaHectares = areaHectares;
        UmidadeCritica = umidadeCritica;
        UmidadeIdeal = umidadeIdeal;
    }

    public FaixaUmidade ClassificarUmidade(decimal umidade) =>
        umidade < UmidadeCritica ? FaixaUmidade.Critica
        : umidade < UmidadeIdeal ? FaixaUmidade.Baixa
        : FaixaUmidade.Adequada;
}
