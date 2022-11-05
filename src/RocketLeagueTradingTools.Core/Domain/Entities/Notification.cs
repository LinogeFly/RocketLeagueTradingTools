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
}