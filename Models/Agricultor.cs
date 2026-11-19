namespace SoloCerto.Api.Models;

/// <summary>Pequeno produtor rural que usa o SoloCerto.</summary>
public class Agricultor : EntidadeBase
{
    public string Nome { get; private set; } = string.Empty;

    public string Cidade { get; private set; } = string.Empty;

    public string? Email { get; private set; }

    private readonly List<Plantacao> _plantacoes = [];
    public IReadOnlyCollection<Plantacao> Plantacoes => _plantacoes;

    private Agricultor() { } // EF Core

    public Agricultor(string nome, string cidade, string? email = null)
    {
        Atualizar(nome, cidade, email);
    }

    public void Atualizar(string nome, string cidade, string? email)
    {
        Nome = TextoObrigatorio(nome, "O nome", 120);
        Cidade = TextoObrigatorio(cidade, "A cidade", 80);
        Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim().ToLowerInvariant();
        Garantir(Email is null || (Email.Contains('@') && Email.Length <= 160), "E-mail inválido.");
    }
}
