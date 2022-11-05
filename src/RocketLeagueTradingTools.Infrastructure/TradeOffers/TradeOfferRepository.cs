using RocketLeagueTradingTools.Core.Application.Interfaces;
using RocketLeagueTradingTools.Core.Domain.Entities;

namespace RocketLeagueTradingTools.Infrastructure.TradeOffers;

public class TradeOfferRepository : ITradeOfferRepository
{
    private readonly RlgDataSource rlgDataSource;

    public TradeOfferRepository(RlgDataSource rlgDataSource)
    {
        this.rlgDataSource = rlgDataSource;
    }

    public async Task<TradeOffersPage> GetTradeOffersPage(CancellationToken cancellationToken)
    {
        return await rlgDataSource.GetTradeOffersPage(cancellationToken);
    }
}