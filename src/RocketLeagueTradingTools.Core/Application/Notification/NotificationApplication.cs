using RocketLeagueTradingTools.Core.Application.Interfaces.Persistence;

namespace RocketLeagueTradingTools.Core.Application.Notification;

public class NotificationApplication
{
    private readonly INotificationPersistenceRepository notificationPersistence;
    private readonly ITradeOfferPersistenceRepository tradeOfferPersistence;
    private readonly INotificationApplicationSettings config;

    public NotificationApplication(
        INotificationPersistenceRepository notificationPersistence,
        ITradeOfferPersistenceRepository tradeOfferPersistence,
        INotificationApplicationSettings config)
    {
        this.notificationPersistence = notificationPersistence;
        this.tradeOfferPersistence = tradeOfferPersistence;
        this.config = config;
    }

    public async Task<IList<Domain.Entities.Notification>> GetNotifications(int pageSize)
    {
        return await notificationPersistence.GetNotifications(pageSize);
    }
    
    public async Task<int> GetNotificationsCount()
    {
        return await notificationPersistence.GetNotificationsCount();
    }

    public async Task RefreshNotifications()
    {
        var oldNotifications = await notificationPersistence.GetNotifications(config.NotificationsExpiration);
        var alertMatchingOfferNotifications = await GetAlertMatchingOfferNotifications();
        var newNotifications = alertMatchingOfferNotifications
            .ExceptBy(oldNotifications.Select(o => o.ScrapedTradeOffer.TradeOffer), n => n.ScrapedTradeOffer.TradeOffer)
            .ToList();

        if (newNotifications.Count > 0)
            await notificationPersistence.AddNotifications(newNotifications);
    }

    public async Task MarkNotificationAsSeen(int id)
    {
        await notificationPersistence.MarkNotificationAsSeen(id);
    }
    
    public async Task MarkAllNotificationsAsSeen()
    {
        await notificationPersistence.MarkAllNotificationAsSeen();
    }
    
    private async Task<IEnumerable<Domain.Entities.Notification>> GetAlertMatchingOfferNotifications()
    {
        var offers = await tradeOfferPersistence.FindAlertMatchingTradeOffers(config.AlertOfferMaxAge);

        return offers.Select(offer => new Domain.Entities.Notification(offer));
    }
}
