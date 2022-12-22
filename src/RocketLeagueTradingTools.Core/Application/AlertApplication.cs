using RocketLeagueTradingTools.Core.Application.Interfaces.Persistence;
using RocketLeagueTradingTools.Core.Domain.Entities;

namespace RocketLeagueTradingTools.Core.Application;

public class AlertApplication
{
    private readonly IAlertPersistenceRepository persistence;

    public AlertApplication(IAlertPersistenceRepository persistence)
    {
        this.persistence = persistence;
    }

    public async Task<IList<Alert>> GetAlerts()
    {
        return await persistence.GetAlerts();
    }

    public async Task AddAlert(Alert alert)
    {
        await persistence.AddAlert(alert);
    }

    public async Task UpdateAlert(Alert alert)
    {
        await persistence.UpdateAlert(alert);
    }

    public async Task DeleteAlert(int id)
    {
        await persistence.DeleteAlert(id);
    }

    public async Task UpdateAlertEnabledState(int id, bool isEnabled)
    {
        var alert = await persistence.GetAlert(id);

        if (alert == null)
            throw new InvalidOperationException("Alert not found.");

        alert.Enabled = isEnabled;

        await persistence.UpdateAlert(alert);
    }
}
