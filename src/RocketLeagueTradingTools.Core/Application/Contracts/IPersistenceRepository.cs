using RocketLeagueTradingTools.Core.Domain.Entities;

namespace RocketLeagueTradingTools.Core.Application.Contracts;

public interface IPersistenceRepository
{
    Task AddBuyOffers(IList<TradeOffer> offers);
    Task AddSellOffers(IList<TradeOffer> offers);
    Task<IList<Alert>> GetAlerts();
}