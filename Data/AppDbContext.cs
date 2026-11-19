using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SoloCerto.Api.Models;

namespace SoloCerto.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Agricultor> Agricultores => Set<Agricultor>();
    public DbSet<Plantacao> Plantacoes => Set<Plantacao>();
    public DbSet<MedicaoSolo> Medicoes => Set<MedicaoSolo>();
    public DbSet<PrevisaoChuva> PrevisoesChuva => Set<PrevisaoChuva>();
    public DbSet<RecomendacaoIrrigacao> Recomendacoes => Set<RecomendacaoIrrigacao>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // SQLite não ordena nem agrega decimal no servidor; persistir como REAL mantém
        // OrderBy/Average/Min/Max traduzidos para SQL (o domínio continua usando decimal).
        configurationBuilder.Properties<decimal>().HaveConversion<double>();
        configurationBuilder.Properties<decimal?>().HaveConversion<double?>();

        // Todas as datas são gravadas em UTC; ao ler do SQLite, marca o Kind como UTC
        // para a API devolver sempre o sufixo "Z" no JSON.
        configurationBuilder.Properties<DateTime>().HaveConversion<DataUtcConverter>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Aplica todas as classes IEntityTypeConfiguration<T> da pasta Data/Configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}

/// <summary>Garante DateTime em UTC na ida e na volta do banco.</summary>
public class DataUtcConverter() : ValueConverter<DateTime, DateTime>(
    valor => valor.Kind == DateTimeKind.Utc ? valor : valor.ToUniversalTime(),
    valor => DateTime.SpecifyKind(valor, DateTimeKind.Utc));
