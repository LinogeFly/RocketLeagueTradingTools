using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using RocketLeagueTradingTools.Core.Application;
using RocketLeagueTradingTools.Core.Domain.Entities;
using RocketLeagueTradingTools.Web.Models.Alert;

namespace RocketLeagueTradingTools.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AlertsController : ControllerBase
{
    private readonly AlertApplication app;
    private readonly IMapper mapper;

    public AlertsController(
        AlertApplication app,
        IMapper mapper)
    {
        this.app = app;
        this.mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IList<AlertDto>>> Get()
    {
        var alerts = await app.GetAlerts();

        return Ok(mapper.Map<List<AlertDto>>(alerts));
    }

    [HttpPost]
    public async Task<ActionResult> Post(AlertRequest alertRequest)
    {
        var alert = mapper.Map<Alert>(alertRequest);

        await app.AddAlert(alert);

        return Ok();
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> Put(int id, AlertRequest alertRequest)
    {
        var alert = mapper.Map<Alert>(alertRequest);
        alert.Id = id;

        await app.UpdateAlert(alert);

        return Ok();
    }

    [HttpPatch("{id:int}")]
    public async Task<ActionResult> Patch(int id, AlertPatchRequest request)
    {
        if (request.Enabled != null)
            await app.UpdateAlertEnabledState(id, request.Enabled.Value);

        return Ok();
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id)
    {
        await app.DeleteAlert(id);

        return Ok();
    }
}
