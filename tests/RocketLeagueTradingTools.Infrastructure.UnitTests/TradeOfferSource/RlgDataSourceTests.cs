using FluentAssertions;
using Moq;
using RocketLeagueTradingTools.Core.Application.Interfaces;
using RocketLeagueTradingTools.Core.Domain.Enumerations;
using RocketLeagueTradingTools.Infrastructure.Common;
using RocketLeagueTradingTools.Infrastructure.TradeOfferSource;
using RocketLeagueTradingTools.Infrastructure.UnitTests.Support;
using RocketLeagueTradingTools.Infrastructure.UnitTests.TradeOfferSource.Support.Rlg;

namespace RocketLeagueTradingTools.Infrastructure.UnitTests.TradeOfferSource;

[TestFixture]
public class RlgDataSourceTests
{
    private RlgDataSource sut = null!;
    private TestContainer testContainer = null!;
    private Mock<IHttp> httpClient = null!;
    private Mock<IDateTime> dateTime = null!;
    private CancellationToken cancellationToken;

    [SetUp]
    public void Setup()
    {
        testContainer = TestContainer.Create();
        cancellationToken = new CancellationToken();
        httpClient = testContainer.MockOf<IHttp>();
        dateTime = testContainer.MockOf<IDateTime>();
        sut = testContainer.GetService<RlgDataSource>();
    }

    [Test]
    public async Task ScrapPageAsync_buy_offer_link_should_be_mapped()
    {
        const string offerId = "88c10b46-a29a-4770-8efa-0304d6be8699";
        const string expected = $"https://rocket-league.com/trade/{offerId}";

        var page = new RlgPageBuilder();

        page.AddTrade(offerId)
            .WithHasItem(A.Credits(100))
            .WithWantsItem(A.RlgItem("Hellfire"));

        httpClient.Setup(c => c.GetStringAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => page.Build());

        var offers = await sut.GetTradeOffersPage(cancellationToken);

        offers.Single().TradeOffer.OfferType.Should().Be(TradeOfferType.Buy);
        offers.Single().TradeOffer.Link.Should().Be(expected);
    }

    [Test]
    public async Task ScrapPageAsync_buy_offer_scraped_date_should_be_mapped()
    {
        var expectedDate = new DateTime(2022, 1, 1);

        var page = new RlgPageBuilder();

        page.AddTrade()
            .WithHasItem(A.Credits(100))
            .WithWantsItem(A.RlgItem("Hellfire"));
        httpClient.Setup(c => c.GetStringAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => page.Build());
        dateTime.SetupGet(d => d.Now).Returns(expectedDate);

        var offers = await sut.GetTradeOffersPage(cancellationToken);

        offers.Single().TradeOffer.OfferType.Should().Be(TradeOfferType.Buy);
        offers.Single().ScrapedDate.Should().Be(expectedDate);
    }

    [Test]
    public async Task ScrapPageAsync_buy_offer_price_should_be_mapped()
    {
        const int expected = 100;

        var page = new RlgPageBuilder();

        page.AddTrade()
            .WithHasItem(A.Credits(expected))
            .WithWantsItem(A.RlgItem("Hellfire"));

        httpClient.Setup(c => c.GetStringAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => page.Build());

        var offers = await sut.GetTradeOffersPage(cancellationToken);

        offers.Single().TradeOffer.OfferType.Should().Be(TradeOfferType.Buy);
        offers.Single().TradeOffer.Price.Should().Be(expected);
    }

    [Test]
    public async Task ScrapPageAsync_buy_offer_trading_site_should_be_mapped()
    {
        var page = new RlgPageBuilder();

        page.AddTrade()
            .WithHasItem(A.Credits(100))
            .WithWantsItem(A.RlgItem("Hellfire"));

        httpClient.Setup(c => c.GetStringAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => page.Build());

        var offers = await sut.GetTradeOffersPage(cancellationToken);

        offers.Single().TradeOffer.OfferType.Should().Be(TradeOfferType.Buy);
        offers.Single().TradeOffer.Trader.TradingSite.Should().Be(TradingSite.RocketLeagueGarage);
    }
    
