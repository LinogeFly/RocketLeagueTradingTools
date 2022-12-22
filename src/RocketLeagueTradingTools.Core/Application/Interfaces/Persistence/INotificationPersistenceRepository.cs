namespace RocketLeagueTradingTools.Core.Application.Interfaces.Persistence;

public interface INotificationPersistenceRepository
{
    Task<IList<Domain.Entities.Notification>> GetNotifications(int pageSize);
    Task<IList<Domain.Entities.Notification>> GetNotifications(TimeSpan notOlderThan);
    Task<int> GetNotificationsCount();
    Task AddNotifications(IList<Domain.Entities.Notification> notifications);
    Task MarkNotificationAsSeen(int id);
    Task MarkAllNotificationAsSeen();
    Task DeleteOldNotifications(TimeSpan maxAge);
}