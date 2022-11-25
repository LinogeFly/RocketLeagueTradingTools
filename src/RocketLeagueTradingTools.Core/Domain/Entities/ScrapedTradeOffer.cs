using RocketLeagueTradingTools.Core.Domain.ValueObjects;

namespace RocketLeagueTradingTools.Core.Domain.Entities;

public class ScrapedTradeOffer
{
    public int Id { get; init; }
    public TradeOffer TradeOffer { get; }
    public DateTime ScrapedDate { get; }

    public ScrapedTradeOffer(TradeOffer tradeOffer, DateTime scrapedDate)
    {
        if (scrapedDate == DateTime.MinValue)
            throw new ArgumentException("The field is required.", nameof(scrapedDate));

        TradeOffer = tradeOffer;
        ScrapedDate = scrapedDate;
    }
}