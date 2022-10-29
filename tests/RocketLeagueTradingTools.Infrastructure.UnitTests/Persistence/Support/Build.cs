using RocketLeagueTradingTools.Infrastructure.Persistence.PersistedTypes;

namespace RocketLeagueTradingTools.Infrastructure.UnitTests.Persistence.Support;

static class Build
{
    public static PersistedTradeOffer PersistedOffer(PersistedAlertOfferType alertOfferType)
    {
        if (alertOfferType == PersistedAlertOfferType.Buy)
            return Activator.CreateInstance<PersistedBuyOffer>();

        if (alertOfferType == PersistedAlertOfferType.Sell)
            return Activator.CreateInstance<PersistedSellOffer>();

        throw new NotSupportedException($"Unknown offer type '{alertOfferType}'.");
    }

    public static PersistedTradeOffer With(this PersistedTradeOffer offer, Action<PersistedTradeOffer> setup)
    {
        setup(offer);

        return offer;
    }
}