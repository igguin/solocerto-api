using Microsoft.EntityFrameworkCore;
using SoloCerto.Api.Data;
using SoloCerto.Api.DTOs;
using SoloCerto.Api.Interfaces;
using SoloCerto.Api.Models;

namespace SoloCerto.Api.Services;

public class MedicaoService(AppDbContext db, IRecomendacaoService recomendacoes, TimeProvider relogio)
    : IMedicaoService
{
    public async Task<RegistroMedicaoResponse> RegistrarAsync(MedicaoRequest dados, CancellationToken ct = default)
    {
        var plantacao = await db.Plantacoes.FirstOrDefaultAsync(p => p.Id == dados.PlantacaoId, ct)
                        ?? throw new RecursoNaoEncontradoException("Plantação", dados.PlantacaoId);

        var agora = relogio.GetUtcNow().UtcDateTime;
        var medidoEm = dados.MedidoEm?.ToUniversalTime() ?? agora;
        var medicao = new MedicaoSolo(plantacao.Id, dados.UmidadePercentual, medidoEm, dados.Origem, agora);

        db.Medicoes.Add(medicao);
        await db.SaveChangesAsync(ct);

        var recomendacao = await recomendacoes.GerarParaMedicaoAsync(plantacao, medicao, ct);
        return new RegistroMedicaoResponse(medicao.ToResponse(plantacao), recomendacao.ToResponse());
    }

    public async Task<IReadOnlyList<MedicaoResponse>> ListarAsync(int plantacaoId, DateTime? desde, DateTime? ate,
                                                                  CancellationToken ct = default)
    {
        var plantacao = await db.Plantacoes.AsNoTracking().FirstOrDefaultAsync(p => p.Id == plantacaoId, ct)
                        ?? throw new RecursoNaoEncontradoException("Plantação", plantacaoId);
        if (desde > ate)
            throw new DomainException("A data inicial deve ser anterior à final.");

        var consulta = db.Medicoes.AsNoTracking().Where(m => m.PlantacaoId == plantacaoId);
        if (desde is not null) consulta = consulta.Where(m => m.MedidoEm >= desde.Value.ToUniversalTime());
        if (ate is not null) consulta = consulta.Where(m => m.MedidoEm <= ate.Value.ToUniversalTime());

        var lista = await consulta.OrderByDescending(m => m.MedidoEm).ToListAsync(ct);
        return lista.Select(m => m.ToResponse(plantacao)).ToList();
    }
}
