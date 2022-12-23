using RocketLeagueTradingTools.Core.Domain.Entities;

namespace RocketLeagueTradingTools.Core.Application.Interfaces.Persistence;

public interface ITradeOfferPersistenceRepository
{
    Task AddTradeOffers(IList<ScrapedTradeOffer> offers);
    Task<IList<ScrapedTradeOffer>> FindAlertMatchingTradeOffers(TimeSpan alertOfferMaxAge);
}