using FluentAssertions;
using RocketLeagueTradingTools.Core.Domain.Enumerations;
using RocketLeagueTradingTools.Core.Domain.ValueObjects;
using RocketLeagueTradingTools.Infrastructure.IntegrationTests.Support;
using RocketLeagueTradingTools.Infrastructure.Persistence.Repositories;
using RocketLeagueTradingTools.Test;

namespace RocketLeagueTradingTools.Infrastructure.IntegrationTests.Persistence;

[TestFixture]
public class TradeOfferPersistenceRepositoryTests
{
    private TradeOfferPersistenceRepository sut = null!;
    private AlertPersistenceRepository alertRepository = null!;
    private BlacklistPersistenceRepository blacklistRepository = null!;
    private TestContainer testContainer = null!;
    
    [SetUp]
    public void Setup()
    {
        testContainer = TestContainer.Create();
        testContainer.ResetDatabase();
        
        alertRepository = testContainer.GetService<AlertPersistenceRepository>();
        blacklistRepository = testContainer.GetService<BlacklistPersistenceRepository>();
        sut = testContainer.GetService<TradeOfferPersistenceRepository>();
    }
    
    [Test]
    public async Task FindAlertMatchingTradeOffers_should_return_offer_matches_with_Id_mapped()
    {
        // Arrange
        await sut.AddTradeOffers(new[]
        {
            A.ScrapedOffer().WithOffer(
                A.TradeOffer().WithItem(
                    A.TradeItem().WithName("Fennec")
                ).WithPrice(300).WithType(TradeOfferType.Sell)
            ).Build()
        });
        await alertRepository.AddAlert(An.Alert().WithItemName("Fennec").WithPrice(0, 300).WithType(TradeOfferType.Sell).Build());
        
        // Act
        var result = await sut.FindAlertMatchingTradeOffers(TimeSpan.FromMinutes(1));

        // Assert
        result.Single().Id.Should().NotBe(default);
    }

    [Test]
    public async Task FindAlertMatchingTradeOffers_should_return_offer_matches_with_ScrapedDate_mapped()
    {
        // Arrange
        var fakeNow = new DateTime(2022, 1, 1);
        testContainer.NowIs(fakeNow);
        await sut.AddTradeOffers(new[]
        {
            A.ScrapedOffer().WithOffer(
                A.TradeOffer().WithItem(
                    A.TradeItem().WithName("Fennec")
                ).WithPrice(300).WithType(TradeOfferType.Sell)
            ).WithScrapedDate(fakeNow).Build()
        });
        await alertRepository.AddAlert(An.Alert().WithItemName("Fennec").WithPrice(0, 300).WithType(TradeOfferType.Sell).Build());

        // Act
        var result = await sut.FindAlertMatchingTradeOffers(TimeSpan.FromMinutes(1));

        // Assert
        result.Single().ScrapedDate.Should().Be(fakeNow);
    }

    [TestCase(TradeOfferType.Buy)]
    [TestCase(TradeOfferType.Sell)]
    public async Task FindAlertMatchingTradeOffers_should_return_offer_matches_with_TradeOffer_OfferType_mapped(TradeOfferType offerType)
    {
        // Arrange
        await sut.AddTradeOffers(new[]
        {
            A.ScrapedOffer().WithOffer(
                A.TradeOffer().WithItem(
                    A.TradeItem().WithName("Fennec")
                ).WithPrice(300).WithType(offerType)
            ).Build()
        });
        await alertRepository.AddAlert(An.Alert().WithItemName("Fennec").WithPrice(0, 300).WithType(offerType).Build());
        
        // Act
        var result = await sut.FindAlertMatchingTradeOffers(TimeSpan.FromMinutes(1));

        // Assert
        result.Single().TradeOffer.OfferType.Should().Be(offerType);
    }

    [Test]
    public async Task FindAlertMatchingTradeOffers_should_return_offer_matches_with_TradeOffer_Link_mapped()
    {
        // Arrange
        var expectedLink = $"https://rocket-league.com/trade/{Guid.NewGuid()}";
        await sut.AddTradeOffers(new[]
        {
            A.ScrapedOffer().WithOffer(
                A.TradeOffer().WithItem(
                    A.TradeItem().WithName("Fennec")
                ).WithPrice(300).WithType(TradeOfferType.Sell).WithLink(expectedLink)
            ).Build()
        });
        await alertRepository.AddAlert(An.Alert().WithItemName("Fennec").WithPrice(0, 300).WithType(TradeOfferType.Sell).Build());
        
        // Act
        var result = await sut.FindAlertMatchingTradeOffers(TimeSpan.FromMinutes(1));

        // Assert
        result.Single().TradeOffer.Link.Should().Be(expectedLink);
    }

