using Microsoft.AspNetCore.Mvc;
using SoloCerto.Api.DTOs;
using SoloCerto.Api.Interfaces;

namespace SoloCerto.Api.Controllers;

/// <summary>Integração com o módulo de IA de previsão de chuva (Random Forest / INMET).</summary>
[ApiController]
[Route("api/previsoes-chuva")]
[Produces("application/json")]
public class PrevisoesChuvaController(IPrevisaoChuvaService servico) : ControllerBase
{
    /// <summary>Recebe a probabilidade de chuva (≥ 1 mm em 24 h) calculada pelo modelo de IA.</summary>
    [HttpPost]
    [ProducesResponseType<PrevisaoChuvaResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PrevisaoChuvaResponse>> Registrar(PrevisaoChuvaRequest dados, CancellationToken ct)
    {
        var criada = await servico.RegistrarAsync(dados, ct);
        return Created($"/api/previsoes-chuva?plantacaoId={dados.PlantacaoId}", criada);
    }

    /// <summary>Lista as previsões recebidas para uma plantação.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<PrevisaoChuvaResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PrevisaoChuvaResponse>>> Listar([FromQuery] int plantacaoId,
                                                                               CancellationToken ct) =>
        Ok(await servico.ListarAsync(plantacaoId, ct));
}
