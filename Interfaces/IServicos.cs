using SoloCerto.Api.DTOs;
using SoloCerto.Api.Models;

namespace SoloCerto.Api.Interfaces;

public interface IAgricultorService
{
    Task<IReadOnlyList<AgricultorResponse>> ListarAsync(string? cidade, CancellationToken ct = default);
    Task<AgricultorResponse> ObterAsync(int id, CancellationToken ct = default);
    Task<AgricultorResponse> CriarAsync(AgricultorRequest dados, CancellationToken ct = default);
    Task AtualizarAsync(int id, AgricultorRequest dados, CancellationToken ct = default);
    Task RemoverAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<PlantacaoResponse>> ListarPlantacoesAsync(int id, CancellationToken ct = default);
}

public interface IPlantacaoService
{
    Task<IReadOnlyList<PlantacaoResponse>> ListarAsync(int? agricultorId, string? cultura, CancellationToken ct = default);
    Task<PlantacaoResponse> ObterAsync(int id, CancellationToken ct = default);
    Task<PlantacaoResponse> CriarAsync(PlantacaoRequest dados, CancellationToken ct = default);
    Task AtualizarAsync(int id, PlantacaoUpdateRequest dados, CancellationToken ct = default);
    Task RemoverAsync(int id, CancellationToken ct = default);
    Task<SituacaoPlantacaoResponse> ObterSituacaoAsync(int id, CancellationToken ct = default);
    Task<HistoricoPlantacaoResponse> ObterHistoricoAsync(int id, int dias, CancellationToken ct = default);
}

public interface IMedicaoService
{
    /// <summary>Registra a medição e já gera a recomendação de irrigação.</summary>
    Task<RegistroMedicaoResponse> RegistrarAsync(MedicaoRequest dados, CancellationToken ct = default);
    Task<IReadOnlyList<MedicaoResponse>> ListarAsync(int plantacaoId, DateTime? desde, DateTime? ate,
                                                     CancellationToken ct = default);
}

public interface IPrevisaoChuvaService
{
    Task<PrevisaoChuvaResponse> RegistrarAsync(PrevisaoChuvaRequest dados, CancellationToken ct = default);
    Task<IReadOnlyList<PrevisaoChuvaResponse>> ListarAsync(int plantacaoId, CancellationToken ct = default);
    Task<PrevisaoChuva?> ObterVigenteAsync(int plantacaoId, CancellationToken ct = default);
}

public interface IRecomendacaoService
{
    /// <summary>Avalia a plantação com a última medição e a previsão vigente, e persiste o resultado.</summary>
    Task<RecomendacaoResponse> AvaliarAsync(int plantacaoId, CancellationToken ct = default);
    Task<RecomendacaoIrrigacao> GerarParaMedicaoAsync(Plantacao plantacao, MedicaoSolo medicao,
                                                     CancellationToken ct = default);
    Task<IReadOnlyList<RecomendacaoResponse>> ListarAsync(int? plantacaoId, TipoRecomendacao? tipo,
                                                          CancellationToken ct = default);
}

public interface IIndicadorService
{
    Task<IndicadoresResponse> ObterAsync(CancellationToken ct = default);
}