    [Test]
    public async Task ScrapPageAsync_buy_offer_trader_name_should_be_mapped()
    {
        const string expected = "RLTrader69";
        
        var page = new RlgPageBuilder();

        page.AddTrade()
            .WithTraderName(expected)
            .WithHasItem(A.Credits(100))
            .WithWantsItem(A.RlgItem("Hellfire"));

        httpClient.Setup(c => c.GetStringAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => page.Build());

        var offers = await sut.GetTradeOffersPage(cancellationToken);

        offers.Single().TradeOffer.OfferType.Should().Be(TradeOfferType.Buy);
        offers.Single().TradeOffer.Trader.Name.Should().Be(expected);
    }
    
    [Test]
    public async Task ScrapPageAsync_buy_offer_item_name_should_be_mapped()
    {
        const string expected = "Hellfire";

        var page = new RlgPageBuilder();

        page.AddTrade()
            .WithHasItem(A.Credits(100))
            .WithWantsItem(A.RlgItem(expected));

        httpClient.Setup(c => c.GetStringAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => page.Build());

        var offers = await sut.GetTradeOffersPage(cancellationToken);

        offers.Single().TradeOffer.OfferType.Should().Be(TradeOfferType.Buy);
        offers.Single().TradeOffer.Item.Name.Should().Be(expected);
    }

    [Test]
    public async Task ScrapPageAsync_buy_offer_item_color_should_be_mapped()
    {
        const string expected = "Titanium White";

        var page = new RlgPageBuilder();

        page.AddTrade()
            .WithHasItem(A.Credits(1700))
            .WithWantsItem(A.RlgItem("Fennec").WithColor(expected));

        httpClient.Setup(c => c.GetStringAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => page.Build());

        var offers = await sut.GetTradeOffersPage(cancellationToken);

        offers.Single().TradeOffer.OfferType.Should().Be(TradeOfferType.Buy);
        offers.Single().TradeOffer.Item.Color.Should().Be(expected);
    }

    [Test]
    public async Task ScrapPageAsync_buy_offer_item_certification_should_be_mapped()
    {
        const string expected = "Goalkeeper";

        var page = new RlgPageBuilder();

        page.AddTrade()
            .WithHasItem(A.Credits(300))
            .WithWantsItem(A.RlgItem("Fennec").WithCertification(expected));

        httpClient.Setup(c => c.GetStringAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => page.Build());

        var offers = await sut.GetTradeOffersPage(cancellationToken);

        offers.Single().TradeOffer.OfferType.Should().Be(TradeOfferType.Buy);
        offers.Single().TradeOffer.Item.Certification.Should().Be(expected);
    }

    [TestCase("/items/bodies/fennec", TradeItemType.Body)]
    [TestCase("/items/bodies/fennec/titaniumwhite", TradeItemType.Body)]
    [TestCase("/items/decals/blackmarket/20xx", TradeItemType.Decal)]
    [TestCase("/items/decals/blackmarket/20xx/crimson", TradeItemType.Decal)]
    [TestCase("/items/paints/anodized-pearl", TradeItemType.PaintFinish)]
    [TestCase("/items/wheels/cristiano", TradeItemType.Wheels)]
    [TestCase("/items/wheels/cristiano/orange", TradeItemType.Wheels)]
    [TestCase("/items/boosts/standard", TradeItemType.RocketBoost)]
    [TestCase("/items/boosts/standard/black", TradeItemType.RocketBoost)]
    [TestCase("/items/toppers/wildcat-ears", TradeItemType.Topper)]
    [TestCase("/items/toppers/wildcat-ears/skyblue", TradeItemType.Topper)]
    [TestCase("/items/antennas/mage-glass-iii", TradeItemType.Antenna)]
    [TestCase("/items/antennas/mage-glass-iii/burntsienna", TradeItemType.Antenna)]
    [TestCase("/items/explosions/dueling-dragons", TradeItemType.GoalExplosion)]
    [TestCase("/items/explosions/dueling-dragons/pink", TradeItemType.GoalExplosion)]
    [TestCase("/items/trails/laser-wave-iii", TradeItemType.Trail)]
    [TestCase("/items/trails/laser-wave-iii/saffron", TradeItemType.Trail)]
    [TestCase("/items/banners/doughnut-eater", TradeItemType.Banner)]
    [TestCase("/items/banners/doughnut-eater/cobalt", TradeItemType.Banner)]
    [TestCase("/items/borders/crown", TradeItemType.AvatarBorder)]
    [TestCase("/items/borders/crown/forestgreen", TradeItemType.AvatarBorder)]
    public async Task ScrapPageAsync_buy_offer_item_type_should_be_mapped(string link, TradeItemType expected)
    {
        var page = new RlgPageBuilder();

        page.AddTrade()
            .WithHasItem(A.Credits(100))
            .WithWantsItem(A.RlgItem("Item").WithItemDetailsLink(link));

        httpClient.Setup(c => c.GetStringAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => page.Build());

        var offers = await sut.GetTradeOffersPage(cancellationToken);

        offers.Single().TradeOffer.OfferType.Should().Be(TradeOfferType.Buy);
        offers.Single().TradeOffer.Item.ItemType.Should().Be(expected);
    }

