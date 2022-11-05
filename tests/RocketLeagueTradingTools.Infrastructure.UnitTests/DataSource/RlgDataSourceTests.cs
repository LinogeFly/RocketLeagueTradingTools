using FluentAssertions;
using Moq;
using RocketLeagueTradingTools.Core.Application.Interfaces;
using RocketLeagueTradingTools.Infrastructure.Common;
using RocketLeagueTradingTools.Infrastructure.TradeOffers;
using RocketLeagueTradingTools.Infrastructure.UnitTests.DataSource.Support.Rlg;

namespace RocketLeagueTradingTools.Infrastructure.UnitTests.DataSource;

[TestFixture]
public class RlgDataSourceTests
{
    private RlgDataSource sut = null!;
    private Mock<IHttp> httpClient = null!;
    private Mock<IDateTime> dateTime = null!;
    private CancellationToken cancellationToken;

    [SetUp]
    public void Setup()
    {
        cancellationToken = new CancellationToken();
        httpClient = new Mock<IHttp>();

        dateTime = new Mock<IDateTime>();
        dateTime.SetupGet(d => d.Now).Returns(DateTime.UtcNow);

        sut = new RlgDataSource(httpClient.Object, new Mock<ILog>().Object, dateTime.Object);
    }

    [Test]
    public async Task ScrapPageAsync_buy_offer_source_id_should_be_mapped()
    {
        const string expected = "88c10b46-a29a-4770-8efa-0304d6be8699";

        var page = new RlgPageBuilder();

        page.AddTrade(expected)
            .WithHasItem(Build.Credits(100))
            .WithWantsItem(Build.RlgItem("Hellfire"));

        httpClient.Setup(c => c.GetStringAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => page.Build());

        var offers = await sut.GetTradeOffersPage(cancellationToken);

        offers.BuyOffers.Single().SourceId.Should().Be(expected);
    }

    [Test]
    public async Task ScrapPageAsync_buy_offer_link_should_be_mapped()
    {
        const string offerId = "88c10b46-a29a-4770-8efa-0304d6be8699";
        const string expected = $"https://rocket-league.com/trade/{offerId}";

        var page = new RlgPageBuilder();

        page.AddTrade(offerId)
            .WithHasItem(Build.Credits(100))
            .WithWantsItem(Build.RlgItem("Hellfire"));

        httpClient.Setup(c => c.GetStringAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => page.Build());

        var offers = await sut.GetTradeOffersPage(cancellationToken);

        offers.BuyOffers.Single().Link.Should().Be(expected);
    }

    [Test]
    public async Task ScrapPageAsync_buy_offer_scraped_date_should_be_mapped()
    {
        var expectedDate = new DateTime(2022, 1, 1);

        var page = new RlgPageBuilder();

        page.AddTrade()
            .WithHasItem(Build.Credits(100))
            .WithWantsItem(Build.RlgItem("Hellfire"));

        httpClient.Setup(c => c.GetStringAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => page.Build());
        dateTime.SetupGet(d => d.Now).Returns(expectedDate);

        var offers = await sut.GetTradeOffersPage(cancellationToken);

        offers.BuyOffers.Single().ScrapedDate.Should().Be(expectedDate);
    }

    [Test]
    public async Task ScrapPageAsync_buy_offer_price_should_be_mapped()
    {
        const int expected = 100;

        var page = new RlgPageBuilder();

        page.AddTrade()
            .WithHasItem(Build.Credits(expected))
            .WithWantsItem(Build.RlgItem("Hellfire"));

        httpClient.Setup(c => c.GetStringAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => page.Build());

        var offers = await sut.GetTradeOffersPage(cancellationToken);

        offers.BuyOffers.Single().Price.Should().Be(expected);
    }

    [Test]
    public async Task ScrapPageAsync_buy_offer_item_name_should_be_mapped()
    {
        const string expected = "Hellfire";

        var page = new RlgPageBuilder();

        page.AddTrade()
            .WithHasItem(Build.Credits(100))
            .WithWantsItem(Build.RlgItem(expected));

        httpClient.Setup(c => c.GetStringAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => page.Build());

        var offers = await sut.GetTradeOffersPage(cancellationToken);

        offers.BuyOffers.Single().Item.Name.Should().Be(expected);
    }

