using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using RocketLeagueTradingTools.Web.Models.Notification;
using RocketLeagueTradingTools.Core.Application.Notifications;

namespace RocketLeagueTradingTools.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly NotificationApplication app;
    private readonly IMapper mapper;

    public NotificationsController(
        NotificationApplication app,
        IMapper mapper)
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
            Items = mapper.Map<List<NotificationDto>>(notifications),
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
