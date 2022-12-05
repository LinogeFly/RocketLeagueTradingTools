using RocketLeagueTradingTools.Core.Application.Interfaces;

namespace RocketLeagueTradingTools.Core.Application.DataRetention;

public class DataRetentionApplication
{
    private readonly IPersistenceRepository persistence;
    private readonly IDataRetentionApplicationSettings config;

    public DataRetentionApplication(
        IPersistenceRepository persistence,
        IDataRetentionApplicationSettings config)
    {
        this.persistence = persistence;
        this.config = config;
    }
    
    public async Task DeleteOldData()
    {
        if (config.NotificationsMaxAge != null)
            await persistence.DeleteOldNotifications(config.NotificationsMaxAge.Value);
        
        await persistence.DeleteOldTradeOffers(config.TradeOffersMaxAge);
    }
}