using Microsoft.EntityFrameworkCore;
using RocketLeagueTradingTools.Infrastructure.Persistence.Models;

namespace RocketLeagueTradingTools.Infrastructure.Persistence;

public class PersistenceDbContext : DbContext
{
    public DbSet<PersistedBuyOffer> BuyOffers { get; set; } = null!;
    public DbSet<PersistedSellOffer> SellOffers { get; set; } = null!;
    public DbSet<PersistedAlert> Alerts { get; set; } = null!;
    public DbSet<PersistedNotification> Notifications { get; set; } = null!;
    public DbSet<PersistedBlacklistedTrader> BlacklistedTraders { get; set; } = null!;

    public PersistenceDbContext(DbContextOptions<PersistenceDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PersistedBuyOffer>().ToTable("BuyOffers");
        modelBuilder.Entity<PersistedBuyOffer>().HasIndex(o => o.ItemName);
        modelBuilder.Entity<PersistedBuyOffer>().HasIndex(o => o.ScrapedDate);
        modelBuilder.Entity<PersistedBuyOffer>().HasIndex(o => o.Price);

        modelBuilder.Entity<PersistedSellOffer>().ToTable("SellOffers");
        modelBuilder.Entity<PersistedSellOffer>().HasIndex(o => o.ItemName);
        modelBuilder.Entity<PersistedSellOffer>().HasIndex(o => o.ScrapedDate);
        modelBuilder.Entity<PersistedSellOffer>().HasIndex(o => o.Price);

        modelBuilder.Entity<PersistedAlert>().ToTable("Alerts");
        modelBuilder.Entity<PersistedAlert>().HasIndex(a => a.ItemName);
        modelBuilder.Entity<PersistedAlert>().HasIndex(p => p.PriceFrom);
        modelBuilder.Entity<PersistedAlert>().HasIndex(p => p.PriceTo);

        modelBuilder.Entity<PersistedNotification>().ToTable("Notifications");
        modelBuilder.Entity<PersistedNotification>().HasIndex(n => n.CreatedDate);
        modelBuilder.Entity<PersistedNotification>().HasIndex(n => n.TradeOfferScrapedDate);

        modelBuilder.Entity<PersistedBlacklistedTrader>().ToTable("Blacklist")
            .HasIndex(b => new { b.TradingSite, b.TraderName })
            .IsUnique();
    }
}