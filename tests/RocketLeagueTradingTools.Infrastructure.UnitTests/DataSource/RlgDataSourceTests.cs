using FluentAssertions;
using Moq;
using RocketLeagueTradingTools.Core.Application.Contracts;
using RocketLeagueTradingTools.Infrastructure.Common;
using RocketLeagueTradingTools.Infrastructure.TradeOffers;
using RocketLeagueTradingTools.Infrastructure.UnitTests.DataSource.Support.Rlg;

namespace RocketLeagueTradingTools.Infrastructure.UnitTests.DataSource;

[TestFixture]
public class RlgDataSourceTests
{
    private RlgDataSource sut;
    private Mock<IHttp> httpClient;
    private CancellationToken cancellationToken;

    [SetUp]
    public void Setup()
    {
        cancellationToken = new CancellationToken();
        httpClient = new Mock<IHttp>();

        sut = new RlgDataSource(httpClient.Object, new Mock<ILog>().Object);
    }

    [Test]
    public async Task ScrapPageAsync_buy_offer_source_id_should_be_mapped()
    {
        const string expected = "88c10b46-a29a-4770-8efa-0304d6be8699";

        var page = new RlgPageBuilder();

        page.AddTrade(expected)
            .WithHasItem(RlgItem.Credits(100))
            .WithWantsItem(RlgItem.Item("Hellfire"));

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
            .WithHasItem(RlgItem.Credits(100))
            .WithWantsItem(RlgItem.Item("Hellfire"));

        httpClient.Setup(c => c.GetStringAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => page.Build());

        var offers = await sut.GetTradeOffersPage(cancellationToken);

        offers.BuyOffers.Single().Link.Should().Be(expected);
    }

    [Test]
    public async Task ScrapPageAsync_buy_offer_price_should_be_mapped()
    {
        const int expected = 100;

        var page = new RlgPageBuilder();

        page.AddTrade()
            .WithHasItem(RlgItem.Credits(expected))
            .WithWantsItem(RlgItem.Item("Hellfire"));

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
            .WithHasItem(RlgItem.Credits(100))
            .WithWantsItem(RlgItem.Item(expected));

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
            .WithHasItem(RlgItem.Credits(1700))
            .WithWantsItem(RlgItem.Item("Fennec").WithColor(expected));

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
            .WithHasItem(RlgItem.Credits(300))
            .WithWantsItem(RlgItem.Item("Fennec").WithCertification(expected));

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
            .WithHasItem(RlgItem.Item("Hellfire"))
            .WithWantsItem(RlgItem.Credits(100));

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
            .WithHasItem(RlgItem.Item("Hellfire"))
            .WithWantsItem(RlgItem.Credits(100));

        httpClient.Setup(c => c.GetStringAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => page.Build());

        var offers = await sut.GetTradeOffersPage(cancellationToken);

        offers.SellOffers.Single().Link.Should().Be(expected);
    }

