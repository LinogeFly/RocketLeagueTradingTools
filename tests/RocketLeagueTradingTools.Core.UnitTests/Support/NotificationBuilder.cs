using RocketLeagueTradingTools.Core.Domain.Entities;

namespace RocketLeagueTradingTools.Core.UnitTests.Support;

internal class NotificationBuilder
{
    private ScrapedTradeOfferBuilder tradeOfferBuilder = null!;

    public NotificationBuilder WithScrapedOffer(ScrapedTradeOfferBuilder tradeOfferBuilder)
    {
        this.tradeOfferBuilder = tradeOfferBuilder;

        return this;
    }

    public Notification Build()
    {
        if (tradeOfferBuilder is null)
            throw new ArgumentException("The field is required.", nameof(tradeOfferBuilder));

        return new Notification(tradeOfferBuilder.Build());
    }
}