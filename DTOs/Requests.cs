using System.ComponentModel.DataAnnotations;
using SoloCerto.Api.Models;

namespace SoloCerto.Api.DTOs;

/// <summary>Dados para cadastrar ou atualizar um agricultor.</summary>
public record AgricultorRequest(
    [Required(ErrorMessage = "O nome é obrigatório."), StringLength(120, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 120 caracteres.")] string Nome,
    [Required(ErrorMessage = "A cidade é obrigatória."), StringLength(80, MinimumLength = 2, ErrorMessage = "A cidade deve ter entre 2 e 80 caracteres.")] string Cidade,
    [EmailAddress(ErrorMessage = "E-mail inválido."), StringLength(160)] string? Email);

/// <summary>Dados para cadastrar uma área de cultivo.</summary>
/// <param name="AgricultorId">Dono da plantação.</param>
/// <param name="Cultura">Ex.: Milho, Tomate, Café.</param>
/// <param name="AreaHectares">Área em hectares (maior que zero).</param>
/// <param name="UmidadeCritica">Umidade do solo (%) abaixo da qual a irrigação é prioritária.</param>
/// <param name="UmidadeIdeal">Umidade do solo (%) considerada adequada.</param>
public record PlantacaoRequest(
    [Range(1, int.MaxValue, ErrorMessage = "Informe um agricultor válido.")] int AgricultorId,
    [Required(ErrorMessage = "A cultura é obrigatória."), StringLength(60, MinimumLength = 2, ErrorMessage = "A cultura deve ter entre 2 e 60 caracteres.")] string Cultura,
    [Range(0.01, 100000, ErrorMessage = "A área deve ser maior que zero.")] decimal AreaHectares,
    [Range(0, 100, ErrorMessage = "A umidade crítica deve estar entre 0 e 100.")] decimal UmidadeCritica,
    [Range(0, 100, ErrorMessage = "A umidade ideal deve estar entre 0 e 100.")] decimal UmidadeIdeal);

/// <summary>Dados para atualizar uma área de cultivo.</summary>
public record PlantacaoUpdateRequest(
    [Required(ErrorMessage = "A cultura é obrigatória."), StringLength(60, MinimumLength = 2, ErrorMessage = "A cultura deve ter entre 2 e 60 caracteres.")] string Cultura,
    [Range(0.01, 100000, ErrorMessage = "A área deve ser maior que zero.")] decimal AreaHectares,
    [Range(0, 100, ErrorMessage = "A umidade crítica deve estar entre 0 e 100.")] decimal UmidadeCritica,
    [Range(0, 100, ErrorMessage = "A umidade ideal deve estar entre 0 e 100.")] decimal UmidadeIdeal);

/// <summary>Leitura de umidade do solo. Ao registrar, a API já devolve a recomendação de irrigação.</summary>
/// <param name="PlantacaoId">Plantação medida.</param>
/// <param name="UmidadePercentual">Umidade do solo em % (0 a 100).</param>
/// <param name="MedidoEm">Momento da leitura (UTC). Se omitido, usa o horário atual.</param>
/// <param name="Origem">Sensor ou Manual.</param>
public record MedicaoRequest(
    [Range(1, int.MaxValue, ErrorMessage = "Informe uma plantação válida.")] int PlantacaoId,
    [Range(0, 100, ErrorMessage = "A umidade deve estar entre 0 e 100.")] decimal UmidadePercentual,
    DateTime? MedidoEm,
    OrigemMedicao Origem = OrigemMedicao.Manual);

/// <summary>Previsão de chuva produzida pelo módulo de IA (Random Forest/INMET) para as próximas 24 h.</summary>
/// <param name="PlantacaoId">Plantação a que a previsão se aplica.</param>
/// <param name="Probabilidade">Probabilidade de chuva ≥ 1 mm em 24 h, entre 0 e 1.</param>
/// <param name="ValidaAPartirDe">Início da janela de 24 h (UTC). Se omitido, usa o horário atual.</param>
/// <param name="Fonte">Origem da previsão.</param>
public record PrevisaoChuvaRequest(
    [Range(1, int.MaxValue, ErrorMessage = "Informe uma plantação válida.")] int PlantacaoId,
    [Range(0, 1, ErrorMessage = "A probabilidade deve estar entre 0 e 1.")] decimal Probabilidade,
    DateTime? ValidaAPartirDe,
    [StringLength(60)] string Fonte = "modelo-random-forest-inmet");
