using RocketLeagueTradingTools.Core.Application.Interfaces;
using RocketLeagueTradingTools.Core.Application.Interfaces.Persistence;

namespace RocketLeagueTradingTools.Core.Application.Notification;

public class NotificationApplication
{
    private readonly INotificationPersistenceRepository notificationPersistence;
    private readonly ITradeOfferPersistenceRepository tradeOfferPersistence;
    private readonly INotificationApplicationSettings config;
    private readonly INotificationSessionStorage sessionStorage;
    private readonly IDateTime dateTime;

    public NotificationApplication(
        INotificationPersistenceRepository notificationPersistence,
        ITradeOfferPersistenceRepository tradeOfferPersistence,
        INotificationApplicationSettings config,
        INotificationSessionStorage sessionStorage,
        IDateTime dateTime)
    {
        this.notificationPersistence = notificationPersistence;
        this.tradeOfferPersistence = tradeOfferPersistence;
        this.config = config;
        this.sessionStorage = sessionStorage;
        this.dateTime = dateTime;
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
        var refreshStarted = dateTime.Now;
        
        var oldNotifications = await notificationPersistence.GetNotifications(config.NotificationsExpiration);
        var alertMatchingOfferNotifications = await GetAlertMatchingOfferNotifications(refreshStarted);
        var newNotifications = alertMatchingOfferNotifications
            .ExceptBy(oldNotifications.Select(o => o.ScrapedTradeOffer.TradeOffer), n => n.ScrapedTradeOffer.TradeOffer)
            .ToList();

        if (newNotifications.Count > 0)
            await notificationPersistence.AddNotifications(newNotifications);

        sessionStorage.LastRefresh = refreshStarted;
    }

    public async Task MarkNotificationAsSeen(int id)
    {
        await notificationPersistence.MarkNotificationAsSeen(id);
    }
    
    public async Task MarkAllNotificationsAsSeen()
    {
        await notificationPersistence.MarkAllNotificationAsSeen();
    }
    
    private async Task<IEnumerable<Domain.Entities.Notification>> GetAlertMatchingOfferNotifications(DateTime refreshStarted)
    {
        var maxAge = sessionStorage.LastRefresh is null ? config.AlertOfferMaxAge : refreshStarted - sessionStorage.LastRefresh.Value;
        var offers = await tradeOfferPersistence.FindAlertMatchingTradeOffers(maxAge);

        return offers.Select(offer => new Domain.Entities.Notification(offer));
    }
}
