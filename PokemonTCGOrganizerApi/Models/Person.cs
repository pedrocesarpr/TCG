public class Person
{
    public int Id { get; set; }
    public string Name { get; set; }

    // Relação com Decks e Cartas
    public ICollection<Deck> Decks { get; set; }
    public ICollection<CardOwnership> OwnedCards { get; set; }
}
