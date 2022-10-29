namespace RocketLeagueTradingTools.Core.Domain.Entities;

public sealed class TradeOffer
{
    // Todo: Many of these properties are mandatory. Move them to the constructor and make
    // a builder helper to build the object in a readable way.
    public string SourceId { get; set; } = "";
    public string Link { get; set; } = "";
    public TradeItem Item { get; set; }
    public int Price { get; set; }
    public DateTime ScrapedDate { get; set; }

    public TradeOffer(TradeItem item)
    {
        Item = item;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as TradeOffer);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(SourceId, Link, Item, Price);
    }

    private bool Equals(TradeOffer? other)
    {
        return other != null &&
            SourceId == other.SourceId &&
            Link == other.Link &&
            Item.Equals(other.Item) &&
            Price == other.Price;
    }
}
