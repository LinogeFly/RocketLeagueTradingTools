using Microsoft.EntityFrameworkCore;
using RocketLeagueTradingTools.Core.Application.Contracts;
using RocketLeagueTradingTools.Core.Domain.Entities;
using RocketLeagueTradingTools.Core.Domain.ValueObjects;
using RocketLeagueTradingTools.Infrastructure.Persistence.PersistedTypes;

namespace RocketLeagueTradingTools.Infrastructure.Persistence;

public class PersistenceRepository : IPersistenceRepository
{
    private readonly PersistenceDbContext dbContext;
    private readonly IDateTime dateTime;

    public PersistenceRepository(
        PersistenceDbContext dbContext,
        IDateTime dateTime)
    {
        this.dbContext = dbContext;
        this.dateTime = dateTime;
    }

    public async Task AddBuyOffers(IList<TradeOffer> offers)
    {
        await dbContext.BuyOffers.AddRangeAsync(offers.Select(Map<PersistedBuyOffer>));

        await dbContext.SaveChangesAsync();
    }

    public async Task AddSellOffers(IList<TradeOffer> offers)
    {
        await dbContext.SellOffers.AddRangeAsync(offers.Select(Map<PersistedSellOffer>));

        await dbContext.SaveChangesAsync();
    }

    public async Task<IList<TradeOffer>> FindAlertMatchingOffers(TimeSpan alertOfferMaxAge)
    {
        var buyOffers = await GetAlertMatchingOffersQuery(dbContext.BuyOffers, alertOfferMaxAge).ToListAsync();
        var sellOffers = await GetAlertMatchingOffersQuery(dbContext.SellOffers, alertOfferMaxAge).ToListAsync();

        return buyOffers.Select(Map)
            .Union(sellOffers.Select(Map))
            .ToList();
    }

    public async Task<IList<Alert>> GetAlerts()
    {
        var items = await dbContext.Alerts
            .ToListAsync();

        return items.Select(Map).ToList();
    }

    public async Task<Alert?> GetAlert(int id)
    {
        var persistedAlert = await dbContext.Alerts.SingleOrDefaultAsync(x => x.Id == id);

        if (persistedAlert == null)
            return null;

        return Map(persistedAlert);
    }

    public async Task AddAlert(Alert alert)
    {
        dbContext.Alerts.Add(new PersistedAlert
        {
            CreatedDate = dateTime.Now,
            ItemName = alert.ItemName,
            OfferType = Map(alert.OfferType),
            PriceFrom = alert.Price.From,
            PriceTo = alert.Price.To,
            Color = alert.Color,
            Certification = alert.Certification,
        });

        await dbContext.SaveChangesAsync();
    }

    public async Task UpdateAlert(Alert alert)
    {
        var persistedAlert = await dbContext.Alerts.SingleOrDefaultAsync(x => x.Id == alert.Id);

        if (persistedAlert == null)
            throw new InvalidOperationException("Alert not found.");

        persistedAlert.ItemName = alert.ItemName;
        persistedAlert.OfferType = Map(alert.OfferType);
        persistedAlert.PriceFrom = alert.Price.From;
        persistedAlert.PriceTo = alert.Price.To;
        persistedAlert.Color = alert.Color;
        persistedAlert.Certification = alert.Certification;
        persistedAlert.Disabled = alert.Disabled;

        await dbContext.SaveChangesAsync();

        Map(persistedAlert);
    }

    public async Task DeleteAlert(int id)
    {
        var persistedAlert = await dbContext.Alerts.SingleOrDefaultAsync(x => x.Id == id);

        if (persistedAlert == null)
            throw new InvalidOperationException("Alert not found.");

        dbContext.Alerts.Remove(persistedAlert);

        await dbContext.SaveChangesAsync();
    }

    public async Task<IList<Notification>> GetNotifications(int pageSize)
    {
        var items = await dbContext.Notifications
            .OrderByDescending(n => n.TradeOfferScrapedDate)
            .Take(pageSize)
            .ToListAsync();

        return items.Select(Map).ToList();
    }

