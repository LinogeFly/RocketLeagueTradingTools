using Microsoft.AspNetCore.Mvc;
using RocketLeagueTradingTools.Core.Application.Alert;
using RocketLeagueTradingTools.Web.Contracts.Alert;
using RocketLeagueTradingTools.Web.Mapping;

namespace RocketLeagueTradingTools.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AlertsController : ControllerBase
{
    private readonly AlertApplication app;

    public AlertsController(AlertApplication app)
    {
        this.app = app;
    }

    [HttpGet]
    public async Task<ActionResult<IList<AlertResponse>>> Get()
    {
        var alerts = await app.GetAlerts();
        var response = alerts.Select(DomainToDtoMapper.Map);

        return Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult> Post(AlertUpsertRequest request)
    {
        var alert = DtoToDomainMapper.Map(request);

        await app.AddAlert(alert);

        return Ok();
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> Put(int id, AlertUpsertRequest request)
    {
        var alert = DtoToDomainMapper.Map(request);
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