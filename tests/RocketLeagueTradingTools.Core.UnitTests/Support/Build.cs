using RocketLeagueTradingTools.Core.Domain.Entities;

namespace RocketLeagueTradingTools.Core.UnitTests.Support;

static class Build
{
    public static TradeItem TradeItem(string itemName)
    {
        return new TradeItem(itemName);
    }

    public static TradeItem WithColor(this TradeItem item, string color)
    {
        item.Color = color;

        return item;
    }

    public static TradeItem WithCertification(this TradeItem item, string cert)
    {
        item.Certification = cert;

        return item;
    }
}