using RocketLeagueTradingTools.Core.Application.Contracts;
using RocketLeagueTradingTools.Core.Domain.Entities;

namespace RocketLeagueTradingTools.Core.Application;

public class NotificationApplication
{
    private readonly ILog log;
    private readonly IPersistenceRepository persistence;
    private readonly IDateTime dateTime;
    private readonly IConfiguration config;

    public NotificationApplication(
        IPersistenceRepository persistence,
        IDateTime dateTime,
        IConfiguration config,
        ILog log)
    {
        this.persistence = persistence;
        this.dateTime = dateTime;
        this.config = config;
        this.log = log;
    }

    public async Task<IList<Notification>> GetNotifications()
    {
        return await persistence.GetNotifications(config.NotificationsPageSize);
    }

    public async Task RefreshNotifications()
    {
        var oldNotifications = await persistence.GetNotifications(TimeSpan.FromHours(config.NotificationsExpirationInHours));
        var alertMatchingOfferNotifications = await GetAlertMatchingOfferNotifications();
        var newNotifications = alertMatchingOfferNotifications.Except(oldNotifications).ToList();

        if (newNotifications.Count > 0)
            await persistence.AddNotifications(newNotifications);
    }

    public async Task MarkNotificationAsSeen(int id)
    {
        await persistence.MarkNotificationAsSeen(id);
    }

    private async Task<IEnumerable<Notification>> GetAlertMatchingOfferNotifications()
    {
        var offers = await persistence.FindAlertMatchingOffers(config.AlertOfferMaxAgeInMinutes);

        return offers.Select(offer => new Notification(offer));
    }
}