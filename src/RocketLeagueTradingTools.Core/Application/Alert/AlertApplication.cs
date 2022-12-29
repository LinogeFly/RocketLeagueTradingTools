using MediatR;
using RocketLeagueTradingTools.Core.Application.Interfaces.Persistence;

namespace RocketLeagueTradingTools.Core.Application.Alert;

public class AlertApplication
{
    private readonly IAlertPersistenceRepository persistence;
    private readonly IMediator mediator;

    public AlertApplication(IAlertPersistenceRepository persistence, IMediator mediator)
    {
        this.persistence = persistence;
        this.mediator = mediator;
    }

    public async Task<IList<Domain.Entities.Alert>> GetAlerts()
    {
        return await persistence.GetAlerts();
    }

    public async Task AddAlert(Domain.Entities.Alert alert)
    {
        await persistence.AddAlert(alert);
        await mediator.Publish(new AlertUpdateEvent());
    }

    public async Task UpdateAlert(Domain.Entities.Alert alert)
    {
        await persistence.UpdateAlert(alert);
        await mediator.Publish(new AlertUpdateEvent());
    }

    public async Task DeleteAlert(int id)
    {
        await persistence.DeleteAlert(id);
        await mediator.Publish(new AlertUpdateEvent());
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