    [Test]
    public async Task ScrapPageAsync_buy_offer_item_color_should_be_mapped()
    {
        const string expected = "Titanium White";

        var page = new RlgPageBuilder();

        page.AddTrade()
            .WithHasItem(Build.Credits(1700))
            .WithWantsItem(Build.RlgItem("Fennec").WithColor(expected));

        httpClient.Setup(c => c.GetStringAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => page.Build());

        var offers = await sut.GetTradeOffersPage(cancellationToken);

        offers.BuyOffers.Single().Item.Color.Should().Be(expected);
    }

    [Test]
    public async Task ScrapPageAsync_buy_offer_item_certification_should_be_mapped()
    {
        const string expected = "Goalkeeper";

        var page = new RlgPageBuilder();

        page.AddTrade()
            .WithHasItem(Build.Credits(300))
            .WithWantsItem(Build.RlgItem("Fennec").WithCertification(expected));

        httpClient.Setup(c => c.GetStringAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => page.Build());

        var offers = await sut.GetTradeOffersPage(cancellationToken);

        offers.BuyOffers.Single().Item.Certification.Should().Be(expected);
    }

    [Test]
    public async Task ScrapPageAsync_sell_offer_source_id_should_be_mapped()
    {
        const string expected = "88c10b46-a29a-4770-8efa-0304d6be8699";

        var page = new RlgPageBuilder();

        page.AddTrade(expected)
            .WithHasItem(Build.RlgItem("Hellfire"))
            .WithWantsItem(Build.Credits(100));

        httpClient.Setup(c => c.GetStringAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => page.Build());

        var offers = await sut.GetTradeOffersPage(cancellationToken);

        offers.SellOffers.Single().SourceId.Should().Be(expected);
    }

    [Test]
    public async Task ScrapPageAsync_sell_offer_link_should_be_mapped()
    {
        const string offerId = "88c10b46-a29a-4770-8efa-0304d6be8699";
        const string expected = $"https://rocket-league.com/trade/{offerId}";

        var page = new RlgPageBuilder();

        page.AddTrade(offerId)
            .WithHasItem(Build.RlgItem("Hellfire"))
            .WithWantsItem(Build.Credits(100));

        httpClient.Setup(c => c.GetStringAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => page.Build());

        var offers = await sut.GetTradeOffersPage(cancellationToken);

        offers.SellOffers.Single().Link.Should().Be(expected);
    }

    [Test]
    public async Task ScrapPageAsync_sell_offer_scraped_date_should_be_mapped()
    {
        var expectedDate = new DateTime(2022, 1, 1);

        var page = new RlgPageBuilder();

        page.AddTrade()
            .WithHasItem(Build.RlgItem("Hellfire"))
            .WithWantsItem(Build.Credits(100));

        httpClient.Setup(c => c.GetStringAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => page.Build());
        dateTime.SetupGet(d => d.Now).Returns(expectedDate);

        var offers = await sut.GetTradeOffersPage(cancellationToken);

        offers.SellOffers.Single().ScrapedDate.Should().Be(expectedDate);
    }

    [Test]
    public async Task ScrapPageAsync_sell_offer_price_should_be_mapped()
    {
        const int expected = 100;

        var page = new RlgPageBuilder();

        page.AddTrade()
            .WithHasItem(Build.RlgItem("Hellfire"))
            .WithWantsItem(Build.Credits(expected));

        httpClient.Setup(c => c.GetStringAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => page.Build());

        var offers = await sut.GetTradeOffersPage(cancellationToken);

        offers.SellOffers.Single().Price.Should().Be(expected);
    }

    [Test]
    public async Task ScrapPageAsync_sell_offer_item_name_should_be_mapped()
    {
        const string expected = "Hellfire";

        var page = new RlgPageBuilder();

        page.AddTrade()
            .WithHasItem(Build.RlgItem(expected))
            .WithWantsItem(Build.Credits(100));

        httpClient.Setup(c => c.GetStringAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => page.Build());

        var offers = await sut.GetTradeOffersPage(cancellationToken);

        offers.SellOffers.Single().Item.Name.Should().Be(expected);
    }

