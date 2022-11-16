using RocketLeagueTradingTools.Infrastructure.Persistence.Models;

namespace RocketLeagueTradingTools.Infrastructure.UnitTests.Persistence.Support;

static class Build
{
    public static PersistedTradeOffer DefaultOffer(string alertOfferType)
    {
        return PersistedOffer(alertOfferType).With(o =>
        {
            o.Link = $"https://rocket-league.com/trade/{nameof(DefaultOffer)}";
            o.ScrapedDate = DateTime.UtcNow;
            o.TradingSite = "RLG";
            o.TraderName = nameof(DefaultOffer);
        });
    }

    public static PersistedTradeOffer With(this PersistedTradeOffer offer, Action<PersistedTradeOffer> setup)
    {
        setup(offer);

        return offer;
    }

    public static PersistedNotification DefaultNotification()
    {
        return new PersistedNotification
        {
            Id = 1,
            CreatedDate = DateTime.UtcNow,
            TradeOfferLink = $"https://rocket-league.com/trade/{nameof(DefaultNotification)}",
            TradeOfferScrapedDate = DateTime.UtcNow,
            TradingSite = "RLG",
            TraderName = "RLTrader69"
        };
    }
    
    public static PersistedNotification With(this PersistedNotification notification, Action<PersistedNotification> setup)
    {
        setup(notification);

        return notification;
    }

    private static PersistedTradeOffer PersistedOffer(string alertOfferType)
    {
        if (alertOfferType.ToLower() == "buy")
            return Activator.CreateInstance<PersistedBuyOffer>();

        if (alertOfferType.ToLower() == "sell")
            return Activator.CreateInstance<PersistedSellOffer>();

        throw new NotSupportedException($"Invalid offer type '{alertOfferType}'.");
    }
}