using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using RocketLeagueTradingTools.Core.Application;
using RocketLeagueTradingTools.Core.Domain.Entities;
using RocketLeagueTradingTools.Web.Models.Blacklist;

namespace RocketLeagueTradingTools.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BlacklistController : ControllerBase
{
    private readonly BlacklistApplication app;
    private readonly IMapper mapper;

    public BlacklistController(
        BlacklistApplication app,
        IMapper mapper)
    {
        this.app = app;
        this.mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IList<BlacklistedTraderDto>>> GetBlacklistedTraders()
    {
        var traders = await app.GetTraders();

        return Ok(mapper.Map<List<BlacklistedTraderDto>>(traders));
    }

    [HttpPost]
    public async Task<ActionResult> AddBlacklistedTrader(BlacklistedTraderRequest blacklistedTraderRequest)
    {
        var trader = mapper.Map<BlacklistedTrader>(blacklistedTraderRequest);

        await app.AddTrader(trader);

        return Ok();
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeleteBlacklistedTrader(int id)
    {
        await app.DeleteTrader(id);

        return Ok();
    }
}
