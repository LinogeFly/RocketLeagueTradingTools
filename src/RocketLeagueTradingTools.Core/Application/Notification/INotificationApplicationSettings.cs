namespace RocketLeagueTradingTools.Core.Application.Notification;

public interface INotificationApplicationSettings
{
    TimeSpan AlertOfferMaxAge { get; }
    TimeSpan NotificationsExpiration { get; }
}