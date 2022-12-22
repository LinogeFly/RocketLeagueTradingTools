using RocketLeagueTradingTools.Core.Application.Interfaces.Persistence;

namespace RocketLeagueTradingTools.Core.Application.DataRetention;

public class DataRetentionApplication
{
    private readonly ITradeOfferPersistenceRepository tradeOfferPersistence;
    private readonly INotificationPersistenceRepository notificationPersistence;
    private readonly IDataRetentionApplicationSettings config;

    public DataRetentionApplication(
        INotificationPersistenceRepository notificationPersistence,
        ITradeOfferPersistenceRepository tradeOfferPersistence,
        IDataRetentionApplicationSettings config)
    {
        this.tradeOfferPersistence = tradeOfferPersistence;
        this.notificationPersistence = notificationPersistence;
        this.config = config;
    }
    
    public async Task DeleteOldData()
    {
        if (config.NotificationsMaxAge != null)
            await notificationPersistence.DeleteOldNotifications(config.NotificationsMaxAge.Value);
        
        await tradeOfferPersistence.DeleteOldTradeOffers(config.TradeOffersMaxAge);
    }
}