using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class CardsController : ControllerBase
{
    private readonly CardScraper _scraper;
    private readonly AppDbContext _context;

    public CardsController(CardScraper scraper, AppDbContext context)
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
}