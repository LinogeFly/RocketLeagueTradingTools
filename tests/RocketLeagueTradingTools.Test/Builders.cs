using RocketLeagueTradingTools.Core.Domain.Enumerations;
using RocketLeagueTradingTools.Test.Builders;

namespace RocketLeagueTradingTools.Test;

public static class A
{
    public static TradeOfferBuilder TradeOffer() => new TradeOfferBuilder()
        .WithTrader(Trader())
        .WithLink("https://rocket-league.com/trade/81dd3df9-4fc7-4e01-9a7d-711db61d4fb2")
        .WithItem(TradeItem())
        .WithPrice(300)
        .WithType(TradeOfferType.Sell);

    public static ScrapedTradeOfferBuilder ScrapedOffer() => new ScrapedTradeOfferBuilder()
        .WithOffer(TradeOffer())
        .WithScrapedDate(DateTime.UtcNow);

    public static TradeItemBuilder TradeItem() => new TradeItemBuilder()
        .WithName("Fennec");

    public static NotificationBuilder Notification() => new NotificationBuilder()
        .WithScrapedOffer(ScrapedOffer());

    public static TraderBuilder Trader() => new TraderBuilder()
        .WithTradingSite(TradingSite.RocketLeagueGarage)
        .WithName("RLTrader69");
}

public static class An
{
    public static AlertBuilder Alert() => new AlertBuilder()
        .WithType(TradeOfferType.Sell)
        .WithItemName("Fennec")
        .WithPriceTo(300);
}