    [Test]
    public async Task ScrapPageAsync_sell_offer_price_should_be_mapped()
    {
        const int expected = 100;

        var page = new RlgPageBuilder();

        page.AddTrade()
            .WithHasItem(RlgItem.Item("Hellfire"))
            .WithWantsItem(RlgItem.Credits(expected));

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
            .WithHasItem(RlgItem.Item(expected))
            .WithWantsItem(RlgItem.Credits(100));

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
            .WithHasItem(RlgItem.Item("Fennec").WithColor(expected))
            .WithWantsItem(RlgItem.Credits(1700));

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
            .WithHasItem(RlgItem.Item("Fennec").WithCertification(expected))
            .WithWantsItem(RlgItem.Credits(300));

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
            .WithHasItem(RlgItem.Credits(500))
            .WithHasItem(RlgItem.Credits(300))
            .WithWantsItem(RlgItem.Item("Dueling Dragons"))
            .WithWantsItem(RlgItem.Item("Fennec"));

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
            .WithHasItem(RlgItem.Item("Dueling Dragons"))
            .WithHasItem(RlgItem.Item("Fennec"))
            .WithWantsItem(RlgItem.Credits(550))
            .WithWantsItem(RlgItem.Credits(350));

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
            .WithHasItem(RlgItem.Credits(60))
            .WithHasItem(RlgItem.Credits(100))
            .WithWantsItem(RlgItem.Item("Supernova III"))
            .WithWantsItem(RlgItem.Item("Hellfire"));

        page.AddTrade()
            .WithHasItem(RlgItem.Item("Dueling Dragons"))
            .WithHasItem(RlgItem.Item("Fennec"))
            .WithWantsItem(RlgItem.Credits(550))
            .WithWantsItem(RlgItem.Credits(350));

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
    public async Task ScrapPageAsync_should_filter_non_credits_offers_but_keep_buy_offers()
    {
        var page = new RlgPageBuilder();

        page.AddTrade()
            .WithHasItem(RlgItem.Item("Dueling Dragons"))
            .WithHasItem(RlgItem.Credits(60))
            .WithHasItem(RlgItem.Item("Mainframe"))
            .WithWantsItem(RlgItem.Item("Meteor Storm"))
            .WithWantsItem(RlgItem.Item("Supernova III"))
            .WithWantsItem(RlgItem.Item("Dissolver"));

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
            .WithHasItem(RlgItem.Item("Dueling Dragons"))
            .WithHasItem(RlgItem.Item("Supernova III"))
            .WithHasItem(RlgItem.Item("Mainframe"))
            .WithWantsItem(RlgItem.Item("Meteor Storm"))
            .WithWantsItem(RlgItem.Credits(90))
            .WithWantsItem(RlgItem.Item("Dissolver"));

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
            .WithHasItem(RlgItem.Credits(800))
            .WithWantsItem(RlgItem.Item("Dueling Dragons"))
            .WithWantsItem(RlgItem.Item("Fennec"));

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
            .WithHasItem(RlgItem.Item("Dueling Dragons"))
            .WithHasItem(RlgItem.Item("Fennec"))
            .WithWantsItem(RlgItem.Credits(800));

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
            .WithHasItem(RlgItem.Credits(300))
            .WithHasItem(RlgItem.Credits(100))
            .WithWantsItem(RlgItem.Items("Fennec", 2))
            .WithWantsItem(RlgItem.Item("Hellfire"));

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
            .WithHasItem(RlgItem.Items("Fennec", 2))
            .WithHasItem(RlgItem.Item("Hellfire"))
            .WithWantsItem(RlgItem.Credits(350))
            .WithWantsItem(RlgItem.Credits(150));

        httpClient.Setup(c => c.GetStringAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => page.Build());

        var offers = await sut.GetTradeOffersPage(cancellationToken);

        offers.BuyOffers.Should().BeEmpty();
        offers.SellOffers.Count.Should().Be(1);
        offers.SellOffers.Single().Item.Name.Should().Be("Hellfire");
        offers.SellOffers.Single().Price.Should().Be(150);
    }

    [Test]
    public async Task ScrapPageAsync_should_filter_has_credits_wants_blueprint_offers()
    {
        var page = new RlgPageBuilder();

        page.AddTrade()
            .WithHasItem(RlgItem.Credits(6500))
            .WithWantsItem(RlgItem.Item("Mainframe Blueprint").WithColor("Titanium White"));

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
            .WithHasItem(RlgItem.Item("Grimalkin").WithColor("Painted set"))
            .WithWantsItem(RlgItem.Credits(6000));

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
            .WithHasItem(RlgItem.Credits(5500))
            .WithWantsItem(RlgItem.Item("Grimalkin").WithColor("Painted set"));

        httpClient.Setup(c => c.GetStringAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => page.Build());

        var offers = await sut.GetTradeOffersPage(cancellationToken);

        offers.BuyOffers.Should().BeEmpty();
        offers.SellOffers.Should().BeEmpty();
    }

    [Test]
    public async Task ScrapPageAsync_should_filter_has_blueprint_wants_credits__offers()
    {
        var page = new RlgPageBuilder();

        page.AddTrade()
            .WithHasItem(RlgItem.Item("Mainframe Blueprint").WithColor("Titanium White"))
            .WithWantsItem(RlgItem.Credits(6500));

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
            .WithHasItem(RlgItem.Credits(1000))
            .WithHasItem(RlgItem.Credits(300))
            .WithWantsItem(RlgItem.Item(offerItemName))
            .WithWantsItem(RlgItem.Item("Fennec"));

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
            .WithHasItem(RlgItem.Item(offerItemName))
            .WithHasItem(RlgItem.Item("Fennec"))
            .WithWantsItem(RlgItem.Credits(1000))
            .WithWantsItem(RlgItem.Credits(350));

        httpClient.Setup(c => c.GetStringAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => page.Build());

        var offers = await sut.GetTradeOffersPage(cancellationToken);

        offers.BuyOffers.Should().BeEmpty();
        offers.SellOffers.Count.Should().Be(1);
        offers.SellOffers.Single().Item.Name.Should().Be("Fennec");
        offers.SellOffers.Single().Price.Should().Be(350);
    }

    [Test]
    public async Task ScrapPageAsync_should_filter_has_item_or_item_trade()
    {
        var page = new RlgPageBuilder();

        page.AddTrade()
            .WithHasItem(RlgItem.Item("Fennec"))
            .WithHasItem(RlgItem.Item("OR"))
            .WithHasItem(RlgItem.Item("Meteor Storm"))
            .WithWantsItem(RlgItem.Credits(350))
            .WithWantsItem(RlgItem.Item("OR"))
            .WithWantsItem(RlgItem.Item("Dingo"));

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
            .WithHasItem(RlgItem.Credits(300))
            .WithHasItem(RlgItem.Item("OR"))
            .WithHasItem(RlgItem.Item("Dingo"))
            .WithWantsItem(RlgItem.Item("Fennec"))
            .WithWantsItem(RlgItem.Item("OR"))
            .WithWantsItem(RlgItem.Item("Meteor Storm"));

        httpClient.Setup(c => c.GetStringAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => page.Build());

        var offers = await sut.GetTradeOffersPage(cancellationToken);

        offers.BuyOffers.Should().BeEmpty();
        offers.SellOffers.Should().BeEmpty();
    }
}
