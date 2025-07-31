using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

[ApiController]
[Route("api/[controller]")]
public class DecksController : ControllerBase
{
    private readonly PokemonDbContext _context;

    public DecksController(PokemonDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> CreateDeck(CreateDeckDto dto)
    {
        var owner = await _context.People.FindAsync(dto.OwnerId);
        if (owner == null)
            return BadRequest("Pessoa não encontrada.");

        var deck = new Deck
        {
            Name = dto.Name,
            OwnerId = dto.OwnerId
        };

        var lines = dto.RawList.Split("\n", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        string currentCategory = null;

        foreach (var line in lines)
        {
            if (line.StartsWith("Pokémon:")) { currentCategory = "Pokémon"; continue; }
            if (line.StartsWith("Trainer:")) { currentCategory = "Trainer"; continue; }
            if (line.StartsWith("Energy:")) { currentCategory = "Energy"; continue; }

            var tokens = line.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (tokens.Length < 4 || !int.TryParse(tokens[0], out var quantity)) continue;

            string number = tokens[^1];
            string name = string.Join(' ', tokens.Skip(1).Take(tokens.Length - 3));

            var card = await _context.PokemonCards
                .FirstOrDefaultAsync(c =>
                    c.CardId.StartsWith(number + "/") &&
                    c.Name.ToLower().Contains(name.ToLower()));

            if (card == null)
            {
                Console.WriteLine($"Carta não encontrada: {line}");
                continue;
            }

            deck.Cards.Add(new DeckCard
            {
                PokemonCardId = card.Id,
                Quantity = quantity,
                Category = currentCategory ?? "Unknown"
            });
        }

        _context.Decks.Add(deck);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetDeckById), new { id = deck.Id }, deck);
    }
    [HttpGet("{id}")]
    public async Task<IActionResult> GetDeckById(int id)
    {
        var deck = await _context.Decks
            .Include(d => d.Owner)
            .Include(d => d.Cards)
                .ThenInclude(dc => dc.PokemonCard)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (deck == null)
            return NotFound();

        return Ok(new
        {
            deck.Id,
            deck.Name,
            Owner = new { deck.Owner.Id, deck.Owner.Name },
            Cards = deck.Cards.Select(c => new
            {
                c.Id,
                c.Quantity,
                c.Category,
                CardId = c.PokemonCard.CardId,
                CardName = c.PokemonCard.Name,
                SetName = c.PokemonCard.SetName,
                ImageUrl = c.PokemonCard.ImageUrl
            })
        });
    }
}