    [Test]
    public async Task FindAlertMatchingTradeOffers_should_return_offer_matches_with_TradeOffer_Price_mapped()
    {
        // Arrange
        await sut.AddTradeOffers(new[]
        {
            A.ScrapedOffer().WithOffer(
                A.TradeOffer().WithItem(
                    A.TradeItem().WithName("Fennec")
                ).WithPrice(300).WithType(TradeOfferType.Sell)
            ).Build()
        });
        await alertRepository.AddAlert(An.Alert().WithItemName("Fennec").WithPrice(0, 300).WithType(TradeOfferType.Sell).Build());
        
        // Act
        var result = await sut.FindAlertMatchingTradeOffers(TimeSpan.FromMinutes(1));

        // Assert
        result.Single().TradeOffer.Price.Should().Be(300);
    }

    [Test]
    public async Task FindAlertMatchingTradeOffers_should_return_offer_matches_with_TradeOffer_Item_Name_mapped()
    {
        // Arrange
        await sut.AddTradeOffers(new[]
        {
            A.ScrapedOffer().WithOffer(
                A.TradeOffer().WithItem(
                    A.TradeItem().WithName("Fennec")
                ).WithPrice(300).WithType(TradeOfferType.Sell)
            ).Build()
        });
        await alertRepository.AddAlert(An.Alert().WithItemName("Fennec").WithPrice(0, 300).WithType(TradeOfferType.Sell).Build());
        
        // Act
        var result = await sut.FindAlertMatchingTradeOffers(TimeSpan.FromMinutes(1));

        // Assert
        result.Single().TradeOffer.Item.Name.Should().Be("Fennec");
    }

    [Test]
    public async Task FindAlertMatchingTradeOffers_should_return_offer_matches_with_TradeOffer_Item_Color_mapped()
    {
        // Arrange
        await sut.AddTradeOffers(new[]
        {
            A.ScrapedOffer().WithOffer(
                A.TradeOffer().WithItem(
                    A.TradeItem().WithName("Fennec").WithColor("Orange")
                ).WithPrice(300).WithType(TradeOfferType.Sell)
            ).Build()
        });
        await alertRepository.AddAlert(
            An.Alert().WithItemName("Fennec").WithPrice(0, 300).WithType(TradeOfferType.Sell).WithColor("*").Build()
        );
        
        // Act
        var result = await sut.FindAlertMatchingTradeOffers(TimeSpan.FromMinutes(1));

        // Assert
        result.Single().TradeOffer.Item.Color.Should().Be("Orange");
    }

    [Test]
    public async Task FindAlertMatchingTradeOffers_should_return_offer_matches_with_TradeOffer_Item_Certification_mapped()
    {
        // Arrange
        await sut.AddTradeOffers(new[]
        {
            A.ScrapedOffer().WithOffer(
                A.TradeOffer().WithItem(
                    A.TradeItem().WithName("Fennec").WithCertification("Sniper")
                ).WithPrice(300).WithType(TradeOfferType.Sell)
            ).Build()
        });
        await alertRepository.AddAlert(
            An.Alert().WithItemName("Fennec").WithPrice(0, 300).WithType(TradeOfferType.Sell).WithCertification("*").Build()
        );
        
        // Act
        var result = await sut.FindAlertMatchingTradeOffers(TimeSpan.FromMinutes(1));

        // Assert
        result.Single().TradeOffer.Item.Certification.Should().Be("Sniper");
    }

    [TestCase(TradeItemType.Body)]
    [TestCase(TradeItemType.Decal)]
    [TestCase(TradeItemType.PaintFinish)]
    [TestCase(TradeItemType.Wheels)]
    [TestCase(TradeItemType.RocketBoost)]
    [TestCase(TradeItemType.Topper)]
    [TestCase(TradeItemType.Antenna)]
    [TestCase(TradeItemType.GoalExplosion)]
    [TestCase(TradeItemType.Trail)]
    [TestCase(TradeItemType.Banner)]
    [TestCase(TradeItemType.AvatarBorder)]
    public async Task FindAlertMatchingTradeOffers_should_return_offer_matches_with_TradeOffer_Item_ItemType_mapped(TradeItemType itemType)
    {
        // Arrange
        await sut.AddTradeOffers(new[]
        {
            A.ScrapedOffer().WithOffer(
                A.TradeOffer().WithItem(
                    A.TradeItem().WithName("Fennec").WithType(itemType)
                ).WithPrice(300).WithType(TradeOfferType.Sell)
            ).Build()
        });
        await alertRepository.AddAlert(An.Alert().WithItemName("Fennec").WithPrice(0, 300).WithType(TradeOfferType.Sell).Build());
        
        // Act
        var result = await sut.FindAlertMatchingTradeOffers(TimeSpan.FromMinutes(1));

        // Assert
        result.Single().TradeOffer.Item.ItemType.Should().Be(itemType);
    }

