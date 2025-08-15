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
                    c.CardName.ToLower().Contains(name.ToLower()));

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
                CardName = c.PokemonCard.CardName,
                SetName = c.PokemonCard.SetName,
                ImageUrl = c.PokemonCard.ImageUrl
            })
        });
    }

    [HttpGet]
    public async Task<IActionResult> GetAllDecks()
    {
        var decks = await _context.Decks
            .Include(d => d.Owner)
            .ToListAsync();

        return Ok(decks.Select(d => new
        {
            d.Id,
            d.Name,
            Owner = new { d.Owner.Id, d.Owner.Name }
        }));
    }
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateDeck(int id, UpdateDeckDto dto)
    {
        var deck = await _context.Decks
            .Include(d => d.Cards)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (deck == null)
            return NotFound();

        deck.Name = dto.Name;

        // Remove cartas atuais
        _context.DeckCards.RemoveRange(deck.Cards);
        deck.Cards.Clear();

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
                    c.CardName.ToLower().Contains(name.ToLower()));

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

        await _context.SaveChangesAsync();
        return NoContent();
    }
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDeck(int id)
    {
        var deck = await _context.Decks
            .Include(d => d.Cards)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (deck == null)
            return NotFound();

        _context.DeckCards.RemoveRange(deck.Cards);
        _context.Decks.Remove(deck);
        await _context.SaveChangesAsync();

        return NoContent();
    }
    [HttpGet("{deckId}/check-availability")]
    public async Task<IActionResult> CheckDeckAvailability(int deckId)
    {
        var deck = await _context.Decks
            .Include(d => d.Cards)
                .ThenInclude(dc => dc.PokemonCard)
            .FirstOrDefaultAsync(d => d.Id == deckId);

        if (deck == null)
            return NotFound("Deck não encontrado.");

        // Todas as cópias existentes da carta (de qualquer pessoa)
        var allPersonCards = await _context.PersonCards
            .Include(pc => pc.Person)
            .ToListAsync();

        // Todas as cópias usadas em qualquer deck, exceto o atual
        var allDeckCards = await _context.DeckCards
            .Include(dc => dc.Deck)
                .ThenInclude(d => d.Owner)
            .Where(dc => dc.DeckId != deckId)
            .ToListAsync();

        var result = new List<object>();

        foreach (var deckCard in deck.Cards)
        {
            var cardId = deckCard.PokemonCardId;
            var required = deckCard.Quantity;

            // Todas as cópias existentes dessa carta
            var allOwned = allPersonCards
                .Where(pc => pc.PokemonCardId == cardId)
                .ToList();

            var totalOwned = allOwned.Sum(pc => pc.Quantity);

            // Cópias que já estão em uso em outros decks
            var inDecks = allDeckCards
                .Where(dc => dc.PokemonCardId == cardId)
                .GroupBy(dc => dc.Deck.Owner)
                .Select(g => new
                {
                    Person = g.Key.Name,
                    DeckName = g.First().Deck.Name,
                    Quantity = g.Sum(dc => dc.Quantity)
                })
                .ToList();

            var totalUsedInDecks = inDecks.Sum(i => i.Quantity);

            // Cópias livres por pessoa
            var available = allOwned
                .Select(pc =>
                {
                    // Quantas cópias essa pessoa já usa em outros decks
                    var usedByThisPerson = allDeckCards
                        .Where(dc => dc.PokemonCardId == cardId && dc.Deck.OwnerId == pc.PersonId)
                        .Sum(dc => dc.Quantity);

                    var availableQty = pc.Quantity - usedByThisPerson;

                    return new
                    {
                        Person = pc.Person.Name,
                        Quantity = availableQty
                    };
                })
                .Where(a => a.Quantity > 0)
                .ToList();

            var availableTotal = available.Sum(a => a.Quantity);
            var missing = Math.Max(0, required - availableTotal);

            result.Add(new
            {
                deckCard.PokemonCard.CardName,
                deckCard.PokemonCard.CardId,
                deckCard.PokemonCard.SetName,
                Required = required,
                TotalOwned = totalOwned,
                InDecks = inDecks,
                Available = available,
                Missing = missing
            });
        }

        return Ok(result);
    }


}
