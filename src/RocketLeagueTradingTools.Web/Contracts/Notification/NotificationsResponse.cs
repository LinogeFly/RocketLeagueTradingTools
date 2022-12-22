namespace RocketLeagueTradingTools.Web.Contracts.Notification;

public record NotificationsResponse
{
    public IList<NotificationDto> Items { get; init; } = new List<NotificationDto>();
    public int Total { get; init; }
}