    [Test]
    public async Task ScrapPageAsync_sell_offer_link_should_be_mapped()
    {
        const string offerId = "88c10b46-a29a-4770-8efa-0304d6be8699";
        const string expected = $"https://rocket-league.com/trade/{offerId}";

        var page = new RlgPageBuilder();

        page.AddTrade(offerId)
            .WithHasItem(A.RlgItem("Hellfire"))
            .WithWantsItem(A.Credits(100));

        httpClient.Setup(c => c.GetStringAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => page.Build());

        var offers = await sut.GetTradeOffersPage(cancellationToken);

        offers.Single().TradeOffer.OfferType.Should().Be(TradeOfferType.Sell);
        offers.Single().TradeOffer.Link.Should().Be(expected);
    }

    [Test]
    public async Task ScrapPageAsync_sell_offer_scraped_date_should_be_mapped()
    {
        var expectedDate = new DateTime(2022, 1, 1);

        var page = new RlgPageBuilder();

        page.AddTrade()
            .WithHasItem(A.RlgItem("Hellfire"))
            .WithWantsItem(A.Credits(100));

        httpClient.Setup(c => c.GetStringAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => page.Build());
        dateTime.SetupGet(d => d.Now).Returns(expectedDate);

        var offers = await sut.GetTradeOffersPage(cancellationToken);

        offers.Single().TradeOffer.OfferType.Should().Be(TradeOfferType.Sell);
        offers.Single().ScrapedDate.Should().Be(expectedDate);
    }

    [Test]
    public async Task ScrapPageAsync_sell_offer_price_should_be_mapped()
    {
        const int expected = 100;

        var page = new RlgPageBuilder();

        page.AddTrade()
            .WithHasItem(A.RlgItem("Hellfire"))
            .WithWantsItem(A.Credits(expected));

        httpClient.Setup(c => c.GetStringAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => page.Build());

        var offers = await sut.GetTradeOffersPage(cancellationToken);

        offers.Single().TradeOffer.OfferType.Should().Be(TradeOfferType.Sell);
        offers.Single().TradeOffer.Price.Should().Be(expected);
    }

    [Test]
    public async Task ScrapPageAsync_sell_offer_trading_site_should_be_mapped()
    {
        var page = new RlgPageBuilder();

        page.AddTrade()
            .WithHasItem(A.RlgItem("Hellfire"))
            .WithWantsItem(A.Credits(100));

        httpClient.Setup(c => c.GetStringAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => page.Build());

        var offers = await sut.GetTradeOffersPage(cancellationToken);

        offers.Single().TradeOffer.OfferType.Should().Be(TradeOfferType.Sell);
        offers.Single().TradeOffer.Trader.TradingSite.Should().Be(TradingSite.RocketLeagueGarage);
    }
    
    [Test]
    public async Task ScrapPageAsync_sell_offer_trader_name_should_be_mapped()
    {
        const string expected = "RLTrader69";
        
        var page = new RlgPageBuilder();

        page.AddTrade()
            .WithTraderName(expected)
            .WithHasItem(A.RlgItem("Hellfire"))
            .WithWantsItem(A.Credits(100));

        httpClient.Setup(c => c.GetStringAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => page.Build());

        var offers = await sut.GetTradeOffersPage(cancellationToken);

        offers.Single().TradeOffer.OfferType.Should().Be(TradeOfferType.Sell);
        offers.Single().TradeOffer.Trader.Name.Should().Be(expected);
    }
    
    [Test]
    public async Task ScrapPageAsync_sell_offer_item_name_should_be_mapped()
    {
        const string expected = "Hellfire";

        var page = new RlgPageBuilder();

        page.AddTrade()
            .WithHasItem(A.RlgItem(expected))
            .WithWantsItem(A.Credits(100));

        httpClient.Setup(c => c.GetStringAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => page.Build());

        var offers = await sut.GetTradeOffersPage(cancellationToken);

        offers.Single().TradeOffer.OfferType.Should().Be(TradeOfferType.Sell);
        offers.Single().TradeOffer.Item.Name.Should().Be(expected);
    }

