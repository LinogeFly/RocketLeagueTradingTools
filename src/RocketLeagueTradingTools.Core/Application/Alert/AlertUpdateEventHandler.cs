using MediatR;
using RocketLeagueTradingTools.Core.Application.Notification;

namespace RocketLeagueTradingTools.Core.Application.Alert;

public class AlertUpdateEventHandler : INotificationHandler<AlertUpdateEvent>
{
    private readonly INotificationSessionStorage sessionStorage;

    public AlertUpdateEventHandler(INotificationSessionStorage sessionStorage)
    {
        this.sessionStorage = sessionStorage;
    }
    public Task Handle(AlertUpdateEvent @event, CancellationToken cancellationToken)
    {
        sessionStorage.LastRefresh = null;

        return Task.CompletedTask;
    }
}
