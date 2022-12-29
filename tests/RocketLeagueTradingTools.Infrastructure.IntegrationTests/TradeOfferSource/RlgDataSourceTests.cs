using FluentAssertions;
using Moq;
using RocketLeagueTradingTools.Core.Application.Interfaces;
using RocketLeagueTradingTools.Core.Domain.Enumerations;
using RocketLeagueTradingTools.Infrastructure.Common;
using RocketLeagueTradingTools.Infrastructure.TradeOfferSource;

namespace RocketLeagueTradingTools.Infrastructure.IntegrationTests.TradeOfferSource;

[TestFixture]
public class RlgDataSourceTests
{
    private RlgDataSource sut = null!;
    private readonly CancellationToken cancellationToken = default;
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

        log = new Mock<ILog>();

        sut = new RlgDataSource(http, log.Object, dateTime.Object);
    }

    [Test]
    public async Task GetTradeOffersPage_should_download_trade_offers()
    {
        var offers = await sut.GetTradeOffersPage(cancellationToken);

        offers.Count.Should().BeGreaterThan(0);
        offers.Count.Should().BeLessThanOrEqualTo(400);
        offers.First().TradeOffer.Link.Should().StartWith("https://rocket-league.com/trade/");
        offers.First().TradeOffer.Item.Name.Should().NotBeEmpty();
        offers.First().TradeOffer.Price.Should().BeGreaterThan(0);
        offers.First().TradeOffer.Trader.TradingSite.Should().Be(TradingSite.RocketLeagueGarage);
        offers.First().TradeOffer.Trader.Name.Should().NotBeEmpty();
    }
}
