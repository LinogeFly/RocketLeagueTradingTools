using Microsoft.EntityFrameworkCore;
using RocketLeagueTradingTools.Infrastructure.Persistence.Models;

namespace RocketLeagueTradingTools.Infrastructure.Persistence;

public class PersistenceDbContext : DbContext
{
    public DbSet<PersistedTradeOffer> TradeOffers { get; set; } = null!;
    public DbSet<PersistedAlert> Alerts { get; set; } = null!;
    public DbSet<PersistedNotification> Notifications { get; set; } = null!;
    public DbSet<PersistedBlacklistedTrader> BlacklistedTraders { get; set; } = null!;

    public PersistenceDbContext(DbContextOptions<PersistenceDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PersistedTradeOffer>().ToTable("TradeOffers");
        modelBuilder.Entity<PersistedTradeOffer>().HasIndex(o => o.ItemName);
        modelBuilder.Entity<PersistedTradeOffer>().HasIndex(o => o.ScrapedDate);
        modelBuilder.Entity<PersistedTradeOffer>().HasIndex(o => o.Price);
        
        modelBuilder.Entity<PersistedAlert>().ToTable("Alerts");
        modelBuilder.Entity<PersistedAlert>().HasIndex(a => a.ItemName);
        modelBuilder.Entity<PersistedAlert>().HasIndex(p => p.PriceFrom);
        modelBuilder.Entity<PersistedAlert>().HasIndex(p => p.PriceTo);

        modelBuilder.Entity<PersistedNotification>().ToTable("Notifications");
        modelBuilder.Entity<PersistedNotification>().HasIndex(n => n.CreatedDate);
        modelBuilder.Entity<PersistedNotification>().HasOne(n => n.TradeOffer);

        modelBuilder.Entity<PersistedBlacklistedTrader>().ToTable("Blacklist")
            .HasIndex(b => new { b.TradingSite, b.TraderName })
            .IsUnique();
    }
}