using Microsoft.AspNetCore.Mvc;
using SoloCerto.Api.DTOs;
using SoloCerto.Api.Interfaces;

namespace SoloCerto.Api.Controllers;

/// <summary>Medições de umidade do solo.</summary>
[ApiController]
[Route("api/medicoes")]
[Produces("application/json")]
public class MedicoesController(IMedicaoService servico) : ControllerBase
{
    /// <summary>Registra uma medição e devolve, na mesma resposta, a recomendação de irrigação.</summary>
    [HttpPost]
    [ProducesResponseType<RegistroMedicaoResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RegistroMedicaoResponse>> Registrar(MedicaoRequest dados, CancellationToken ct)
    {
        var resultado = await servico.RegistrarAsync(dados, ct);
        return Created($"/api/medicoes?plantacaoId={dados.PlantacaoId}", resultado);
    }

    /// <summary>Lista as medições de uma plantação, com período opcional.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<MedicaoResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<MedicaoResponse>>> Listar(
        [FromQuery] int plantacaoId, [FromQuery] DateTime? desde, [FromQuery] DateTime? ate, CancellationToken ct) =>
        Ok(await servico.ListarAsync(plantacaoId, desde, ate, ct));
}