    [Test]
    public async Task ScrapPageAsync_sell_offer_item_color_should_be_mapped()
    {
        const string expected = "Titanium White";

        var page = new RlgPageBuilder();

        page.AddTrade()
            .WithHasItem(A.RlgItem("Fennec").WithColor(expected))
            .WithWantsItem(A.Credits(1700));

        httpClient.Setup(c => c.GetStringAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => page.Build());

        var offers = await sut.GetTradeOffersPage(cancellationToken);

        offers.Single().TradeOffer.OfferType.Should().Be(TradeOfferType.Sell);
        offers.Single().TradeOffer.Item.Color.Should().Be(expected);
    }

    [Test]
    public async Task ScrapPageAsync_sell_offer_item_certification_should_be_mapped()
    {
        const string expected = "Goalkeeper";

        var page = new RlgPageBuilder();

        page.AddTrade()
            .WithHasItem(A.RlgItem("Fennec").WithCertification(expected))
            .WithWantsItem(A.Credits(300));

        httpClient.Setup(c => c.GetStringAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => page.Build());

        var offers = await sut.GetTradeOffersPage(cancellationToken);

        offers.Single().TradeOffer.OfferType.Should().Be(TradeOfferType.Sell);
        offers.Single().TradeOffer.Item.Certification.Should().Be(expected);
    }

    [TestCase("/items/bodies/fennec", TradeItemType.Body)]
    [TestCase("/items/bodies/fennec/titaniumwhite", TradeItemType.Body)]
    [TestCase("/items/decals/blackmarket/20xx", TradeItemType.Decal)]
    [TestCase("/items/decals/blackmarket/20xx/crimson", TradeItemType.Decal)]
    [TestCase("/items/paints/anodized-pearl", TradeItemType.PaintFinish)]
    [TestCase("/items/wheels/cristiano", TradeItemType.Wheels)]
    [TestCase("/items/wheels/cristiano/orange", TradeItemType.Wheels)]
    [TestCase("/items/boosts/standard", TradeItemType.RocketBoost)]
    [TestCase("/items/boosts/standard/black", TradeItemType.RocketBoost)]
    [TestCase("/items/toppers/wildcat-ears", TradeItemType.Topper)]
    [TestCase("/items/toppers/wildcat-ears/skyblue", TradeItemType.Topper)]
    [TestCase("/items/antennas/mage-glass-iii", TradeItemType.Antenna)]
    [TestCase("/items/antennas/mage-glass-iii/burntsienna", TradeItemType.Antenna)]
    [TestCase("/items/explosions/dueling-dragons", TradeItemType.GoalExplosion)]
    [TestCase("/items/explosions/dueling-dragons/pink", TradeItemType.GoalExplosion)]
    [TestCase("/items/trails/laser-wave-iii", TradeItemType.Trail)]
    [TestCase("/items/trails/laser-wave-iii/saffron", TradeItemType.Trail)]
    [TestCase("/items/banners/doughnut-eater", TradeItemType.Banner)]
    [TestCase("/items/banners/doughnut-eater/cobalt", TradeItemType.Banner)]
    [TestCase("/items/borders/crown", TradeItemType.AvatarBorder)]
    [TestCase("/items/borders/crown/forestgreen", TradeItemType.AvatarBorder)]
    public async Task ScrapPageAsync_sell_offer_item_type_should_be_mapped(string link, TradeItemType expected)
    {
        var page = new RlgPageBuilder();

        page.AddTrade()
            .WithHasItem(A.RlgItem("Item").WithItemDetailsLink(link))
            .WithWantsItem(A.Credits(140));

        httpClient.Setup(c => c.GetStringAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => page.Build());

        var offers = await sut.GetTradeOffersPage(cancellationToken);

        offers.Single().TradeOffer.OfferType.Should().Be(TradeOfferType.Sell);
        offers.Single().TradeOffer.Item.ItemType.Should().Be(expected);
    }

