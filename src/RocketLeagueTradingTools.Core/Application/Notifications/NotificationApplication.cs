using RocketLeagueTradingTools.Core.Application.Interfaces;
using RocketLeagueTradingTools.Core.Domain.Entities;

namespace RocketLeagueTradingTools.Core.Application.Notifications;

public class NotificationApplication
{
    private readonly IPersistenceRepository persistence;
    private readonly INotificationApplicationSettings config;

    public NotificationApplication(
        IPersistenceRepository persistence,
        INotificationApplicationSettings config)
    {
        this.persistence = persistence;
        this.config = config;
    }

    public async Task<IList<Notification>> GetNotifications(int pageSize)
    {
        return await persistence.GetNotifications(pageSize);
    }
    
    public async Task<int> GetNotificationsCount()
    {
        return await persistence.GetNotificationsCount();
    }

    public async Task RefreshNotifications()
    {
        var oldNotifications = await persistence.GetNotifications(config.NotificationsExpiration);
        var alertMatchingOfferNotifications = await GetAlertMatchingOfferNotifications();
        var newNotifications = alertMatchingOfferNotifications
            .ExceptBy(oldNotifications.Select(o => o.ScrapedTradeOffer.TradeOffer), n => n.ScrapedTradeOffer.TradeOffer)
            .ToList();

        if (newNotifications.Count > 0)
            await persistence.AddNotifications(newNotifications);
    }

    public async Task MarkNotificationAsSeen(int id)
    {
        await persistence.MarkNotificationAsSeen(id);
    }
    
    public async Task MarkAllNotificationsAsSeen()
    {
        await persistence.MarkAllNotificationAsSeen();
    }
    
    private async Task<IEnumerable<Notification>> GetAlertMatchingOfferNotifications()
    {
        var offers = await persistence.FindAlertMatchingTradeOffers(config.AlertOfferMaxAge);

        return offers.Select(offer => new Notification(offer));
    }
}
