using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class PersonController : ControllerBase
{
    private readonly PokemonDbContext _context;
    public PersonController(PokemonDbContext context) => _context = context;

    [HttpGet]
    public async Task<ActionResult<List<Person>>> Get() =>
        await _context.People.Include(p => p.OwnedCards).ToListAsync();

    [HttpGet("{id}")]
    public async Task<ActionResult<Person>> Get(int id)
    {
        var person = await _context.People.Include(p => p.OwnedCards).FirstOrDefaultAsync(p => p.Id == id);
        return person == null ? NotFound() : Ok(person);
    }

    [HttpPost]
    public async Task<IActionResult> Create(PersonDto person)
    {
        Person item = new Person();
        item.Name = person.Name;
        _context.People.Add(item);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = item.Id }, item);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Person updated)
    {
        if (id != updated.Id) return BadRequest();
        _context.Entry(updated).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var person = await _context.People.FindAsync(id);
        if (person == null) return NotFound();
        _context.People.Remove(person);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
