using RocketLeagueTradingTools.Core.Domain.Entities;
using RocketLeagueTradingTools.Core.Domain.ValueObjects;

namespace RocketLeagueTradingTools.Core.Application.Interfaces;

public interface IPersistenceRepository
{
    Task AddTradeOffers(IList<ScrapedTradeOffer> offers);
    Task<IList<ScrapedTradeOffer>> FindAlertMatchingOffers(TimeSpan alertOfferMaxAge);
    Task DeleteOldOffers(TimeSpan maxAge);

    Task<IList<Alert>> GetAlerts();
    Task<Alert?> GetAlert(int id);
    Task AddAlert(Alert alert);
    Task UpdateAlert(Alert alert);
    Task DeleteAlert(int id);

    Task<IList<Notification>> GetNotifications(int pageSize);
    Task<IList<Notification>> GetNotifications(TimeSpan notOlderThan);
    Task AddNotifications(IList<Notification> notifications);
    Task MarkNotificationAsSeen(int id);
    Task MarkAllNotificationAsSeen();
    Task DeleteOldNotifications(TimeSpan maxAge);
    
    Task<IList<Trader>> GetBlacklistedTraders();
    Task AddBlacklistedTrader(Trader trader);
    Task DeleteBlacklistedTrader(int id);
}