using RocketLeagueTradingTools.Core.Domain.Entities;

namespace RocketLeagueTradingTools.Core.Application.Contracts;

public interface ITradeOfferRepository
{
    Task<TradeOffersPage> GetTradeOffersPage(CancellationToken cancellationToken);
}