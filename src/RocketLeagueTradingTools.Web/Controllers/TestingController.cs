using Microsoft.AspNetCore.Mvc;
using RocketLeagueTradingTools.Core.Domain.Entities;
using RocketLeagueTradingTools.Web.Models.Testing;
using RocketLeagueTradingTools.Core.Application.Interfaces;
using RocketLeagueTradingTools.Common;
using AutoMapper;
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

    [HttpPost("offers/seed")]
    public async Task<ActionResult> OffersSeed(TradeOfferRequest request)
    {
        var age = request.Age.ToTimeSpan();
        var offers = new List<TradeOffer>();

        for (int i = 0; i < request.Amount; i++)
        {
            var id = Guid.NewGuid().ToString();
            var offer = new TradeOffer
            (
                Map(request),
                request.Price,
                dateTime.Now.Add(-age),
                id,
                $"https://rocket-league.com/trade/{id}"
            );

            offers.Add(offer);
        }

        if (request.OfferType == OfferTypeDto.Buy)
            await persistence.AddBuyOffers(offers);
        if (request.OfferType == OfferTypeDto.Sell)
            await persistence.AddSellOffers(offers);

        return Ok();
    }

    [HttpPost("alerts/seed")]
    public async Task<ActionResult> AlertsSeed()
    {
        var alerts = new List<Alert>
        {
            new (AlertOfferType.Sell, "Fennec", 300) {Certification = "*"},
            new (AlertOfferType.Sell, "Hellfire", 100) {Certification = "*"},
            new (AlertOfferType.Sell, "Dueling Dragons", 500) {Certification = "*"},
            new (AlertOfferType.Sell, "Dingo", 200) {Certification = "*"},
            new (AlertOfferType.Sell, "Gravity Bomb", 600) {Certification = "*"},
            new (AlertOfferType.Sell, "20xx", 200) {Certification = "*"},
            new (AlertOfferType.Sell, "Supernova III", 60) {Certification = "*"},
            new (AlertOfferType.Sell, "Mainframe", 500) {Certification = "*"},
            new (AlertOfferType.Sell, "Neuro-Agitator", 50) {Certification = "*"},
            new (AlertOfferType.Sell, "Shattered", 60) {Certification = "*", ItemType = AlertItemType.GoalExplosion},
            new (AlertOfferType.Sell, "Encryption", 800) {Certification = "*"},
            new (AlertOfferType.Sell, "Carbonator", 750) {Certification = "*"},
            new (AlertOfferType.Sell, "Dissolver", 500) {Certification = "*"},
            new (AlertOfferType.Sell, "Sub-Zero", 60) {Certification = "*"},
            new (AlertOfferType.Sell, "Singularity", 50) {Certification = "*"},
            new (AlertOfferType.Sell, "Buffy-Sugo", 1100) {Certification = "*"},
            new (AlertOfferType.Sell, "Nomster", 50) {Certification = "*", Color = "Sky Blue", ItemType = AlertItemType.GoalExplosion},
            new (AlertOfferType.Sell, "Ion", 300) {Certification = "*", Color = "Titanium White"},
            new (AlertOfferType.Sell, "Ion", 110) {Certification = "*", Color = "Lime"},
        };

        foreach (var alert in alerts)
        {
            await persistence.AddAlert(alert);
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
}
