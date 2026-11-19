using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SoloCerto.Api.Models;

namespace SoloCerto.Api.Data.Configurations;

public class AgricultorConfiguration : IEntityTypeConfiguration<Agricultor>
{
    public void Configure(EntityTypeBuilder<Agricultor> b)
    {
        b.ToTable("Agricultores");
        b.HasKey(a => a.Id);
        b.Property(a => a.Nome).HasMaxLength(120).IsRequired();
        b.Property(a => a.Cidade).HasMaxLength(80).IsRequired();
        b.Property(a => a.Email).HasMaxLength(160);
        b.HasIndex(a => a.Cidade);
        b.HasMany(a => a.Plantacoes).WithOne(p => p.Agricultor)
            .HasForeignKey(p => p.AgricultorId).OnDelete(DeleteBehavior.Cascade);
        b.Navigation(a => a.Plantacoes).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public class PlantacaoConfiguration : IEntityTypeConfiguration<Plantacao>
{
    public void Configure(EntityTypeBuilder<Plantacao> b)
    {
        b.ToTable("Plantacoes");
        b.Property(p => p.Cultura).HasMaxLength(60).IsRequired();
        b.HasIndex(p => new { p.AgricultorId, p.Cultura });
        b.HasMany(p => p.Medicoes).WithOne(m => m.Plantacao)
            .HasForeignKey(m => m.PlantacaoId).OnDelete(DeleteBehavior.Cascade);
        b.HasMany(p => p.Recomendacoes).WithOne(r => r.Plantacao)
            .HasForeignKey(r => r.PlantacaoId).OnDelete(DeleteBehavior.Cascade);
        b.Navigation(p => p.Medicoes).UsePropertyAccessMode(PropertyAccessMode.Field);
        b.Navigation(p => p.Recomendacoes).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public class MedicaoSoloConfiguration : IEntityTypeConfiguration<MedicaoSolo>
{
    public void Configure(EntityTypeBuilder<MedicaoSolo> b)
    {
        b.ToTable("MedicoesSolo");
        b.Property(m => m.Origem).HasConversion<string>().HasMaxLength(20);
        b.HasIndex(m => new { m.PlantacaoId, m.MedidoEm });
    }
}

public class PrevisaoChuvaConfiguration : IEntityTypeConfiguration<PrevisaoChuva>
{
    public void Configure(EntityTypeBuilder<PrevisaoChuva> b)
    {
        b.ToTable("PrevisoesChuva");
        b.Property(p => p.Fonte).HasMaxLength(60).IsRequired();
        b.HasOne(p => p.Plantacao).WithMany()
            .HasForeignKey(p => p.PlantacaoId).OnDelete(DeleteBehavior.Cascade);
        b.HasIndex(p => new { p.PlantacaoId, p.ValidaAPartirDe });
    }
}

public class RecomendacaoIrrigacaoConfiguration : IEntityTypeConfiguration<RecomendacaoIrrigacao>
{
    public void Configure(EntityTypeBuilder<RecomendacaoIrrigacao> b)
    {
        b.ToTable("RecomendacoesIrrigacao");
        b.Property(r => r.Tipo).HasConversion<string>().HasMaxLength(30);
        b.Property(r => r.RegraAplicada).HasMaxLength(60).IsRequired();
        b.Property(r => r.Justificativa).HasMaxLength(400).IsRequired();
        b.Ignore(r => r.IrrigacaoEvitada);
        b.HasOne(r => r.MedicaoSolo).WithMany()
            .HasForeignKey(r => r.MedicaoSoloId).OnDelete(DeleteBehavior.SetNull);
        b.HasIndex(r => new { r.PlantacaoId, r.CriadoEm });
        b.HasIndex(r => r.Tipo);
    }
}
