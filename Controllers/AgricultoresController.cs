using Microsoft.AspNetCore.Mvc;
using SoloCerto.Api.DTOs;
using SoloCerto.Api.Interfaces;

namespace SoloCerto.Api.Controllers;

/// <summary>Cadastro dos pequenos produtores rurais.</summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AgricultoresController(IAgricultorService servico) : ControllerBase
{
    /// <summary>Lista os agricultores, opcionalmente filtrando por cidade.</summary>
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<AgricultorResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AgricultorResponse>>> Listar([FromQuery] string? cidade,
                                                                            CancellationToken ct) =>
        Ok(await servico.ListarAsync(cidade, ct));

    /// <summary>Consulta um agricultor pelo ID.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType<AgricultorResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AgricultorResponse>> BuscarPorId(int id, CancellationToken ct) =>
        Ok(await servico.ObterAsync(id, ct));

    /// <summary>Lista as plantações de um agricultor.</summary>
    [HttpGet("{id:int}/plantacoes")]
    [ProducesResponseType<IReadOnlyList<PlantacaoResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<PlantacaoResponse>>> ListarPlantacoes(int id, CancellationToken ct) =>
        Ok(await servico.ListarPlantacoesAsync(id, ct));

    /// <summary>Cadastra um agricultor.</summary>
    [HttpPost]
    [ProducesResponseType<AgricultorResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AgricultorResponse>> Cadastrar(AgricultorRequest dados, CancellationToken ct)
    {
        var criado = await servico.CriarAsync(dados, ct);
        return CreatedAtAction(nameof(BuscarPorId), new { id = criado.Id }, criado);
    }

    /// <summary>Atualiza nome, cidade e e-mail de um agricultor.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Atualizar(int id, AgricultorRequest dados, CancellationToken ct)
    {
        await servico.AtualizarAsync(id, dados, ct);
        return NoContent();
    }

    /// <summary>Exclui um agricultor e, em cascata, suas plantações e histórico.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Excluir(int id, CancellationToken ct)
    {
        await servico.RemoverAsync(id, ct);
        return NoContent();
    }
}
