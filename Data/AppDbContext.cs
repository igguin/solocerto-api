
using Microsoft.EntityFrameworkCore;
using SoloCerto.Api.Models;

namespace SoloCerto.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Agricultor> Agricultores { get; set; }
        = null!;
}
