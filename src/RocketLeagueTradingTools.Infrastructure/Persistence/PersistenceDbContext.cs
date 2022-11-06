using Microsoft.EntityFrameworkCore;
using RocketLeagueTradingTools.Infrastructure.Persistence.Models;

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
                p.Name,
                p.ScrapedDate,
                p.Price,
                p.ItemType,
                p.Color,
                p.Certification
            });

        modelBuilder.Entity<PersistedSellOffer>()
            .ToTable("SellOffers")
            .HasIndex(p => new
            {
                p.Id,
                p.Name,
                p.ScrapedDate,
                p.Price,
                p.ItemType,
                p.Color,
                p.Certification
            });

        modelBuilder.Entity<PersistedAlert>()
            .ToTable("Alerts")
            .HasIndex(p => new
            {
                p.Id,
                p.ItemName,
                p.Enabled,
                p.OfferType,
                p.PriceFrom,
                p.PriceTo,
                p.ItemType,
                p.Color,
                p.Certification
            });

        modelBuilder.Entity<PersistedNotification>()
            .ToTable("Notifications")
            .HasIndex(p => new
            {
                p.Id,
                p.CreatedDate,
                p.TradeOfferScrapedDate
            });
    }
}