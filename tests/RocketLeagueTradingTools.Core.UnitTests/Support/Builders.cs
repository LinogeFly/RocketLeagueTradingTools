using RocketLeagueTradingTools.Core.Domain.Enumerations;

namespace RocketLeagueTradingTools.Core.UnitTests.Support;

internal static class A
{
    public static TradeOfferBuilder TradeOffer() => new TradeOfferBuilder()
        .WithTrader(TradingSite.RocketLeagueGarage, "RLTrader69")
        .WithLink("https://rocket-league.com/trade/1")
        .WithItem(TradeItem())
        .WithPrice(300)
        .WithType(TradeOfferType.Sell);

    public static TradeOfferBuilder SellOffer() => new TradeOfferBuilder()
        .WithType(TradeOfferType.Sell);

    public static ScrapedTradeOfferBuilder ScrapedOffer() => new ScrapedTradeOfferBuilder()
        .WithOffer(TradeOffer())
        .WithScrapedDate(DateTime.UtcNow);

    public static TradeItemBuilder TradeItem() => new TradeItemBuilder()
        .WithName("Fennec");

    public static NotificationBuilder Notification() => new();
}