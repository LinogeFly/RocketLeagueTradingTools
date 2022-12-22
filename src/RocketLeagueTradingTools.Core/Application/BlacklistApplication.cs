using RocketLeagueTradingTools.Core.Application.Interfaces.Persistence;
using RocketLeagueTradingTools.Core.Domain.Entities;
using RocketLeagueTradingTools.Core.Domain.ValueObjects;

namespace RocketLeagueTradingTools.Core.Application;

public class BlacklistApplication
{
    private readonly IBlacklistPersistenceRepository persistence;

    public BlacklistApplication(IBlacklistPersistenceRepository persistence)
    {
        this.persistence = persistence;
    }

    public async Task<IList<BlacklistedTrader>> GetTraders()
    {
        return await persistence.GetBlacklistedTraders();
    }

    public async Task AddTrader(Trader trader)
    {
        await persistence.AddBlacklistedTrader(trader);
    }

    public async Task DeleteTrader(int id)
    {
        await persistence.DeleteBlacklistedTrader(id);
    }
}