    [Test]
    public async Task ScrapPageAsync_sell_offer_item_color_should_be_mapped()
    {
        const string expected = "Titanium White";

        var page = new RlgPageBuilder();

        page.AddTrade()
            .WithHasItem(Build.RlgItem("Fennec").WithColor(expected))
            .WithWantsItem(Build.Credits(1700));

        httpClient.Setup(c => c.GetStringAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => page.Build());

        var offers = await sut.GetTradeOffersPage(cancellationToken);

        offers.SellOffers.Single().Item.Color.Should().Be(expected);
    }

    [Test]
    public async Task ScrapPageAsync_sell_offer_item_certification_should_be_mapped()
    {
        const string expected = "Goalkeeper";

        var page = new RlgPageBuilder();

        page.AddTrade()
            .WithHasItem(Build.RlgItem("Fennec").WithCertification(expected))
            .WithWantsItem(Build.Credits(300));

        httpClient.Setup(c => c.GetStringAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => page.Build());

        var offers = await sut.GetTradeOffersPage(cancellationToken);

        offers.SellOffers.Single().Item.Certification.Should().Be(expected);
    }

    [Test]
    public async Task ScrapPageAsync_should_split_has_credits_wants_multiple_items_trade_into_separate_buy_offers()
    {
        var page = new RlgPageBuilder();

        page.AddTrade()
            .WithHasItem(Build.Credits(500))
            .WithHasItem(Build.Credits(300))
            .WithWantsItem(Build.RlgItem("Dueling Dragons"))
            .WithWantsItem(Build.RlgItem("Fennec"));

        httpClient.Setup(c => c.GetStringAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => page.Build());

        var buyOffers = (await sut.GetTradeOffersPage(cancellationToken)).BuyOffers.ToArray();

        buyOffers.Length.Should().Be(2);
        buyOffers[0].Item.Name.Should().Be("Dueling Dragons");
        buyOffers[0].Price.Should().Be(500);
        buyOffers[1].Item.Name.Should().Be("Fennec");
        buyOffers[1].Price.Should().Be(300);
    }

    [Test]
    public async Task ScrapPageAsync_should_split_has_multiple_items_wants_credits_trade_into_separate_sell_offers()
    {
        var page = new RlgPageBuilder();

        page.AddTrade()
            .WithHasItem(Build.RlgItem("Dueling Dragons"))
            .WithHasItem(Build.RlgItem("Fennec"))
            .WithWantsItem(Build.Credits(550))
            .WithWantsItem(Build.Credits(350));

        httpClient.Setup(c => c.GetStringAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => page.Build());

        var sellOffers = (await sut.GetTradeOffersPage(cancellationToken)).SellOffers.ToArray();

        sellOffers.Length.Should().Be(2);
        sellOffers[0].Item.Name.Should().Be("Dueling Dragons");
        sellOffers[0].Price.Should().Be(550);
        sellOffers[1].Item.Name.Should().Be("Fennec");
        sellOffers[1].Price.Should().Be(350);
    }

    [Test]
    public async Task ScrapPageAsync_should_combine_offers_from_multiple_trades()
    {
        var page = new RlgPageBuilder();

        page.AddTrade()
            .WithHasItem(Build.Credits(60))
            .WithHasItem(Build.Credits(100))
            .WithWantsItem(Build.RlgItem("Supernova III"))
            .WithWantsItem(Build.RlgItem("Hellfire"));

        page.AddTrade()
            .WithHasItem(Build.RlgItem("Dueling Dragons"))
            .WithHasItem(Build.RlgItem("Fennec"))
            .WithWantsItem(Build.Credits(550))
            .WithWantsItem(Build.Credits(350));

        httpClient.Setup(c => c.GetStringAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => page.Build());

        var offers = await sut.GetTradeOffersPage(cancellationToken);
        var buyOffers = offers.BuyOffers.ToArray();
        var sellOffers = offers.SellOffers.ToArray();

        buyOffers.Length.Should().Be(2);
        buyOffers[0].Item.Name.Should().Be("Supernova III");
        buyOffers[0].Price.Should().Be(60);
        buyOffers[1].Item.Name.Should().Be("Hellfire");
        buyOffers[1].Price.Should().Be(100);
        sellOffers.Length.Should().Be(2);
        sellOffers[0].Item.Name.Should().Be("Dueling Dragons");
        sellOffers[0].Price.Should().Be(550);
        sellOffers[1].Item.Name.Should().Be("Fennec");
        sellOffers[1].Price.Should().Be(350);
    }

