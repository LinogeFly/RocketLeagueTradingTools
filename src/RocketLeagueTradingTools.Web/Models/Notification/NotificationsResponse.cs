namespace RocketLeagueTradingTools.Web.Models.Notification;

public sealed class NotificationsResponse
{
    public IList<NotificationDto> Items { get; set; } = new List<NotificationDto>();
    public int Total { get; set; }
}