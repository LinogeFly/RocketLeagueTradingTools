using Microsoft.EntityFrameworkCore;
using RocketLeagueTradingTools.Core.Application.Interfaces;
using RocketLeagueTradingTools.Core.Domain.Entities;
using RocketLeagueTradingTools.Core.Domain.Enumerations;
using RocketLeagueTradingTools.Core.Domain.ValueObjects;
using RocketLeagueTradingTools.Infrastructure.Persistence.Models;

namespace RocketLeagueTradingTools.Infrastructure.Persistence;

public class PersistenceRepository : IPersistenceRepository
{
    private readonly IDateTime dateTime;
    private readonly IDbContextFactory<PersistenceDbContext> dbContextFactory;

    public PersistenceRepository(
        IDbContextFactory<PersistenceDbContext> dbContextFactory,
        IDateTime dateTime)
    {
        this.dbContextFactory = dbContextFactory;
        this.dateTime = dateTime;
    }

    public async Task AddTradeOffers(IList<ScrapedTradeOffer> offers)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        await dbContext.TradeOffers.AddRangeAsync(offers.Select(Map));

        await dbContext.SaveChangesAsync();
    }

    public async Task<IList<ScrapedTradeOffer>> FindAlertMatchingTradeOffers(TimeSpan alertOfferMaxAge)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        var offers = await GetAlertMatchingOffersQuery(dbContext, alertOfferMaxAge).ToListAsync();

        return offers.Select(Map).ToList();
    }

    public async Task DeleteOldTradeOffers(TimeSpan maxAge)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        var offersToDelete =
            from o in dbContext.TradeOffers
            from n in dbContext.Notifications.Where(n => n.TradeOffer.Id == o.Id).DefaultIfEmpty()
            where
                o.ScrapedDate < dateTime.Now.Add(-maxAge) &&
                n.TradeOffer == null
            select o;

        dbContext.TradeOffers.RemoveRange(offersToDelete);

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
            OfferType = MapTradeOfferType(alert.OfferType),
            PriceFrom = alert.Price.From,
            PriceTo = alert.Price.To,
            ItemType = MapAlertItemType(alert.ItemType),
            Color = alert.Color,
            Certification = alert.Certification
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
        persistedAlert.OfferType = MapTradeOfferType(alert.OfferType);
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
            .OrderByDescending(n => n.CreatedDate)
            .Include(n => n.TradeOffer)
            .Take(pageSize)
            .ToListAsync();

        return items.Select(Map).ToList();
    }

    public async Task<IList<Notification>> GetNotifications(TimeSpan notOlderThan)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        var items = await dbContext.Notifications
            .Where(n => n.CreatedDate >= dateTime.Now - notOlderThan)
            .Include(n => n.TradeOffer)
            .ToListAsync();

        return items.Select(Map).ToList();
    }

    public async Task<int> GetNotificationsCount()
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        return await dbContext.Notifications.CountAsync();
    }

    public async Task AddNotifications(IList<Notification> notifications)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        
        dbContext.Notifications.AddRange(notifications.Select(n =>
            new PersistedNotification
            {
                TradeOffer = dbContext.TradeOffers.Single(o => o.Id == n.ScrapedTradeOffer.Id),
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

    public async Task MarkAllNotificationAsSeen()
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        await dbContext.Notifications
            .Where(n => n.SeenDate == null)
            .ForEachAsync(n => n.SeenDate = dateTime.Now);

        await dbContext.SaveChangesAsync();
    }

    public async Task DeleteOldNotifications(TimeSpan maxAge)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        var notificationsToDelete = dbContext.Notifications.Where(o => o.CreatedDate < dateTime.Now.Add(-maxAge));

        dbContext.Notifications.RemoveRange(notificationsToDelete);

        await dbContext.SaveChangesAsync();
    }

    public async Task<IList<Trader>> GetBlacklistedTraders()
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        var items = await dbContext.BlacklistedTraders.ToListAsync();

        return items.Select(Map).ToList();
    }

    public async Task AddBlacklistedTrader(Trader trader)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        dbContext.BlacklistedTraders.Add(new PersistedBlacklistedTrader
        {
            TraderName = trader.Name,
            TradingSite = MapTradingSite(trader.TradingSite)
        });

        await dbContext.SaveChangesAsync();
    }

    public async Task DeleteBlacklistedTrader(int id)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        var itemToDelete = await dbContext.BlacklistedTraders.SingleOrDefaultAsync(x => x.Id == id);

        if (itemToDelete == null)
            throw new InvalidOperationException("Blacklisted trader not found.");

        dbContext.BlacklistedTraders.Remove(itemToDelete);

        await dbContext.SaveChangesAsync();
    }

    private IQueryable<PersistedTradeOffer> GetAlertMatchingOffersQuery(PersistenceDbContext dbContext, TimeSpan alertOfferMaxAge)
    {
        return
            from o in dbContext.TradeOffers
            join a in dbContext.Alerts on o.ItemName.ToLower() equals a.ItemName.ToLower()
            where
                a.Enabled == BoolToString(true) &&
                a.OfferType == o.OfferType &&
                o.ScrapedDate >= dateTime.Now.Add(-alertOfferMaxAge) &&
                o.Price >= a.PriceFrom &&
                o.Price <= a.PriceTo &&
                (a.ItemType == "*" || o.ItemType.ToLower() == a.ItemType.ToLower()) &&
                (a.Color == "*" || o.Color.ToLower() == a.Color.ToLower() || (a.Color == "+" && o.Color != "")) &&
                (a.Certification == "*" || o.Certification.ToLower() == a.Certification.ToLower() || (a.Certification == "+" && o.Certification != "")) &&
                !(
                    from b in dbContext.BlacklistedTraders
                    where b.TradingSite == o.TradingSite
                    select b.TraderName.ToLower()
                ).Contains(o.TraderName.ToLower())
            select o;
    }

    private PersistedTradeOffer Map(ScrapedTradeOffer offer)
    {
        return new PersistedTradeOffer
        {
            ScrapedDate = offer.ScrapedDate,
            Link = offer.TradeOffer.Link,
            OfferType = MapTradeOfferType(offer.TradeOffer.OfferType),
            ItemName = offer.TradeOffer.Item.Name,
            Price = offer.TradeOffer.Price,
            ItemType = MapTradeItemType(offer.TradeOffer.Item.ItemType),
            Color = offer.TradeOffer.Item.Color,
            Certification = offer.TradeOffer.Item.Certification,
            TradingSite = MapTradingSite(offer.TradeOffer.Trader.TradingSite),
            TraderName = offer.TradeOffer.Trader.Name,
        };
    }

    private ScrapedTradeOffer Map(PersistedTradeOffer offer)
    {
        var tradeItem = new TradeItem(offer.ItemName)
        {
            ItemType = MapTradeItemType(offer.ItemType),
            Color = offer.Color,
            Certification = offer.Certification
        };
        var trader = new Trader(MapTradingSite(offer.TradingSite), offer.TraderName);
        var tradeOffer = new TradeOffer
        (
            MapTradeOfferType(offer.OfferType),
            tradeItem,
            offer.Price,
            offer.Link,
            trader
        );

        return new ScrapedTradeOffer(tradeOffer, offer.ScrapedDate)
        {
            Id = offer.Id
        };
    }

    private Alert Map(PersistedAlert alert)
    {
        return new Alert(
            MapTradeOfferType(alert.OfferType),
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
        var tradeOffer = Map(notification.TradeOffer);

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

    private TradeOfferType MapTradeOfferType(string offerType)
    {
        switch (offerType.ToLower())
        {
            case "buy":
                return TradeOfferType.Buy;
            case "sell":
                return TradeOfferType.Sell;
            default:
                throw new InvalidOperationException($"Unable to map '{offerType}' to alert offer type.");
        }
    }

    private string MapTradeOfferType(TradeOfferType offerType)
    {
        switch (offerType)
        {
            case TradeOfferType.Buy:
                return "Buy";
            case TradeOfferType.Sell:
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

    private TradingSite MapTradingSite(string site)
    {
        switch (site.ToLower())
        {
            case "rlg":
                return TradingSite.RocketLeagueGarage;
            default:
                throw new NotSupportedException($"Not supported trading site '{site}'.");
        }
    }

    private string MapTradingSite(TradingSite site)
    {
        switch (site)
        {
            case TradingSite.RocketLeagueGarage:
                return "RLG";
            default:
                throw new NotSupportedException($"Not supported trading site '{site}'.");
        }
    }

    private Trader Map(PersistedBlacklistedTrader trader)
    {
        return new Trader(MapTradingSite(trader.TradingSite), trader.TraderName);
    }
}