    [Test]
    public async Task ScrapPageAsync_should_filter_duplicate_buy_offers()
    {
        var page = new RlgPageBuilder();

        page.AddTrade()
            .WithHasItem(Build.Credits(300))
            .WithHasItem(Build.Credits(300))
            .WithWantsItem(Build.RlgItem("Fennec"))
            .WithWantsItem(Build.RlgItem("Fennec"));

        httpClient.Setup(c => c.GetStringAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => page.Build());

        var offers = await sut.GetTradeOffersPage(cancellationToken);

        offers.SellOffers.Should().BeEmpty();
        offers.BuyOffers.Count.Should().Be(1);
        offers.BuyOffers.Single().Item.Name.Should().Be("Fennec");
        offers.BuyOffers.Single().Price.Should().Be(300);
    }

    [Test]
    public async Task ScrapPageAsync_should_filter_duplicate_sell_offers()
    {
        var page = new RlgPageBuilder();

        page.AddTrade()
            .WithHasItem(Build.RlgItem("Fennec"))
            .WithHasItem(Build.RlgItem("Fennec"))
            .WithWantsItem(Build.Credits(350))
            .WithWantsItem(Build.Credits(350));

        httpClient.Setup(c => c.GetStringAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => page.Build());

        var offers = await sut.GetTradeOffersPage(cancellationToken);

        offers.BuyOffers.Should().BeEmpty();
        offers.SellOffers.Count.Should().Be(1);
        offers.SellOffers.Single().Item.Name.Should().Be("Fennec");
        offers.SellOffers.Single().Price.Should().Be(350);
    }

    [Test]
    public async Task ScrapPageAsync_should_filter_non_credits_offers_but_keep_buy_offers()
    {
        var page = new RlgPageBuilder();

        page.AddTrade()
            .WithHasItem(Build.RlgItem("Dueling Dragons"))
            .WithHasItem(Build.Credits(60))
            .WithHasItem(Build.RlgItem("Mainframe"))
            .WithWantsItem(Build.RlgItem("Meteor Storm"))
            .WithWantsItem(Build.RlgItem("Supernova III"))
            .WithWantsItem(Build.RlgItem("Dissolver"));

        httpClient.Setup(c => c.GetStringAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => page.Build());

        var offers = await sut.GetTradeOffersPage(cancellationToken);

        offers.SellOffers.Should().BeEmpty();
        offers.BuyOffers.Single().Item.Name.Should().Be("Supernova III");
        offers.BuyOffers.Single().Price.Should().Be(60);
    }

    [Test]
    public async Task ScrapPageAsync_should_filter_non_credits_offers_but_keep_sell_offers()
    {
        var page = new RlgPageBuilder();

        page.AddTrade()
            .WithHasItem(Build.RlgItem("Dueling Dragons"))
            .WithHasItem(Build.RlgItem("Supernova III"))
            .WithHasItem(Build.RlgItem("Mainframe"))
            .WithWantsItem(Build.RlgItem("Meteor Storm"))
            .WithWantsItem(Build.Credits(90))
            .WithWantsItem(Build.RlgItem("Dissolver"));

        httpClient.Setup(c => c.GetStringAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => page.Build());

        var offers = await sut.GetTradeOffersPage(cancellationToken);

        offers.BuyOffers.Should().BeEmpty();
        offers.SellOffers.Single().Item.Name.Should().Be("Supernova III");
        offers.SellOffers.Single().Price.Should().Be(90);
    }

    [Test]
    public async Task ScrapPageAsync_should_filter_buy_trade_with_mismatching_amount_of_has_and_wants_items()
    {
        var page = new RlgPageBuilder();

        page.AddTrade()
            .WithHasItem(Build.Credits(800))
            .WithWantsItem(Build.RlgItem("Dueling Dragons"))
            .WithWantsItem(Build.RlgItem("Fennec"));

        httpClient.Setup(c => c.GetStringAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => page.Build());

        var offers = await sut.GetTradeOffersPage(cancellationToken);

        offers.BuyOffers.Should().BeEmpty();
        offers.SellOffers.Should().BeEmpty();
    }

