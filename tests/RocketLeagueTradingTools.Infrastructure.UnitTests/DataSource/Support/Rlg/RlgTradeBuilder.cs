using System.Text;

namespace RocketLeagueTradingTools.Infrastructure.UnitTests.DataSource.Support.Rlg;

class RlgTradeBuilder : IHtmlBuilder
{
    private readonly string id;
    private readonly List<IHtmlBuilder> hasItemBuilders = new();
    private readonly List<IHtmlBuilder> wantItemBuilders = new();

    public RlgTradeBuilder(string id)
    {
        this.id = id;
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
        result.AppendLine(BuildContent());
        result.AppendLine(BuildActions());
        result.AppendLine("</div>");

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