    public async Task<IList<Notification>> GetNotifications(TimeSpan notOlderThan)
    {
        var items = await dbContext.Notifications
            .Where(n => n.CreatedDate >= dateTime.Now - notOlderThan)
            .ToListAsync();

        return items.Select(Map).ToList();
    }

    public async Task AddNotifications(IList<Notification> notifications)
    {
        dbContext.Notifications.AddRange(notifications.Select(n =>
            new PersistedNotification
            {
                TradeItemName = n.TradeOffer.Item.Name,
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
        var persistedItem = await dbContext.Notifications.SingleOrDefaultAsync(x => x.Id == id);

        if (persistedItem == null)
            throw new InvalidOperationException("Notification not found.");

        if (persistedItem.SeenDate != null)
            return;

        persistedItem.SeenDate = dateTime.Now;

        await dbContext.SaveChangesAsync();
    }

    public IQueryable<T> GetAlertMatchingOffersQuery<T>(DbSet<T> offers, TimeSpan alertOfferMaxAge) where T : PersistedTradeOffer
    {
        return
            from o in offers
            join a in dbContext.Alerts on o.Name.ToLower() equals a.ItemName.ToLower()
            where
                a.Disabled == false &&
                a.OfferType == GetAlertOfferTypeFor(typeof(T)) &&
                o.ScrapedDate >= dateTime.Now.Add(-alertOfferMaxAge) &&
                o.Price >= a.PriceFrom &&
                o.Price <= a.PriceTo &&
                (a.Color == "*" || o.Color.ToLower() == a.Color.ToLower()) &&
                (a.Certification == "*" || o.Certification.ToLower() == a.Certification.ToLower())
            select o;
    }

    private PersistedAlertOfferType GetAlertOfferTypeFor(Type tradeOfferType)
    {
        if (tradeOfferType == typeof(PersistedBuyOffer))
            return PersistedAlertOfferType.Buy;
        if (tradeOfferType == typeof(PersistedSellOffer))
            return PersistedAlertOfferType.Sell;

        throw new InvalidOperationException($"Invalid offer type '{tradeOfferType.FullName}'.");
    }

    private T Map<T>(TradeOffer offer) where T : PersistedTradeOffer, new()
    {
        return new T
        {
            SourceId = offer.SourceId,
            Link = offer.Link,
            ScrapedDate = offer.ScrapedDate,
            Price = offer.Price,
            Name = offer.Item.Name,
            Color = offer.Item.Color,
            Certification = offer.Item.Certification
        };
    }

    private TradeOffer Map(PersistedTradeOffer offer)
    {
        var tradeItem = new TradeItem(offer.Name)
        {
            Color = offer.Color,
            Certification = offer.Certification
        };

        return new TradeOffer(tradeItem)
        {
            SourceId = offer.SourceId,
            Link = offer.Link,
            ScrapedDate = offer.ScrapedDate,
            Price = offer.Price
        };
    }

    private Alert Map(PersistedAlert alert)
    {
        return new Alert
        {
            Id = alert.Id,
            CreatedDate = alert.CreatedDate,
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
                throw new InvalidOperationException($"Invalid offer type '{offerType}'.");
        }
    }

    private PersistedAlertOfferType Map(AlertOfferType offerType)
    {
        switch (offerType)
        {
            case AlertOfferType.Buy:
                return PersistedAlertOfferType.Buy;
            case AlertOfferType.Sell:
                return PersistedAlertOfferType.Sell;
            default:
                throw new InvalidOperationException($"Invalid offer type '{offerType}'.");
        }
    }

    private Notification Map(PersistedNotification notification)
    {
        var tradeItem = new TradeItem(notification.TradeItemName)
        {
            Color = notification.TradeItemColor,
            Certification = notification.TradeItemCertification
        };

        var tradeOffer = new TradeOffer(tradeItem)
        {
            SourceId = notification.TradeOfferSourceId,
            Link = notification.TradeOfferLink,
            Price = notification.TradeOfferPrice,
            ScrapedDate = notification.TradeOfferScrapedDate
        };

        return new Notification(tradeOffer)
        {
            Id = notification.Id,
            SeenDate = notification.SeenDate
        };
    }
}