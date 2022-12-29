using RocketLeagueTradingTools.Core.Application.Notification;

namespace RocketLeagueTradingTools.Web.Infrastructure;

class NotificationSessionStorage : INotificationSessionStorage
{
    public DateTime? LastRefresh { get; set; }
}