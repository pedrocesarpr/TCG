public class Deck
{
    public int Id { get; set; }
    public string Name { get; set; }

    public int OwnerId { get; set; }
    public Person Owner { get; set; }

    public List<DeckCard> Cards { get; set; } = new();
}
