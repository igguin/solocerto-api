using Microsoft.AspNetCore.Mvc;
using SoloCerto.Api.DTOs;
using SoloCerto.Api.Interfaces;

namespace SoloCerto.Api.Controllers;

/// <summary>Áreas de cultivo e sua situação de umidade.</summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PlantacoesController(IPlantacaoService servico) : ControllerBase
{
    /// <summary>Lista plantações, com filtros opcionais por agricultor e cultura.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<PlantacaoResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PlantacaoResponse>>> Listar(
        [FromQuery] int? agricultorId, [FromQuery] string? cultura, CancellationToken ct) =>
        Ok(await servico.ListarAsync(agricultorId, cultura, ct));

    /// <summary>Consulta uma plantação pelo ID.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType<PlantacaoResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PlantacaoResponse>> BuscarPorId(int id, CancellationToken ct) =>
        Ok(await servico.ObterAsync(id, ct));

    /// <summary>Situação atual: última medição, previsão de chuva vigente e última recomendação.</summary>
    [HttpGet("{id:int}/situacao")]
    [ProducesResponseType<SituacaoPlantacaoResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SituacaoPlantacaoResponse>> Situacao(int id, CancellationToken ct) =>
        Ok(await servico.ObterSituacaoAsync(id, ct));

    /// <summary>Histórico de medições e recomendações dos últimos N dias, com média, mínima e máxima.</summary>
    [HttpGet("{id:int}/historico")]
    [ProducesResponseType<HistoricoPlantacaoResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<HistoricoPlantacaoResponse>> Historico(int id, [FromQuery] int dias = 30,
                                                                          CancellationToken ct = default) =>
        Ok(await servico.ObterHistoricoAsync(id, dias, ct));

    /// <summary>Cadastra uma plantação com a faixa de umidade adequada para a cultura.</summary>
    [HttpPost]
    [ProducesResponseType<PlantacaoResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PlantacaoResponse>> Cadastrar(PlantacaoRequest dados, CancellationToken ct)
    {
        var criada = await servico.CriarAsync(dados, ct);
        return CreatedAtAction(nameof(BuscarPorId), new { id = criada.Id }, criada);
    }

    /// <summary>Atualiza cultura, área e faixa de umidade.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Atualizar(int id, PlantacaoUpdateRequest dados, CancellationToken ct)
    {
        await servico.AtualizarAsync(id, dados, ct);
        return NoContent();
    }

    /// <summary>Exclui a plantação e seu histórico.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Excluir(int id, CancellationToken ct)
    {
        await servico.RemoverAsync(id, ct);
        return NoContent();
    }
}
