public class PersonCard
{
    public int Id { get; set; }

    public int PersonId { get; set; }
    public Person Person { get; set; } = null!;

    public int PokemonCardId { get; set; }
    public PokemonCard PokemonCard { get; set; } = null!;
    public string Condition { get; set; }
    public string Printing { get; set; }
    public string Language { get; set; }
    public int Quantity { get; set; }
    public string PurchasedBy { get; set; } = null!; // nome de quem comprou originalmente
}
