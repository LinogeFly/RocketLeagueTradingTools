using Microsoft.AspNetCore.Mvc;
using RocketLeagueTradingTools.Core.Domain.Entities;
using RocketLeagueTradingTools.Web.Models.Testing;
using RocketLeagueTradingTools.Core.Application.Interfaces;
using RocketLeagueTradingTools.Core.Common;
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
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

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
            new Alert
            {
                ItemName = "Fennec",
                OfferType = AlertOfferType.Sell,
                Price = new Core.Domain.ValueObjects.PriceRange(300),
                Certification = "*"
            },
            new Alert
            {
                ItemName = "Hellfire",
                OfferType = AlertOfferType.Sell,
                Price = new Core.Domain.ValueObjects.PriceRange(100),
                Certification = "*"
            },
            new Alert
            {
                ItemName = "Dueling Dragons",
                OfferType = AlertOfferType.Sell,
                Price = new Core.Domain.ValueObjects.PriceRange(500),
                Certification = "*"
            },
            new Alert
            {
                ItemName = "Dingo",
                OfferType = AlertOfferType.Sell,
                Price = new Core.Domain.ValueObjects.PriceRange(200),
                Certification = "*"
            },
            new Alert
            {
                ItemName = "Gravity Bomb",
                OfferType = AlertOfferType.Sell,
                Price = new Core.Domain.ValueObjects.PriceRange(600),
                Certification = "*"
            },
            new Alert
            {
                ItemName = "20xx",
                OfferType = AlertOfferType.Sell,
                Price = new Core.Domain.ValueObjects.PriceRange(200),
                Certification = "*"
            },
            new Alert
            {
                ItemName = "Supernova III",
                OfferType = AlertOfferType.Sell,
                Price = new Core.Domain.ValueObjects.PriceRange(60),
                Certification = "*"
            },
            new Alert
            {
                ItemName = "Mainframe",
                OfferType = AlertOfferType.Sell,
                Price = new Core.Domain.ValueObjects.PriceRange(500),
                Certification = "*"
            },
            new Alert
            {
                ItemName = "Neuro-Agitator",
                OfferType = AlertOfferType.Sell,
                Price = new Core.Domain.ValueObjects.PriceRange(50),
                Certification = "*"
            },
            new Alert
            {
                ItemName = "Shattered",
                OfferType = AlertOfferType.Sell,
                Price = new Core.Domain.ValueObjects.PriceRange(60),
                Certification = "*"
            },
            new Alert
            {
                ItemName = "Encryption",
                OfferType = AlertOfferType.Sell,
                Price = new Core.Domain.ValueObjects.PriceRange(800),
                Certification = "*"
            },
            new Alert
            {
                ItemName = "Carbonator",
                OfferType = AlertOfferType.Sell,
                Price = new Core.Domain.ValueObjects.PriceRange(750),
                Certification = "*"
            },
            new Alert
            {
                ItemName = "Dissolver",
                OfferType = AlertOfferType.Sell,
                Price = new Core.Domain.ValueObjects.PriceRange(500),
                Certification = "*"
            },
            new Alert
            {
                ItemName = "Sub-Zero",
                OfferType = AlertOfferType.Sell,
                Price = new Core.Domain.ValueObjects.PriceRange(60),
                Certification = "*"
            },
            new Alert
            {
                ItemName = "Singularity",
                OfferType = AlertOfferType.Sell,
                Price = new Core.Domain.ValueObjects.PriceRange(50),
                Certification = "*"
            },
            new Alert
            {
                ItemName = "Buffy-Sugo",
                OfferType = AlertOfferType.Sell,
                Price = new Core.Domain.ValueObjects.PriceRange(1100),
                Certification = "*"
            },
            new Alert
            {
                ItemName = "Laser Wave II",
                OfferType = AlertOfferType.Sell,
                Price = new Core.Domain.ValueObjects.PriceRange(20),
                Certification = "*"
            },
            new Alert
            {
                ItemName = "Nomster",
                OfferType = AlertOfferType.Sell,
                Price = new Core.Domain.ValueObjects.PriceRange(50),
                Color = "Sky Blue",
                Certification = "*"
            },
            new Alert
            {
                ItemName = "Ion",
                OfferType = AlertOfferType.Sell,
                Price = new Core.Domain.ValueObjects.PriceRange(50),
                Color = "Titanium White",
                Certification = "*"
            },
            new Alert
            {
                ItemName = "Ion",
                OfferType = AlertOfferType.Sell,
                Price = new Core.Domain.ValueObjects.PriceRange(50),
                Color = "Lime",
                Certification = "*"
            },
            new Alert
            {
                ItemName = "Pixel Fire",
                OfferType = AlertOfferType.Sell,
                Price = new Core.Domain.ValueObjects.PriceRange(50),
                Color = "Titanium White",
                Certification = "*"
            },
            new Alert
            {
                ItemName = "Neo-Thermal",
                OfferType = AlertOfferType.Sell,
                Price = new Core.Domain.ValueObjects.PriceRange(50),
                Color = "Titanium White",
                Certification = "*"
            },
            new Alert
            {
                ItemName = "Power-Shot",
                OfferType = AlertOfferType.Sell,
                Price = new Core.Domain.ValueObjects.PriceRange(50),
                Color = "Titanium White",
                Certification = "*"
            },
            new Alert
            {
                ItemName = "Standard",
                OfferType = AlertOfferType.Sell,
                Price = new Core.Domain.ValueObjects.PriceRange(50),
                Color = "Titanium White",
                Certification = "*"
            },
            new Alert
            {
                ItemName = "Hexphase",
                OfferType = AlertOfferType.Sell,
                Price = new Core.Domain.ValueObjects.PriceRange(50),
                Color = "Titanium White",
                Certification = "*"
            },
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
