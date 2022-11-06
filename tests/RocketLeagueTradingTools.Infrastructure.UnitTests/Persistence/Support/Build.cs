using RocketLeagueTradingTools.Infrastructure.Persistence.Models;

namespace RocketLeagueTradingTools.Infrastructure.UnitTests.Persistence.Support;

static class Build
{
    public static PersistedTradeOffer PersistedOffer(string alertOfferType)
    {
        if (alertOfferType.ToLower() == "buy")
            return Activator.CreateInstance<PersistedBuyOffer>();

        if (alertOfferType.ToLower() == "sell")
            return Activator.CreateInstance<PersistedSellOffer>();

        throw new NotSupportedException($"Invalid offer type '{alertOfferType}'.");
    }

    public static PersistedTradeOffer With(this PersistedTradeOffer offer, Action<PersistedTradeOffer> setup)
    {
        setup(offer);

        return offer;
    }
}