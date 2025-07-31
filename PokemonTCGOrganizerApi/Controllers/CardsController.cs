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
        var cards = await _scraper.ScrapeScarletVioletSeriesAsync();

        var existingIds = _context.PokemonCards.Select(c => c.CardId + c.SetName).ToHashSet();
        var newCards = cards
            .Where(c => !existingIds.Contains(c.CardId + c.SetName))
            .ToList();

        _context.PokemonCards.AddRange(newCards);
        await _context.SaveChangesAsync();

        return Ok(new { imported = newCards.Count });
    }

    [HttpGet("search")]
    public async Task<ActionResult<List<PokemonCard>>> Search(
    [FromQuery] string? cardId,
    [FromQuery] string? name,
    [FromQuery] string? set,
    [FromQuery] string? orderBy = "name",
    [FromQuery] string? orderDirection = "asc",
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 20)
    {
        var query = _context.PokemonCards.AsQueryable();

        if (!string.IsNullOrWhiteSpace(cardId))
            query = query.Where(c => EF.Functions.Like(c.CardId.ToLower(), $"%{cardId.ToLower()}%"));

        if (!string.IsNullOrWhiteSpace(name))
            query = query.Where(c => EF.Functions.Like(c.Name.ToLower(), $"%{name.ToLower()}%"));

        if (!string.IsNullOrWhiteSpace(set))
            query = query.Where(c => EF.Functions.Like(c.SetName.ToLower(), $"%{set.ToLower()}%"));

        // Ordenação
        query = (orderBy?.ToLower(), orderDirection?.ToLower()) switch
        {
            ("cardid", "desc") => query.OrderByDescending(c => c.CardId),
            ("cardid", _) => query.OrderBy(c => c.CardId),

            ("setname", "desc") => query.OrderByDescending(c => c.SetName),
            ("setname", _) => query.OrderBy(c => c.SetName),

            ("name", "desc") => query.OrderByDescending(c => c.Name),
            _ => query.OrderBy(c => c.Name), // Padrão: name asc
        };

        // Paginação
        var totalItems = await query.CountAsync();
        var cards = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return Ok(new
        {
            page,
            pageSize,
            totalItems,
            totalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
            items = cards
        });
    }
}