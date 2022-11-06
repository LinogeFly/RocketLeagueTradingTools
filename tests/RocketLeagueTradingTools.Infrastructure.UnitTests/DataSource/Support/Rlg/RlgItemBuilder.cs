using System.Text;

namespace RocketLeagueTradingTools.Infrastructure.UnitTests.DataSource.Support.Rlg;

class RlgItemBuilder : IHtmlBuilder
{
    private readonly string name;
    private readonly int quantity = 1;
    private string color = "";
    private string certification = "";
    private string itemDetailsLink = "";

    public RlgItemBuilder(string name)
    {
        this.name = name;
    }

    public RlgItemBuilder(string name, int quantity) : this(name)
    {
        this.quantity = quantity;
    }

    public RlgItemBuilder WithColor(string color)
    {
        this.color = color;

        return this;
    }

    public RlgItemBuilder WithCertification(string certification)
    {
        this.certification = certification;

        return this;
    }

    public RlgItemBuilder WithItemDetailsLink(string link)
    {
        this.itemDetailsLink = link;

        return this;
    }

    string IHtmlBuilder.Build()
    {
        var result = new StringBuilder();

        result.AppendLine("<div class=\"rlg-item\">");

        // Color
        if (!string.IsNullOrEmpty(color))
            result.AppendLine($"<div class=\"rlg-item__paint\"> {color} </div>");

        // Text
        result.AppendLine("<div class=\"rlg-item__text\">");
        //
        // Name
        result.AppendLine($"<h2 class=\"rlg-item__name\"> {name} </h2>");
        //
        // Certification
        if (!string.IsNullOrEmpty(certification))
            result.AppendLine($"<div class=\"rlg-item__cert\"> {certification} </div>");
        //
        result.AppendLine("</div>");

        // Quantity
        if (quantity > 1)
            result.AppendLine($"<div class=\"rlg-item__quantity\"> {quantity} </div>");

        // Links
        if (!string.IsNullOrEmpty(itemDetailsLink))
        {
            result.AppendLine($"<div class=\"rlg-item-links\">");
            result.AppendLine($"<a class=\"rlg-btn-primary --small\" href=\"{itemDetailsLink}\">Item details</a>");
            result.AppendLine("</div>");
        }

        result.AppendLine("</div>");

        return result.ToString();
    }
}
