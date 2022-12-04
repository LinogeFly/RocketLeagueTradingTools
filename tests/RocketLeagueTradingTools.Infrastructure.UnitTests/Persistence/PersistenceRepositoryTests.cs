using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using RocketLeagueTradingTools.Core.Application.Interfaces;
using RocketLeagueTradingTools.Core.Domain.Enumerations;
using RocketLeagueTradingTools.Infrastructure.Persistence;
using RocketLeagueTradingTools.Infrastructure.UnitTests.Persistence.Support;

namespace RocketLeagueTradingTools.Infrastructure.UnitTests.Persistence;

[TestFixture]
public class PersistenceRepositoryTests
{
    private PersistenceRepository sut = null!;
    private IDbContextFactory<PersistenceDbContext> dbContextFactory = null!;
    private SqliteConnection keepAliveConnection = null!; 
    private Mock<IDateTime> dateTime = null!;

    [SetUp]
    public void Setup()
    {
        dateTime = new Mock<IDateTime>();
        dateTime.SetupGet(d => d.Now).Returns(DateTime.UtcNow);

        // Initialize SQLite in-memory database with open connection.
        // The connection is closed at the end of each test run in the TearDown method.
        // Otherwise we need to open connection explicitly in every test.
        keepAliveConnection = new SqliteConnection("Filename=:memory:");
        keepAliveConnection.Open();
        var dbContextOptions = new DbContextOptionsBuilder<PersistenceDbContext>()
            .UseSqlite(keepAliveConnection)
            .Options;
        
        // Setting up DbContextFactory object with mocked CreateDbContextAsync method.
        var dbContextFactoryMock = new Mock<IDbContextFactory<PersistenceDbContext>>();
        dbContextFactoryMock.Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>())).ReturnsAsync(() =>
        {
            var dbContext = new PersistenceDbContext(dbContextOptions);
            dbContext.Database.EnsureCreated();
            
            return dbContext;
        });
        dbContextFactory = dbContextFactoryMock.Object;

        sut = new PersistenceRepository(dbContextFactory, dateTime.Object);
    }

    [TearDown]
    public void TearDown()
    {
        keepAliveConnection.Close();
    }
    
    [Test]
    public async Task GetNotifications_should_return_notifications_with_properties_mapped()
    {
        var dbContext = await dbContextFactory.CreateDbContextAsync();
        dbContext.AddRange(A.Notification().With(n =>
        {
            n.Id = 1;
            n.SeenDate = new DateTime(2022, 1, 1);
            n.TradeOffer.Id = 2;
            n.TradeOffer.Link = "https://rocket-league.com/trade/88c10b46-a29a-4770-8efa-0304d6be8699";
            n.TradeOffer.ScrapedDate = new DateTime(2022, 1, 2);
            n.TradeOffer.OfferType = "Sell";
            n.TradeOffer.ItemName = "Fennec";
            n.TradeOffer.ItemType = "Body";
            n.TradeOffer.Price = 500;
            n.TradeOffer.Color = "Grey";
            n.TradeOffer.Certification = "Sniper";
            n.TradeOffer.TradingSite = "RLG";
            n.TradeOffer.TraderName = "RLTrader69";
        }));
        await dbContext.SaveChangesAsync();
    
        var notification = (await sut.GetNotifications(20)).Single();
    
        notification.SeenDate.Should().Be(new DateTime(2022, 1, 1));
        notification.Id.Should().Be(1);
        notification.ScrapedTradeOffer.Id.Should().Be(2);
        notification.ScrapedTradeOffer.ScrapedDate.Should().Be(new DateTime(2022, 1, 2));
        notification.ScrapedTradeOffer.TradeOffer.OfferType.Should().Be(TradeOfferType.Sell);
        notification.ScrapedTradeOffer.TradeOffer.Item.Name.Should().Be("Fennec");
        notification.ScrapedTradeOffer.TradeOffer.Item.ItemType.Should().Be(TradeItemType.Body);
        notification.ScrapedTradeOffer.TradeOffer.Item.Color.Should().Be("Grey");
        notification.ScrapedTradeOffer.TradeOffer.Item.Certification.Should().Be("Sniper");
        notification.ScrapedTradeOffer.TradeOffer.Price.Should().Be(500);
        notification.ScrapedTradeOffer.TradeOffer.Link.Should().Be("https://rocket-league.com/trade/88c10b46-a29a-4770-8efa-0304d6be8699");
        notification.ScrapedTradeOffer.TradeOffer.Trader.TradingSite.Should().Be(TradingSite.RocketLeagueGarage);
        notification.ScrapedTradeOffer.TradeOffer.Trader.Name.Should().Be("RLTrader69");
    }
    
    [Test]
    public async Task GetNotifications_should_return_notifications_page()
    {
        var dbContext = await dbContextFactory.CreateDbContextAsync();
        dbContext.AddRange(A.Notification(), A.Notification());
        await dbContext.SaveChangesAsync();
    
        var notifications = await sut.GetNotifications(1);
    
        notifications.Count.Should().Be(1);
    }
    
    [Test]
    public async Task GetNotifications_should_return_notifications_ordered_by_created_date_newest_first()
    {
        var dbContext = await dbContextFactory.CreateDbContextAsync();
        dbContext.AddRange(A.Notification().With(n =>
            {
                n.Id = 1;
                n.CreatedDate = new DateTime(2022, 1, 1);
            }),
            A.Notification().With(n =>
            {
                n.Id = 2;
                n.CreatedDate = new DateTime(2022, 1, 2);
            })
        );
        await dbContext.SaveChangesAsync();
    
        var notifications = await sut.GetNotifications(1);
    
        notifications.Count.Should().Be(1);
        notifications.Single().Id.Should().Be(2);
    }
    
    [Test]
    public async Task GetNotifications_should_not_return_expired_notifications()
    {
        var dbContext = await dbContextFactory.CreateDbContextAsync();
        dbContext.AddRange(A.Notification().With(n =>
            {
                n.Id = 1;
                n.CreatedDate = DateTime.UtcNow.AddHours(-2);
            }),
            A.Notification().With(n =>
            {
                n.Id = 2;
                n.CreatedDate = DateTime.UtcNow;
            })
        );
        await dbContext.SaveChangesAsync();
    
        var notifications = await sut.GetNotifications(TimeSpan.FromHours(1));
    
        notifications.Count.Should().Be(1);
        notifications.Single().Id.Should().Be(2);
    }
    
    [Test]
    public async Task GetNotificationsCount_should_return_total_notifications_count()
    {
        var dbContext = await dbContextFactory.CreateDbContextAsync();
        dbContext.AddRange(A.Notification(), A.Notification());
        await dbContext.SaveChangesAsync();
    
        var count = await sut.GetNotificationsCount();
    
        count.Should().Be(2);
    }
    
    [Test]
    public async Task FindAlertMatchingOffers_should_return_offer_matches_with_ScrapedDate_mapped()
    {
        var expected = DateTime.UtcNow;
        var dbContext = await dbContextFactory.CreateDbContextAsync();
        dbContext.AddRange(An.Alert());
        dbContext.AddRange(A.TradeOffer().With(o =>
        {
            o.ScrapedDate = expected;
        }));
        await dbContext.SaveChangesAsync();
    
        var offers = await sut.FindAlertMatchingOffers(TimeSpan.FromMinutes(20));
    
        offers.Single().ScrapedDate.Should().Be(expected);
    }
    
    [TestCase("Buy", TradeOfferType.Buy)]
    [TestCase("Sell", TradeOfferType.Sell)]
    public async Task FindAlertMatchingOffers_should_return_offer_matches_with_TradeOffer_OfferType_mapped(string offerType, TradeOfferType expected)
    {
        var dbContext = await dbContextFactory.CreateDbContextAsync();
        dbContext.AddRange(An.Alert().With(a =>
        {
            a.OfferType = offerType;
        }));
        dbContext.AddRange(A.TradeOffer().With(o =>
        {
            o.OfferType = offerType;
        }));
        await dbContext.SaveChangesAsync();
    
        var offers = await sut.FindAlertMatchingOffers(TimeSpan.FromMinutes(20));
    
        offers.Single().TradeOffer.OfferType.Should().Be(expected);
    }
    
    [Test]
    public async Task FindAlertMatchingOffers_should_return_offer_matches_with_TradeOffer_Link_mapped()
    {
        const string expected = "https://rocket-league.com/trade/1";
    
        var dbContext = await dbContextFactory.CreateDbContextAsync();
        dbContext.AddRange(An.Alert());
        dbContext.AddRange(A.TradeOffer().With(o =>
        {
            o.Link = expected;
        }));
        await dbContext.SaveChangesAsync();
    
        var offers = await sut.FindAlertMatchingOffers(TimeSpan.FromMinutes(20));
    
        offers.Single().TradeOffer.Link.Should().Be(expected);
    }
    
    [Test]
    public async Task FindAlertMatchingOffers_should_return_offer_matches_with_TradeOffer_Price_mapped()
    {
        const int expected = 300;
    
        var dbContext = await dbContextFactory.CreateDbContextAsync();
        dbContext.AddRange(An.Alert().With(a =>
        {
            a.PriceTo = expected;
        }));
        dbContext.AddRange(A.TradeOffer().With(o =>
        {
            o.Price = expected;
        }));
        await dbContext.SaveChangesAsync();
    
        var offers = await sut.FindAlertMatchingOffers(TimeSpan.FromMinutes(20));
    
        offers.Single().TradeOffer.Price.Should().Be(expected);
    }
    
    [Test]
    public async Task FindAlertMatchingOffers_should_return_offer_matches_with_TradeOffer_Item_Name_mapped()
    {
        const string expected = "Fennec";
    
        var dbContext = await dbContextFactory.CreateDbContextAsync();
        dbContext.AddRange(An.Alert().With(a =>
        {
            a.ItemName = expected;
        }));
        dbContext.AddRange(A.TradeOffer().With(o =>
        {
            o.ItemName = expected;
        }));
        await dbContext.SaveChangesAsync();
    
        var offers = await sut.FindAlertMatchingOffers(TimeSpan.FromMinutes(20));
    
        offers.Single().TradeOffer.Item.Name.Should().Be(expected);
    }
    
    [Test]
    public async Task FindAlertMatchingOffers_should_return_offer_matches_with_TradeOffer_Item_Color_mapped()
    {
        const string expected = "Grey";
    
        var dbContext = await dbContextFactory.CreateDbContextAsync();
        dbContext.AddRange(An.Alert());
        dbContext.AddRange(A.TradeOffer().With(o =>
        {
            o.Color = expected;
        }));
        await dbContext.SaveChangesAsync();
    
        var offers = await sut.FindAlertMatchingOffers(TimeSpan.FromMinutes(20));
    
        offers.Single().TradeOffer.Item.Color.Should().Be(expected);
    }
    
    [Test]
    public async Task FindAlertMatchingOffers_should_return_offer_matches_with_TradeOffer_Item_Certification_mapped()
    {
        const string expected = "Aviator";
    
        var dbContext = await dbContextFactory.CreateDbContextAsync();
        dbContext.AddRange(An.Alert());
        dbContext.AddRange(A.TradeOffer().With(o =>
        {
            o.Certification = expected;
        }));
        await dbContext.SaveChangesAsync();
    
        var offers = await sut.FindAlertMatchingOffers(TimeSpan.FromMinutes(20));
    
        offers.Single().TradeOffer.Item.Certification.Should().Be(expected);
    }
    
    [TestCase("Body", TradeItemType.Body)]
    [TestCase("Decal", TradeItemType.Decal)]
    [TestCase("Paint Finish", TradeItemType.PaintFinish)]
    [TestCase("Wheels", TradeItemType.Wheels)]
    [TestCase("Boost", TradeItemType.RocketBoost)]
    [TestCase("Topper", TradeItemType.Topper)]
    [TestCase("Antenna", TradeItemType.Antenna)]
    [TestCase("Goal Explosion", TradeItemType.GoalExplosion)]
    [TestCase("Trail", TradeItemType.Trail)]
    [TestCase("Banner", TradeItemType.Banner)]
    [TestCase("Avatar Border", TradeItemType.AvatarBorder)]
    public async Task FindAlertMatchingOffers_should_return_offer_matches_with_TradeOffer_Item_ItemType_mapped(string itemType, TradeItemType expected)
    {
        var dbContext = await dbContextFactory.CreateDbContextAsync();
        dbContext.AddRange(An.Alert());
        dbContext.AddRange(A.TradeOffer().With(o =>
        {
            o.ItemType = itemType;
        }));
        await dbContext.SaveChangesAsync();
    
        var offers = await sut.FindAlertMatchingOffers(TimeSpan.FromMinutes(20));
    
        offers.Single().TradeOffer.Item.ItemType.Should().Be(expected);
    }
    
    [TestCase("RLG", TradingSite.RocketLeagueGarage)]
    public async Task FindAlertMatchingOffers_should_return_offer_matches_with_TradeOffer_Trader_TradingSite_mapped(string tradingSite, TradingSite expected)
    {
        var dbContext = await dbContextFactory.CreateDbContextAsync();
        dbContext.AddRange(An.Alert());
        dbContext.AddRange(A.TradeOffer().With(o =>
        {
            o.TradingSite = tradingSite;
        }));
        await dbContext.SaveChangesAsync();
    
        var offers = await sut.FindAlertMatchingOffers(TimeSpan.FromMinutes(20));
    
        offers.Single().TradeOffer.Trader.TradingSite.Should().Be(expected);
    }
    
    [Test]
    public async Task FindAlertMatchingOffers_should_return_offer_matches_with_TradeOffer_Trader_Name_mapped()
    {
        const string expected = "RLTrader69";
    
        var dbContext = await dbContextFactory.CreateDbContextAsync();
        dbContext.AddRange(An.Alert());
        dbContext.AddRange(A.TradeOffer().With(o =>
        {
            o.TraderName = expected;
        }));
        await dbContext.SaveChangesAsync();
    
        var offers = await sut.FindAlertMatchingOffers(TimeSpan.FromMinutes(20));
    
        offers.Single().TradeOffer.Trader.Name.Should().Be(expected);
    }
    
    [TestCase("Buy", "Buy", true)]
    [TestCase("Buy", "Sell", false)]
    public async Task FindAlertMatchingOffers_should_return_offers_matching_by_offer_type(string alertOfferType, string tradeOfferType, bool expected)
    {
        var dbContext = await dbContextFactory.CreateDbContextAsync();
        dbContext.AddRange(An.Alert().With(a =>
        {
            a.OfferType = alertOfferType;
        }));
        dbContext.AddRange(A.TradeOffer().With(o =>
        {
            o.OfferType = tradeOfferType;
        }));
        await dbContext.SaveChangesAsync();
    
        var offers = await sut.FindAlertMatchingOffers(TimeSpan.FromMinutes(20));
    
        offers.Count.Should().Be(expected ? 1 : 0);
    }
    
    [TestCase("Hellfire", "Hellfire", true)]
    [TestCase("Hellfire", "hellfire", true)]
    [TestCase("hellfire", "Hellfire", true)]
    [TestCase("Hellfire", "Fennec", false)]
    [TestCase("Hell", "Hellfire", false)]
    public async Task FindAlertMatchingOffers_should_return_offers_matching_by_item_name(string alertItemName, string offerName, bool expected)
    {
        var dbContext = await dbContextFactory.CreateDbContextAsync();
        dbContext.AddRange(An.Alert().With(a =>
        {
            a.ItemName = alertItemName;
        }));
        dbContext.AddRange(A.TradeOffer().With(o =>
        {
            o.ItemName = offerName;
        }));
        await dbContext.SaveChangesAsync();
    
        var offers = await sut.FindAlertMatchingOffers(TimeSpan.FromMinutes(20));
    
        offers.Count.Should().Be(expected ? 1 : 0);
    }
    
    [TestCase(150, 200, 150, true)]
    [TestCase(150, 200, 160, true)]
    [TestCase(150, 200, 140, false)]
    [TestCase(150, 200, 210, false)]
    public async Task FindAlertMatchingOffers_should_return_offers_matching_by_price(int alertPriceFrom, int alertPriceTo, int offerPrice, bool expected)
    {
        var dbContext = await dbContextFactory.CreateDbContextAsync();
        dbContext.AddRange(An.Alert().With(a =>
        {
            a.PriceFrom = alertPriceFrom;
            a.PriceTo = alertPriceTo;
        }));
        dbContext.AddRange(A.TradeOffer().With(o =>
        {
            o.Price = offerPrice;
        }));
        await dbContext.SaveChangesAsync();
    
        var offers = await sut.FindAlertMatchingOffers(TimeSpan.FromMinutes(20));
    
        offers.Count.Should().Be(expected ? 1 : 0);
    }
    
    [TestCase("Goal Explosion", "Goal Explosion", true)]
    [TestCase("Goal Explosion", "Goal explosion", true)]
    [TestCase("Goal explosion", "Goal Explosion", true)]
    [TestCase("*", "Goal Explosion", true)]
    [TestCase("*", "", true)]
    [TestCase("", "", true)]
    [TestCase("Goal Explosion", "Wheels", false)]
    [TestCase("Goal Explosion", "", false)]
    [TestCase("", "Goal explosion", false)]
    public async Task FindAlertMatchingOffers_should_return_offers_matching_by_item_type(string alertItemType, string offerItemType, bool expected)
    {
        var dbContext = await dbContextFactory.CreateDbContextAsync();
        dbContext.AddRange(An.Alert().With(a =>
        {
            a.ItemType = alertItemType;
        }));
        dbContext.AddRange(A.TradeOffer().With(o =>
        {
            o.ItemType = offerItemType;
        }));
        await dbContext.SaveChangesAsync();
    
        var offers = await sut.FindAlertMatchingOffers(TimeSpan.FromMinutes(20));
    
        offers.Count.Should().Be(expected ? 1 : 0);
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
    public async Task FindAlertMatchingOffers_should_return_offers_matching_by_color(string alertColor, string offerColor, bool expected)
    {
        var dbContext = await dbContextFactory.CreateDbContextAsync();
        dbContext.AddRange(An.Alert().With(a =>
        {
            a.Color = alertColor;
        }));
        dbContext.AddRange(A.TradeOffer().With(o =>
        {
            o.Color = offerColor;
        }));
        await dbContext.SaveChangesAsync();
    
        var offers = await sut.FindAlertMatchingOffers(TimeSpan.FromMinutes(20));
    
        offers.Count.Should().Be(expected ? 1 : 0);
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
    public async Task FindAlertMatchingOffers_should_return_offers_matching_by_certification(string alertCertification, string offerCertification, bool expected)
    {
        var dbContext = await dbContextFactory.CreateDbContextAsync();
        dbContext.AddRange(An.Alert().With(a =>
        {
            a.Certification = alertCertification;
        }));
        dbContext.AddRange(A.TradeOffer().With(o =>
        {
            o.Certification = offerCertification;
        }));
        await dbContext.SaveChangesAsync();
    
        var offers = await sut.FindAlertMatchingOffers(TimeSpan.FromMinutes(20));
    
        offers.Count.Should().Be(expected ? 1 : 0);
    }
    
    [Test]
    public async Task FindAlertMatchingOffers_should_not_return_old_offers()
    {
        var now = new DateTime(2022, 1, 1);
        var dbContext = await dbContextFactory.CreateDbContextAsync();
        dateTime.SetupGet(d => d.Now).Returns(now);
        dbContext.AddRange(An.Alert());
        dbContext.AddRange(A.TradeOffer().With(o =>
        {
            o.ScrapedDate = now.AddMinutes(-21);
        }));
        await dbContext.SaveChangesAsync();
    
        var offers = await sut.FindAlertMatchingOffers(TimeSpan.FromMinutes(20));
    
        offers.Count.Should().Be(0);
    }
    
    [Test]
    public async Task FindAlertMatchingOffers_should_not_return_matching_offers_for_disabled_alerts()
    {
        var dbContext = await dbContextFactory.CreateDbContextAsync();
        dbContext.AddRange(An.Alert().With(a =>
        {
            a.Enabled = "No";
        }));
        dbContext.AddRange(A.TradeOffer());
        await dbContext.SaveChangesAsync();
    
        var offers = await sut.FindAlertMatchingOffers(TimeSpan.FromMinutes(20));
    
        offers.Count.Should().Be(0);
    }
    
    [TestCase("AnnoyingSpammer", "AnnoyingSpammer")]
    [TestCase("annoyingSpammer", "AnnoyingSpammer")]
    public async Task FindAlertMatchingOffers_should_not_return_matching_offer_from_blacklisted_traders(string offerTrader, string blacklistedTrader)
    {
        var dbContext = await dbContextFactory.CreateDbContextAsync();
        dbContext.AddRange(An.Alert());
        dbContext.AddRange(A.TradeOffer().With(o =>
        {
            o.TraderName = offerTrader;
        }));
        dbContext.AddRange(A.BlacklistedTrader().With(t =>
        {
            t.TraderName = blacklistedTrader;
        }));
        await dbContext.SaveChangesAsync();
        
        var offers = await sut.FindAlertMatchingOffers(TimeSpan.FromMinutes(20));
    
        offers.Count.Should().Be(0);
    }
    
    [Test]
    public async Task DeleteOldOffers_should_delete_old_offers_and_related_notifications()
    {
        // Arrange
        var offer = A.TradeOffer().With(o =>
        {
            o.ScrapedDate = DateTime.UtcNow.AddDays(-7);
        });
        var notification = A.Notification().With(n =>
        {
            n.CreatedDate = DateTime.UtcNow.AddDays(-7);
            n.TradeOffer = offer;
        });
        var dbContext = await dbContextFactory.CreateDbContextAsync();
        dbContext.AddRange(offer, notification);
        await dbContext.SaveChangesAsync();
    
        // Act
        await sut.DeleteOldOffers(TimeSpan.FromDays(5));
    
        // Assert
        dbContext.TradeOffers.Count().Should().Be(0);
        dbContext.Notifications.Count().Should().Be(0);
    }
    
    [Test]
    public async Task DeleteOldNotifications_should_delete_old_notifications_but_keep_related_offers()
    {
        // Arrange
        var offer = A.TradeOffer().With(o =>
        {
            o.ScrapedDate = DateTime.UtcNow.AddDays(-7);
        });
        var notification = A.Notification().With(n =>
        {
            n.CreatedDate = DateTime.UtcNow.AddDays(-7);
            n.TradeOffer = offer;
        });
        var dbContext = await dbContextFactory.CreateDbContextAsync();
        dbContext.AddRange(offer, notification);
        await dbContext.SaveChangesAsync();
    
        // Act
        await sut.DeleteOldNotifications(TimeSpan.FromDays(5));
    
        // Assert
        dbContext.Notifications.Count().Should().Be(0);
        dbContext.TradeOffers.Count().Should().Be(1);
    }
}