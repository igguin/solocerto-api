namespace SoloCerto.Api.Models;

/// <summary>
/// Classe base abstrata de todas as entidades persistidas do domínio.
/// Concentra a identidade e a data de criação (herança).
/// </summary>
public abstract class EntidadeBase
{
    public int Id { get; protected set; }

    public DateTime CriadoEm { get; protected set; } = DateTime.UtcNow;

    /// <summary>Lança <see cref="DomainException"/> quando a condição de negócio não é atendida.</summary>
    protected static void Garantir(bool condicao, string mensagem)
    {
        if (!condicao)
            throw new DomainException(mensagem);
    }

    protected static string TextoObrigatorio(string? valor, string campo, int maximo)
    {
        Garantir(!string.IsNullOrWhiteSpace(valor), $"{campo} é obrigatório.");
        var limpo = valor!.Trim();
        Garantir(limpo.Length <= maximo, $"{campo} deve ter no máximo {maximo} caracteres.");
        return limpo;
    }
}
