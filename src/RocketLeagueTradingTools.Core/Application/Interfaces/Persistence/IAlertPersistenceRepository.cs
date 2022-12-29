namespace RocketLeagueTradingTools.Core.Application.Interfaces.Persistence;

public interface IAlertPersistenceRepository
{
    Task<IList<Domain.Entities.Alert>> GetAlerts();
    Task<Domain.Entities.Alert?> GetAlert(int id);
    Task<int> AddAlert(Domain.Entities.Alert alert);
    Task UpdateAlert(Domain.Entities.Alert alert);
    Task DeleteAlert(int id);
}