    [Test]
    public async Task ScrapPageAsync_should_split_has_credits_wants_multiple_items_trade_into_separate_buy_offers()
    {
        var page = new RlgPageBuilder();

        page.AddTrade()
            .WithHasItem(A.Credits(500))
            .WithHasItem(A.Credits(300))
            .WithWantsItem(A.RlgItem("Dueling Dragons"))
            .WithWantsItem(A.RlgItem("Fennec"));

        httpClient.Setup(c => c.GetStringAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => page.Build());

        var offers = (await sut.GetTradeOffersPage(cancellationToken)).ToArray();

        offers.Length.Should().Be(2);
        offers[0].TradeOffer.OfferType.Should().Be(TradeOfferType.Buy);
        offers[0].TradeOffer.Item.Name.Should().Be("Dueling Dragons");
        offers[0].TradeOffer.Price.Should().Be(500);
        offers[1].TradeOffer.OfferType.Should().Be(TradeOfferType.Buy);
        offers[1].TradeOffer.Item.Name.Should().Be("Fennec");
        offers[1].TradeOffer.Price.Should().Be(300);
    }

    [Test]
    public async Task ScrapPageAsync_should_split_has_multiple_items_wants_credits_trade_into_separate_sell_offers()
    {
        var page = new RlgPageBuilder();

        page.AddTrade()
            .WithHasItem(A.RlgItem("Dueling Dragons"))
            .WithHasItem(A.RlgItem("Fennec"))
            .WithWantsItem(A.Credits(550))
            .WithWantsItem(A.Credits(350));

        httpClient.Setup(c => c.GetStringAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => page.Build());

        var offers = (await sut.GetTradeOffersPage(cancellationToken)).ToArray();

        offers.Length.Should().Be(2);
        offers[0].TradeOffer.OfferType.Should().Be(TradeOfferType.Sell);
        offers[0].TradeOffer.Item.Name.Should().Be("Dueling Dragons");
        offers[0].TradeOffer.Price.Should().Be(550);
        offers[1].TradeOffer.OfferType.Should().Be(TradeOfferType.Sell);
        offers[1].TradeOffer.Item.Name.Should().Be("Fennec");
        offers[1].TradeOffer.Price.Should().Be(350);
    }

    [Test]
    public async Task ScrapPageAsync_should_combine_offers_from_multiple_trades()
    {
        var page = new RlgPageBuilder();
        page.AddTrade()
            .WithHasItem(A.Credits(60))
            .WithHasItem(A.Credits(100))
            .WithWantsItem(A.RlgItem("Supernova III"))
            .WithWantsItem(A.RlgItem("Hellfire"));
        page.AddTrade()
            .WithHasItem(A.RlgItem("Dueling Dragons"))
            .WithHasItem(A.RlgItem("Fennec"))
            .WithWantsItem(A.Credits(550))
            .WithWantsItem(A.Credits(350));
        httpClient.Setup(c => c.GetStringAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => page.Build());

        var offers = (await sut.GetTradeOffersPage(cancellationToken)).ToArray();

        offers.Length.Should().Be(4);
        offers[0].TradeOffer.OfferType.Should().Be(TradeOfferType.Buy);
        offers[0].TradeOffer.Item.Name.Should().Be("Supernova III");
        offers[0].TradeOffer.Price.Should().Be(60);
        offers[1].TradeOffer.OfferType.Should().Be(TradeOfferType.Buy);
        offers[1].TradeOffer.Item.Name.Should().Be("Hellfire");
        offers[1].TradeOffer.Price.Should().Be(100);
        offers[2].TradeOffer.OfferType.Should().Be(TradeOfferType.Sell);
        offers[2].TradeOffer.Item.Name.Should().Be("Dueling Dragons");
        offers[2].TradeOffer.Price.Should().Be(550);
        offers[3].TradeOffer.OfferType.Should().Be(TradeOfferType.Sell);
        offers[3].TradeOffer.Item.Name.Should().Be("Fennec");
        offers[3].TradeOffer.Price.Should().Be(350);
    }

    [Test]
    public async Task ScrapPageAsync_offers_from_multiple_trades_should_have_the_same_scraped_date()
    {
        var expectedDate = new DateTime(2022, 1, 1);
        var page = new RlgPageBuilder();
        page.AddTrade()
            .WithHasItem(A.Credits(100))
            .WithWantsItem(A.RlgItem("Hellfire"));
        page.AddTrade()
            .WithHasItem(A.RlgItem("Fennec"))
            .WithWantsItem(A.Credits(300));
        httpClient.Setup(c => c.GetStringAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => page.Build());
        dateTime.SetupGet(d => d.Now).Returns(expectedDate);
        
        var offers = (await sut.GetTradeOffersPage(cancellationToken)).ToArray();

        offers[0].ScrapedDate.Should().Be(expectedDate);
        offers[1].ScrapedDate.Should().Be(expectedDate);
    }