    [Test]
    public async Task ScrapPageAsync_should_filter_sell_trade_with_mismatching_amount_of_has_and_wants_items()
    {
        var page = new RlgPageBuilder();

        page.AddTrade()
            .WithHasItem(Build.RlgItem("Dueling Dragons"))
            .WithHasItem(Build.RlgItem("Fennec"))
            .WithWantsItem(Build.Credits(800));

        httpClient.Setup(c => c.GetStringAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => page.Build());

        var offers = await sut.GetTradeOffersPage(cancellationToken);

        offers.BuyOffers.Should().BeEmpty();
        offers.SellOffers.Should().BeEmpty();
    }

    [Test]
    public async Task ScrapPageAsync_should_filter_has_credits_wants_grouped_items_offers()
    {
        var page = new RlgPageBuilder();

        page.AddTrade()
            .WithHasItem(Build.Credits(300))
            .WithHasItem(Build.Credits(100))
            .WithWantsItem(Build.RlgItems("Fennec", 2))
            .WithWantsItem(Build.RlgItem("Hellfire"));

        httpClient.Setup(c => c.GetStringAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => page.Build());

        var offers = await sut.GetTradeOffersPage(cancellationToken);

        offers.SellOffers.Should().BeEmpty();
        offers.BuyOffers.Count.Should().Be(1);
        offers.BuyOffers.Single().Item.Name.Should().Be("Hellfire");
        offers.BuyOffers.Single().Price.Should().Be(100);
    }

    [Test]
    public async Task ScrapPageAsync_should_filter_has_grouped_items_wants_credits_offers()
    {
        var page = new RlgPageBuilder();

        page.AddTrade()
            .WithHasItem(Build.RlgItems("Fennec", 2))
            .WithHasItem(Build.RlgItem("Hellfire"))
            .WithWantsItem(Build.Credits(350))
            .WithWantsItem(Build.Credits(150));

        httpClient.Setup(c => c.GetStringAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => page.Build());

        var offers = await sut.GetTradeOffersPage(cancellationToken);

        offers.BuyOffers.Should().BeEmpty();
        offers.SellOffers.Count.Should().Be(1);
        offers.SellOffers.Single().Item.Name.Should().Be("Hellfire");
        offers.SellOffers.Single().Price.Should().Be(150);
    }

    [Test]
    public async Task ScrapPageAsync_should_filter_has_blueprint_wants_credits_offers()
    {
        var page = new RlgPageBuilder();

        page.AddTrade()
            .WithHasItem(Build.RlgItem("Mainframe Blueprint").WithColor("Titanium White"))
            .WithWantsItem(Build.Credits(6500));

        httpClient.Setup(c => c.GetStringAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => page.Build());

        var offers = await sut.GetTradeOffersPage(cancellationToken);

        offers.BuyOffers.Should().BeEmpty();
        offers.SellOffers.Should().BeEmpty();
    }

    [Test]
    public async Task ScrapPageAsync_should_filter_has_credits_wants_blueprint_offers()
    {
        var page = new RlgPageBuilder();

        page.AddTrade()
            .WithHasItem(Build.Credits(6500))
            .WithWantsItem(Build.RlgItem("Mainframe Blueprint").WithColor("Titanium White"));

        httpClient.Setup(c => c.GetStringAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => page.Build());

        var offers = await sut.GetTradeOffersPage(cancellationToken);

        offers.BuyOffers.Should().BeEmpty();
        offers.SellOffers.Should().BeEmpty();
    }

    [Test]
    public async Task ScrapPageAsync_should_filter_has_painted_set_wants_credits_offers()
    {
        var page = new RlgPageBuilder();

        page.AddTrade()
            .WithHasItem(Build.RlgItem("Grimalkin").WithColor("Painted set"))
            .WithWantsItem(Build.Credits(6000));

        httpClient.Setup(c => c.GetStringAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => page.Build());

        var offers = await sut.GetTradeOffersPage(cancellationToken);

        offers.BuyOffers.Should().BeEmpty();
        offers.SellOffers.Should().BeEmpty();
    }

