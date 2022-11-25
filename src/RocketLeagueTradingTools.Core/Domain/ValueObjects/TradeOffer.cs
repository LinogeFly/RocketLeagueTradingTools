using RocketLeagueTradingTools.Common;
using RocketLeagueTradingTools.Core.Domain.Enumerations;

namespace RocketLeagueTradingTools.Core.Domain.ValueObjects;

public sealed class TradeOffer : ValueObject
{
    public TradeOfferType OfferType { get; }
    public TradeItem Item { get; }
    public int Price { get; }
    public string Link { get; }
    public Trader Trader { get; }

    public TradeOffer(
        TradeOfferType offerType,
        TradeItem item,
        int price,
        string link,
        Trader trader)
    {
        if (price <= 0)
            throw new ArgumentException($"{nameof(price)} has to be more than 0.");
        
        
        if (link.IsEmpty())
            throw new ArgumentException("The field is required.", nameof(link));


        OfferType = offerType;
        Item = item;
        Price = price;
        Link = link;
        Trader = trader;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return OfferType;
        yield return Item;
        yield return Price;
        yield return Link;
        yield return Trader;
    }
}
