using RocketLeagueTradingTools.Core.Application.Interfaces;

namespace RocketLeagueTradingTools.Core.Application;

public class DataRetentionApplication
{
    private readonly IPersistenceRepository persistence;

    public DataRetentionApplication(IPersistenceRepository persistence)
    {
        this.persistence = persistence;
    }
    public async Task DeleteOldTradeOffers(TimeSpan maxAge)
    {
        await persistence.DeleteOldOffers(maxAge);
    }

    public async Task DeleteOldNotifications(TimeSpan maxAge)
    {
        await persistence.DeleteOldNotifications(maxAge);
    }
}