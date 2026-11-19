namespace SoloCerto.Api.Models;

/// <summary>Violação de uma regra de negócio do SoloCerto (vira HTTP 400).</summary>
public class DomainException(string mensagem) : Exception(mensagem);

/// <summary>Recurso inexistente (vira HTTP 404).</summary>
public class RecursoNaoEncontradoException(string recurso, int id)
    : Exception($"{recurso} {id} não encontrado(a).");
