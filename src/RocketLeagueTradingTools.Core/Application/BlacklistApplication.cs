using RocketLeagueTradingTools.Core.Application.Interfaces;
using RocketLeagueTradingTools.Core.Domain.ValueObjects;

namespace RocketLeagueTradingTools.Core.Application;

public class BlacklistApplication
{
    private readonly IPersistenceRepository persistence;

    public BlacklistApplication(IPersistenceRepository persistence)
    {
        this.persistence = persistence;
    }

    public async Task<IList<Trader>> GetTraders()
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
