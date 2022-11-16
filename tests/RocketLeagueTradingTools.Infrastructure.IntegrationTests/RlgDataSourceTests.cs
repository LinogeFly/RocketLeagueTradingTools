using RocketLeagueTradingTools.Infrastructure.TradeOffers;
using RocketLeagueTradingTools.Infrastructure.Common;
using Moq;
using FluentAssertions;
using RocketLeagueTradingTools.Core.Application.Interfaces;
using RocketLeagueTradingTools.Core.Domain.Enumerations;

namespace RocketLeagueTradingTools.Infrastructure.IntegrationTests;

[TestFixture]
public class RlgDataSourceTests
{
    private RlgDataSource sut = null!;
    private CancellationToken cancellationToken;
    private IHttp http = null!;
    private Mock<ILog> log = null!;
    private Mock<IDateTime> dateTime = null!;

    [SetUp]
    public void Setup()
    {
        dateTime = new Mock<IDateTime>();
        dateTime.SetupGet(d => d.Now).Returns(DateTime.UtcNow);

        http = new Http
        {
            Timeout = TimeSpan.FromSeconds(30),
            DefaultRequestUserAgent = "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/106.0.0.0 Safari/537.36"
        };

        cancellationToken = new CancellationToken();
        log = new Mock<ILog>();


        sut = new RlgDataSource(http, log.Object, dateTime.Object);
    }

    [Test]
    public async Task GetTradeOffersPage_should_download_trade_offers()
    {
        var offersPage = await sut.GetTradeOffersPage(cancellationToken);

        offersPage.BuyOffers.Count.Should().BeGreaterThan(0);
        offersPage.BuyOffers.Count.Should().BeLessThanOrEqualTo(400);
        offersPage.BuyOffers.First().Link.Should().StartWith("https://rocket-league.com/trade/");
        offersPage.BuyOffers.First().Item.Name.Should().NotBeEmpty();
        offersPage.BuyOffers.First().Price.Should().BeGreaterThan(0);
        offersPage.BuyOffers.First().TradingSite.Should().Be(TradingSite.RocketLeagueGarage);
        offersPage.BuyOffers.First().TraderName.Should().NotBeEmpty();
        offersPage.SellOffers.Count.Should().BeGreaterThan(0);
        offersPage.SellOffers.Count.Should().BeLessThanOrEqualTo(400);
        offersPage.SellOffers.First().Link.Should().StartWith("https://rocket-league.com/trade/");
        offersPage.SellOffers.First().Item.Name.Should().NotBeEmpty();
        offersPage.SellOffers.First().Price.Should().BeGreaterThan(0);
        offersPage.SellOffers.First().TradingSite.Should().Be(TradingSite.RocketLeagueGarage);
        offersPage.SellOffers.First().TraderName.Should().NotBeEmpty();
    }

    [Test]
    public async Task GetTradeOffersPage_all_trade_offers_should_have_the_same_date()
    {
        var offersPage = await sut.GetTradeOffersPage(cancellationToken);

        var offers = offersPage.BuyOffers.Union(offersPage.SellOffers);

        offers.DistinctBy(o => o.ScrapedDate).ToList().Count.Should().Be(1);
    }
}
