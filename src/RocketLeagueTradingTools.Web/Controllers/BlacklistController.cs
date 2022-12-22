using Microsoft.AspNetCore.Mvc;
using RocketLeagueTradingTools.Core.Application;
using RocketLeagueTradingTools.Web.Contracts.Blacklist;
using RocketLeagueTradingTools.Web.Mapping;

namespace RocketLeagueTradingTools.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BlacklistController : ControllerBase
{
    private readonly BlacklistApplication app;

    public BlacklistController(BlacklistApplication app)
    {
        this.app = app;
    }

    [HttpGet]
    public async Task<ActionResult<IList<BlacklistedTraderResponse>>> GetBlacklistedTraders()
    {
        var traders = await app.GetTraders();
        var response = traders.Select(DomainToDtoMapper.Map);

        return Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult> AddBlacklistedTrader(BlacklistedTraderRequest blacklistedTraderRequest)
    {
        var trader = DtoToDomainMapper.Map(blacklistedTraderRequest);

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