    [Test]
    public async Task FindAlertMatchingTradeOffers_should_return_offer_matches_with_TradeOffer_Trader_mapped()
    {
        // Arrange
        var expectedTrader = new Trader(TradingSite.RocketLeagueGarage, Guid.NewGuid().ToString());
        await sut.AddTradeOffers(new[]
        {
            A.ScrapedOffer().WithOffer(
                A.TradeOffer().WithItem(
                    A.TradeItem().WithName("Fennec")
                ).WithPrice(300).WithType(TradeOfferType.Sell).WithTrader(expectedTrader)
            ).Build()
        });
        await alertRepository.AddAlert(An.Alert().WithItemName("Fennec").WithPrice(0, 300).WithType(TradeOfferType.Sell).Build());
        
        // Act
        var result = await sut.FindAlertMatchingTradeOffers(TimeSpan.FromMinutes(1));

        // Assert
        result.Single().TradeOffer.Trader.Should().Be(expectedTrader);
    }

    [TestCase(TradeOfferType.Buy, TradeOfferType.Buy, true)]
    [TestCase(TradeOfferType.Buy, TradeOfferType.Sell, false)]
    public async Task FindAlertMatchingTradeOffers_should_return_offers_matching_by_offer_type(TradeOfferType alertOfferType, TradeOfferType tradeOfferType, bool expected)
    {
        // Arrange
        await sut.AddTradeOffers(new[]
        {
            A.ScrapedOffer().WithOffer(
                A.TradeOffer().WithItem(
                    A.TradeItem().WithName("Fennec")
                ).WithPrice(300).WithType(tradeOfferType)
            ).Build()
        });
        await alertRepository.AddAlert(An.Alert().WithItemName("Fennec").WithPrice(0, 300).WithType(alertOfferType).Build());
        
        // Act
        var result = await sut.FindAlertMatchingTradeOffers(TimeSpan.FromMinutes(1));

        // Assert
        result.Count.Should().Be(expected ? 1 : 0);
    }

    [TestCase("Hellfire", "Hellfire", true)]
    [TestCase("Hellfire", "hellfire", true)]
    [TestCase("hellfire", "Hellfire", true)]
    [TestCase("Hellfire", "Fennec", false)]
    [TestCase("Hell", "Hellfire", false)]
    public async Task FindAlertMatchingTradeOffers_should_return_offers_matching_by_item_name(string alertItemName, string offerItemName, bool expected)
    {
        // Arrange
        await sut.AddTradeOffers(new[]
        {
            A.ScrapedOffer().WithOffer(
                A.TradeOffer().WithItem(
                    A.TradeItem().WithName(offerItemName)
                ).WithPrice(300).WithType(TradeOfferType.Sell)
            ).Build()
        });
        await alertRepository.AddAlert(An.Alert().WithItemName(alertItemName).WithPrice(0, 300).WithType(TradeOfferType.Sell).Build());
        
        // Act
        var result = await sut.FindAlertMatchingTradeOffers(TimeSpan.FromMinutes(1));

        // Assert
        result.Count.Should().Be(expected ? 1 : 0);
    }

    [TestCase(150, 200, 150, true)]
    [TestCase(150, 200, 160, true)]
    [TestCase(150, 200, 140, false)]
    [TestCase(150, 200, 210, false)]
    public async Task FindAlertMatchingTradeOffers_should_return_offers_matching_by_price(int alertPriceFrom, int alertPriceTo, int offerPrice, bool expected)
    {
        // Arrange
        await sut.AddTradeOffers(new[]
        {
            A.ScrapedOffer().WithOffer(
                A.TradeOffer().WithItem(
                    A.TradeItem().WithName("Fennec")
                ).WithPrice(offerPrice).WithType(TradeOfferType.Sell)
            ).Build()
        });
        await alertRepository.AddAlert(An.Alert().WithItemName("Fennec").WithPrice(alertPriceFrom, alertPriceTo).WithType(TradeOfferType.Sell).Build());
        
        // Act
        var result = await sut.FindAlertMatchingTradeOffers(TimeSpan.FromMinutes(1));

        // Assert
        result.Count.Should().Be(expected ? 1 : 0);
    }

