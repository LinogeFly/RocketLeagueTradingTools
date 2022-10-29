using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using RocketLeagueTradingTools.Core.Application;
using RocketLeagueTradingTools.Web.Models.Notification;

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
    public async Task<ActionResult<IList<NotificationDto>>> Get()
    {
        await notificationApplication.RefreshNotifications();

        var notifications = await notificationApplication.GetNotifications();

        return Ok(mapper.Map<List<NotificationDto>>(notifications));
    }

    [HttpPatch("{id:int}")]
    public async Task<ActionResult> Patch(int id, NotificationPatchRequest request)
    {
        if (request.MarkAsSeen != null && request.MarkAsSeen == true)
            await notificationApplication.MarkNotificationAsSeen(id);

        return Ok();
    }
}
