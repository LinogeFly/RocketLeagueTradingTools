using RocketLeagueTradingTools.Common;
using RocketLeagueTradingTools.Core.Domain.Enumerations;

namespace RocketLeagueTradingTools.Core.Domain.Entities;

public sealed class TradeOffer
{
    public TradeItem Item { get; }
    public int Price { get; }
    public DateTime ScrapedDate { get; }
    public string Link { get; }
    public TradingSite TradingSite { get; }
    public string TraderName { get; }

    public TradeOffer(
        TradeItem item,
        int price,
        DateTime scrapedDate, 
        string link,
        TradingSite tradingSite,
        string traderName)
    {
        if (price <= 0)
            throw new ArgumentException($"{nameof(price)} has to be more than 0.");
        
        if (scrapedDate == DateTime.MinValue)
            throw new ArgumentException($"{nameof(scrapedDate)} is required.");
        
        if (link.IsEmpty())
            throw new ArgumentException($"{nameof(link)} is required.");
        
        if (traderName.IsEmpty())
            throw new ArgumentException($"{nameof(traderName)} is required.");
        
        Item = item;
        Price = price;
        ScrapedDate = scrapedDate;
        Link = link;
        TradingSite = tradingSite;
        TraderName = traderName;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as TradeOffer);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Item, Price, Link, TradingSite, TraderName);
    }

    private bool Equals(TradeOffer? other)
    {
        return other != null &&
            Item.Equals(other.Item) &&
            Price == other.Price &&
            Link == other.Link &&
            TradingSite == other.TradingSite &&
            TraderName == other.TraderName;
    }
}
