namespace RocketLeagueTradingTools.Core.Domain.Entities;

public sealed class TradeOffer
{
    public TradeItem Item { get; }
    public int Price { get; }
    public DateTime ScrapedDate { get; }
    public string SourceId { get; }
    public string Link { get; }

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