    [Test]
    public async Task ScrapPageAsync_should_filter_has_credits_wants_painted_set_offers()
    {
        var page = new RlgPageBuilder();

        page.AddTrade()
            .WithHasItem(Build.Credits(5500))
            .WithWantsItem(Build.RlgItem("Grimalkin").WithColor("Painted set"));

        httpClient.Setup(c => c.GetStringAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => page.Build());

        var offers = await sut.GetTradeOffersPage(cancellationToken);

        offers.BuyOffers.Should().BeEmpty();
        offers.SellOffers.Should().BeEmpty();
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
            .WithHasItem(Build.Credits(1000))
            .WithHasItem(Build.Credits(300))
            .WithWantsItem(Build.RlgItem(offerItemName))
            .WithWantsItem(Build.RlgItem("Fennec"));

        httpClient.Setup(c => c.GetStringAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => page.Build());

        var offers = await sut.GetTradeOffersPage(cancellationToken);

        offers.SellOffers.Should().BeEmpty();
        offers.BuyOffers.Count.Should().Be(1);
        offers.BuyOffers.Single().Item.Name.Should().Be("Fennec");
        offers.BuyOffers.Single().Price.Should().Be(300);
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
            .WithHasItem(Build.RlgItem(offerItemName))
            .WithHasItem(Build.RlgItem("Fennec"))
            .WithWantsItem(Build.Credits(1000))
            .WithWantsItem(Build.Credits(350));

        httpClient.Setup(c => c.GetStringAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => page.Build());

        var offers = await sut.GetTradeOffersPage(cancellationToken);

        offers.BuyOffers.Should().BeEmpty();
        offers.SellOffers.Count.Should().Be(1);
        offers.SellOffers.Single().Item.Name.Should().Be("Fennec");
        offers.SellOffers.Single().Price.Should().Be(350);
    }

    [Test]
    public async Task ScrapPageAsync_should_filter_has_credits_without_amount_offers()
    {
        var page = new RlgPageBuilder();

        page.AddTrade()
            .WithHasItem(Build.Credits(0))
            .WithWantsItem(Build.RlgItem("Supernova III"));

        httpClient.Setup(c => c.GetStringAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => page.Build());

        var offers = await sut.GetTradeOffersPage(cancellationToken);

        offers.BuyOffers.Should().BeEmpty();
        offers.SellOffers.Should().BeEmpty();
    }

    [Test]
    public async Task ScrapPageAsync_should_filter_wants_credits_without_amount_offers()
    {
        var page = new RlgPageBuilder();

        page.AddTrade()
            .WithHasItem(Build.RlgItem("Supernova III"))
            .WithWantsItem(Build.Credits(0));

        httpClient.Setup(c => c.GetStringAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => page.Build());

        var offers = await sut.GetTradeOffersPage(cancellationToken);

        offers.BuyOffers.Should().BeEmpty();
        offers.SellOffers.Should().BeEmpty();
    }

    [Test]
    public async Task ScrapPageAsync_should_filter_has_item_or_item_trade()
    {
        var page = new RlgPageBuilder();

        page.AddTrade()
            .WithHasItem(Build.RlgItem("Fennec"))
            .WithHasItem(Build.RlgItem("OR"))
            .WithHasItem(Build.RlgItem("Meteor Storm"))
            .WithWantsItem(Build.Credits(350))
            .WithWantsItem(Build.RlgItem("OR"))
            .WithWantsItem(Build.RlgItem("Dingo"));

        httpClient.Setup(c => c.GetStringAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => page.Build());

        var offers = await sut.GetTradeOffersPage(cancellationToken);

        offers.BuyOffers.Should().BeEmpty();
        offers.SellOffers.Should().BeEmpty();
    }

    [Test]
    public async Task ScrapPageAsync_should_filter_has_credits_or_item_trade()
    {
        var page = new RlgPageBuilder();

        page.AddTrade()
            .WithHasItem(Build.Credits(300))
            .WithHasItem(Build.RlgItem("OR"))
            .WithHasItem(Build.RlgItem("Dingo"))
            .WithWantsItem(Build.RlgItem("Fennec"))
            .WithWantsItem(Build.RlgItem("OR"))
            .WithWantsItem(Build.RlgItem("Meteor Storm"));

        httpClient.Setup(c => c.GetStringAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => page.Build());

        var offers = await sut.GetTradeOffersPage(cancellationToken);

        offers.BuyOffers.Should().BeEmpty();
        offers.SellOffers.Should().BeEmpty();
    }
}
