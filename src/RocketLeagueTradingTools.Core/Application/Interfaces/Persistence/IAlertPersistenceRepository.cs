using RocketLeagueTradingTools.Core.Domain.Entities;

namespace RocketLeagueTradingTools.Core.Application.Interfaces.Persistence;

public interface IAlertPersistenceRepository
{
    Task<IList<Alert>> GetAlerts();
    Task<Alert?> GetAlert(int id);
    Task<int> AddAlert(Alert alert);
    Task UpdateAlert(Alert alert);
    Task DeleteAlert(int id);
}