using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SoloCerto.Api.Models;

namespace SoloCerto.Api.Infra;

/// <summary>
/// Converte exceções do domínio em respostas HTTP padronizadas (RFC 7807 / ProblemDetails),
/// para que os controllers fiquem limpos, sem try/catch.
/// </summary>
public class TratadorDeErros(ILogger<TratadorDeErros> logger, IProblemDetailsService problemDetails)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext http, Exception erro, CancellationToken ct)
    {
        var (status, titulo) = erro switch
        {
            RecursoNaoEncontradoException => (StatusCodes.Status404NotFound, "Recurso não encontrado"),
            DomainException => (StatusCodes.Status400BadRequest, "Regra de negócio violada"),
            _ => (StatusCodes.Status500InternalServerError, "Erro inesperado"),
        };

        if (status == StatusCodes.Status500InternalServerError)
            logger.LogError(erro, "Erro não tratado em {Rota}", http.Request.Path);

        http.Response.StatusCode = status;
        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = http,
            Exception = erro,
            ProblemDetails = new ProblemDetails
            {
                Status = status,
                Title = titulo,
                Detail = status == StatusCodes.Status500InternalServerError
                    ? "Tente novamente em instantes."
                    : erro.Message,
            },
        });
    }
}
