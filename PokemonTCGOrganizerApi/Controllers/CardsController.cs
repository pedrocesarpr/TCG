using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class CardsController : ControllerBase
{
    private readonly CardScraper _scraper;
    private readonly PokemonDbContext _context;

    public CardsController(CardScraper scraper, PokemonDbContext context)
    {
        _scraper = scraper;
        _context = context;
    }

    [HttpPost("import-all-sets")]
    public async Task<IActionResult> ImportAllSets()
    {
        // 🔹 1. Limpa todos os registros da tabela
        await _context.Database.ExecuteSqlRawAsync("DELETE FROM PokemonCards");
        await _context.Database.ExecuteSqlRawAsync("DELETE FROM sqlite_sequence WHERE name='PokemonCards'"); // reseta autoincremento

        // 🔹 2. Chama o scraper para buscar os sets/cartas
        var cards = await _scraper.ScrapeScarletVioletSeriesAsync();

        var existingIds = _context.PokemonCards.Select(c => c.CardId + c.SetName).ToHashSet();
        var newCards = cards
            .Where(c => !existingIds.Contains(c.CardId + c.SetName))
            .ToList();

        // 🔹 3. Insere os novos registros
        await _context.PokemonCards.AddRangeAsync(newCards);
        await _context.SaveChangesAsync();

        return Ok(new { message = $"{cards.Count} cartas importadas com sucesso." });
    }

    [HttpPost]
    public async Task<ActionResult<PokemonCard>> PostPokemonCard(PokemonCard card)
    {
        if (string.IsNullOrWhiteSpace(card.CardId))
        {
            card.CardId = card.CardNumber;
        }

        _context.PokemonCards.Add(card);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(PokemonCard), new { id = card.Id }, card);
    }


    //[HttpGet("search")]
    //public async Task<ActionResult<List<PokemonCard>>> Search(
    //[FromQuery] string? cardId,
    //[FromQuery] string? name,
    //[FromQuery] string? set,
    //[FromQuery] string? orderBy = "name",
    //[FromQuery] string? orderDirection = "asc",
    //[FromQuery] int page = 1,
    //[FromQuery] int pageSize = 20)
    //{
    //    var query = _context.PokemonCards.AsQueryable();

    //    if (!string.IsNullOrWhiteSpace(cardId))
    //        query = query.Where(c => EF.Functions.Like(c.CardId.ToLower(), $"%{cardId.ToLower()}%"));

    //    if (!string.IsNullOrWhiteSpace(name))
    //        query = query.Where(c => EF.Functions.Like(c.Name.ToLower(), $"%{name.ToLower()}%"));

    //    if (!string.IsNullOrWhiteSpace(set))
    //        query = query.Where(c => EF.Functions.Like(c.SetName.ToLower(), $"%{set.ToLower()}%"));

    //    // Ordenação
    //    query = (orderBy?.ToLower(), orderDirection?.ToLower()) switch
    //    {
    //        ("cardid", "desc") => query.OrderByDescending(c => c.CardId),
    //        ("cardid", _) => query.OrderBy(c => c.CardId),

    //        ("setname", "desc") => query.OrderByDescending(c => c.SetName),
    //        ("setname", _) => query.OrderBy(c => c.SetName),

    //        ("name", "desc") => query.OrderByDescending(c => c.Name),
    //        _ => query.OrderBy(c => c.Name), // Padrão: name asc
    //    };

    //    // Paginação
    //    var totalItems = await query.CountAsync();
    //    var cards = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

    //    return Ok(new
    //    {
    //        page,
    //        pageSize,
    //        totalItems,
    //        totalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
    //        items = cards
    //    });
    //}
    [HttpGet]
    public IActionResult GetCards([FromQuery] string? search, [FromQuery] string? setName)
    {
        var query = _context.PokemonCards.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.ToLower();
            query = query.Where(c =>
                c.CardId.ToLower().Contains(searchLower) ||
                c.CardName.ToLower().Contains(searchLower));
        }

        if (!string.IsNullOrWhiteSpace(setName))
        {
            query = query.Where(c => c.SetName.Equals(setName, StringComparison.OrdinalIgnoreCase));
        }

        var result = query
            .Take(20)
            .ToList();

        return Ok(result);
    }
    [HttpGet("sets")]
    public IActionResult GetSets()
    {
        var sets = _context.PokemonCards
            .Select(c => c.SetName)
            .Distinct()
            .OrderBy(s => s)
            .ToList();

        return Ok(sets);
    }
}