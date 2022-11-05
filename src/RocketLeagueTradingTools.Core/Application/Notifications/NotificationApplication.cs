using RocketLeagueTradingTools.Core.Application.Interfaces;
using RocketLeagueTradingTools.Core.Domain.Entities;

namespace RocketLeagueTradingTools.Core.Application.Notifications;

public class NotificationApplication
{
    private readonly ILog log;
    private readonly IPersistenceRepository persistence;
    private readonly IDateTime dateTime;
    private readonly INotificationApplicationSettings config;

    public NotificationApplication(
        IPersistenceRepository persistence,
        IDateTime dateTime,
        INotificationApplicationSettings config,
        ILog log)
    {
        this.persistence = persistence;
        this.dateTime = dateTime;
        this.config = config;
        this.log = log;
    }

    public async Task<IList<Notification>> GetNotifications(int pageSize)
    {
        return await persistence.GetNotifications(pageSize);
    }

    public async Task RefreshNotifications()
    {
        var oldNotifications = await persistence.GetNotifications(config.NotificationsExpiration);
        var alertMatchingOfferNotifications = await GetAlertMatchingOfferNotifications();
        var newNotifications = alertMatchingOfferNotifications.Except(oldNotifications, new TradeOfferEqualityComparer()).ToList();

        if (newNotifications.Count > 0)
            await persistence.AddNotifications(newNotifications);
    }

    public async Task MarkNotificationAsSeen(int id)
    {
        await persistence.MarkNotificationAsSeen(id);
    }

    private async Task<IEnumerable<Notification>> GetAlertMatchingOfferNotifications()
    {
        var offers = await persistence.FindAlertMatchingOffers(config.AlertOfferMaxAge);

        return offers.Select(offer => new Notification(offer));
    }
}
