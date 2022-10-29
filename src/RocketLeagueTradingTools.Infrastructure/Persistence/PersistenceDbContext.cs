using Microsoft.EntityFrameworkCore;
using RocketLeagueTradingTools.Infrastructure.Persistence.PersistedTypes;

namespace RocketLeagueTradingTools.Infrastructure.Persistence;

public class PersistenceDbContext : DbContext
{
    public DbSet<PersistedBuyOffer> BuyOffers { get; set; } = null!;
    public DbSet<PersistedSellOffer> SellOffers { get; set; } = null!;
    public DbSet<PersistedAlert> Alerts { get; set; } = null!;
    public DbSet<PersistedNotification> Notifications { get; set; } = null!;

    public PersistenceDbContext(DbContextOptions<PersistenceDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PersistedBuyOffer>()
            .ToTable("BuyOffers")
            .HasIndex(p => new
            {
                p.Id,
                p.ScrapedDate,
                p.Name,
                p.Price,
                p.Color,
                p.Certification
            });

        modelBuilder.Entity<PersistedSellOffer>()
            .ToTable("SellOffers")
            .HasIndex(p => new
            {
                p.Id,
                p.ScrapedDate,
                p.Name,
                p.Price,
                p.Color,
                p.Certification
            });

        modelBuilder.Entity<PersistedAlert>()
            .ToTable("Alerts");

        modelBuilder.Entity<PersistedNotification>()
            .ToTable("Notifications");
    }
}