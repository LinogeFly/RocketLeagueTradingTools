using System.Text;

namespace RocketLeagueTradingTools.Infrastructure.UnitTests.TradeOfferSource.Support.Rlg;

class RlgTradeBuilder
{
    private readonly string id;
    private string traderName = nameof(RlgTradeBuilder);
    private readonly List<RlgItemBuilder> hasItemBuilders = new();
    private readonly List<RlgItemBuilder> wantItemBuilders = new();

    public RlgTradeBuilder(string id)
    {
        this.id = id;
    }

    public RlgTradeBuilder WithTraderName(string traderName)
    {
        this.traderName = traderName;

        return this;
    }

    public RlgTradeBuilder WithHasItem(RlgItemBuilder builder)
    {
        hasItemBuilders.Add(builder);

        return this;
    }

    public RlgTradeBuilder WithWantsItem(RlgItemBuilder builder)
    {
        wantItemBuilders.Add(builder);

        return this;
    }

    public string Build()
    {
        var result = new StringBuilder();

        result.AppendLine("<div class=\"rlg-trade\">");
        result.AppendLine(BuildHeader());
        result.AppendLine(BuildContent());
        result.AppendLine(BuildActions());
        result.AppendLine("</div>");

        return result.ToString();
    }

    private string BuildHeader()
    {
        var result = new StringBuilder();

        result.AppendLine("<header class=\"rlg-trade__header\">");
        result.AppendLine("<a class=\"rlg-trade__user\">");
        result.AppendLine("<div class=\"rlg-trade__meta\">");
        result.AppendLine("<div class=\"rlg-trade__username\">");
        result.AppendLine("<img class=\"rlg-premium__rank\" src=\"https://static.rocket-league.com/assets/999f99302d8d0bd83d263fa15fb6050f97e3f25b/images/premium/titanium.png\" alt=\"titanium\">");
        result.AppendLine("<span class=\"rlg-rank --titanium\"> Titanium </span>");
        result.AppendLine($" {traderName} ");
        result.AppendLine("</div>");
        result.AppendLine("</div>");
        result.AppendLine("</a>");
        result.AppendLine("</header>");

        return result.ToString();
    }

    private string BuildContent()
    {
        var result = new StringBuilder();

        result.AppendLine("<div class=\"rlg-trade__content\">");
        result.AppendLine("<div class=\"rlg-trade__items\">");

        // Has items
        result.AppendLine("<div class=\"rlg-trade__itemshas\">");
        //
        foreach (var hasItemBuilder in hasItemBuilders)
            result.AppendLine(hasItemBuilder.Build());
        //
        result.AppendLine("</div>");

        // Wants items
        result.AppendLine("<div class=\"rlg-trade__itemswants\">");
        //
        foreach (var wantItemBuilder in wantItemBuilders)
            result.AppendLine(wantItemBuilder.Build());
        //
        result.AppendLine("</div>");

        result.AppendLine("</div></div>");

        return result.ToString();
    }

    private string BuildActions()
    {
        var result = new StringBuilder();

        result.AppendLine("<div class=\"rlg-trade__actions\">");
        result.AppendLine($"<a href=\"/trade/{id}\" class=\"rlg-trade__action --comments\"></a>");
        result.AppendLine($"<a href=\"\" class=\"rlg-trade__action rlg-trade__bookmark --bookmark\" data-alias=\"{id}\"></a>");
        result.AppendLine("</div>");

        return result.ToString();
    }
}