using RocketLeagueTradingTools.Core.Domain.Entities;

namespace RocketLeagueTradingTools.Core.Application.Interfaces;

public interface ITradeOfferRepository
{
    Task<TradeOffersPage> GetTradeOffersPage(CancellationToken cancellationToken);
}