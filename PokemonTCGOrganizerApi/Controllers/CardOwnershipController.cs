using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class CardOwnershipController : ControllerBase
{
    private readonly PokemonDbContext _context;
    public CardOwnershipController(PokemonDbContext context) => _context = context;

    [HttpGet]
    public async Task<ActionResult<List<PersonCard>>> Get() =>
        await _context.PersonCards
            .Include(o => o.Person)
            .Include(o => o.PokemonCard)
            .ToListAsync();

    [HttpPost]
    public async Task<IActionResult> Create(PersonCard ownership)
    {
        _context.PersonCards.Add(ownership);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = ownership.Id }, ownership);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, PersonCard updated)
    {
        if (id != updated.Id) return BadRequest();
        _context.Entry(updated).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var record = await _context.PersonCards.FindAsync(id);
        if (record == null) return NotFound();
        _context.PersonCards.Remove(record);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
