using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using static System.Runtime.InteropServices.JavaScript.JSType;

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

    [HttpPost("import-csv/{personId}")]
    public async Task<IActionResult> ImportCsv(int personId, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("Arquivo CSV inválido.");

        using var reader = new StreamReader(file.OpenReadStream());
        var allCards = await _context.PokemonCards.ToListAsync();

        // Criar dicionário de cartas com chave (SetCode, CardNumber, primeiros 5 chars do nome)
        var cardDict = allCards
            .GroupBy(c => (
                c.SetCode.Trim(),
                c.CardNumber.Trim(),
                c.CardName.Substring(0, Math.Min(5, c.CardName.Length)).Trim().ToUpper()
            ))
            .ToDictionary(g => g.Key, g => g.First());

        var person = await _context.People.FindAsync(personId);
        if (person == null)
            return NotFound("Pessoa não encontrada.");

        var errors = new List<string>();
        var lineNumber = 0;
        var personCards = new List<PersonCard>();

        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            lineNumber++;

            if (lineNumber == 1) continue; // linha sep=,
            if (lineNumber == 2) continue; // cabeçalho

            var values = line.Split(',');

            if (values.Length < 10)
            {
                errors.Add($"Linha {lineNumber} inválida: {line}");
                continue;
            }
            
            var folderName = values[0].Trim();
            var quantity = int.TryParse(values[1], out var q) ? q : 0;
            var tradeQty = int.TryParse(values[2], out var tq) ? tq : 0;
            var cardName = values[3].Trim()
                .Replace("’", "'")
                .Replace("\"", "");
            

            var setCode = values[4].Trim().Replace("1","I");
            if (setCode.Contains("-"))
                setCode = setCode.Split('-')[0].Trim();
            var setName = values[5].Trim();
            var cardNumber = values[6].Trim();
            if (cardNumber.Contains("/"))
                cardNumber = cardNumber.Split('/')[0].Trim();
            var condition = values[7].Trim();
            var printing = values[8].Trim();
            var language = values[9].Trim();
            var purchasedBy = folderName; // usei Folder Name como "quem comprou"

            // 🔑 Criar chave única (SetCode, CardNumber, 5 primeiros chars do nome)
            var key = (
                setCode,
                cardNumber,
                cardName.Substring(0, Math.Min(5, cardName.Length)).Trim().ToUpper()
            );

            if (!cardDict.TryGetValue(key, out var card))
            {
                errors.Add($"Linha {lineNumber}: Carta não encontrada {setCode}-{cardNumber} ({cardName})");
                continue;
            }

            // Procurar se já existe registro para a pessoa + carta + condition + language
            var existing = await _context.PersonCards
                .FirstOrDefaultAsync(pc =>
                    pc.PersonId == personId &&
                    pc.PokemonCardId == card.Id &&
                    pc.Printing == printing &&
                    pc.Language == language
                );

            if (existing != null)
            {
                // Atualiza quantidade
                existing.Quantity += quantity;
            }
            else
            {
                // Adiciona novo registro
                var personCard = new PersonCard
                {
                    PersonId = personId,
                    PokemonCardId = card.Id,
                    Condition = condition,
                    Printing = printing,
                    Language = language,
                    Quantity = quantity,
                    PurchasedBy = purchasedBy
                };
                personCards.Add(personCard);
            }
        }
        _context.PersonCards.AddRange();
        await _context.SaveChangesAsync();

        return Ok(new
        {
            Message = "Importação concluída",
            Errors = errors
        });
    }



    // ----- Helpers -----

    private static int SafeInt(string s)
    {
        if (int.TryParse(s?.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var v))
            return v;
        return 0;
    }

    // Parser CSV simples que respeita aspas simples e duplas
    private static string[] ParseCsvLine(string line)
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
