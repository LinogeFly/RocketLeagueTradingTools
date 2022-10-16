namespace RocketLeagueTradingTools.Core.Domain.Entities;

public sealed class TradeOffer
{
    public string SourceId { get; set; } = "";
    public string Link { get; set; } = "";
    public TradeItem Item { get; set; } = null!;
    public int Price { get; set; }

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
