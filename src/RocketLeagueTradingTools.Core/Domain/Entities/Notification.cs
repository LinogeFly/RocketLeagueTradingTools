namespace RocketLeagueTradingTools.Core.Domain.Entities;

public sealed class Notification
{
    public int Id { get; set; }
    public ScrapedTradeOffer ScrapedTradeOffer { get; }
    public DateTime? SeenDate { get; set; }

    public Notification(ScrapedTradeOffer tradeOffer)
    {
        ScrapedTradeOffer = tradeOffer;
    }
}