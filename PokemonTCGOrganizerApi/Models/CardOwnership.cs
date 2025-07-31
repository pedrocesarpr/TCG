public class CardOwnership
{
    public int Id { get; set; }

    public int OwnerId { get; set; }
    public Person Owner { get; set; } = null!;

    public int PokemonCardId { get; set; }
    public PokemonCard PokemonCard { get; set; } = null!;

    public int Quantity { get; set; }
    public string PurchasedBy { get; set; } = null!; // nome de quem comprou originalmente
}
