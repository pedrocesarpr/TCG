using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class CardOwnershipController : ControllerBase
{
    private readonly PokemonDbContext _context;
    public CardOwnershipController(PokemonDbContext context) => _context = context;

    [HttpGet]
    public async Task<ActionResult<List<CardOwnership>>> Get() =>
        await _context.CardOwnerships
            .Include(o => o.Owner)
            .Include(o => o.PokemonCard)
            .ToListAsync();

    [HttpPost]
    public async Task<IActionResult> Create(CardOwnership ownership)
    {
        _context.CardOwnerships.Add(ownership);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = ownership.Id }, ownership);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, CardOwnership updated)
    {
        if (id != updated.Id) return BadRequest();
        _context.Entry(updated).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var record = await _context.CardOwnerships.FindAsync(id);
        if (record == null) return NotFound();
        _context.CardOwnerships.Remove(record);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
