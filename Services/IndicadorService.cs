using Microsoft.EntityFrameworkCore;
using SoloCerto.Api.Data;
using SoloCerto.Api.DTOs;
using SoloCerto.Api.Interfaces;
using SoloCerto.Api.Models;

namespace SoloCerto.Api.Services;

/// <summary>Indicadores de impacto calculados com LINQ (agrupamentos executados no banco).</summary>
public class IndicadorService(AppDbContext db) : IIndicadorService
{
    public async Task<IndicadoresResponse> ObterAsync(CancellationToken ct = default)
    {
        var totalAgricultores = await db.Agricultores.CountAsync(ct);
        var totalPlantacoes = await db.Plantacoes.CountAsync(ct);
        var totalMedicoes = await db.Medicoes.CountAsync(ct);
        var areas = await db.Plantacoes.Select(p => p.AreaHectares).ToListAsync(ct);

        var porTipo = await db.Recomendacoes
            .GroupBy(r => r.Tipo)
            .Select(g => new TotalPorTipo(g.Key, g.Count()))
            .ToListAsync(ct);
        porTipo = porTipo.OrderByDescending(t => t.Quantidade).ThenBy(t => t.Tipo).ToList();

        var totalRecomendacoes = porTipo.Sum(t => t.Quantidade);
        int Qtd(TipoRecomendacao t) => porTipo.FirstOrDefault(x => x.Tipo == t)?.Quantidade ?? 0;

        // Situações em que o solo pedia água (abaixo do ideal, sem ser crítico): quantas a chuva resolveu?
        var evitadas = Qtd(TipoRecomendacao.AguardarChuva);
        var candidatas = evitadas + Qtd(TipoRecomendacao.IrrigarHoje);
        var percentual = candidatas == 0 ? 0 : decimal.Round(100m * evitadas / candidatas, 1);

        var umidadePorCultura = await db.Medicoes
            .GroupBy(m => m.Plantacao!.Cultura)
            .Select(g => new UmidadePorCultura(g.Key, g.Count(), g.Average(m => m.UmidadePercentual)))
            .ToListAsync(ct);
        umidadePorCultura = umidadePorCultura
            .Select(u => u with { UmidadeMedia = decimal.Round(u.UmidadeMedia, 1) })
            .OrderBy(u => u.Cultura).ToList();

        // Plantações cuja recomendação MAIS RECENTE é de irrigação prioritária
        var ultimas = await db.Recomendacoes
            .GroupBy(r => r.PlantacaoId)
            .Select(g => g.OrderByDescending(r => r.CriadoEm).ThenByDescending(r => r.Id).First().Id)
            .ToListAsync(ct);
        var criticas = await db.Recomendacoes
            .Where(r => ultimas.Contains(r.Id) && r.Tipo == TipoRecomendacao.IrrigacaoPrioritaria)
            .OrderBy(r => r.UmidadeConsiderada)
            .Select(r => new PlantacaoCritica(r.PlantacaoId, r.Plantacao!.Cultura, r.Plantacao.Agricultor!.Nome,
                                              r.Plantacao.Agricultor.Cidade, r.UmidadeConsiderada, r.CriadoEm))
            .ToListAsync(ct);

        return new IndicadoresResponse(totalAgricultores, totalPlantacoes, totalMedicoes, totalRecomendacoes,
                                       porTipo, evitadas, percentual, decimal.Round(areas.Sum(), 2),
                                       umidadePorCultura, criticas);
    }
}
