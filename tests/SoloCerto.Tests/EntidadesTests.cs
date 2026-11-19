using SoloCerto.Api.Models;

namespace SoloCerto.Tests;

/// <summary>Regras de negócio que vivem dentro das próprias entidades (encapsulamento).</summary>
public class EntidadesTests
{
    private static readonly DateTime Agora = new(2026, 11, 19, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Agricultor_normaliza_dados()
    {
        var a = new Agricultor("  João  ", " Campinas ", " JOAO@Exemplo.com ");
        Assert.Equal("João", a.Nome);
        Assert.Equal("Campinas", a.Cidade);
        Assert.Equal("joao@exemplo.com", a.Email);
    }

    [Theory]
    [InlineData("", "Campinas", null, "nome")]
    [InlineData("João", " ", null, "cidade")]
    [InlineData("João", "Campinas", "email-sem-arroba", "E-mail")]
    public void Agricultor_rejeita_dados_invalidos(string nome, string cidade, string? email, string trecho)
    {
        var erro = Assert.Throws<DomainException>(() => new Agricultor(nome, cidade, email));
        Assert.Contains(trecho, erro.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Plantacao_exige_umidade_critica_menor_que_ideal()
    {
        var erro = Assert.Throws<DomainException>(() => new Plantacao(1, "Milho", 5, 40, 30));
        Assert.Contains("crítica deve ser menor", erro.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-2)]
    public void Plantacao_exige_area_positiva(decimal area) =>
        Assert.Throws<DomainException>(() => new Plantacao(1, "Milho", area, 25, 35));

    [Theory]
    [InlineData(10, FaixaUmidade.Critica)]
    [InlineData(24.9, FaixaUmidade.Critica)]
    [InlineData(25, FaixaUmidade.Baixa)]
    [InlineData(34.9, FaixaUmidade.Baixa)]
    [InlineData(35, FaixaUmidade.Adequada)]
    [InlineData(60, FaixaUmidade.Adequada)]
    public void Plantacao_classifica_umidade_pelos_limites(decimal umidade, FaixaUmidade esperada)
    {
        var milho = new Plantacao(1, "Milho", 5, 25, 35);
        Assert.Equal(esperada, milho.ClassificarUmidade(umidade));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(100.1)]
    public void Medicao_rejeita_umidade_fora_de_0_a_100(decimal umidade) =>
        Assert.Throws<DomainException>(() => new MedicaoSolo(1, umidade, Agora, OrigemMedicao.Sensor, Agora));

    [Fact]
    public void Medicao_rejeita_data_no_futuro() =>
        Assert.Throws<DomainException>(() => new MedicaoSolo(1, 30, Agora.AddHours(2), OrigemMedicao.Manual, Agora));

    [Fact]
    public void Medicao_fica_desatualizada_depois_da_validade()
    {
        var m = new MedicaoSolo(1, 30, Agora.AddHours(-25), OrigemMedicao.Sensor, Agora);
        Assert.True(m.EstaDesatualizada(Agora, TimeSpan.FromHours(24)));
        Assert.False(m.EstaDesatualizada(Agora.AddHours(-2), TimeSpan.FromHours(24)));
    }

    [Fact]
    public void Previsao_so_vale_na_janela_de_24_horas()
    {
        var p = new PrevisaoChuva(1, 0.8m, Agora, "teste");
        Assert.True(p.EstaVigente(Agora.AddHours(23)));
        Assert.False(p.EstaVigente(Agora.AddHours(24)));
        Assert.False(p.EstaVigente(Agora.AddMinutes(-1)));
    }

    [Theory]
    [InlineData(-0.1)]
    [InlineData(1.01)]
    public void Previsao_rejeita_probabilidade_fora_de_0_a_1(decimal prob) =>
        Assert.Throws<DomainException>(() => new PrevisaoChuva(1, prob, Agora, "teste"));
}
