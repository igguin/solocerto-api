using Microsoft.AspNetCore.Mvc;
using SoloCerto.Api.DTOs;
using SoloCerto.Api.Interfaces;

namespace SoloCerto.Api.Controllers;

/// <summary>Indicadores de impacto da CitizenTech.</summary>
[ApiController]
[Route("api/indicadores")]
[Produces("application/json")]
public class IndicadoresController(IIndicadorService servico) : ControllerBase
{
    /// <summary>Totais, recomendações por tipo, % de irrigações evitadas e plantações em situação crítica.</summary>
    [HttpGet]
    [ProducesResponseType<IndicadoresResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IndicadoresResponse>> Obter(CancellationToken ct) =>
        Ok(await servico.ObterAsync(ct));
}
