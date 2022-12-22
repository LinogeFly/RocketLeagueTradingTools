using RocketLeagueTradingTools.Common.Exceptions;
using RocketLeagueTradingTools.Core.Domain.Entities;
using RocketLeagueTradingTools.Core.Domain.Enumerations;
using RocketLeagueTradingTools.Core.Domain.ValueObjects;
using RocketLeagueTradingTools.Infrastructure.Persistence.Models;

namespace RocketLeagueTradingTools.Infrastructure.Persistence.Mapping;

public static class PersistenceMapper
{
    public static Alert Map(PersistedAlert alert)
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

    public static string MapAlertItemType(AlertItemType itemType)
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

    public static string MapTradeOfferType(TradeOfferType offerType)
    {
        switch (offerType)
        {
            case TradeOfferType.Buy:
                return "Buy";
            case TradeOfferType.Sell:
                return "Sell";
            default:
                throw new MappingException(offerType, typeof(TradeOfferType), typeof(string));
        }
    }

    public static string BoolToString(bool value)
    {
        return value ? "Yes" : "No";
    }

    public static string MapTradingSite(TradingSite value)
    {
        switch (value)
        {
            case TradingSite.RocketLeagueGarage:
                return "RLG";
            default:
                throw new MappingException(value, typeof(TradingSite), typeof(string));
        }
    }

    public static Trader MapToTrader(PersistedBlacklistedTrader trader)
    {
        return new Trader(MapTradingSite(trader.TradingSite), trader.TraderName);
    }
    
    public static BlacklistedTrader MapToBlacklistedTrader(PersistedBlacklistedTrader trader)
    {
        return new BlacklistedTrader(MapToTrader(trader))
        {
            Id = trader.Id
        };
    }

    public static ScrapedTradeOffer Map(PersistedTradeOffer offer)
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

    public static PersistedTradeOffer Map(ScrapedTradeOffer offer)
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
            TraderName = offer.TradeOffer.Trader.Name
        };
    }

    public static Notification Map(PersistedNotification notification)
    {
        var tradeOffer = Map(notification.TradeOffer);

        return new Notification(tradeOffer)
        {
            Id = notification.Id,
            SeenDate = notification.SeenDate
        };
    }

    private static TradeItemType MapTradeItemType(string itemType)
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

    private static string MapTradeItemType(TradeItemType itemType)
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

    private static AlertItemType MapAlertItemType(string itemType)
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

    private static TradeOfferType MapTradeOfferType(string offerType)
    {
        switch (offerType.ToLower())
        {
            case "buy":
                return TradeOfferType.Buy;
            case "sell":
                return TradeOfferType.Sell;
            default:
                throw new MappingException(offerType, typeof(string), typeof(TradeOfferType));
        }
    }

    private static bool StringToBool(string value)
    {
        switch (value.ToLower())
        {
            case "yes":
                return true;
            case "no":
                return false;
            default:
                throw new MappingException(value, typeof(string), typeof(bool));
        }
    }

    private static TradingSite MapTradingSite(string value)
    {
        switch (value.ToLower())
        {
            case "rlg":
                return TradingSite.RocketLeagueGarage;
            default:
                throw new MappingException(value, typeof(string), typeof(TradingSite));
        }
    }
}