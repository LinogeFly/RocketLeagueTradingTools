namespace RocketLeagueTradingTools.Core.Domain.Entities;

public sealed class Notification
{
    public int Id { get; set; }
    public TradeOffer TradeOffer { get; }
    public DateTime? SeenDate { get; set; }

    public Notification(TradeOffer tradeOffer)
    {
        TradeOffer = tradeOffer;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as Notification);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(TradeOffer);
    }

    // Todo: Add custom comparer and use that in NotificationApplication.GetNotifications,
    // instead of having default Equals checking only TradeOffer
    private bool Equals(Notification? other)
    {
        return other != null &&
            TradeOffer.Equals(other.TradeOffer);
    }
}