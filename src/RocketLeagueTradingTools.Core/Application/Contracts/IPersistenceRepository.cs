using RocketLeagueTradingTools.Core.Domain.Entities;

namespace RocketLeagueTradingTools.Core.Application.Contracts;

public interface IPersistenceRepository
{
    Task AddBuyOffers(IList<TradeOffer> offers);
    Task AddSellOffers(IList<TradeOffer> offers);
    Task<IList<TradeOffer>> FindAlertMatchingOffers(int alertOfferMaxAgeInMinutes);

    Task<IList<Alert>> GetAlerts();
    Task<Alert?> GetAlert(int id);
    Task AddAlert(Alert alert);
    Task UpdateAlert(Alert alert);
    Task DeleteAlert(int id);

    Task<IList<Notification>> GetNotifications(int pageSize);
    Task<IList<Notification>> GetNotifications(TimeSpan notOlderThan);
    Task AddNotifications(IList<Notification> notifications);
    Task MarkNotificationAsSeen(int id);
}