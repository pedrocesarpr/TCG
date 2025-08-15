public class PokemonCard
{
    public int Id { get; set; } // gerado pelo banco    

    public string CardName { get; set; }
    public string SetCode { get; set; }
    public string SetName { get; set; }
    public string CardNumber { get; set; }
    public string CardId { get; set; } // default = CardNumber
    public string ImageUrl { get; set; }
}