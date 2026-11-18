
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SoloCerto.Api.Data;
using SoloCerto.Api.Models;

namespace SoloCerto.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AgricultoresController : ControllerBase
{
    private readonly AppDbContext _context;

    public AgricultoresController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/agricultores
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Agricultor>>> Listar()
    {
        var agricultores = await _context.Agricultores
            .AsNoTracking()
            .OrderBy(a => a.Nome)
            .ToListAsync();

        return Ok(agricultores);
    }

    // GET: api/agricultores/1
    [HttpGet("{id}")]
    public async Task<ActionResult<Agricultor>> BuscarPorId(int id)
    {
        var agricultor = await _context.Agricultores
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id);

        if (agricultor == null)
            return NotFound();

        return Ok(agricultor);
    }

    // POST: api/agricultores
    [HttpPost]
    public async Task<ActionResult<Agricultor>> Cadastrar(
        Agricultor agricultor)
    {
        if (string.IsNullOrWhiteSpace(agricultor.Nome))
            return BadRequest("O nome é obrigatório.");

        if (string.IsNullOrWhiteSpace(agricultor.Cidade))
            return BadRequest("A cidade é obrigatória.");

        agricultor.Id = 0;

        _context.Agricultores.Add(agricultor);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(BuscarPorId),
            new { id = agricultor.Id },
            agricultor);
    }

    // PUT: api/agricultores/1
    [HttpPut("{id}")]
    public async Task<IActionResult> Atualizar(
        int id, Agricultor dados)
    {
        var agricultor = await _context.Agricultores.FindAsync(id);

        if (agricultor == null)
            return NotFound("Agricultor não encontrado.");

        if (string.IsNullOrWhiteSpace(dados.Nome))
            return BadRequest("O nome é obrigatório.");

        if (string.IsNullOrWhiteSpace(dados.Cidade))
            return BadRequest("A cidade é obrigatória.");

        agricultor.Nome = dados.Nome;
        agricultor.Cidade = dados.Cidade;
        agricultor.Email = dados.Email;

        await _context.SaveChangesAsync();

        return NoContent();
    }

}
