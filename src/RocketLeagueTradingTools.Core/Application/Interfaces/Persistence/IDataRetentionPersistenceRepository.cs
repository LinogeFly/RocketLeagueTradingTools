namespace RocketLeagueTradingTools.Core.Application.Interfaces.Persistence;

public interface IDataRetentionPersistenceRepository
{
    Task DeleteOldTradeOffers(TimeSpan maxAge);
    Task DeleteOldNotifications(TimeSpan maxAge);
    Task Vacuum();
}