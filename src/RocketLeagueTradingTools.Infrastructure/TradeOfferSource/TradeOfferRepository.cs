using RocketLeagueTradingTools.Core.Application.Interfaces;
using RocketLeagueTradingTools.Core.Domain.Entities;

namespace RocketLeagueTradingTools.Infrastructure.TradeOfferSource;

public class TradeOfferRepository : ITradeOfferRepository
{
    private readonly RlgDataSource rlgDataSource;

    public TradeOfferRepository(RlgDataSource rlgDataSource)
    {
        this.rlgDataSource = rlgDataSource;
    }

    public async Task<IList<ScrapedTradeOffer>> GetTradeOffersPage(CancellationToken cancellationToken)
    {
        return await rlgDataSource.GetTradeOffersPage(cancellationToken);
    }
}