    [Test]
    public async Task ScrapPageAsync_should_filter_duplicate_buy_offers()
    {
        var page = new RlgPageBuilder();

        page.AddTrade()
            .WithHasItem(A.Credits(300))
            .WithHasItem(A.Credits(300))
            .WithWantsItem(A.RlgItem("Fennec"))
            .WithWantsItem(A.RlgItem("Fennec"));

        httpClient.Setup(c => c.GetStringAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => page.Build());

        var offers = await sut.GetTradeOffersPage(cancellationToken);

        offers.Count.Should().Be(1);
        offers.Single().TradeOffer.OfferType.Should().Be(TradeOfferType.Buy);
        offers.Single().TradeOffer.Item.Name.Should().Be("Fennec");
        offers.Single().TradeOffer.Price.Should().Be(300);
    }

    [Test]
    public async Task ScrapPageAsync_should_filter_duplicate_sell_offers()
    {
        var page = new RlgPageBuilder();

        page.AddTrade()
            .WithHasItem(A.RlgItem("Fennec"))
            .WithHasItem(A.RlgItem("Fennec"))
            .WithWantsItem(A.Credits(350))
            .WithWantsItem(A.Credits(350));

        httpClient.Setup(c => c.GetStringAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => page.Build());

        var offers = await sut.GetTradeOffersPage(cancellationToken);

        offers.Count.Should().Be(1);
        offers.Single().TradeOffer.OfferType.Should().Be(TradeOfferType.Sell);
        offers.Single().TradeOffer.Item.Name.Should().Be("Fennec");
        offers.Single().TradeOffer.Price.Should().Be(350);
    }

    [Test]
    public async Task ScrapPageAsync_should_filter_non_credits_offers_but_keep_buy_offers()
    {
        var page = new RlgPageBuilder();

        page.AddTrade()
            .WithHasItem(A.RlgItem("Dueling Dragons"))
            .WithHasItem(A.Credits(60))
            .WithHasItem(A.RlgItem("Mainframe"))
            .WithWantsItem(A.RlgItem("Meteor Storm"))
            .WithWantsItem(A.RlgItem("Supernova III"))
            .WithWantsItem(A.RlgItem("Dissolver"));

        httpClient.Setup(c => c.GetStringAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => page.Build());

        var offers = await sut.GetTradeOffersPage(cancellationToken);

        offers.Single().TradeOffer.OfferType.Should().Be(TradeOfferType.Buy);
        offers.Single().TradeOffer.Item.Name.Should().Be("Supernova III");
        offers.Single().TradeOffer.Price.Should().Be(60);
    }

    [Test]
    public async Task ScrapPageAsync_should_filter_non_credits_offers_but_keep_sell_offers()
    {
        var page = new RlgPageBuilder();

        page.AddTrade()
            .WithHasItem(A.RlgItem("Dueling Dragons"))
            .WithHasItem(A.RlgItem("Supernova III"))
            .WithHasItem(A.RlgItem("Mainframe"))
            .WithWantsItem(A.RlgItem("Meteor Storm"))
            .WithWantsItem(A.Credits(90))
            .WithWantsItem(A.RlgItem("Dissolver"));

        httpClient.Setup(c => c.GetStringAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => page.Build());

        var offers = await sut.GetTradeOffersPage(cancellationToken);

        offers.Single().TradeOffer.OfferType.Should().Be(TradeOfferType.Sell);
        offers.Single().TradeOffer.Item.Name.Should().Be("Supernova III");
        offers.Single().TradeOffer.Price.Should().Be(90);
    }

    [Test]
    public async Task ScrapPageAsync_should_filter_buy_trade_with_mismatching_amount_of_has_and_wants_items()
    {
        var page = new RlgPageBuilder();

        page.AddTrade()
            .WithHasItem(A.Credits(800))
            .WithWantsItem(A.RlgItem("Dueling Dragons"))
            .WithWantsItem(A.RlgItem("Fennec"));

        httpClient.Setup(c => c.GetStringAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => page.Build());

        var offers = await sut.GetTradeOffersPage(cancellationToken);

        offers.Should().BeEmpty();
    }

