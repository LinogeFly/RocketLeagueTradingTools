using Microsoft.EntityFrameworkCore;
using RocketLeagueTradingTools.Core.Application.Interfaces;
using RocketLeagueTradingTools.Core.Domain.Entities;
using RocketLeagueTradingTools.Core.Domain.ValueObjects;
using RocketLeagueTradingTools.Infrastructure.Persistence.Models;

namespace RocketLeagueTradingTools.Infrastructure.Persistence;

public class PersistenceRepository : IPersistenceRepository
{
    private readonly IDbContextFactory<PersistenceDbContext> dbContextFactory;
    private readonly IDateTime dateTime;

    public PersistenceRepository(
        IDbContextFactory<PersistenceDbContext> dbContextFactory,
        IDateTime dateTime)
    {
        this.dbContextFactory = dbContextFactory;
        this.dateTime = dateTime;
    }

    public async Task AddBuyOffers(IList<TradeOffer> offers)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        await dbContext.BuyOffers.AddRangeAsync(offers.Select(Map<PersistedBuyOffer>));

        await dbContext.SaveChangesAsync();
    }

    public async Task AddSellOffers(IList<TradeOffer> offers)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        await dbContext.SellOffers.AddRangeAsync(offers.Select(Map<PersistedSellOffer>));

        await dbContext.SaveChangesAsync();
    }

    public async Task<IList<TradeOffer>> FindAlertMatchingOffers(TimeSpan alertOfferMaxAge)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        var buyOffers = await GetAlertMatchingOffersQuery(dbContext, dbContext.BuyOffers, alertOfferMaxAge).ToListAsync();
        var sellOffers = await GetAlertMatchingOffersQuery(dbContext, dbContext.SellOffers, alertOfferMaxAge).ToListAsync();

        return buyOffers.Select(Map)
            .Union(sellOffers.Select(Map))
            .ToList();
    }

    public async Task DeleteOldOffers(TimeSpan maxAge)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        var sellOffersToDelete = dbContext.SellOffers.Where(o => o.ScrapedDate < dateTime.Now.Add(-maxAge));
        var buyOffersToDelete = dbContext.BuyOffers.Where(o => o.ScrapedDate < dateTime.Now.Add(-maxAge));

        dbContext.SellOffers.RemoveRange(sellOffersToDelete);
        dbContext.BuyOffers.RemoveRange(buyOffersToDelete);

        await dbContext.SaveChangesAsync();
    }

    public async Task<IList<Alert>> GetAlerts()
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        var items = await dbContext.Alerts.ToListAsync();

        return items.Select(Map).ToList();
    }

    public async Task<Alert?> GetAlert(int id)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        var persistedAlert = await dbContext.Alerts.SingleOrDefaultAsync(x => x.Id == id);

        if (persistedAlert == null)
            return null;

        return Map(persistedAlert);
    }

    public async Task AddAlert(Alert alert)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        dbContext.Alerts.Add(new PersistedAlert
        {
            CreatedDate = dateTime.Now,
            ItemName = alert.ItemName,
            OfferType = MapAlertOfferType(alert.OfferType),
            PriceFrom = alert.Price.From,
            PriceTo = alert.Price.To,
            ItemType = MapAlertItemType(alert.ItemType),
            Color = alert.Color,
            Certification = alert.Certification,
        });

        await dbContext.SaveChangesAsync();
    }

    public async Task UpdateAlert(Alert alert)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        var persistedAlert = await dbContext.Alerts.SingleOrDefaultAsync(x => x.Id == alert.Id);

        if (persistedAlert == null)
            throw new InvalidOperationException("Alert not found.");

        persistedAlert.ItemName = alert.ItemName;
        persistedAlert.OfferType = MapAlertOfferType(alert.OfferType);
        persistedAlert.ItemType = MapAlertItemType(alert.ItemType);
        persistedAlert.PriceFrom = alert.Price.From;
        persistedAlert.PriceTo = alert.Price.To;
        persistedAlert.Color = alert.Color;
        persistedAlert.Certification = alert.Certification;
        persistedAlert.Enabled = BoolToString(alert.Enabled);

        await dbContext.SaveChangesAsync();

        Map(persistedAlert);
    }

    public async Task DeleteAlert(int id)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        var persistedAlert = await dbContext.Alerts.SingleOrDefaultAsync(x => x.Id == id);

        if (persistedAlert == null)
            throw new InvalidOperationException("Alert not found.");

        dbContext.Alerts.Remove(persistedAlert);

        await dbContext.SaveChangesAsync();
    }

    public async Task<IList<Notification>> GetNotifications(int pageSize)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        var items = await dbContext.Notifications
            .OrderByDescending(n => n.TradeOfferScrapedDate)
            .Take(pageSize)
            .ToListAsync();

        return items.Select(Map).ToList();
    }

    public async Task<IList<Notification>> GetNotifications(TimeSpan notOlderThan)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        var items = await dbContext.Notifications
            .Where(n => n.CreatedDate >= dateTime.Now - notOlderThan)
            .ToListAsync();

        return items.Select(Map).ToList();
    }

    public async Task AddNotifications(IList<Notification> notifications)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        dbContext.Notifications.AddRange(notifications.Select(n =>
            new PersistedNotification
            {
                TradeItemName = n.TradeOffer.Item.Name,
                TradeItemType = MapTradeItemType(n.TradeOffer.Item.ItemType),
                TradeItemColor = n.TradeOffer.Item.Color,
                TradeItemCertification = n.TradeOffer.Item.Certification,
                TradeOfferSourceId = n.TradeOffer.SourceId,
                TradeOfferLink = n.TradeOffer.Link,
                TradeOfferPrice = n.TradeOffer.Price,
                TradeOfferScrapedDate = n.TradeOffer.ScrapedDate,
                CreatedDate = dateTime.Now
            }
        ));

        await dbContext.SaveChangesAsync();
    }

    public async Task MarkNotificationAsSeen(int id)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        
        var persistedItem = await dbContext.Notifications.SingleOrDefaultAsync(x => x.Id == id);

        if (persistedItem == null)
            throw new InvalidOperationException("Notification not found.");

        if (persistedItem.SeenDate != null)
            return;

        persistedItem.SeenDate = dateTime.Now;

        await dbContext.SaveChangesAsync();
    }

    public async Task DeleteOldNotifications(TimeSpan maxAge)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        var notificationsToDelete = dbContext.Notifications.Where(o => o.CreatedDate < dateTime.Now.Add(-maxAge));

        dbContext.Notifications.RemoveRange(notificationsToDelete);

        await dbContext.SaveChangesAsync();
    }

    private IQueryable<T> GetAlertMatchingOffersQuery<T>(PersistenceDbContext dbContext, DbSet<T> offers, TimeSpan alertOfferMaxAge) where T : PersistedTradeOffer
    {
        return
            from o in offers
            join a in dbContext.Alerts on o.Name.ToLower() equals a.ItemName.ToLower()
            where
                a.Enabled == BoolToString(true) &&
                a.OfferType == GetAlertOfferTypeFor(typeof(T)) &&
                o.ScrapedDate >= dateTime.Now.Add(-alertOfferMaxAge) &&
                o.Price >= a.PriceFrom &&
                o.Price <= a.PriceTo &&
                (a.ItemType == "*" || o.ItemType.ToLower() == a.ItemType.ToLower()) &&
                (a.Color == "*" || o.Color.ToLower() == a.Color.ToLower()) &&
                (a.Certification == "*" || o.Certification.ToLower() == a.Certification.ToLower())
            select o;
    }

    private string GetAlertOfferTypeFor(Type tradeOfferType)
    {
        if (tradeOfferType == typeof(PersistedBuyOffer))
            return "Buy";
        if (tradeOfferType == typeof(PersistedSellOffer))
            return "Sell";

        throw new InvalidOperationException($"Invalid offer type '{tradeOfferType.FullName}'.");
    }

    private T Map<T>(TradeOffer offer) where T : PersistedTradeOffer, new()
    {
        return new T
        {
            SourceId = offer.SourceId,
            Link = offer.Link,
            ScrapedDate = offer.ScrapedDate,
            Name = offer.Item.Name,
            Price = offer.Price,
            ItemType = MapTradeItemType(offer.Item.ItemType),
            Color = offer.Item.Color,
            Certification = offer.Item.Certification
        };
    }

    private TradeOffer Map(PersistedTradeOffer offer)
    {
        var tradeItem = new TradeItem(offer.Name)
        {
            ItemType = MapTradeItemType(offer.ItemType),
            Color = offer.Color,
            Certification = offer.Certification
        };

        return new TradeOffer
        (
            tradeItem,
            offer.Price,
            offer.ScrapedDate,
            offer.SourceId,
            offer.Link
        );
    }

    private Alert Map(PersistedAlert alert)
    {
        return new Alert(
            MapAlertOfferType(alert.OfferType),
            alert.ItemName,
            new PriceRange(alert.PriceFrom, alert.PriceTo)
        )
        {
            Id = alert.Id,
            CreatedDate = alert.CreatedDate,
            ItemType = MapAlertItemType(alert.ItemType),
            Color = alert.Color,
            Certification = alert.Certification,
            Enabled = StringToBool(alert.Enabled)
        };
    }

    private Notification Map(PersistedNotification notification)
    {
        var tradeItem = new TradeItem(notification.TradeItemName)
        {
            ItemType = MapTradeItemType(notification.TradeItemType),
            Color = notification.TradeItemColor,
            Certification = notification.TradeItemCertification
        };

        var tradeOffer = new TradeOffer
        (
            tradeItem,
            notification.TradeOfferPrice,
            notification.TradeOfferScrapedDate,
            notification.TradeOfferSourceId,
            notification.TradeOfferLink
        );

        return new Notification(tradeOffer)
        {
            Id = notification.Id,
            SeenDate = notification.SeenDate
        };
    }

    private string MapTradeItemType(TradeItemType itemType)
    {
        switch (itemType)
        {
            case TradeItemType.Body:
                return "Body";
            case TradeItemType.Decal:
                return "Decal";
            case TradeItemType.PaintFinish:
                return "Paint Finish";
            case TradeItemType.Wheels:
                return "Wheels";
            case TradeItemType.RocketBoost:
                return "Boost";
            case TradeItemType.Topper:
                return "Topper";
            case TradeItemType.Antenna:
                return "Antenna";
            case TradeItemType.GoalExplosion:
                return "Goal Explosion";
            case TradeItemType.Trail:
                return "Trail";
            case TradeItemType.Banner:
                return "Banner";
            case TradeItemType.AvatarBorder:
                return "Avatar Border";
            case TradeItemType.Unknown:
            default:
                return "";
        }
    }

    private TradeItemType MapTradeItemType(string itemType)
    {
        switch (itemType.ToLower())
        {
            case "body":
                return TradeItemType.Body;
            case "decal":
                return TradeItemType.Decal;
            case "paint finish":
                return TradeItemType.PaintFinish;
            case "wheels":
                return TradeItemType.Wheels;
            case "boost":
                return TradeItemType.RocketBoost;
            case "topper":
                return TradeItemType.Topper;
            case "antenna":
                return TradeItemType.Antenna;
            case "goal explosion":
                return TradeItemType.GoalExplosion;
            case "trail":
                return TradeItemType.Trail;
            case "banner":
                return TradeItemType.Banner;
            case "avatar border":
                return TradeItemType.AvatarBorder;
            default:
                return TradeItemType.Unknown;
        }
    }

    private string MapAlertItemType(AlertItemType itemType)
    {
        switch (itemType)
        {
            case AlertItemType.Body:
                return "Body";
            case AlertItemType.Decal:
                return "Decal";
            case AlertItemType.PaintFinish:
                return "Paint Finish";
            case AlertItemType.Wheels:
                return "Wheels";
            case AlertItemType.RocketBoost:
                return "Boost";
            case AlertItemType.Topper:
                return "Topper";
            case AlertItemType.Antenna:
                return "Antenna";
            case AlertItemType.GoalExplosion:
                return "Goal Explosion";
            case AlertItemType.Trail:
                return "Trail";
            case AlertItemType.Banner:
                return "Banner";
            case AlertItemType.AvatarBorder:
                return "Avatar Border";
            case AlertItemType.Any:
            default:
                return "*";
        }
    }

    private AlertItemType MapAlertItemType(string itemType)
    {
        switch (itemType.ToLower())
        {
            case "body":
                return AlertItemType.Body;
            case "decal":
                return AlertItemType.Decal;
            case "paint finish":
                return AlertItemType.PaintFinish;
            case "wheels":
                return AlertItemType.Wheels;
            case "boost":
                return AlertItemType.RocketBoost;
            case "topper":
                return AlertItemType.Topper;
            case "antenna":
                return AlertItemType.Antenna;
            case "goal explosion":
                return AlertItemType.GoalExplosion;
            case "trail":
                return AlertItemType.Trail;
            case "banner":
                return AlertItemType.Banner;
            case "avatar border":
                return AlertItemType.AvatarBorder;
            default:
                return AlertItemType.Any;
        }
    }

    private AlertOfferType MapAlertOfferType(string offerType)
    {
        switch (offerType.ToLower())
        {
            case "buy":
                return AlertOfferType.Buy;
            case "sell":
                return AlertOfferType.Sell;
            default:
                throw new InvalidOperationException($"Unable to map '{offerType}' to alert offer type.");
        }
    }

    private string MapAlertOfferType(AlertOfferType offerType)
    {
        switch (offerType)
        {
            case AlertOfferType.Buy:
                return "Buy";
            case AlertOfferType.Sell:
                return "Sell";
            default:
                throw new InvalidOperationException($"Invalid offer type '{offerType}'.");
        }
    }

    private bool StringToBool(string value)
    {
        switch (value.ToLower())
        {
            case "yes":
                return true;
            case "no":
                return false;
            default:
                throw new InvalidOperationException($"Unable to map '{value}' to boolean.");
        }
    }

    private string BoolToString(bool value)
    {
        return value ? "Yes" : "No";
    }
}