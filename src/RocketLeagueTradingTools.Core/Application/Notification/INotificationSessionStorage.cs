namespace RocketLeagueTradingTools.Core.Application.Notification;

public interface INotificationSessionStorage
{
    DateTime? LastRefresh { get; set; }
}