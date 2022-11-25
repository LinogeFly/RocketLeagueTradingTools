using RocketLeagueTradingTools.Infrastructure.Persistence.Models;

namespace RocketLeagueTradingTools.Infrastructure.UnitTests.Persistence.Support;

internal static class A
{
    public static PersistedTradeOffer TradeOffer() => new()
    {
        Link = "https://rocket-league.com/trade/88c10b46-a29a-4770-8efa-0304d6be8699",
        ScrapedDate = DateTime.UtcNow,
        OfferType = "Sell",
        ItemName = "Fennec",
        Price = 300,
        TradingSite = "RLG",
        TraderName = "RLTrader69"
    };
    
    public static PersistedNotification Notification() => new()
    {
        TradeOffer = TradeOffer(),
        CreatedDate = DateTime.UtcNow,
    };

    public static PersistedBlacklistedTrader BlacklistedTrader() => new()
    {
        TradingSite = "RLG",
        TraderName = "AnnoyingSpammer"
    };
}

internal static class An
{
    public static PersistedAlert Alert() => new()
    {
        CreatedDate = DateTime.UtcNow,
        OfferType = "Sell",
        ItemName = "Fennec",
        ItemType = "*",
        Color = "*",
        Certification = "*",
        PriceFrom = 0,
        PriceTo = 300,
        Enabled = "Yes"
    };
}

internal static class BuilderExtensions
{
    public static T With<T>(this T obj, Action<T> setup) where T : class
    {
        setup(obj);

        return obj;
    }
}