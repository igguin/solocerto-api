using Microsoft.EntityFrameworkCore;
using SoloCerto.Api.Models;

namespace SoloCerto.Api.Data;

/// <summary>
/// Popula o banco com dados FICTÍCIOS de demonstração, apenas se ele estiver vazio
/// (configuração "SoloCerto:CarregarDadosDemonstracao").
/// </summary>
public static class DadosDemonstracao
{
    public static async Task CarregarAsync(AppDbContext db, CancellationToken ct = default)
    {
        if (await db.Agricultores.AnyAsync(ct))
            return;

        var joao = new Agricultor("João da Silva (fictício)", "Campinas", "joao.demo@exemplo.com");
        var maria = new Agricultor("Maria Souza (fictícia)", "Bragança Paulista", null);
        db.Agricultores.AddRange(joao, maria);
        await db.SaveChangesAsync(ct);

        db.Plantacoes.AddRange(
            new Plantacao(joao.Id, "Milho", 5m, 25m, 35m),
            new Plantacao(joao.Id, "Tomate", 1.5m, 30m, 45m),
            new Plantacao(maria.Id, "Café", 3m, 22m, 32m));
        await db.SaveChangesAsync(ct);
    }
}
