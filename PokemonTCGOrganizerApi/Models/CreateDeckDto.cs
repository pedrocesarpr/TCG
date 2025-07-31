public class CreateDeckDto
{
    public string Name { get; set; }
    public int OwnerId { get; set; } // Antes era string OwnerName
    public string RawList { get; set; }
}