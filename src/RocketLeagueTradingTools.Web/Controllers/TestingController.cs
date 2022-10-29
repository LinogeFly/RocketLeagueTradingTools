using Microsoft.AspNetCore.Mvc;
using RocketLeagueTradingTools.Core.Domain.Entities;
using RocketLeagueTradingTools.Web.Models.Testing;
using RocketLeagueTradingTools.Core.Application.Contracts;

namespace RocketLeagueTradingTools.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestingController : ControllerBase
{
    private readonly IPersistenceRepository persistence;
    private readonly IDateTime dateTime;

    public TestingController(
        IPersistenceRepository persistence,
        IDateTime dateTime)
    {
        this.persistence = persistence;
        this.dateTime = dateTime;
    }

    [HttpPost("seed")]
    public async Task<ActionResult> Seed(TradeOfferRequest request)
    {
        if (request.Amount == 0)
            throw new ArgumentException(nameof(request.Amount));

        if (request.AgeInMinutes == 0)
            throw new ArgumentException(nameof(request.AgeInMinutes));

        if (request.OfferType == OfferTypeRequest.None)
            throw new ArgumentException(nameof(request.OfferType));

        var offers = new List<TradeOffer>();

        for (int i = 0; i < request.Amount; i++)
        {
            var id = Guid.NewGuid().ToString();
            var offer = new TradeOffer(Map(request))
            {
                Link = $"https://rocket-league.com/trade/{id}",
                SourceId = id,
                Price = request.Price,
                ScrapedDate = dateTime.Now.AddMinutes(-request.AgeInMinutes)
            };

            offers.Add(offer);
        }

        if (request.OfferType == OfferTypeRequest.Buy)
            await persistence.AddBuyOffers(offers);
        if (request.OfferType == OfferTypeRequest.Sell)
            await persistence.AddSellOffers(offers);

        return Ok();
    }

    private TradeItem Map(TradeOfferRequest request)
    {
        return new TradeItem(request.Name)
        {
            Color = request.Color,
            Certification = request.Certification
        };
    }

}
