namespace RocketLeagueTradingTools.Core.Domain.Entities;

public sealed class TradeOffersPage
{
    public IList<TradeOffer> BuyOffers { get; init; } = new List<TradeOffer>();
    public IList<TradeOffer> SellOffers { get; init; } = new List<TradeOffer>();

    public override bool Equals(object? obj)
    {
        return Equals(obj as TradeOffersPage);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(BuyOffers, SellOffers);
    }

    private bool Equals(TradeOffersPage? other)
    {
        return other != null &&
            BuyOffers.Equals(other.BuyOffers) &&
            SellOffers.Equals(other.SellOffers);
    }
}