    [Test]
    public async Task ScrapPageAsync_should_filter_sell_trade_with_mismatching_amount_of_has_and_wants_items()
    {
        var page = new RlgPageBuilder();

        page.AddTrade()
            .WithHasItem(A.RlgItem("Dueling Dragons"))
            .WithHasItem(A.RlgItem("Fennec"))
            .WithWantsItem(A.Credits(800));

        httpClient.Setup(c => c.GetStringAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => page.Build());

        var offers = await sut.GetTradeOffersPage(cancellationToken);

        offers.Should().BeEmpty();
    }

    [Test]
    public async Task ScrapPageAsync_should_filter_has_credits_wants_grouped_items_offers()
    {
        var page = new RlgPageBuilder();

        page.AddTrade()
            .WithHasItem(A.Credits(300))
            .WithHasItem(A.Credits(100))
            .WithWantsItem(A.RlgItems("Fennec", 2))
            .WithWantsItem(A.RlgItem("Hellfire"));

        httpClient.Setup(c => c.GetStringAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => page.Build());

        var offers = await sut.GetTradeOffersPage(cancellationToken);

        offers.Single().TradeOffer.OfferType.Should().Be(TradeOfferType.Buy);
        offers.Count.Should().Be(1);
        offers.Single().TradeOffer.Item.Name.Should().Be("Hellfire");
        offers.Single().TradeOffer.Price.Should().Be(100);
    }

    [Test]
    public async Task ScrapPageAsync_should_filter_has_grouped_items_wants_credits_offers()
    {
        var page = new RlgPageBuilder();

        page.AddTrade()
            .WithHasItem(A.RlgItems("Fennec", 2))
            .WithHasItem(A.RlgItem("Hellfire"))
            .WithWantsItem(A.Credits(350))
            .WithWantsItem(A.Credits(150));

        httpClient.Setup(c => c.GetStringAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => page.Build());

        var offers = await sut.GetTradeOffersPage(cancellationToken);

        offers.Single().TradeOffer.OfferType.Should().Be(TradeOfferType.Sell);
        offers.Count.Should().Be(1);
        offers.Single().TradeOffer.Item.Name.Should().Be("Hellfire");
        offers.Single().TradeOffer.Price.Should().Be(150);
    }

    [Test]
    public async Task ScrapPageAsync_should_filter_has_blueprint_wants_credits_offers()
    {
        var page = new RlgPageBuilder();

        page.AddTrade()
            .WithHasItem(A.RlgItem("Mainframe Blueprint").WithColor("Titanium White"))
            .WithWantsItem(A.Credits(6500));

        httpClient.Setup(c => c.GetStringAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => page.Build());

        var offers = await sut.GetTradeOffersPage(cancellationToken);

        offers.Should().BeEmpty();
    }

    [Test]
    public async Task ScrapPageAsync_should_filter_has_credits_wants_blueprint_offers()
    {
        var page = new RlgPageBuilder();

        page.AddTrade()
            .WithHasItem(A.Credits(6500))
            .WithWantsItem(A.RlgItem("Mainframe Blueprint").WithColor("Titanium White"));

        httpClient.Setup(c => c.GetStringAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => page.Build());

        var offers = await sut.GetTradeOffersPage(cancellationToken);

        offers.Should().BeEmpty();
    }

    [Test]
    public async Task ScrapPageAsync_should_filter_has_painted_set_wants_credits_offers()
    {
        var page = new RlgPageBuilder();

        page.AddTrade()
            .WithHasItem(A.RlgItem("Grimalkin").WithColor("Painted set"))
            .WithWantsItem(A.Credits(6000));

        httpClient.Setup(c => c.GetStringAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => page.Build());

        var offers = await sut.GetTradeOffersPage(cancellationToken);

        offers.Should().BeEmpty();
    }

    [Test]
    public async Task ScrapPageAsync_should_filter_has_credits_wants_painted_set_offers()
    {
        var page = new RlgPageBuilder();

        page.AddTrade()
            .WithHasItem(A.Credits(5500))
            .WithWantsItem(A.RlgItem("Grimalkin").WithColor("Painted set"));

        httpClient.Setup(c => c.GetStringAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => page.Build());

        var offers = await sut.GetTradeOffersPage(cancellationToken);

        offers.Should().BeEmpty();
    }

