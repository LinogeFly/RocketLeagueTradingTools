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
        await app.RefreshNotifications();

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
}
