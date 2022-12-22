using System.Text;

namespace RocketLeagueTradingTools.Infrastructure.UnitTests.TradeOfferSource.Support.Rlg;

class RlgPageBuilder
{
    private readonly List<RlgTradeBuilder> tradeBuilders = new();

    public RlgTradeBuilder AddTrade()
    {
        var id = Guid.NewGuid().ToString();
        var builder = new RlgTradeBuilder(id);

        tradeBuilders.Add(builder);

        return builder;
    }

    public RlgTradeBuilder AddTrade(string id)
    {
        var builder = new RlgTradeBuilder(id);

        tradeBuilders.Add(builder);

        return builder;
    }

    public string Build()
    {
        var result = new StringBuilder();

        result.AppendLine("<!doctype html><html prefix=\"og: http://ogp.me/ns#\"><head><body>");
        result.AppendLine("<div id=\"trading-trades\">");

        foreach (var tradeBuilder in tradeBuilders)
            result.AppendLine(tradeBuilder.Build());

        result.AppendLine("</div>");
        result.AppendLine("</body></html>");

        return result.ToString();
    }
}