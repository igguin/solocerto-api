using SoloCerto.Api.Models;

namespace SoloCerto.Api.DTOs;

public record AgricultorResponse(int Id, string Nome, string Cidade, string? Email, DateTime CriadoEm,
                                 int TotalPlantacoes);

public record PlantacaoResponse(int Id, int AgricultorId, string Cultura, decimal AreaHectares,
                                decimal UmidadeCritica, decimal UmidadeIdeal, DateTime CriadoEm);

public record MedicaoResponse(int Id, int PlantacaoId, decimal UmidadePercentual, DateTime MedidoEm,
                              OrigemMedicao Origem, FaixaUmidade Faixa);

public record PrevisaoChuvaResponse(int Id, int PlantacaoId, decimal Probabilidade, DateTime ValidaAPartirDe,
                                    DateTime ValidaAte, string Fonte);

/// <summary>Recomendação de irrigação com mensagem simples para o produtor.</summary>
public record RecomendacaoResponse(int Id, int PlantacaoId, int? MedicaoSoloId, TipoRecomendacao Tipo,
                                   string Mensagem, string Justificativa, string RegraAplicada,
                                   decimal? UmidadeConsiderada, decimal? ProbabilidadeChuvaConsiderada,
                                   bool IrrigacaoEvitada, DateTime GeradaEm);

public record RegistroMedicaoResponse(MedicaoResponse Medicao, RecomendacaoResponse Recomendacao);

public record SituacaoPlantacaoResponse(PlantacaoResponse Plantacao, MedicaoResponse? UltimaMedicao,
                                        PrevisaoChuvaResponse? PrevisaoVigente,
                                        RecomendacaoResponse? UltimaRecomendacao);

public record HistoricoPlantacaoResponse(int PlantacaoId, DateTime Desde, int TotalMedicoes,
                                         decimal? UmidadeMedia, decimal? UmidadeMinima, decimal? UmidadeMaxima,
                                         IReadOnlyList<MedicaoResponse> Medicoes,
                                         IReadOnlyList<RecomendacaoResponse> Recomendacoes);

public record TotalPorTipo(TipoRecomendacao Tipo, int Quantidade);

public record UmidadePorCultura(string Cultura, int Medicoes, decimal UmidadeMedia);

public record PlantacaoCritica(int PlantacaoId, string Cultura, string Agricultor, string Cidade,
                               decimal? UmidadeConsiderada, DateTime RecomendadoEm);

/// <summary>Indicadores de impacto do SoloCerto.</summary>
/// <param name="PercentualIrrigacoesEvitadas">
/// Das recomendações em que a umidade estava abaixo do ideal, quantas % foram "aguardar chuva"
/// (irrigação potencialmente desnecessária evitada).
/// </param>
public record IndicadoresResponse(int TotalAgricultores, int TotalPlantacoes, int TotalMedicoes,
                                  int TotalRecomendacoes, IReadOnlyList<TotalPorTipo> RecomendacoesPorTipo,
                                  int IrrigacoesEvitadas, decimal PercentualIrrigacoesEvitadas,
                                  decimal AreaMonitoradaHectares, IReadOnlyList<UmidadePorCultura> UmidadePorCultura,
                                  IReadOnlyList<PlantacaoCritica> PlantacoesEmSituacaoCritica);
