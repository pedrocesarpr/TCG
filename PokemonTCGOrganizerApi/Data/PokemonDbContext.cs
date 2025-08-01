using Microsoft.EntityFrameworkCore;

public class PokemonDbContext : DbContext
{
    public PokemonDbContext(DbContextOptions<PokemonDbContext> options) : base(options) { }

    public DbSet<PokemonCard> PokemonCards { get; set; }
    public DbSet<Person> People { get; set; }
    public DbSet<PersonCard> PersonCards { get; set; }
    public DbSet<Deck> Decks { get; set; }
    public DbSet<DeckCard> DeckCards { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Chave composta para DeckCard
        modelBuilder.Entity<DeckCard>()
            .HasKey(dc => new { dc.DeckId, dc.PokemonCardId });

        // Relacionamentos
        modelBuilder.Entity<DeckCard>()
            .HasOne(dc => dc.Deck)
            .WithMany(d => d.Cards)
            .HasForeignKey(dc => dc.DeckId);

        modelBuilder.Entity<DeckCard>()
            .HasOne(dc => dc.PokemonCard)
            .WithMany()
            .HasForeignKey(dc => dc.PokemonCardId);

        modelBuilder.Entity<Person>().HasMany(p => p.Decks).WithOne(d => d.Owner);
        modelBuilder.Entity<Person>().HasMany(p => p.OwnedCards).WithOne(oc => oc.Person);

        // Chave composta para OwnedCard
        modelBuilder.Entity<PersonCard>()
            .HasKey(oc => new { oc.PokemonCardId, oc.PersonId });

        base.OnModelCreating(modelBuilder);
    }
}