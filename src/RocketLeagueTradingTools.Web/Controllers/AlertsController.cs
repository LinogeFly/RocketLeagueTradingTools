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
    private readonly AlertApplication alertApplication;
    private readonly IMapper mapper;

    public AlertsController(
        AlertApplication alertApplication,
        IMapper mapper)
    {
        this.alertApplication = alertApplication;
        this.mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IList<AlertDto>>> Get()
    {
        var alerts = await alertApplication.GetAlerts();

        return Ok(mapper.Map<List<AlertDto>>(alerts));
    }

    [HttpPost]
    public async Task<ActionResult> Post(AlertRequest alertRequest)
    {
        var alert = mapper.Map<Alert>(alertRequest);

        await alertApplication.AddAlert(alert);

        return Ok();
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> Put(int id, AlertRequest alertRequest)
    {
        var alert = mapper.Map<Alert>(alertRequest);
        alert.Id = id;

        await alertApplication.UpdateAlert(alert);

        return Ok();
    }

    [HttpPatch("{id:int}")]
    public async Task<ActionResult> Patch(int id, AlertPatchRequest request)
    {
        if (request.Enabled != null)
            await alertApplication.UpdateAlertEnabledState(id, request.Enabled.Value);

        return Ok();
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id)
    {
        await alertApplication.DeleteAlert(id);

        return Ok();
    }
}