    [TestCase(AlertItemType.Body, TradeItemType.Body)]
    [TestCase(AlertItemType.Decal, TradeItemType.Decal)]
    [TestCase(AlertItemType.PaintFinish, TradeItemType.PaintFinish)]
    [TestCase(AlertItemType.Wheels, TradeItemType.Wheels)]
    [TestCase(AlertItemType.RocketBoost, TradeItemType.RocketBoost)]
    [TestCase(AlertItemType.Topper, TradeItemType.Topper)]
    [TestCase(AlertItemType.Antenna, TradeItemType.Antenna)]
    [TestCase(AlertItemType.GoalExplosion, TradeItemType.GoalExplosion)]
    [TestCase(AlertItemType.Trail, TradeItemType.Trail)]
    [TestCase(AlertItemType.Banner, TradeItemType.Banner)]
    [TestCase(AlertItemType.AvatarBorder, TradeItemType.AvatarBorder)]
    [TestCase(AlertItemType.Any, TradeItemType.Unknown)]
    [TestCase(AlertItemType.Any, TradeItemType.Body)]
    [TestCase(AlertItemType.Any, TradeItemType.Decal)]
    [TestCase(AlertItemType.Any, TradeItemType.PaintFinish)]
    [TestCase(AlertItemType.Any, TradeItemType.Wheels)]
    [TestCase(AlertItemType.Any, TradeItemType.RocketBoost)]
    [TestCase(AlertItemType.Any, TradeItemType.Topper)]
    [TestCase(AlertItemType.Any, TradeItemType.Antenna)]
    [TestCase(AlertItemType.Any, TradeItemType.GoalExplosion)]
    [TestCase(AlertItemType.Any, TradeItemType.Trail)]
    [TestCase(AlertItemType.Any, TradeItemType.Banner)]
    [TestCase(AlertItemType.Any, TradeItemType.AvatarBorder)]
    public async Task FindAlertMatchingTradeOffers_should_return_offers_matching_by_item_type(AlertItemType alertItemType, TradeItemType offerItemType)
    {
        // Arrange
        await sut.AddTradeOffers(new[]
        {
            A.ScrapedOffer().WithOffer(
                A.TradeOffer().WithItem(
                    A.TradeItem().WithName("Fennec").WithType(offerItemType)
                ).WithPrice(300).WithType(TradeOfferType.Sell)
            ).Build()
        });
        await alertRepository.AddAlert(
            An.Alert().WithItemName("Fennec").WithPrice(0, 300).WithType(TradeOfferType.Sell).WithItemType(alertItemType)
                .Build()
        );
        
        // Act
        var result = await sut.FindAlertMatchingTradeOffers(TimeSpan.FromMinutes(1));

        // Assert
        result.Count.Should().Be(1);
    }

    [TestCase("Sky Blue", "Sky Blue", true)]
    [TestCase("Sky Blue", "Sky blue", true)]
    [TestCase("Sky blue", "Sky Blue", true)]
    [TestCase("*", "Sky Blue", true)]
    [TestCase("*", "", true)]
    [TestCase("+", "Sky Blue", true)]
    [TestCase("", "", true)]
    [TestCase("Lime", "Sky Blue", false)]
    [TestCase("Sky", "Sky Blue", false)]
    [TestCase("", "Sky Blue", false)]
    [TestCase("+", "", false)]
    public async Task FindAlertMatchingTradeOffers_should_return_offers_matching_by_color(string alertColor, string offerColor, bool expected)
    {
        // Arrange
        await sut.AddTradeOffers(new[]
        {
            A.ScrapedOffer().WithOffer(
                A.TradeOffer().WithItem(
                    A.TradeItem().WithName("Fennec").WithColor(offerColor)
                ).WithPrice(300).WithType(TradeOfferType.Sell)
            ).Build()
        });
        await alertRepository.AddAlert(
            An.Alert().WithItemName("Fennec").WithPrice(0, 300).WithType(TradeOfferType.Sell).WithColor(alertColor).Build()
        );
        
        // Act
        var result = await sut.FindAlertMatchingTradeOffers(TimeSpan.FromMinutes(1));

        // Assert
        result.Count.Should().Be(expected ? 1 : 0);
    }

