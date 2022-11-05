using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using RocketLeagueTradingTools.Core.Application.Interfaces;
using RocketLeagueTradingTools.Infrastructure.Persistence;
using RocketLeagueTradingTools.Infrastructure.Persistence.PersistedTypes;
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
        var contextOptions = new DbContextOptionsBuilder<PersistenceDbContext>()
            .UseSqlite(keepAliveConnection)
            .Options;
        dbContext = new PersistenceDbContext(contextOptions);
        dbContext.Database.EnsureCreated();

        sut = new PersistenceRepository(dbContext, dateTime.Object);
    }

    [TearDown]
    public void TearDown()
    {
        dbContext.Dispose();
    }

    [Test]
    public async Task GetNotifications_should_return_notifications_page()
    {
        dbContext.AddRange(new PersistedNotification
        {
            Id = 1,
            TradeItemName = "Hellfire",
            TradeOfferPrice = 100,
            CreatedDate = new DateTime(2022, 1, 1)
        },
        new PersistedNotification
        {
            Id = 2,
            TradeItemName = "Supernova III",
            TradeOfferPrice = 90,
            CreatedDate = new DateTime(2022, 1, 1)
        });
        await dbContext.SaveChangesAsync();

        var notifications = await sut.GetNotifications(1);

        notifications.Count.Should().Be(1);
    }

    [Test]
    public async Task GetNotifications_should_return_notifications_ordered_by_scraped_date_newest_first()
    {
        dbContext.AddRange(new PersistedNotification
        {
            Id = 1,
            TradeItemName = "Hellfire",
            TradeOfferPrice = 100,
            CreatedDate = new DateTime(2022, 1, 2),
            TradeOfferScrapedDate = new DateTime(2022, 1, 1, 12, 0, 0)
        },
        new PersistedNotification
        {
            Id = 2,
            TradeItemName = "Supernova III",
            TradeOfferPrice = 90,
            CreatedDate = new DateTime(2022, 1, 2),
            TradeOfferScrapedDate = new DateTime(2022, 1, 1, 13, 0, 0)
        });
        await dbContext.SaveChangesAsync();

        var notifications = await sut.GetNotifications(1);

        notifications.Count.Should().Be(1);
        notifications.Single().TradeOffer.Item.Name.Should().Be("Supernova III");
    }

    [Test]
    public async Task GetNotifications_should_not_return_expired_notifications()
    {
        dbContext.AddRange(new PersistedNotification
        {
            Id = 1,
            TradeItemName = "Hellfire",
            TradeOfferPrice = 100,
            CreatedDate = DateTime.UtcNow.AddHours(-2)
        },
        new PersistedNotification
        {
            Id = 2,
            TradeItemName = "Supernova III",
            TradeOfferPrice = 90,
            CreatedDate = DateTime.UtcNow
        });
        await dbContext.SaveChangesAsync();

        var notifications = await sut.GetNotifications(TimeSpan.FromHours(1));

        notifications.Count.Should().Be(1);
        notifications.Single().TradeOffer.Item.Name.Should().Be("Supernova III");
    }

    [TestCase(PersistedAlertOfferType.Buy)]
    [TestCase(PersistedAlertOfferType.Sell)]
    public async Task FindAlertMatchingOffers_should_return_offer_matches_with_source_id_mapped(PersistedAlertOfferType alertOfferType)
    {
        const string expectedSourceId = "1";

        dbContext.AddRange(new PersistedAlert
        {
            OfferType = alertOfferType,
            ItemName = "Hellfire",
            PriceFrom = 150,
            PriceTo = 10000
        });
        dbContext.AddRange(Build.PersistedOffer(alertOfferType)
            .With(o =>
            {
                o.Name = "Hellfire";
                o.Price = 150;
                o.SourceId = expectedSourceId;
                o.ScrapedDate = DateTime.UtcNow;
            })
        );
        await dbContext.SaveChangesAsync();

        var offers = await sut.FindAlertMatchingOffers(TimeSpan.FromMinutes(20));

        offers.Single().SourceId.Should().Be(expectedSourceId);
    }

    [TestCase(PersistedAlertOfferType.Buy)]
    [TestCase(PersistedAlertOfferType.Sell)]
    public async Task FindAlertMatchingOffers_should_return_offer_matches_with_link_mapped(PersistedAlertOfferType alertOfferType)
    {
        const string expectedLink = "https://rocket-league.com/trade/1";

        dbContext.AddRange(new PersistedAlert
        {
            OfferType = alertOfferType,
            ItemName = "Hellfire",
            PriceFrom = 150,
            PriceTo = 10000
        });
        dbContext.AddRange(Build.PersistedOffer(alertOfferType)
            .With(o =>
            {
                o.Name = "Hellfire";
                o.Price = 150;
                o.Link = expectedLink;
                o.ScrapedDate = DateTime.UtcNow;
            })
        );
        await dbContext.SaveChangesAsync();

        var offers = await sut.FindAlertMatchingOffers(TimeSpan.FromMinutes(20));

        offers.Single().Link.Should().Be(expectedLink);
    }

    [TestCase(PersistedAlertOfferType.Buy)]
    [TestCase(PersistedAlertOfferType.Sell)]
    public async Task FindAlertMatchingOffers_should_return_offer_matches_with_item_mapped(PersistedAlertOfferType alertOfferType)
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
        dbContext.AddRange(Build.PersistedOffer(alertOfferType)
            .With(o =>
            {
                o.Name = "Hellfire";
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

    [TestCase(PersistedAlertOfferType.Buy)]
    [TestCase(PersistedAlertOfferType.Sell)]
    public async Task FindAlertMatchingOffers_should_return_offer_matches_with_price_mapped(PersistedAlertOfferType alertOfferType)
    {
        const int expectedPrice = 150;

        dbContext.AddRange(new PersistedAlert
        {
            OfferType = alertOfferType,
            ItemName = "Hellfire",
            PriceFrom = 150,
            PriceTo = 10000
        });
        dbContext.AddRange(Build.PersistedOffer(alertOfferType)
            .With(o =>
            {
                o.Name = "Hellfire";
                o.Price = expectedPrice;
                o.ScrapedDate = DateTime.UtcNow;
            })
        );
        await dbContext.SaveChangesAsync();

        var offers = await sut.FindAlertMatchingOffers(TimeSpan.FromMinutes(20));

        offers.Single().Price.Should().Be(expectedPrice);
    }

    [TestCase(PersistedAlertOfferType.Buy)]
    [TestCase(PersistedAlertOfferType.Sell)]
    public async Task FindAlertMatchingOffers_should_return_offer_matches_with_scraped_date_mapped(PersistedAlertOfferType alertOfferType)
    {
        var expectedDate = DateTime.UtcNow;

        dbContext.AddRange(new PersistedAlert
        {
            OfferType = alertOfferType,
            ItemName = "Hellfire",
            PriceFrom = 150,
            PriceTo = 10000
        });
        dbContext.AddRange(Build.PersistedOffer(alertOfferType)
            .With(o =>
            {
                o.Name = "Hellfire";
                o.Price = 150;
                o.ScrapedDate = expectedDate;
            })
        );
        await dbContext.SaveChangesAsync();

        var offers = await sut.FindAlertMatchingOffers(TimeSpan.FromMinutes(20));

        offers.Single().ScrapedDate.Should().Be(expectedDate);
    }

    [TestCase(PersistedAlertOfferType.Buy, "Hellfire", "hellfire", true)]
    [TestCase(PersistedAlertOfferType.Buy, "Hellfire", "Hellfire", true)]
    [TestCase(PersistedAlertOfferType.Buy, "hellfire", "Hellfire", true)]
    [TestCase(PersistedAlertOfferType.Buy, "Hellfire", "Fennec", false)]
    [TestCase(PersistedAlertOfferType.Buy, "Hell", "Hellfire", false)]
    [TestCase(PersistedAlertOfferType.Sell, "Hellfire", "Hellfire", true)]
    [TestCase(PersistedAlertOfferType.Sell, "Hellfire", "hellfire", true)]
    [TestCase(PersistedAlertOfferType.Sell, "hellfire", "Hellfire", true)]
    [TestCase(PersistedAlertOfferType.Sell, "Hellfire", "Fennec", false)]
    [TestCase(PersistedAlertOfferType.Sell, "Hell", "Hellfire", false)]
    public async Task FindAlertMatchingOffers_should_return_offers_matching_by_name(PersistedAlertOfferType alertOfferType, string alertItemName, string offerName, bool shouldMatch)
    {
        dbContext.AddRange(new PersistedAlert
        {
            OfferType = alertOfferType,
            ItemName = alertItemName,
            PriceFrom = 150,
            PriceTo = 10000
        });
        dbContext.AddRange(Build.PersistedOffer(alertOfferType)
            .With(o =>
            {
                o.Name = offerName;
                o.Price = 150;
                o.ScrapedDate = DateTime.UtcNow;
            })
        );
        await dbContext.SaveChangesAsync();

        var offers = await sut.FindAlertMatchingOffers(TimeSpan.FromMinutes(20));

        offers.Count.Should().Be(shouldMatch ? 1 : 0);
    }

    [TestCase(PersistedAlertOfferType.Buy, 150, 200, 150, true)]
    [TestCase(PersistedAlertOfferType.Buy, 150, 200, 160, true)]
    [TestCase(PersistedAlertOfferType.Buy, 150, 200, 140, false)]
    [TestCase(PersistedAlertOfferType.Buy, 150, 200, 210, false)]
    [TestCase(PersistedAlertOfferType.Sell, 0, 100, 100, true)]
    [TestCase(PersistedAlertOfferType.Sell, 0, 100, 90, true)]
    [TestCase(PersistedAlertOfferType.Sell, 0, 100, 110, false)]
    public async Task FindAlertMatchingOffers_should_return_offers_matching_by_price(PersistedAlertOfferType alertOfferType, int alertPriceFrom, int alertPriceTo, int offerPrice, bool shouldMatch)
    {
        dbContext.AddRange(new PersistedAlert
        {
            OfferType = alertOfferType,
            ItemName = "Hellfire",
            PriceFrom = alertPriceFrom,
            PriceTo = alertPriceTo
        });
        dbContext.AddRange(Build.PersistedOffer(alertOfferType)
            .With(o =>
            {
                o.Name = "Hellfire";
                o.Price = offerPrice;
                o.ScrapedDate = DateTime.UtcNow;
            })
        );
        await dbContext.SaveChangesAsync();

        var offers = await sut.FindAlertMatchingOffers(TimeSpan.FromMinutes(20));

        offers.Count.Should().Be(shouldMatch ? 1 : 0);
    }

    [TestCase(PersistedAlertOfferType.Buy, "Sky blue", "Sky blue", true)]
    [TestCase(PersistedAlertOfferType.Buy, "Sky Blue", "Sky blue", true)]
    [TestCase(PersistedAlertOfferType.Buy, "Sky blue", "Sky Blue", true)]
    [TestCase(PersistedAlertOfferType.Buy, "*", "Sky blue", true)]
    [TestCase(PersistedAlertOfferType.Buy, "*", "Sky Blue", true)]
    [TestCase(PersistedAlertOfferType.Buy, "*", "", true)]
    [TestCase(PersistedAlertOfferType.Buy, "", "", true)]
    [TestCase(PersistedAlertOfferType.Buy, "Lime", "Sky blue", false)]
    [TestCase(PersistedAlertOfferType.Buy, "Sky", "Sky blue", false)]
    [TestCase(PersistedAlertOfferType.Buy, "", "Sky blue", false)]
    [TestCase(PersistedAlertOfferType.Sell, "Sky blue", "Sky blue", true)]
    [TestCase(PersistedAlertOfferType.Sell, "Sky Blue", "Sky blue", true)]
    [TestCase(PersistedAlertOfferType.Sell, "Sky blue", "Sky Blue", true)]
    [TestCase(PersistedAlertOfferType.Sell, "*", "Sky blue", true)]
    [TestCase(PersistedAlertOfferType.Sell, "*", "Sky Blue", true)]
    [TestCase(PersistedAlertOfferType.Sell, "*", "", true)]
    [TestCase(PersistedAlertOfferType.Sell, "", "", true)]
    [TestCase(PersistedAlertOfferType.Sell, "Lime", "Sky blue", false)]
    [TestCase(PersistedAlertOfferType.Sell, "Sky", "Sky blue", false)]
    [TestCase(PersistedAlertOfferType.Sell, "", "Sky blue", false)]
    public async Task FindAlertMatchingOffers_should_return_offers_matching_by_color(PersistedAlertOfferType alertOfferType, string alertColor, string offerColor, bool shouldMatch)
    {
        dbContext.AddRange(new PersistedAlert
        {
            OfferType = alertOfferType,
            ItemName = "Hellfire",
            PriceFrom = 150,
            PriceTo = 10000,
            Color = alertColor
        });
        dbContext.AddRange(Build.PersistedOffer(alertOfferType)
            .With(o =>
            {
                o.Name = "Hellfire";
                o.Price = 150;
                o.Color = offerColor;
                o.ScrapedDate = DateTime.UtcNow;
            })
        );
        await dbContext.SaveChangesAsync();

        var offers = await sut.FindAlertMatchingOffers(TimeSpan.FromMinutes(20));

        offers.Count.Should().Be(shouldMatch ? 1 : 0);
    }

    [TestCase(PersistedAlertOfferType.Buy, "Guardian", "Guardian", true)]
    [TestCase(PersistedAlertOfferType.Buy, "Guardian", "guardian", true)]
    [TestCase(PersistedAlertOfferType.Buy, "guardian", "Guardian", true)]
    [TestCase(PersistedAlertOfferType.Buy, "*", "Guardian", true)]
    [TestCase(PersistedAlertOfferType.Buy, "*", "guardian", true)]
    [TestCase(PersistedAlertOfferType.Buy, "*", "", true)]
    [TestCase(PersistedAlertOfferType.Buy, "", "", true)]
    [TestCase(PersistedAlertOfferType.Buy, "Sniper", "Guardian", false)]
    [TestCase(PersistedAlertOfferType.Buy, "Guard", "Guardian", false)]
    [TestCase(PersistedAlertOfferType.Buy, "", "Guardian", false)]
    [TestCase(PersistedAlertOfferType.Sell, "Guardian", "Guardian", true)]
    [TestCase(PersistedAlertOfferType.Sell, "Guardian", "guardian", true)]
    [TestCase(PersistedAlertOfferType.Sell, "guardian", "Guardian", true)]
    [TestCase(PersistedAlertOfferType.Sell, "*", "Guardian", true)]
    [TestCase(PersistedAlertOfferType.Sell, "*", "guardian", true)]
    [TestCase(PersistedAlertOfferType.Sell, "*", "", true)]
    [TestCase(PersistedAlertOfferType.Sell, "", "", true)]
    [TestCase(PersistedAlertOfferType.Sell, "Sniper", "Guardian", false)]
    [TestCase(PersistedAlertOfferType.Sell, "Guard", "Guardian", false)]
    [TestCase(PersistedAlertOfferType.Sell, "", "Guardian", false)]
    public async Task FindAlertMatchingOffers_should_return_offers_matching_by_certification(PersistedAlertOfferType alertOfferType, string alertCertification, string offerCertification, bool shouldMatch)
    {
        dbContext.AddRange(new PersistedAlert
        {
            OfferType = alertOfferType,
            ItemName = "Hellfire",
            PriceFrom = 150,
            PriceTo = 10000,
            Certification = alertCertification
        });
        dbContext.AddRange(Build.PersistedOffer(alertOfferType)
            .With(o =>
            {
                o.Name = "Hellfire";
                o.Price = 150;
                o.Certification = offerCertification;
                o.ScrapedDate = DateTime.UtcNow;
            })
        );
        await dbContext.SaveChangesAsync();

        var offers = await sut.FindAlertMatchingOffers(TimeSpan.FromMinutes(20));

        offers.Count.Should().Be(shouldMatch ? 1 : 0);
    }

    [TestCase(PersistedAlertOfferType.Buy)]
    [TestCase(PersistedAlertOfferType.Sell)]
    public async Task FindAlertMatchingOffers_should_not_return_old_offers(PersistedAlertOfferType alertOfferType)
    {
        var now = new DateTime(2022, 1, 1);
        dateTime.SetupGet(d => d.Now).Returns(now);

        dbContext.AddRange(new PersistedAlert
        {
            OfferType = alertOfferType,
            ItemName = "Hellfire",
            PriceFrom = 150,
            PriceTo = 10000,
            CreatedDate = now
        });
        dbContext.AddRange(Build.PersistedOffer(alertOfferType)
            .With(o =>
            {
                o.Name = "Hellfire";
                o.Price = 150;
                o.ScrapedDate = now.AddMinutes(-21);
            })
        );
        await dbContext.SaveChangesAsync();

        var offers = await sut.FindAlertMatchingOffers(TimeSpan.FromMinutes(20));

        offers.Count.Should().Be(0);
    }

    [TestCase(PersistedAlertOfferType.Buy)]
    [TestCase(PersistedAlertOfferType.Sell)]
    public async Task FindAlertMatchingOffers_should_not_return_matching_offers_for_disabled_alerts(PersistedAlertOfferType alertOfferType)
    {
        dbContext.AddRange(new PersistedAlert
        {
            OfferType = alertOfferType,
            ItemName = "Hellfire",
            PriceFrom = 150,
            PriceTo = 10000,
            Disabled = true
        });
        dbContext.AddRange(Build.PersistedOffer(alertOfferType)
            .With(o =>
            {
                o.Name = "Hellfire";
                o.Price = 150;
                o.ScrapedDate = DateTime.UtcNow;
            })
        );
        await dbContext.SaveChangesAsync();

        var offers = await sut.FindAlertMatchingOffers(TimeSpan.FromMinutes(20));

        offers.Count.Should().Be(0);
    }

    [Test]
    public async Task FindAlertMatchingOffers_should_not_return_buy_offer_matches_for_sell_offer_type_alerts()
    {
        dbContext.AddRange(new PersistedAlert
        {
            OfferType = PersistedAlertOfferType.Sell,
            ItemName = "Hellfire",
            PriceFrom = 0,
            PriceTo = 100,
        });
        dbContext.AddRange(Build.PersistedOffer(PersistedAlertOfferType.Buy)
            .With(o =>
            {
                o.Name = "Hellfire";
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
            OfferType = PersistedAlertOfferType.Buy,
            ItemName = "Hellfire",
            PriceFrom = 150,
            PriceTo = 10000,
        });
        dbContext.AddRange(Build.PersistedOffer(PersistedAlertOfferType.Sell)
            .With(o =>
            {
                o.Name = "Hellfire";
                o.Price = 160;
                o.ScrapedDate = DateTime.UtcNow;
            })
        );

        await dbContext.SaveChangesAsync();

        var offers = await sut.FindAlertMatchingOffers(TimeSpan.FromMinutes(20));

        offers.Count.Should().Be(0);
    }
}