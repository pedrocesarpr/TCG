public class DeckCard
{
    public int Id { get; set; }

    public int DeckId { get; set; }
    public Deck Deck { get; set; }

    public int PokemonCardId { get; set; }
    public PokemonCard PokemonCard { get; set; }

    public int Quantity { get; set; }

    public string Category { get; set; } = string.Empty; // "Pokémon", "Trainer", "Energy"
}
