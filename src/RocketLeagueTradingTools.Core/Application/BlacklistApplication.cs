using RocketLeagueTradingTools.Core.Application.Interfaces;
using RocketLeagueTradingTools.Core.Domain.Entities;

namespace RocketLeagueTradingTools.Core.Application;

public class BlacklistApplication
{
    private readonly IPersistenceRepository persistence;

    public BlacklistApplication(IPersistenceRepository persistence)
    {
        this.persistence = persistence;
    }

    public async Task<IList<BlacklistedTrader>> GetTraders()
    {
        return await persistence.GetBlacklistedTraders();
    }

    public async Task AddTrader(BlacklistedTrader trader)
    {
        await persistence.AddBlacklistedTrader(trader);
    }

    public async Task DeleteTrader(int id)
    {
        await persistence.DeleteBlacklistedTrader(id);
    }
}
