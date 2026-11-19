using Microsoft.AspNetCore.Mvc;
using SoloCerto.Api.DTOs;
using SoloCerto.Api.Interfaces;
using SoloCerto.Api.Models;

namespace SoloCerto.Api.Controllers;

/// <summary>Recomendações de irrigação.</summary>
[ApiController]
[Route("api/recomendacoes")]
[Produces("application/json")]
public class RecomendacoesController(IRecomendacaoService servico) : ControllerBase
{
    /// <summary>Reavalia a plantação agora (última medição + previsão vigente) e grava a recomendação.</summary>
    [HttpPost("avaliar/{plantacaoId:int}")]
    [ProducesResponseType<RecomendacaoResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RecomendacaoResponse>> Avaliar(int plantacaoId, CancellationToken ct)
    {
        var recomendacao = await servico.AvaliarAsync(plantacaoId, ct);
        return Created($"/api/recomendacoes?plantacaoId={plantacaoId}", recomendacao);
    }

    /// <summary>Lista recomendações (mais recentes primeiro), com filtros opcionais.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<RecomendacaoResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<RecomendacaoResponse>>> Listar(
        [FromQuery] int? plantacaoId, [FromQuery] TipoRecomendacao? tipo, CancellationToken ct) =>
        Ok(await servico.ListarAsync(plantacaoId, tipo, ct));
}
