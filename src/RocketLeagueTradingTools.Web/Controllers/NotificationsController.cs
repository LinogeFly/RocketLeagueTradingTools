using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using RocketLeagueTradingTools.Web.Models.Notification;
using RocketLeagueTradingTools.Core.Application.Notifications;

namespace RocketLeagueTradingTools.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly NotificationApplication notificationApplication;
    private readonly IMapper mapper;

    public NotificationsController(
        NotificationApplication notificationApplication,
        IMapper mapper)
    {
        this.notificationApplication = notificationApplication;
        this.mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IList<NotificationDto>>> Get(int pageSize = 20)
    {
        await notificationApplication.RefreshNotifications();

        var notifications = await notificationApplication.GetNotifications(pageSize);

        return Ok(mapper.Map<List<NotificationDto>>(notifications));
    }

    [HttpPatch("{id:int}")]
    public async Task<ActionResult> Patch(int id, NotificationPatchRequest request)
    {
        if (request.MarkAsSeen is true)
            await notificationApplication.MarkAsSeen(id);

        return Ok();
    }
    
    [HttpPost("mark-all-as-seen")]
    public async Task<ActionResult> MarkAllAsSeen()
    {
        await notificationApplication.MarkAllAsSeen();

        return Ok();
    }
}
