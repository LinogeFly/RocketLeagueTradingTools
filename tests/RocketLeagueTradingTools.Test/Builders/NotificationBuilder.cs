using RocketLeagueTradingTools.Core.Domain.Entities;

namespace RocketLeagueTradingTools.Test.Builders;

public class NotificationBuilder
{
    private ScrapedTradeOffer tradeOffer = null!;

    public NotificationBuilder WithScrapedOffer(ScrapedTradeOfferBuilder tradeOfferBuilder)
    {
        this.tradeOffer = tradeOfferBuilder.Build();

        return this;
    }
    
    public NotificationBuilder WithScrapedOffer(ScrapedTradeOffer tradeOffer)
    {
        this.tradeOffer = tradeOffer;

        return this;
    }

    public Notification Build()
    {
        ArgumentNullException.ThrowIfNull(tradeOffer);

        return new Notification(tradeOffer);
    }
}