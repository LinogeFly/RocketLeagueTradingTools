using RocketLeagueTradingTools.Core.Application.Contracts;
using RocketLeagueTradingTools.Core.Domain.Entities;

namespace RocketLeagueTradingTools.Core.Application;

public class AlertApplication
{
    private readonly ILog log;
    private readonly IPersistenceRepository persistence;

    public AlertApplication(
        ILog log,
        IPersistenceRepository persistence)
    {
        this.log = log;
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

    public async Task UpdateAlertEnabledState(int id, bool isDisabled)
    {
        var alert = await persistence.GetAlert(id);

        if (alert == null)
            throw new InvalidOperationException("Alert not found.");

        alert.Disabled = isDisabled;

        await persistence.UpdateAlert(alert);
    }
}
