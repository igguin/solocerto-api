using Microsoft.EntityFrameworkCore;
using SoloCerto.Api.Data;
using SoloCerto.Api.DTOs;
using SoloCerto.Api.Interfaces;
using SoloCerto.Api.Models;

namespace SoloCerto.Api.Services;

/// <summary>
/// Recebe as previsões de chuva calculadas pelo módulo de IA do SoloCerto (contrato:
/// POST /api/previsoes-chuva) e entrega ao motor a previsão vigente de cada plantação.
/// </summary>
public class PrevisaoChuvaService(AppDbContext db, TimeProvider relogio) : IPrevisaoChuvaService
{
    public async Task<PrevisaoChuvaResponse> RegistrarAsync(PrevisaoChuvaRequest dados, CancellationToken ct = default)
    {
        if (!await db.Plantacoes.AnyAsync(p => p.Id == dados.PlantacaoId, ct))
            throw new RecursoNaoEncontradoException("Plantação", dados.PlantacaoId);

        var inicio = dados.ValidaAPartirDe?.ToUniversalTime() ?? relogio.GetUtcNow().UtcDateTime;
        var previsao = new PrevisaoChuva(dados.PlantacaoId, dados.Probabilidade, inicio, dados.Fonte);
        db.PrevisoesChuva.Add(previsao);
        await db.SaveChangesAsync(ct);
        return previsao.ToResponse();
    }

    public async Task<IReadOnlyList<PrevisaoChuvaResponse>> ListarAsync(int plantacaoId, CancellationToken ct = default)
    {
        var lista = await db.PrevisoesChuva.AsNoTracking()
            .Where(p => p.PlantacaoId == plantacaoId)
            .OrderByDescending(p => p.ValidaAPartirDe)
            .ToListAsync(ct);
        return lista.Select(p => p.ToResponse()).ToList();
    }

    public async Task<PrevisaoChuva?> ObterVigenteAsync(int plantacaoId, CancellationToken ct = default)
    {
        var agora = relogio.GetUtcNow().UtcDateTime;
        var inicioMinimo = agora.AddHours(-24);
        // a mais recente cuja janela de 24 h contém o momento atual
        return await db.PrevisoesChuva.AsNoTracking()
            .Where(p => p.PlantacaoId == plantacaoId && p.ValidaAPartirDe <= agora && p.ValidaAPartirDe > inicioMinimo)
            .OrderByDescending(p => p.ValidaAPartirDe).ThenByDescending(p => p.Id)
            .FirstOrDefaultAsync(ct);
    }
}
