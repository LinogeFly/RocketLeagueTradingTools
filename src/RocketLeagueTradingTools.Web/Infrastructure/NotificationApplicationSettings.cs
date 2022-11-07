using RocketLeagueTradingTools.Common;
using RocketLeagueTradingTools.Infrastructure.Common;

namespace RocketLeagueTradingTools.Web.Infrastructure;

class NotificationApplicationSettings : Core.Application.Notifications.INotificationApplicationSettings
{
    private readonly IConfiguration config;

    public NotificationApplicationSettings(IConfiguration config)
    {
        this.config = config;
    }

    public TimeSpan AlertOfferMaxAge => config.GetRequiredValue<string>("NotificationsSettings:AlertOfferMaxAge").ToTimeSpan();

    public TimeSpan NotificationsExpiration => config.GetValue<string>("NotificationsSettings:NotificationsExpiration", "12:00:00").ToTimeSpan();
}