using Microsoft.EntityFrameworkCore;
using SoloCerto.Api.Data;
using SoloCerto.Api.DTOs;
using SoloCerto.Api.Interfaces;
using SoloCerto.Api.Models;

namespace SoloCerto.Api.Services;

public class AgricultorService(AppDbContext db) : IAgricultorService
{
    public async Task<IReadOnlyList<AgricultorResponse>> ListarAsync(string? cidade, CancellationToken ct = default)
    {
        var consulta = db.Agricultores.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(cidade))
            consulta = consulta.Where(a => a.Cidade.ToLower() == cidade.Trim().ToLower());

        return await consulta
            .OrderBy(a => a.Nome)
            .Select(a => new AgricultorResponse(a.Id, a.Nome, a.Cidade, a.Email, a.CriadoEm, a.Plantacoes.Count))
            .ToListAsync(ct);
    }

    public async Task<AgricultorResponse> ObterAsync(int id, CancellationToken ct = default) =>
        await db.Agricultores.AsNoTracking()
            .Where(a => a.Id == id)
            .Select(a => new AgricultorResponse(a.Id, a.Nome, a.Cidade, a.Email, a.CriadoEm, a.Plantacoes.Count))
            .FirstOrDefaultAsync(ct)
        ?? throw new RecursoNaoEncontradoException("Agricultor", id);

    public async Task<AgricultorResponse> CriarAsync(AgricultorRequest dados, CancellationToken ct = default)
    {
        var agricultor = new Agricultor(dados.Nome, dados.Cidade, dados.Email);
        db.Agricultores.Add(agricultor);
        await db.SaveChangesAsync(ct);
        return new AgricultorResponse(agricultor.Id, agricultor.Nome, agricultor.Cidade, agricultor.Email,
                                      agricultor.CriadoEm, 0);
    }

    public async Task AtualizarAsync(int id, AgricultorRequest dados, CancellationToken ct = default)
    {
        var agricultor = await BuscarAsync(id, ct);
        agricultor.Atualizar(dados.Nome, dados.Cidade, dados.Email);
        await db.SaveChangesAsync(ct);
    }

    public async Task RemoverAsync(int id, CancellationToken ct = default)
    {
        var agricultor = await BuscarAsync(id, ct);
        db.Agricultores.Remove(agricultor); // plantações, medições e recomendações saem em cascata
        await db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<PlantacaoResponse>> ListarPlantacoesAsync(int id, CancellationToken ct = default)
    {
        if (!await db.Agricultores.AnyAsync(a => a.Id == id, ct))
            throw new RecursoNaoEncontradoException("Agricultor", id);

        var plantacoes = await db.Plantacoes.AsNoTracking()
            .Where(p => p.AgricultorId == id)
            .OrderBy(p => p.Cultura)
            .ToListAsync(ct);
        return plantacoes.Select(p => p.ToResponse()).ToList();
    }

    private async Task<Agricultor> BuscarAsync(int id, CancellationToken ct) =>
        await db.Agricultores.FindAsync([id], ct) ?? throw new RecursoNaoEncontradoException("Agricultor", id);
}
