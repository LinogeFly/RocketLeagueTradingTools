using RocketLeagueTradingTools.Infrastructure.TradeOffers;
using RocketLeagueTradingTools.Infrastructure.Common;
using Moq;
using FluentAssertions;
using RocketLeagueTradingTools.Core.Application.Contracts;

namespace RocketLeagueTradingTools.Infrastructure.IntegrationTests;

[TestFixture]
public class RlgDataSourceTests
{
    private RlgDataSource sut = null!;
    private CancellationToken cancellationToken;
    private IHttp http = null!;
    private Mock<ILog> log = null!;
    private Mock<IConfiguration> config = null!;
    private Mock<IDateTime> dateTime = null!;

    [SetUp]
    public void Setup()
    {
        dateTime = new Mock<IDateTime>();
        dateTime.SetupGet(d => d.Now).Returns(DateTime.UtcNow);

        cancellationToken = new CancellationToken();
        config = new Mock<IConfiguration>();
        log = new Mock<ILog>();
        http = new Http(config.Object);

        sut = new RlgDataSource(http, log.Object, dateTime.Object);
    }

    [Test]
    public async Task GetTradeOffersPage_should_download_trade_offers()
    {
        var offersPage = await sut.GetTradeOffersPage(cancellationToken);

        offersPage.BuyOffers.Count.Should().BeGreaterThan(0);
        offersPage.BuyOffers.Count.Should().BeLessThanOrEqualTo(400);
        offersPage.BuyOffers.First().SourceId.Should().NotBeEmpty();
        offersPage.BuyOffers.First().Link.Should().StartWith("https://rocket-league.com/trade/");
        offersPage.BuyOffers.First().Item.Name.Should().NotBeEmpty();
        offersPage.BuyOffers.First().Price.Should().BeGreaterThan(0);
        offersPage.SellOffers.Count.Should().BeGreaterThan(0);
        offersPage.SellOffers.Count.Should().BeLessThanOrEqualTo(400);
        offersPage.SellOffers.First().SourceId.Should().NotBeEmpty();
        offersPage.SellOffers.First().Link.Should().StartWith("https://rocket-league.com/trade/");
        offersPage.SellOffers.First().Item.Name.Should().NotBeEmpty();
        offersPage.SellOffers.First().Price.Should().BeGreaterThan(0);
    }

    [Test]
    public async Task GetTradeOffersPage_all_trade_offers_should_have_the_same_date()
    {
        var offersPage = await sut.GetTradeOffersPage(cancellationToken);

        var offers = offersPage.BuyOffers.Union(offersPage.SellOffers);

        offers.DistinctBy(o => o.ScrapedDate).ToList().Count.Should().Be(1);
    }
}
