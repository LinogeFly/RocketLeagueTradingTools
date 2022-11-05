namespace RocketLeagueTradingTools.Core.Application.Notifications;

public interface INotificationApplicationSettings
{
    TimeSpan AlertOfferMaxAge { get; }
    TimeSpan NotificationsExpiration { get; }
}