    [TestCase("Offer")]
    [TestCase("Credits offer")]
    [TestCase("Black Marker offer")]
    [TestCase("Overpay")]
    [TestCase("Placeholder")]
    public async Task ScrapPageAsync_should_filter_has_credits_wants_offer_offers(string offerItemName)
    {
        var page = new RlgPageBuilder();

        page.AddTrade()
            .WithHasItem(A.Credits(1000))
            .WithHasItem(A.Credits(300))
            .WithWantsItem(A.RlgItem(offerItemName))
            .WithWantsItem(A.RlgItem("Fennec"));

        httpClient.Setup(c => c.GetStringAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => page.Build());

        var offers = await sut.GetTradeOffersPage(cancellationToken);

        offers.Single().TradeOffer.OfferType.Should().Be(TradeOfferType.Buy);
        offers.Count.Should().Be(1);
        offers.Single().TradeOffer.Item.Name.Should().Be("Fennec");
        offers.Single().TradeOffer.Price.Should().Be(300);
    }

    [TestCase("Offer")]
    [TestCase("Credits offer")]
    [TestCase("Black Marker offer")]
    [TestCase("Overpay")]
    [TestCase("Placeholder")]
    public async Task ScrapPageAsync_should_filter_has_offer_wants_credits_offers(string offerItemName)
    {
        var page = new RlgPageBuilder();

        page.AddTrade()
            .WithHasItem(A.RlgItem(offerItemName))
            .WithHasItem(A.RlgItem("Fennec"))
            .WithWantsItem(A.Credits(1000))
            .WithWantsItem(A.Credits(350));

        httpClient.Setup(c => c.GetStringAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => page.Build());

        var offers = await sut.GetTradeOffersPage(cancellationToken);

        offers.Single().TradeOffer.OfferType.Should().Be(TradeOfferType.Sell);
        offers.Count.Should().Be(1);
        offers.Single().TradeOffer.Item.Name.Should().Be("Fennec");
        offers.Single().TradeOffer.Price.Should().Be(350);
    }

    [Test]
    public async Task ScrapPageAsync_should_filter_has_credits_without_amount_offers()
    {
        var page = new RlgPageBuilder();

        page.AddTrade()
            .WithHasItem(A.Credits(0))
            .WithWantsItem(A.RlgItem("Supernova III"));

        httpClient.Setup(c => c.GetStringAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => page.Build());

        var offers = await sut.GetTradeOffersPage(cancellationToken);

        offers.Should().BeEmpty();
    }

    [Test]
    public async Task ScrapPageAsync_should_filter_wants_credits_without_amount_offers()
    {
        var page = new RlgPageBuilder();

        page.AddTrade()
            .WithHasItem(A.RlgItem("Supernova III"))
            .WithWantsItem(A.Credits(0));

        httpClient.Setup(c => c.GetStringAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => page.Build());

        var offers = await sut.GetTradeOffersPage(cancellationToken);

        offers.Should().BeEmpty();
    }

    [Test]
    public async Task ScrapPageAsync_should_filter_has_item_or_item_trade()
    {
        var page = new RlgPageBuilder();

        page.AddTrade()
            .WithHasItem(A.RlgItem("Fennec"))
            .WithHasItem(A.RlgItem("OR"))
            .WithHasItem(A.RlgItem("Meteor Storm"))
            .WithWantsItem(A.Credits(350))
            .WithWantsItem(A.RlgItem("OR"))
            .WithWantsItem(A.RlgItem("Dingo"));

        httpClient.Setup(c => c.GetStringAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => page.Build());

        var offers = await sut.GetTradeOffersPage(cancellationToken);

        offers.Should().BeEmpty();
    }

    [Test]
    public async Task ScrapPageAsync_should_filter_has_credits_or_item_trade()
    {
        var page = new RlgPageBuilder();

        page.AddTrade()
            .WithHasItem(A.Credits(300))
            .WithHasItem(A.RlgItem("OR"))
            .WithHasItem(A.RlgItem("Dingo"))
            .WithWantsItem(A.RlgItem("Fennec"))
            .WithWantsItem(A.RlgItem("OR"))
            .WithWantsItem(A.RlgItem("Meteor Storm"));

        httpClient.Setup(c => c.GetStringAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => page.Build());

        var offers = await sut.GetTradeOffersPage(cancellationToken);

        offers.Should().BeEmpty();
    }
}
