using RocketLeagueTradingTools.Core.Domain.Entities;

namespace RocketLeagueTradingTools.Core.Application.Notifications;

class TradeOfferEqualityComparer : IEqualityComparer<Notification>
{
    public bool Equals(Notification? x, Notification? y)
    {
        if (x == null && y == null)
            return true;
        if (x == null || y == null)
            return false;

        return x.TradeOffer.Equals(y.TradeOffer);
    }

    public int GetHashCode(Notification obj)
    {
        return HashCode.Combine(obj.TradeOffer);
    }
}