using Microsoft.EntityFrameworkCore;
using SoloCerto.Api.Data;
using SoloCerto.Api.DTOs;
using SoloCerto.Api.Interfaces;
using SoloCerto.Api.Models;

namespace SoloCerto.Api.Services;

public class PlantacaoService(AppDbContext db, IPrevisaoChuvaService previsoes, TimeProvider relogio)
    : IPlantacaoService
{
    public async Task<IReadOnlyList<PlantacaoResponse>> ListarAsync(int? agricultorId, string? cultura,
                                                                   CancellationToken ct = default)
    {
        var consulta = db.Plantacoes.AsNoTracking();
        if (agricultorId is not null)
            consulta = consulta.Where(p => p.AgricultorId == agricultorId);
        if (!string.IsNullOrWhiteSpace(cultura))
            consulta = consulta.Where(p => p.Cultura.ToLower() == cultura.Trim().ToLower());

        var lista = await consulta.OrderBy(p => p.AgricultorId).ThenBy(p => p.Cultura).ToListAsync(ct);
        return lista.Select(p => p.ToResponse()).ToList();
    }

    public async Task<PlantacaoResponse> ObterAsync(int id, CancellationToken ct = default) =>
        (await BuscarAsync(id, ct, rastrear: false)).ToResponse();

    public async Task<PlantacaoResponse> CriarAsync(PlantacaoRequest dados, CancellationToken ct = default)
    {
        if (!await db.Agricultores.AnyAsync(a => a.Id == dados.AgricultorId, ct))
            throw new RecursoNaoEncontradoException("Agricultor", dados.AgricultorId);

        var plantacao = new Plantacao(dados.AgricultorId, dados.Cultura, dados.AreaHectares,
                                      dados.UmidadeCritica, dados.UmidadeIdeal);
        db.Plantacoes.Add(plantacao);
        await db.SaveChangesAsync(ct);
        return plantacao.ToResponse();
    }

    public async Task AtualizarAsync(int id, PlantacaoUpdateRequest dados, CancellationToken ct = default)
    {
        var plantacao = await BuscarAsync(id, ct);
        plantacao.Atualizar(dados.Cultura, dados.AreaHectares, dados.UmidadeCritica, dados.UmidadeIdeal);
        await db.SaveChangesAsync(ct);
    }

    public async Task RemoverAsync(int id, CancellationToken ct = default)
    {
        db.Plantacoes.Remove(await BuscarAsync(id, ct));
        await db.SaveChangesAsync(ct);
    }

    public async Task<SituacaoPlantacaoResponse> ObterSituacaoAsync(int id, CancellationToken ct = default)
    {
        var plantacao = await BuscarAsync(id, ct, rastrear: false);

        var ultimaMedicao = await db.Medicoes.AsNoTracking()
            .Where(m => m.PlantacaoId == id)
            .OrderByDescending(m => m.MedidoEm)
            .FirstOrDefaultAsync(ct);
        var ultimaRecomendacao = await db.Recomendacoes.AsNoTracking()
            .Where(r => r.PlantacaoId == id)
            .OrderByDescending(r => r.CriadoEm).ThenByDescending(r => r.Id)
            .FirstOrDefaultAsync(ct);
        var previsao = await previsoes.ObterVigenteAsync(id, ct);

        return new SituacaoPlantacaoResponse(plantacao.ToResponse(), ultimaMedicao?.ToResponse(plantacao),
                                             previsao?.ToResponse(), ultimaRecomendacao?.ToResponse());
    }

    public async Task<HistoricoPlantacaoResponse> ObterHistoricoAsync(int id, int dias, CancellationToken ct = default)
    {
        if (dias is < 1 or > 365)
            throw new DomainException("O período deve ser de 1 a 365 dias.");

        var plantacao = await BuscarAsync(id, ct, rastrear: false);
        var desde = relogio.GetUtcNow().UtcDateTime.AddDays(-dias);

        var medicoesQuery = db.Medicoes.AsNoTracking().Where(m => m.PlantacaoId == id && m.MedidoEm >= desde);

        // Agregações calculadas no banco (LINQ → SQL)
        var estatisticas = await medicoesQuery
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Total = g.Count(),
                Media = g.Average(m => m.UmidadePercentual),
                Minima = g.Min(m => m.UmidadePercentual),
                Maxima = g.Max(m => m.UmidadePercentual),
            })
            .FirstOrDefaultAsync(ct);

        var medicoes = await medicoesQuery.OrderByDescending(m => m.MedidoEm).ToListAsync(ct);
        var recomendacoes = await db.Recomendacoes.AsNoTracking()
            .Where(r => r.PlantacaoId == id && r.CriadoEm >= desde)
            .OrderByDescending(r => r.CriadoEm).ThenByDescending(r => r.Id)
            .ToListAsync(ct);

        return new HistoricoPlantacaoResponse(
            id, desde, estatisticas?.Total ?? 0,
            estatisticas is null ? null : decimal.Round(estatisticas.Media, 1),
            estatisticas?.Minima, estatisticas?.Maxima,
            medicoes.Select(m => m.ToResponse(plantacao)).ToList(),
            recomendacoes.Select(r => r.ToResponse()).ToList());
    }

    private async Task<Plantacao> BuscarAsync(int id, CancellationToken ct, bool rastrear = true)
    {
        var consulta = rastrear ? db.Plantacoes : db.Plantacoes.AsNoTracking();
        return await consulta.FirstOrDefaultAsync(p => p.Id == id, ct)
               ?? throw new RecursoNaoEncontradoException("Plantação", id);
    }
}
