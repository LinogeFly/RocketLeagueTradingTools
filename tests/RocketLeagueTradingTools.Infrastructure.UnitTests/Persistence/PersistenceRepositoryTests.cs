using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using RocketLeagueTradingTools.Core.Application.Interfaces;
using RocketLeagueTradingTools.Core.Domain.Enumerations;
using RocketLeagueTradingTools.Infrastructure.Persistence;
using RocketLeagueTradingTools.Infrastructure.Persistence.Models;
using RocketLeagueTradingTools.Infrastructure.UnitTests.Persistence.Support;

namespace RocketLeagueTradingTools.Infrastructure.UnitTests.Persistence;

[TestFixture]
public class PersistenceRepositoryTests
{
    private PersistenceRepository sut = null!;
    private PersistenceDbContext dbContext = null!;
    private Mock<IDateTime> dateTime = null!;

    [SetUp]
    public void Setup()
    {
        dateTime = new Mock<IDateTime>();
        dateTime.SetupGet(d => d.Now).Returns(DateTime.UtcNow);

        // Initialize SQLite in-memory database with open connection.
        // The connection is closed at the end of each test run in the TearDown method.
        var keepAliveConnection = new SqliteConnection("Filename=:memory:");
        keepAliveConnection.Open();
        var dbContextOptions = new DbContextOptionsBuilder<PersistenceDbContext>()
            .UseSqlite(keepAliveConnection)
            .Options;

        // Create DbContext
        var dbContextFactory = new Mock<IDbContextFactory<PersistenceDbContext>>();
        dbContext = new PersistenceDbContext(dbContextOptions);
        dbContext.Database.EnsureCreated();
        dbContextFactory.Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>())).ReturnsAsync(dbContext);

        sut = new PersistenceRepository(dbContextFactory.Object, dateTime.Object);
    }

    [TearDown]
    public void TearDown()
    {
        dbContext.Dispose();
    }

    [Test]
    public async Task GetNotifications_should_return_notifications_with_trade_offer_mapped()
    {
        dbContext.AddRange(Build.DefaultNotification()
            .With(n =>
            {
                n.TradeItemName = "Fennec";
                n.TradeItemType = "Body";
                n.TradeOfferLink = "https://rocket-league.com/trade/88c10b46-a29a-4770-8efa-0304d6be8699";
                n.TradeOfferPrice = 300;
                n.TradeOfferScrapedDate = new DateTime(2022, 1, 1);
            })
        );
        await dbContext.SaveChangesAsync();

        var tradeOffer = (await sut.GetNotifications(20)).Single().TradeOffer;

        tradeOffer.Link.Should().Be("https://rocket-league.com/trade/88c10b46-a29a-4770-8efa-0304d6be8699");
        tradeOffer.Price.Should().Be(300);
        tradeOffer.ScrapedDate.Should().Be(new DateTime(2022, 1, 1));
    }

    [Test]
    public async Task GetNotifications_should_return_notifications_that_has_trade_offer_with_trade_item_mapped()
    {
        dbContext.AddRange(Build.DefaultNotification()
            .With(n =>
            {
                n.TradeItemName = "Fennec";
                n.TradeItemType = "Body";
                n.TradeItemColor = "Orange";
                n.TradeItemCertification = "Aviator";
                n.TradeOfferPrice = 300;
                n.TradeOfferScrapedDate = new DateTime(2022, 1, 1);
            })
        );
        await dbContext.SaveChangesAsync();

        var tradeItem = (await sut.GetNotifications(20)).Single().TradeOffer.Item;

        tradeItem.Name.Should().Be("Fennec");
        tradeItem.ItemType.Should().Be(TradeItemType.Body);
        tradeItem.Color.Should().Be("Orange");
        tradeItem.Certification.Should().Be("Aviator");
    }

    [Test]
    public async Task GetNotifications_should_return_notifications_page()
    {
        dbContext.AddRange(Build.DefaultNotification()
            .With(n =>
            {
                n.Id = 1;
                n.TradeItemName = "Hellfire";
                n.TradeOfferPrice = 100;
                n.CreatedDate = new DateTime(2022, 1, 1);
            }), Build.DefaultNotification()
            .With(n =>
            {
                n.Id = 2;
                n.TradeItemName = "Supernova III";
                n.TradeOfferPrice = 90;
                n.CreatedDate = new DateTime(2022, 1, 1);  
            })
        );
        await dbContext.SaveChangesAsync();

        var notifications = await sut.GetNotifications(1);

        notifications.Count.Should().Be(1);
    }

    [Test]
    public async Task GetNotifications_should_return_notifications_ordered_by_scraped_date_newest_first()
    {
        dbContext.AddRange(Build.DefaultNotification()
            .With(n =>
            {
                n.Id = 1;
                n.TradeItemName = "Hellfire";
                n.TradeOfferPrice = 100;
                n.CreatedDate = new DateTime(2022, 1, 2);
                n.TradeOfferScrapedDate = new DateTime(2022, 1, 1, 12, 0, 0);
            }), Build.DefaultNotification()
            .With(n =>
            {
                n.Id = 2;
                n.TradeItemName = "Supernova III";
                n.TradeOfferPrice = 90;
                n.CreatedDate = new DateTime(2022, 1, 2);
                n.TradeOfferScrapedDate = new DateTime(2022, 1, 1, 13, 0, 0); 
            })
        );
        await dbContext.SaveChangesAsync();

        var notifications = await sut.GetNotifications(1);

        notifications.Count.Should().Be(1);
        notifications.Single().TradeOffer.Item.Name.Should().Be("Supernova III");
    }

    [Test]
    public async Task GetNotifications_should_not_return_expired_notifications()
    {
        dbContext.AddRange(Build.DefaultNotification()
            .With(n =>
            {
                n.Id = 1;
                n.TradeItemName = "Hellfire";
                n.TradeOfferPrice = 100;
                n.CreatedDate = DateTime.UtcNow.AddHours(-2);
            }), Build.DefaultNotification()
            .With(n =>
            {
                n.Id = 2;
                n.TradeItemName = "Supernova III";
                n.TradeOfferPrice = 90;
                n.CreatedDate = DateTime.UtcNow;
            })
        );
        await dbContext.SaveChangesAsync();

        var notifications = await sut.GetNotifications(TimeSpan.FromHours(1));

        notifications.Count.Should().Be(1);
        notifications.Single().TradeOffer.Item.Name.Should().Be("Supernova III");
    }

    [TestCase("Buy")]
    [TestCase("Sell")]
    public async Task FindAlertMatchingOffers_should_return_offer_matches_with_link_mapped(string alertOfferType)
    {
        const string expectedLink = "https://rocket-league.com/trade/1";

        dbContext.AddRange(new PersistedAlert
        {
            OfferType = alertOfferType,
            ItemName = "Hellfire",
            PriceFrom = 150,
            PriceTo = 10000
        });
        dbContext.AddRange(Build.DefaultOffer(alertOfferType)
            .With(o =>
            {
                o.ItemName = "Hellfire";
                o.Price = 150;
                o.Link = expectedLink;
                o.ScrapedDate = DateTime.UtcNow;
            })
        );
        await dbContext.SaveChangesAsync();

        var offers = await sut.FindAlertMatchingOffers(TimeSpan.FromMinutes(20));

        offers.Single().Link.Should().Be(expectedLink);
    }

    [TestCase("Buy")]
    [TestCase("Sell")]
    public async Task FindAlertMatchingOffers_should_return_offer_matches_with_trade_item_mapped(string alertOfferType)
    {
        const string expectedItemName = "Hellfire";
        const string expectedItemColor = "Lime";
        const string expectedItemCertification = "Guardian";

        dbContext.AddRange(new PersistedAlert
        {
            OfferType = alertOfferType,
            ItemName = expectedItemName,
            Color = expectedItemColor,
            Certification = expectedItemCertification,
            PriceFrom = 150,
            PriceTo = 10000
        });
        dbContext.AddRange(Build.DefaultOffer(alertOfferType)
            .With(o =>
            {
                o.ItemName = "Hellfire";
                o.Price = 150;
                o.Color = expectedItemColor;
                o.Certification = expectedItemCertification;
                o.ScrapedDate = DateTime.UtcNow;
            })
        );
        await dbContext.SaveChangesAsync();

        var offers = await sut.FindAlertMatchingOffers(TimeSpan.FromMinutes(20));

        offers.Single().Item.Name.Should().Be(expectedItemName);
        offers.Single().Item.Color.Should().Be(expectedItemColor);
        offers.Single().Item.Certification.Should().Be(expectedItemCertification);
    }

    [TestCase("Buy", "Body", TradeItemType.Body)]
    [TestCase("Buy", "Decal", TradeItemType.Decal)]
    [TestCase("Buy", "Paint Finish", TradeItemType.PaintFinish)]
    [TestCase("Buy", "Wheels", TradeItemType.Wheels)]
    [TestCase("Buy", "Boost", TradeItemType.RocketBoost)]
    [TestCase("Buy", "Topper", TradeItemType.Topper)]
    [TestCase("Buy", "Antenna", TradeItemType.Antenna)]
    [TestCase("Buy", "Goal Explosion", TradeItemType.GoalExplosion)]
    [TestCase("Buy", "Trail", TradeItemType.Trail)]
    [TestCase("Buy", "Banner", TradeItemType.Banner)]
    [TestCase("Buy", "Avatar Border", TradeItemType.AvatarBorder)]
    [TestCase("Sell", "Body", TradeItemType.Body)]
    [TestCase("Sell", "Decal", TradeItemType.Decal)]
    [TestCase("Sell", "Paint Finish", TradeItemType.PaintFinish)]
    [TestCase("Sell", "Wheels", TradeItemType.Wheels)]
    [TestCase("Sell", "Boost", TradeItemType.RocketBoost)]
    [TestCase("Sell", "Topper", TradeItemType.Topper)]
    [TestCase("Sell", "Antenna", TradeItemType.Antenna)]
    [TestCase("Sell", "Goal Explosion", TradeItemType.GoalExplosion)]
    [TestCase("Sell", "Trail", TradeItemType.Trail)]
    [TestCase("Sell", "Banner", TradeItemType.Banner)]
    [TestCase("Sell", "Avatar Border", TradeItemType.AvatarBorder)]
    public async Task FindAlertMatchingOffers_should_return_offer_matches_with_trade_item_type_mapped(string alertOfferType, string persistedItemType, TradeItemType expected)
    {
        dbContext.AddRange(new PersistedAlert
        {
            OfferType = alertOfferType,
            ItemName = "Item",
            ItemType = "*",
            PriceFrom = 100,
            PriceTo = 150
        });
        dbContext.AddRange(Build.DefaultOffer(alertOfferType)
            .With(o =>
            {
                o.ItemName = "Item";
                o.Price = 100;
                o.ItemType = persistedItemType;
                o.ScrapedDate = DateTime.UtcNow;
            })
        );
        await dbContext.SaveChangesAsync();

        var offers = await sut.FindAlertMatchingOffers(TimeSpan.FromMinutes(20));

        offers.Single().Item.ItemType.Should().Be(expected);
    }

    [TestCase("Buy")]
    [TestCase("Sell")]
    public async Task FindAlertMatchingOffers_should_return_offer_matches_with_price_mapped(string alertOfferType)
    {
        const int expectedPrice = 150;

        dbContext.AddRange(new PersistedAlert
        {
            OfferType = alertOfferType,
            ItemName = "Hellfire",
            PriceFrom = 150,
            PriceTo = 10000
        });
        dbContext.AddRange(Build.DefaultOffer(alertOfferType)
            .With(o =>
            {
                o.ItemName = "Hellfire";
                o.Price = expectedPrice;
                o.ScrapedDate = DateTime.UtcNow;
            })
        );
        await dbContext.SaveChangesAsync();

        var offers = await sut.FindAlertMatchingOffers(TimeSpan.FromMinutes(20));

        offers.Single().Price.Should().Be(expectedPrice);
    }

    [TestCase("Buy")]
    [TestCase("Sell")]
    public async Task FindAlertMatchingOffers_should_return_offer_matches_with_scraped_date_mapped(string alertOfferType)
    {
        var expectedDate = DateTime.UtcNow;

        dbContext.AddRange(new PersistedAlert
        {
            OfferType = alertOfferType,
            ItemName = "Hellfire",
            PriceFrom = 150,
            PriceTo = 10000
        });
        dbContext.AddRange(Build.DefaultOffer(alertOfferType)
            .With(o =>
            {
                o.ItemName = "Hellfire";
                o.Price = 150;
                o.ScrapedDate = expectedDate;
            })
        );
        await dbContext.SaveChangesAsync();

        var offers = await sut.FindAlertMatchingOffers(TimeSpan.FromMinutes(20));

        offers.Single().ScrapedDate.Should().Be(expectedDate);
    }

    [TestCase("Buy", "Hellfire", "Hellfire", true)]
    [TestCase("Buy", "Hellfire", "hellfire", true)]
    [TestCase("Buy", "hellfire", "Hellfire", true)]
    [TestCase("Buy", "Hellfire", "Fennec", false)]
    [TestCase("Buy", "Hell", "Hellfire", false)]
    [TestCase("Sell", "Hellfire", "Hellfire", true)]
    [TestCase("Sell", "Hellfire", "hellfire", true)]
    [TestCase("Sell", "hellfire", "Hellfire", true)]
    [TestCase("Sell", "Hellfire", "Fennec", false)]
    [TestCase("Sell", "Hell", "Hellfire", false)]
    public async Task FindAlertMatchingOffers_should_return_offers_matching_by_item_name(string alertOfferType, string alertItemName, string offerName, bool shouldMatch)
    {
        dbContext.AddRange(new PersistedAlert
        {
            OfferType = alertOfferType,
            ItemName = alertItemName,
            PriceFrom = 150,
            PriceTo = 10000
        });
        dbContext.AddRange(Build.DefaultOffer(alertOfferType)
            .With(o =>
            {
                o.ItemName = offerName;
                o.Price = 150;
                o.ScrapedDate = DateTime.UtcNow;
            })
        );
        await dbContext.SaveChangesAsync();

        var offers = await sut.FindAlertMatchingOffers(TimeSpan.FromMinutes(20));

        offers.Count.Should().Be(shouldMatch ? 1 : 0);
    }

    [TestCase("Buy", 150, 200, 150, true)]
    [TestCase("Buy", 150, 200, 160, true)]
    [TestCase("Buy", 150, 200, 140, false)]
    [TestCase("Buy", 150, 200, 210, false)]
    [TestCase("Sell", 0, 100, 100, true)]
    [TestCase("Sell", 0, 100, 90, true)]
    [TestCase("Sell", 0, 100, 110, false)]
    public async Task FindAlertMatchingOffers_should_return_offers_matching_by_price(string alertOfferType, int alertPriceFrom, int alertPriceTo, int offerPrice, bool shouldMatch)
    {
        dbContext.AddRange(new PersistedAlert
        {
            OfferType = alertOfferType,
            ItemName = "Hellfire",
            PriceFrom = alertPriceFrom,
            PriceTo = alertPriceTo
        });
        dbContext.AddRange(Build.DefaultOffer(alertOfferType)
            .With(o =>
            {
                o.ItemName = "Hellfire";
                o.Price = offerPrice;
                o.ScrapedDate = DateTime.UtcNow;
            })
        );
        await dbContext.SaveChangesAsync();

        var offers = await sut.FindAlertMatchingOffers(TimeSpan.FromMinutes(20));

        offers.Count.Should().Be(shouldMatch ? 1 : 0);
    }

    [TestCase("Buy", "Goal Explosion", "Goal Explosion", true)]
    [TestCase("Buy", "Goal Explosion", "Goal explosion", true)]
    [TestCase("Buy", "Goal explosion", "Goal Explosion", true)]
    [TestCase("Buy", "*", "Goal Explosion", true)]
    [TestCase("Buy", "*", "", true)]
    [TestCase("Buy", "", "", true)]
    [TestCase("Buy", "Goal Explosion", "Wheels", false)]
    [TestCase("Buy", "Goal Explosion", "", false)]
    [TestCase("Buy", "", "Goal explosion", false)]
    [TestCase("Sell", "Goal Explosion", "Goal Explosion", true)]
    [TestCase("Sell", "Goal Explosion", "Goal explosion", true)]
    [TestCase("Sell", "Goal explosion", "Goal Explosion", true)]
    [TestCase("Sell", "*", "Goal Explosion", true)]
    [TestCase("Sell", "*", "", true)]
    [TestCase("Sell", "", "", true)]
    [TestCase("Sell", "Goal Explosion", "Wheels", false)]
    [TestCase("Sell", "Goal Explosion", "", false)]
    [TestCase("Sell", "", "Goal explosion", false)]
    public async Task FindAlertMatchingOffers_should_return_offers_matching_by_item_type(string alertOfferType, string alertItemType, string offerItemType, bool shouldMatch)
    {
        dbContext.AddRange(new PersistedAlert
        {
            OfferType = alertOfferType,
            ItemName = "Reaper",
            PriceFrom = 0,
            PriceTo = 650,
            ItemType = alertItemType
        });
        dbContext.AddRange(Build.DefaultOffer(alertOfferType)
            .With(o =>
            {
                o.ItemName = "Reaper";
                o.Price = 650;
                o.ItemType = offerItemType;
                o.ScrapedDate = DateTime.UtcNow;
            })
        );
        await dbContext.SaveChangesAsync();

        var offers = await sut.FindAlertMatchingOffers(TimeSpan.FromMinutes(20));

        offers.Count.Should().Be(shouldMatch ? 1 : 0);
    }

    [TestCase("Buy", "Sky Blue", "Sky Blue", true)]
    [TestCase("Buy", "Sky Blue", "Sky blue", true)]
    [TestCase("Buy", "Sky blue", "Sky Blue", true)]
    [TestCase("Buy", "*", "Sky Blue", true)]
    [TestCase("Buy", "*", "", true)]
    [TestCase("Buy", "+", "Sky Blue", true)]
    [TestCase("Buy", "", "", true)]
    [TestCase("Buy", "Lime", "Sky Blue", false)]
    [TestCase("Buy", "Sky", "Sky Blue", false)]
    [TestCase("Buy", "", "Sky Blue", false)]
    [TestCase("Buy", "+", "", false)]
    [TestCase("Sell", "Sky Blue", "Sky Blue", true)]
    [TestCase("Sell", "Sky Blue", "Sky blue", true)]
    [TestCase("Sell", "Sky blue", "Sky Blue", true)]
    [TestCase("Sell", "*", "Sky Blue", true)]
    [TestCase("Sell", "*", "", true)]
    [TestCase("Sell", "+", "Sky Blue", true)]
    [TestCase("Sell", "", "", true)]
    [TestCase("Sell", "Lime", "Sky Blue", false)]
    [TestCase("Sell", "Sky", "Sky Blue", false)]
    [TestCase("Sell", "", "Sky Blue", false)]
    [TestCase("Sell", "+", "", false)]
    public async Task FindAlertMatchingOffers_should_return_offers_matching_by_color(string alertOfferType, string alertColor, string offerColor, bool shouldMatch)
    {
        dbContext.AddRange(new PersistedAlert
        {
            OfferType = alertOfferType,
            ItemName = "Hellfire",
            PriceFrom = 150,
            PriceTo = 10000,
            Color = alertColor
        });
        dbContext.AddRange(Build.DefaultOffer(alertOfferType)
            .With(o =>
            {
                o.ItemName = "Hellfire";
                o.Price = 150;
                o.Color = offerColor;
                o.ScrapedDate = DateTime.UtcNow;
            })
        );
        await dbContext.SaveChangesAsync();

        var offers = await sut.FindAlertMatchingOffers(TimeSpan.FromMinutes(20));

        offers.Count.Should().Be(shouldMatch ? 1 : 0);
    }

    [TestCase("Buy", "Guardian", "Guardian", true)]
    [TestCase("Buy", "Guardian", "guardian", true)]
    [TestCase("Buy", "guardian", "Guardian", true)]
    [TestCase("Buy", "*", "Guardian", true)]
    [TestCase("Buy", "*", "", true)]
    [TestCase("Buy", "+", "Guardian", true)]
    [TestCase("Buy", "", "", true)]
    [TestCase("Buy", "Sniper", "Guardian", false)]
    [TestCase("Buy", "Guard", "Guardian", false)]
    [TestCase("Buy", "", "Guardian", false)]
    [TestCase("Buy", "+", "", false)]
    [TestCase("Sell", "Guardian", "Guardian", true)]
    [TestCase("Sell", "Guardian", "guardian", true)]
    [TestCase("Sell", "guardian", "Guardian", true)]
    [TestCase("Sell", "*", "Guardian", true)]
    [TestCase("Sell", "*", "", true)]
    [TestCase("Sell", "+", "Guardian", true)]
    [TestCase("Sell", "", "", true)]
    [TestCase("Sell", "Sniper", "Guardian", false)]
    [TestCase("Sell", "Guard", "Guardian", false)]
    [TestCase("Sell", "", "Guardian", false)]
    [TestCase("Sell", "+", "", false)]
    public async Task FindAlertMatchingOffers_should_return_offers_matching_by_certification(string alertOfferType, string alertCertification, string offerCertification, bool shouldMatch)
    {
        dbContext.AddRange(new PersistedAlert
        {
            OfferType = alertOfferType,
            ItemName = "Hellfire",
            PriceFrom = 150,
            PriceTo = 10000,
            Certification = alertCertification
        });
        dbContext.AddRange(Build.DefaultOffer(alertOfferType)
            .With(o =>
            {
                o.ItemName = "Hellfire";
                o.Price = 150;
                o.Certification = offerCertification;
                o.ScrapedDate = DateTime.UtcNow;
            })
        );
        await dbContext.SaveChangesAsync();

        var offers = await sut.FindAlertMatchingOffers(TimeSpan.FromMinutes(20));

        offers.Count.Should().Be(shouldMatch ? 1 : 0);
    }

    [TestCase("Buy")]
    [TestCase("Sell")]
    public async Task FindAlertMatchingOffers_should_not_return_old_offers(string alertOfferType)
    {
        var now = new DateTime(2022, 1, 1);
        dateTime.SetupGet(d => d.Now).Returns(now);

        dbContext.AddRange(new PersistedAlert
        {
            OfferType = alertOfferType,
            ItemName = "Hellfire",
            PriceFrom = 150,
            PriceTo = 10000
        });
        dbContext.AddRange(Build.DefaultOffer(alertOfferType)
            .With(o =>
            {
                o.ItemName = "Hellfire";
                o.Price = 150;
                o.ScrapedDate = now.AddMinutes(-21);
            })
        );
        await dbContext.SaveChangesAsync();

        var offers = await sut.FindAlertMatchingOffers(TimeSpan.FromMinutes(20));

        offers.Count.Should().Be(0);
    }

    [TestCase("Buy")]
    [TestCase("Sell")]
    public async Task FindAlertMatchingOffers_should_not_return_matching_offers_for_disabled_alerts(string alertOfferType)
    {
        dbContext.AddRange(new PersistedAlert
        {
            OfferType = alertOfferType,
            ItemName = "Hellfire",
            PriceFrom = 150,
            PriceTo = 10000,
            Enabled = "No"
        });
        dbContext.AddRange(Build.DefaultOffer(alertOfferType)
            .With(o =>
            {
                o.ItemName = "Hellfire";
                o.Price = 150;
                o.ScrapedDate = DateTime.UtcNow;
            })
        );
        await dbContext.SaveChangesAsync();

        var offers = await sut.FindAlertMatchingOffers(TimeSpan.FromMinutes(20));

        offers.Count.Should().Be(0);
    }

    [TestCase("Buy")]
    [TestCase("Sell")]
    public async Task FindAlertMatchingOffers_should_not_return_matching_offer_from_blacklisted_traders(string alertOfferType)
    {
        dbContext.AddRange(new PersistedAlert
        {
            OfferType = alertOfferType,
            ItemName = "Hellfire",
            PriceFrom = 150,
            PriceTo = 10000
        });
        dbContext.AddRange(Build.DefaultOffer(alertOfferType)
            .With(o =>
            {
                o.ItemName = "Hellfire";
                o.Price = 150;
                o.TradingSite = "RLG";
                o.TraderName = "AnnoyingSpammer";
            })
        );
        dbContext.AddRange(new PersistedBlacklistedTrader
        {
            TradingSite = "RLG",
            TraderName = "AnnoyingSpammer"
        });
        await dbContext.SaveChangesAsync();
        
        var offers = await sut.FindAlertMatchingOffers(TimeSpan.FromMinutes(20));

        offers.Count.Should().Be(0);
    }

    [Test]
    public async Task FindAlertMatchingOffers_should_not_return_buy_offer_matches_for_sell_offer_type_alerts()
    {
        dbContext.AddRange(new PersistedAlert
        {
            OfferType = "Sell",
            ItemName = "Hellfire",
            PriceFrom = 0,
            PriceTo = 100,
        });
        dbContext.AddRange(Build.DefaultOffer("Buy")
            .With(o =>
            {
                o.ItemName = "Hellfire";
                o.Price = 90;
                o.ScrapedDate = DateTime.UtcNow;
            })
        );

        await dbContext.SaveChangesAsync();

        var offers = await sut.FindAlertMatchingOffers(TimeSpan.FromMinutes(20));

        offers.Count.Should().Be(0);
    }

    [Test]
    public async Task FindAlertMatchingOffers_should_not_return_sell_offer_matches_for_buy_offer_type_alerts()
    {
        dbContext.AddRange(new PersistedAlert
        {
            OfferType = "Buy",
            ItemName = "Hellfire",
            PriceFrom = 150,
            PriceTo = 10000,
        });
        dbContext.AddRange(Build.DefaultOffer("Sell")
            .With(o =>
            {
                o.ItemName = "Hellfire";
                o.Price = 160;
                o.ScrapedDate = DateTime.UtcNow;
            })
        );

        await dbContext.SaveChangesAsync();

        var offers = await sut.FindAlertMatchingOffers(TimeSpan.FromMinutes(20));

        offers.Count.Should().Be(0);
    }
}