    [TestCase("Guardian", "Guardian", true)]
    [TestCase("Guardian", "guardian", true)]
    [TestCase("guardian", "Guardian", true)]
    [TestCase("*", "Guardian", true)]
    [TestCase("*", "", true)]
    [TestCase("+", "Guardian", true)]
    [TestCase("", "", true)]
    [TestCase("Sniper", "Guardian", false)]
    [TestCase("Guard", "Guardian", false)]
    [TestCase("", "Guardian", false)]
    [TestCase("+", "", false)]
    public async Task FindAlertMatchingTradeOffers_should_return_offers_matching_by_certification(string alertCertification, string offerCertification, bool expected)
    {
        // Arrange
        await sut.AddTradeOffers(new[]
        {
            A.ScrapedOffer().WithOffer(
                A.TradeOffer().WithItem(
                    A.TradeItem().WithName("Fennec").WithCertification(offerCertification)
                ).WithPrice(300).WithType(TradeOfferType.Sell)
            ).Build()
        });
        await alertRepository.AddAlert(
            An.Alert().WithItemName("Fennec").WithPrice(0, 300).WithType(TradeOfferType.Sell).WithCertification(alertCertification).Build()
        );
        
        // Act
        var result = await sut.FindAlertMatchingTradeOffers(TimeSpan.FromMinutes(1));

        // Assert
        result.Count.Should().Be(expected ? 1 : 0);
    }

    [Test]
    public async Task FindAlertMatchingTradeOffers_should_not_return_old_offers()
    {
        // Arrange
        var fakeNow = new DateTime(2022, 1, 1);
        await sut.AddTradeOffers(new[]
        {
            A.ScrapedOffer().WithOffer(
                A.TradeOffer().WithItem(
                    A.TradeItem().WithName("Fennec")
                ).WithPrice(300).WithType(TradeOfferType.Sell)
            ).WithScrapedDate(fakeNow).Build()
        });
        await alertRepository.AddAlert(An.Alert().WithItemName("Fennec").WithPrice(0, 300).WithType(TradeOfferType.Sell).Build());
        testContainer.NowIs(fakeNow.AddMinutes(10));
        
        // Act
        var result = await sut.FindAlertMatchingTradeOffers(TimeSpan.FromMinutes(9));

        // Assert
        result.Count.Should().Be(0);
    }

    [Test]
    public async Task FindAlertMatchingTradeOffers_should_not_return_matching_offers_for_disabled_alerts()
    {
        // Arrange
        await sut.AddTradeOffers(new[]
        {
            A.ScrapedOffer().WithOffer(
                A.TradeOffer().WithItem(
                    A.TradeItem().WithName("Fennec")
                ).WithPrice(300).WithType(TradeOfferType.Sell)
            ).Build()
        });
        var alertId = await alertRepository.AddAlert(
            An.Alert().WithItemName("Fennec").WithPrice(0, 300).WithType(TradeOfferType.Sell).Build()
        );
        var alert = await alertRepository.GetAlert(alertId);
        alert!.Enabled = false;
        await alertRepository.UpdateAlert(alert);
        
        // Act
        var result = await sut.FindAlertMatchingTradeOffers(TimeSpan.FromMinutes(1));

        // Assert
        result.Count.Should().Be(0);
    }

    [TestCase("AnnoyingSpammer", "AnnoyingSpammer")]
    [TestCase("annoyingSpammer", "AnnoyingSpammer")]
    public async Task FindAlertMatchingTradeOffers_should_not_return_matching_offer_from_blacklisted_traders(string offerTrader, string blacklistedTrader)
    {
        // Arrange
        await sut.AddTradeOffers(new[]
        {
            A.ScrapedOffer().WithOffer(
                A.TradeOffer().WithItem(
                    A.TradeItem().WithName("Fennec")
                ).WithPrice(300).WithType(TradeOfferType.Sell).WithTrader(
                    A.Trader().WithName(offerTrader)
                )
            ).Build()
        });
        await alertRepository.AddAlert(An.Alert().WithItemName("Fennec").WithPrice(0, 300).WithType(TradeOfferType.Sell).Build());
        await blacklistRepository.AddBlacklistedTrader(A.Trader().WithName(blacklistedTrader).Build());
        
        // Act
        var result = await sut.FindAlertMatchingTradeOffers(TimeSpan.FromMinutes(1));

        // Assert
        result.Count.Should().Be(0);
    }
}