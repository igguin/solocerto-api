using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SoloCerto.Api.Data;
using SoloCerto.Api.DTOs;
using SoloCerto.Api.Interfaces;
using SoloCerto.Api.Models;

namespace SoloCerto.Api.Services;

public class RecomendacaoService(AppDbContext db, IMotorRecomendacao motor, IPrevisaoChuvaService previsoes,
                                 IOptions<ParametrosIrrigacao> parametros, TimeProvider relogio)
    : IRecomendacaoService
{
    public async Task<RecomendacaoResponse> AvaliarAsync(int plantacaoId, CancellationToken ct = default)
    {
        var plantacao = await db.Plantacoes.FirstOrDefaultAsync(p => p.Id == plantacaoId, ct)
                        ?? throw new RecursoNaoEncontradoException("Plantação", plantacaoId);
        var ultima = await db.Medicoes
            .Where(m => m.PlantacaoId == plantacaoId)
            .OrderByDescending(m => m.MedidoEm)
            .FirstOrDefaultAsync(ct);

        var recomendacao = await GerarAsync(plantacao, ultima, ct);
        return recomendacao.ToResponse();
    }

    public Task<RecomendacaoIrrigacao> GerarParaMedicaoAsync(Plantacao plantacao, MedicaoSolo medicao,
                                                            CancellationToken ct = default) =>
        GerarAsync(plantacao, medicao, ct);

    public async Task<IReadOnlyList<RecomendacaoResponse>> ListarAsync(int? plantacaoId, TipoRecomendacao? tipo,
                                                                       CancellationToken ct = default)
    {
        var consulta = db.Recomendacoes.AsNoTracking();
        if (plantacaoId is not null) consulta = consulta.Where(r => r.PlantacaoId == plantacaoId);
        if (tipo is not null) consulta = consulta.Where(r => r.Tipo == tipo);

        var lista = await consulta.OrderByDescending(r => r.CriadoEm).ThenByDescending(r => r.Id)
                                  .Take(200).ToListAsync(ct);
        return lista.Select(r => r.ToResponse()).ToList();
    }

    private async Task<RecomendacaoIrrigacao> GerarAsync(Plantacao plantacao, MedicaoSolo? medicao,
                                                         CancellationToken ct)
    {
        var agora = relogio.GetUtcNow().UtcDateTime;
        var previsao = await previsoes.ObterVigenteAsync(plantacao.Id, ct);
        var contexto = new ContextoAvaliacao(plantacao, medicao, previsao, agora, parametros.Value);

        var (resultado, regra) = motor.Avaliar(contexto);
        var recomendacao = new RecomendacaoIrrigacao(
            plantacao.Id, medicao?.Id, resultado.Tipo, regra, resultado.Justificativa,
            medicao?.UmidadePercentual, contexto.ProbabilidadeChuva, agora);

        db.Recomendacoes.Add(recomendacao);
        await db.SaveChangesAsync(ct);
        return recomendacao;
    }
}
