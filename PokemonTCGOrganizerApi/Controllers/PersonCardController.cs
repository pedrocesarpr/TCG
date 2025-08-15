using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

[ApiController]
[Route("api/[controller]")]
public class PersonCardController : ControllerBase
{
    private readonly PokemonDbContext _context;
    public PersonCardController(PokemonDbContext context) => _context = context;

    [HttpGet]
    public async Task<ActionResult<List<PersonCard>>> Get() =>
        await _context.PersonCards
            .Include(o => o.Person)
            .Include(o => o.PokemonCard)
            .ToListAsync();

    [HttpPost]
    public async Task<IActionResult> Create(PersonCardDto personCardDto)
    {
        PersonCard item = new PersonCard();
        item.PersonId = personCardDto.PersonId;
        item.PokemonCardId = personCardDto.PokemonCardId;
        item.Quantity = personCardDto.Quantity;

        _context.PersonCards.Add(item);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = item.Id }, item);
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
    [HttpPost("{personId}/import-csv")]
    public async Task<IActionResult> ImportCsv(int personId, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("Arquivo CSV não fornecido.");

        using var stream = new StreamReader(file.OpenReadStream());
        bool firstLine = true;
        var personCards = new List<PersonCard>();

        while (!stream.EndOfStream)
        {
            var line = await stream.ReadLineAsync();

            if (firstLine && line.StartsWith("sep="))
            {
                firstLine = false;
                continue;
            }
            firstLine = false;

            if (string.IsNullOrWhiteSpace(line))
                continue;

            var fields = ParseCsvLine(line);
            if (fields.Length < 12) continue; // Linha inválida

            var quantity = int.Parse(fields[1]);
            var cardName = fields[3];
            var setCode = fields[4];
            var cardNumber = fields[6];
            var condition = fields[7];
            var printing = fields[8];
            var language = fields[9];
           
            // Busca a carta no banco
            var card = await _context.PokemonCards.FirstOrDefaultAsync(c =>
                c.CardNumber == cardNumber &&
                c.SetCode == setCode);
            var person = await _context.People.FirstOrDefaultAsync(c =>
                c.Id == personId);

            if (card == null) continue;

            var personCard = new PersonCard
            {
                PersonId = personId,
                PokemonCardId = card.Id,
                Quantity = quantity,
                Condition = condition,
                Printing = printing,
                Language = language,
                PurchasedBy = person?.Name ?? ""
            };

            personCards.Add(personCard);
        }

        _context.PersonCards.AddRange(personCards);
        await _context.SaveChangesAsync();

        return Ok(new { imported = personCards.Count });
    }

    private string[] ParseCsvLine(string line)
    {
        var pattern = @"
        (?!\s*$)                                      
        \s*                                            
        (?:                                            
          '(?<val>[^'\\]*(?:\\.[^'\\]*)*)'           
        | ""(?<val>[^""\\]*(?:\\.[^""\\]*)*)""       
        | (?<val>[^,'\""]*)                           
        )                                            
        \s*                                            
        (?:,|$)                                      
    ";
        var matches = Regex.Matches(line, pattern, RegexOptions.IgnorePatternWhitespace);
        return matches.Cast<Match>().Select(m => m.Groups["val"].Value).ToArray();
    }


}
