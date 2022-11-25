using RocketLeagueTradingTools.Core.Domain.Entities;

namespace RocketLeagueTradingTools.Core.Application.Interfaces;

public interface ITradeOfferRepository
{
    Task<IList<ScrapedTradeOffer>> GetTradeOffersPage(CancellationToken cancellationToken);
}