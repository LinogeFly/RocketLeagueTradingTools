using Microsoft.AspNetCore.Mvc;
using RocketLeagueTradingTools.Core.Domain.Entities;
using RocketLeagueTradingTools.Web.Models.Testing;
using RocketLeagueTradingTools.Core.Application.Interfaces;
using RocketLeagueTradingTools.Common;
using AutoMapper;
using RocketLeagueTradingTools.Core.Domain.Enumerations;
using RocketLeagueTradingTools.Core.Domain.ValueObjects;
using RocketLeagueTradingTools.Web.Models.Common;

namespace RocketLeagueTradingTools.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestingController : ControllerBase
{
    private readonly IPersistenceRepository persistence;
    private readonly IDateTime dateTime;
    private readonly IMapper mapper;

    public TestingController(
        IPersistenceRepository persistence,
        IDateTime dateTime,
        IMapper mapper)
    {
        this.persistence = persistence;
        this.dateTime = dateTime;
        this.mapper = mapper;
    }

    [HttpPost("offers/clone")]
    public async Task<ActionResult> OffersClone(TradeOfferRequest request)
    {
        var age = request.Age.ToTimeSpan();
        var offers = new List<ScrapedTradeOffer>();

        for (int i = 0; i < request.Amount; i++)
        {
            var id = Guid.NewGuid().ToString();
            var tradeOffer = new TradeOffer
            (
                MapTradeOfferType(request.OfferType),
                Map(request),
                request.Price,
                $"https://rocket-league.com/trade/{id}",
                new Trader(TradingSite.RocketLeagueGarage, nameof(OffersClone))
            );
            var scrapedTradeOffer = new ScrapedTradeOffer(tradeOffer, dateTime.Now.Add(-age));

            offers.Add(scrapedTradeOffer);
        }

        await persistence.AddTradeOffers(offers);

        return Ok();
    }
    
    [HttpPost("alerts/seed")]
    public async Task<ActionResult> AlertsSeed()
    {
        var alerts = new List<Alert>
        {
            new (TradeOfferType.Sell, "Fennec", 300) {Certification = "*"},
            new (TradeOfferType.Sell, "Hellfire", 90) {Certification = "*"},
            new (TradeOfferType.Sell, "Dueling Dragons", 450) {Certification = "*"},
            new (TradeOfferType.Sell, "Dingo", 150) {Certification = "*"},
            new (TradeOfferType.Sell, "Gravity Bomb", 600) {Certification = "*"},
            new (TradeOfferType.Sell, "20xx", 200) {Certification = "*"},
            new (TradeOfferType.Sell, "Supernova III", 60) {Certification = "*"},
            new (TradeOfferType.Sell, "Mainframe", 500) {Certification = "*"},
            new (TradeOfferType.Sell, "Neuro-Agitator", 50) {Certification = "*"},
            new (TradeOfferType.Sell, "Shattered", 60) {Certification = "*", ItemType = AlertItemType.GoalExplosion},
            new (TradeOfferType.Sell, "Encryption", 600) {Certification = "*"},
            new (TradeOfferType.Sell, "Carbonator", 600) {Certification = "*"},
            new (TradeOfferType.Sell, "Dissolver", 500) {Certification = "*"},
            new (TradeOfferType.Sell, "Sub-Zero", 60) {Certification = "*"},
            new (TradeOfferType.Sell, "Singularity", 50) {Certification = "*"},
            new (TradeOfferType.Sell, "Buffy-Sugo", 1100) {Certification = "*"},
            new (TradeOfferType.Sell, "Heatwave", 290) {Certification = "*"},
            new (TradeOfferType.Sell, "Beach Party", 400) {Certification = "*"},
            new (TradeOfferType.Sell, "Nomster", 50) {Certification = "*", Color = "Sky Blue", ItemType = AlertItemType.GoalExplosion},
            new (TradeOfferType.Sell, "Ion", 300) {Certification = "*", Color = "Titanium White"},
            new (TradeOfferType.Sell, "Ion", 110) {Certification = "*", Color = "Lime"},
            new (TradeOfferType.Sell, "Ion", 100) {Certification = "*", Color = "+"},
            new (TradeOfferType.Sell, "Ninja Star", 40) {Certification = "*", Color = "+"},
            new (TradeOfferType.Sell, "Fennec", 1400) {Certification = "*", Color = "Titanium White"},
            new (TradeOfferType.Sell, "Merc", 60) {Certification = "*", Color = "Titanium White"},
            new (TradeOfferType.Sell, "Endo", 200) {Certification = "*", Color = "Titanium White"},
            new (TradeOfferType.Sell, "OEM", 500) {Certification = "*", Color = "Black"}
        };

        foreach (var alert in alerts)
        {
            await persistence.AddAlert(alert);
        }

        return Ok();
    }

    [HttpPost("blacklist/seed")]
    public async Task<ActionResult> BlacklistSeed(int count)
    {
        if (count == 0)
            return BadRequest($"Parameter {nameof(count)} has to be greater than 0.");
        
        for (int i = 0; i < count; i++)
        {
            var traderName = Guid.NewGuid().ToString();
            var trader = new Trader(TradingSite.RocketLeagueGarage, traderName);

            await persistence.AddBlacklistedTrader(trader);
        }

        return Ok();
    }

    private TradeItem Map(TradeOfferRequest request)
    {
        return new TradeItem(request.Name)
        {
            ItemType = mapper.Map<TradeItemType>(request.ItemType),
            Color = request.Color,
            Certification = request.Certification
        };
    }
    
    private TradeOfferType MapTradeOfferType(OfferTypeDto? offerType)
    {
        switch (offerType)
        {
            case OfferTypeDto.Buy:
                return TradeOfferType.Buy;
            case OfferTypeDto.Sell:
                return TradeOfferType.Sell;
            default:
                throw new InvalidOperationException($"Invalid offer type '{offerType}'.");
        }
    }
}
