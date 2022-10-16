using RocketLeagueTradingTools.Infrastructure.TradeOffers;
using RocketLeagueTradingTools.Infrastructure.Common;
using Moq;
using FluentAssertions;
using RocketLeagueTradingTools.Core.Application.Contracts;

namespace RocketLeagueTradingTools.Infrastructure.IntegrationTests;

[TestFixture]
public class RlgDataSourceTests
{
    private RlgDataSource sut;
    private CancellationToken cancellationToken;
    private IHttp http;
    private Mock<ILog> log;

    [SetUp]
    public void Setup()
    {
        cancellationToken = new CancellationToken();
        log = new Mock<ILog>();

        http = new Http();
        http.SetTimeout(TimeSpan.FromSeconds(15));

        sut = new RlgDataSource(http, log.Object);
    }

    [Test]
    public async Task GetTradeOffersPage_should_download_buy_trade_offers()
    {
        var offersPage = await sut.GetTradeOffersPage(cancellationToken);

        offersPage.BuyOffers.Count.Should().BeGreaterThan(0);
        offersPage.BuyOffers.Count.Should().BeLessThanOrEqualTo(400);
        offersPage.BuyOffers.First().SourceId.Should().NotBeEmpty();
        offersPage.BuyOffers.First().Link.Should().StartWith("https://rocket-league.com/trade/");
        offersPage.BuyOffers.First().Item.Name.Should().NotBeEmpty();
        offersPage.BuyOffers.First().Price.Should().BeGreaterThan(0);
        offersPage.BuyOffers.First().Item.Name.Should().NotBeEmpty();
    }

    [Test]
    public async Task GetTradeOffersPage_should_download_sell_trade_offers()
    {
        var offersPage = await sut.GetTradeOffersPage(cancellationToken);

        offersPage.SellOffers.Count.Should().BeGreaterThan(0);
        offersPage.SellOffers.Count.Should().BeLessThanOrEqualTo(400);
        offersPage.SellOffers.First().SourceId.Should().NotBeEmpty();
        offersPage.SellOffers.First().Link.Should().StartWith("https://rocket-league.com/trade/");
        offersPage.SellOffers.First().Item.Name.Should().NotBeEmpty();
        offersPage.SellOffers.First().Price.Should().BeGreaterThan(0);
        offersPage.SellOffers.First().Item.Name.Should().NotBeEmpty();
    }
}
