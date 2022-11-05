namespace RocketLeagueTradingTools.Core.Domain.Entities;

public sealed class TradeOffer
{
    public TradeItem Item { get; set; }
    public int Price { get; set; }
    public DateTime ScrapedDate { get; set; }
    public string SourceId { get; set; }
    public string Link { get; set; }

    public TradeOffer(TradeItem item, int price, DateTime scrapedDate, string sourceId, string link)
    {
        Item = item;
        Price = price;
        ScrapedDate = scrapedDate;
        SourceId = sourceId;
        Link = link;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as TradeOffer);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Item, Price, SourceId, Link);
    }

    private bool Equals(TradeOffer? other)
    {
        return other != null &&
            Item.Equals(other.Item) &&
            Price == other.Price &&
            SourceId == other.SourceId &&
            Link == other.Link;
    }
}
