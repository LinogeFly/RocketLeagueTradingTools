using Microsoft.EntityFrameworkCore;
using RocketLeagueTradingTools.Core.Application.Contracts;
using RocketLeagueTradingTools.Core.Domain.Entities;
using RocketLeagueTradingTools.Core.Domain.ValueObjects;
using RocketLeagueTradingTools.Infrastructure.Persistence.SQLite.PersistedTypes;

namespace RocketLeagueTradingTools.Infrastructure.Persistence.SQLite;

public class SQLiteDbContext : DbContext, IPersistenceRepository
{
    private readonly ILog log;
    public string Id { get; }

    public DbSet<PersistedBuyOffer> BuyOffers { get; set; } = null!;
    public DbSet<PersistedSellOffer> SellOffers { get; set; } = null!;
    public DbSet<PersistedAlert> Alerts { get; set; } = null!;

    public SQLiteDbContext(DbContextOptions<SQLiteDbContext> options, ILog log) : base(options)
    {
        this.log = log;
        this.Id = Guid.NewGuid().ToString();
    }

    public async Task AddBuyOffers(IList<TradeOffer> offers)
    {
        await BuyOffers.AddRangeAsync(offers.Select(Map<PersistedBuyOffer>));

        await SaveChangesAsync();
    }

    public async Task AddSellOffers(IList<TradeOffer> offers)
    {
        await SellOffers.AddRangeAsync(offers.Select(Map<PersistedSellOffer>));

        await SaveChangesAsync();
    }

    public async Task<IList<Alert>> GetAlerts()
    {
        return (await Alerts.ToListAsync()).Select(Map).ToList();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PersistedBuyOffer>()
            .ToTable("BuyOffers")
            .HasIndex(p => new
            {
                p.Id,
                p.SourceId,
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
                p.SourceId,
                p.ScrapedDate,
                p.Name,
                p.Price,
                p.Color,
                p.Certification
            });

        modelBuilder.Entity<PersistedAlert>()
            .ToTable("Alerts")
            .HasData(
                new PersistedAlert()
                {
                    Id = 1,
                    ItemName = "Hellfire",
                    Color = "None",
                    OfferType = PersistedAlertOfferType.Sell,
                    PriceTo = 100,
                },
                new PersistedAlert()
                {
                    Id = 2,
                    ItemName = "Dueling Dragons",
                    Color = "None",
                    OfferType = PersistedAlertOfferType.Sell,
                    PriceTo = 500,
                }
            );
    }

    private TradeOffer Map(PersistedTradeOffer persisted)
    {
        return new TradeOffer
        {
            SourceId = persisted.SourceId,
            Link = persisted.Link,
            Item = new TradeItem(persisted.Name)
            {
                Color = persisted.Color,
                Certification = persisted.Certification
            },
            Price = persisted.Price
        };
    }

    private T Map<T>(TradeOffer offer) where T : PersistedTradeOffer, new()
    {
        return new T
        {
            SourceId = offer.SourceId,
            Link = offer.Link,
            ScrapedDate = DateTime.Now,
            Name = offer.Item.Name,
            Price = offer.Price,
            Color = offer.Item.Color,
            Certification = offer.Item.Certification
        };
    }

    private Alert Map(PersistedAlert alert)
    {
        return new Alert
        {
            Id = alert.Id,
            ItemName = alert.ItemName,
            OfferType = Map(alert.OfferType),
            Price = new PriceRange(alert.PriceFrom, alert.PriceTo),
            Color = alert.Color,
            Certification = alert.Certification,
            Disabled = alert.Disabled
        };
    }

    private AlertOfferType Map(PersistedAlertOfferType offerType)
    {
        switch (offerType)
        {
            case PersistedAlertOfferType.Buy:
                return AlertOfferType.Buy;
            case PersistedAlertOfferType.Sell:
                return AlertOfferType.Sell;
            default:
                throw new InvalidOperationException($"Unknown offer type '{offerType}'.");
        }
    }
}
