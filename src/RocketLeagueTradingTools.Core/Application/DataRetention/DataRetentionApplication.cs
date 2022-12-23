using RocketLeagueTradingTools.Core.Application.Interfaces.Persistence;

namespace RocketLeagueTradingTools.Core.Application.DataRetention;

public class DataRetentionApplication
{
    private readonly IDataRetentionPersistenceRepository repository;
    private readonly IDataRetentionApplicationSettings config;

    public DataRetentionApplication(
        IDataRetentionApplicationSettings config,
        IDataRetentionPersistenceRepository repository)
    {
        this.config = config;
        this.repository = repository;
    }
    
    public async Task DeleteOldData()
    {
        if (config.NotificationsMaxAge != null)
            await repository.DeleteOldNotifications(config.NotificationsMaxAge.Value);
        
        await repository.DeleteOldTradeOffers(config.TradeOffersMaxAge);

        await repository.Vacuum();
    }
}