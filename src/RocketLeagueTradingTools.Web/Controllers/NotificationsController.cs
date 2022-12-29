using Microsoft.AspNetCore.Mvc;
using RocketLeagueTradingTools.Core.Application.Notification;
using RocketLeagueTradingTools.Web.Contracts.Notification;
using RocketLeagueTradingTools.Web.Mapping;

namespace RocketLeagueTradingTools.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly NotificationApplication app;
    private readonly DomainToDtoMapper mapper;
    private static readonly SemaphoreSlim RefreshLock = new(1, 1);

    public NotificationsController(
        NotificationApplication app,
        DomainToDtoMapper mapper)
    {
        this.app = app;
        this.mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<NotificationsResponse>> Get(int pageSize = 20)
    {
        // Todo: Refreshing should not be done every time when GET request is made.
        // Instead, it should be a background process constantly running and refreshing notifications with an interval.
        // Look into making background tasks with hosted services in ASP.NET Core.
        await RefreshNotifications();

        var total = await app.GetNotificationsCount();
        var notifications = await app.GetNotifications(pageSize);
        var result = new NotificationsResponse
        {
            Items = mapper.Map(notifications),
            Total = total
        };

        return Ok(result);
    }

    [HttpPatch("{id:int}")]
    public async Task<ActionResult> Patch(int id, NotificationPatchRequest request)
    {
        if (request.MarkAsSeen is true)
            await app.MarkNotificationAsSeen(id);

        return Ok();
    }

    [HttpPost("mark-all-as-seen")]
    public async Task<ActionResult> MarkAllAsSeen()
    {
        await app.MarkAllNotificationsAsSeen();

        return Ok();
    }

    private async Task RefreshNotifications()
    {
        // Locking the refresh operation, so only one thread can refresh notifications at a time.
        // This is to prevent duplicate notifications creation which can happen if multiple threads trigger the refresh.
        await RefreshLock.WaitAsync();

        try
        {
            await app.RefreshNotifications();
        }
        finally
        {
            RefreshLock.Release();
        }
    }
}