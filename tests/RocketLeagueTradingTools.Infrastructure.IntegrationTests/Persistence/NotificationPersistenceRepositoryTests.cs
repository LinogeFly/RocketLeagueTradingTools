using FluentAssertions;
using RocketLeagueTradingTools.Core.Domain.Entities;
using RocketLeagueTradingTools.Core.Domain.Enumerations;
using RocketLeagueTradingTools.Core.Domain.ValueObjects;
using RocketLeagueTradingTools.Infrastructure.IntegrationTests.Support;
using RocketLeagueTradingTools.Infrastructure.Persistence.Repositories;
using RocketLeagueTradingTools.Test;

namespace RocketLeagueTradingTools.Infrastructure.IntegrationTests.Persistence;

[TestFixture]
public class NotificationPersistenceRepositoryTests
{
    private NotificationPersistenceRepository sut = null!;
    private TradeOfferPersistenceRepository offerRepository = null!;
    private AlertPersistenceRepository alertRepository = null!;
    private TestContainer testContainer = null!;

    [SetUp]
    public void Setup()
    {
        testContainer = TestContainer.Create();
        testContainer.ResetDatabase();

        offerRepository = testContainer.GetService<TradeOfferPersistenceRepository>();
        alertRepository = testContainer.GetService<AlertPersistenceRepository>();
        sut = testContainer.GetService<NotificationPersistenceRepository>();
    }

    [TestCaseSource(nameof(GetNotificationsActionsSource))]
    public async Task GetNotifications_should_return_notifications_with_Id_mapped(
        Func<NotificationPersistenceRepository, Task<IList<Notification>>> act)
    {
        // Arrange
        await offerRepository.AddTradeOffers(new[]
        {
            A.ScrapedOffer().WithOffer(
                A.TradeOffer().WithItem(
                    A.TradeItem().WithName("Fennec")
                ).WithPrice(300).WithType(TradeOfferType.Sell)
            ).Build()
        });
        await alertRepository.AddAlert(An.Alert().WithItemName("Fennec").WithPrice(0, 300).WithType(TradeOfferType.Sell).Build());
        var offer = (await offerRepository.FindAlertMatchingTradeOffers(TimeSpan.FromMinutes(1))).Single();
        await sut.AddNotifications(new[]
        {
            A.Notification().WithScrapedOffer(offer).Build()
        });

        // Act
        var result = await act(sut);

        // Assert
        result.Single().Id.Should().NotBe(default);
    }

    [TestCaseSource(nameof(GetNotificationsActionsSource))]
    public async Task GetNotifications_should_return_notifications_with_ScrapedTradeOffer_Id_mapped(
        Func<NotificationPersistenceRepository, Task<IList<Notification>>> act)
    {
        // Arrange
        await offerRepository.AddTradeOffers(new[]
        {
            A.ScrapedOffer().WithOffer(
                A.TradeOffer().WithItem(
                    A.TradeItem().WithName("Fennec")
                ).WithPrice(300).WithType(TradeOfferType.Sell)
            ).Build()
        });
        await alertRepository.AddAlert(An.Alert().WithItemName("Fennec").WithPrice(0, 300).WithType(TradeOfferType.Sell).Build());
        var offer = (await offerRepository.FindAlertMatchingTradeOffers(TimeSpan.FromMinutes(1))).Single();
        await sut.AddNotifications(new[]
        {
            A.Notification().WithScrapedOffer(offer).Build()
        });

        // Act
        var result = await act(sut);

        // Assert
        result.Single().ScrapedTradeOffer.Id.Should().NotBe(default);
    }

    [TestCaseSource(nameof(GetNotificationsActionsSource))]
    public async Task GetNotifications_should_return_notifications_with_ScrapedTradeOffer_ScrapedDate_mapped(
        Func<NotificationPersistenceRepository, Task<IList<Notification>>> act)
    {
        // Arrange
        var fakeNow = new DateTime(2022, 1, 1);
        testContainer.NowIs(fakeNow);
        await offerRepository.AddTradeOffers(new[]
        {
            A.ScrapedOffer().WithOffer(
                A.TradeOffer().WithItem(
                    A.TradeItem().WithName("Fennec")
                ).WithPrice(300).WithType(TradeOfferType.Sell)
            ).WithScrapedDate(fakeNow).Build()
        });
        await alertRepository.AddAlert(An.Alert().WithItemName("Fennec").WithPrice(0, 300).WithType(TradeOfferType.Sell).Build());
        var offer = (await offerRepository.FindAlertMatchingTradeOffers(TimeSpan.FromMinutes(1))).Single();
        await sut.AddNotifications(new[]
        {
            A.Notification().WithScrapedOffer(offer).Build()
        });

        // Act
        var result = await act(sut);

        // Assert
        result.Single().ScrapedTradeOffer.ScrapedDate.Should().Be(fakeNow);
    }

    [Test, Combinatorial]
    public async Task GetNotifications_should_return_notifications_with_ScrapedTradeOffer_TradeOffer_OfferType_mapped(
        [Values(TradeOfferType.Buy, TradeOfferType.Sell)]
        TradeOfferType offerType,
        [ValueSource(nameof(GetNotificationsActionsSource))]
        Func<NotificationPersistenceRepository, Task<IList<Notification>>> act)
    {
        // Arrange
        await offerRepository.AddTradeOffers(new[]
        {
            A.ScrapedOffer().WithOffer(
                A.TradeOffer().WithItem(
                    A.TradeItem().WithName("Fennec")
                ).WithPrice(300).WithType(offerType)
            ).Build()
        });
        await alertRepository.AddAlert(An.Alert().WithItemName("Fennec").WithPrice(0, 300).WithType(offerType).Build());
        var offer = (await offerRepository.FindAlertMatchingTradeOffers(TimeSpan.FromMinutes(1))).Single();
        await sut.AddNotifications(new[]
        {
            A.Notification().WithScrapedOffer(offer).Build()
        });

        // Act
        var result = await act(sut);

        // Assert
        result.Single().ScrapedTradeOffer.TradeOffer.OfferType.Should().Be(offerType);
    }

    [TestCaseSource(nameof(GetNotificationsActionsSource))]
    public async Task GetNotifications_should_return_notifications_with_ScrapedTradeOffer_TradeOffer_Item_Name_mapped(
        Func<NotificationPersistenceRepository, Task<IList<Notification>>> act)
    {
        // Arrange
        await offerRepository.AddTradeOffers(new[]
        {
            A.ScrapedOffer().WithOffer(
                A.TradeOffer().WithItem(
                    A.TradeItem().WithName("Fennec")
                ).WithPrice(300).WithType(TradeOfferType.Sell)
            ).Build()
        });
        await alertRepository.AddAlert(An.Alert().WithItemName("Fennec").WithPrice(0, 300).WithType(TradeOfferType.Sell).Build());
        var offer = (await offerRepository.FindAlertMatchingTradeOffers(TimeSpan.FromMinutes(1))).Single();
        await sut.AddNotifications(new[]
        {
            A.Notification().WithScrapedOffer(offer).Build()
        });

        // Act
        var result = await act(sut);

        // Assert
        result.Single().ScrapedTradeOffer.TradeOffer.Item.Name.Should().Be("Fennec");
    }

    [Test, Combinatorial]
    public async Task GetNotifications_should_return_notifications_with_ScrapedTradeOffer_TradeOffer_Item_ItemType_mapped(
        [Values(TradeItemType.Unknown, TradeItemType.Body, TradeItemType.Decal, TradeItemType.PaintFinish, TradeItemType.Wheels,
            TradeItemType.RocketBoost, TradeItemType.Topper, TradeItemType.Antenna, TradeItemType.GoalExplosion, TradeItemType.Trail,
            TradeItemType.Banner, TradeItemType.AvatarBorder)]
        TradeItemType itemType,
        [ValueSource(nameof(GetNotificationsActionsSource))]
        Func<NotificationPersistenceRepository, Task<IList<Notification>>> act)
    {
        // Arrange
        await offerRepository.AddTradeOffers(new[]
        {
            A.ScrapedOffer().WithOffer(
                A.TradeOffer().WithItem(
                    A.TradeItem().WithName("Fennec").WithType(itemType)
                ).WithPrice(300).WithType(TradeOfferType.Sell)
            ).Build()
        });
        await alertRepository.AddAlert(An.Alert().WithItemName("Fennec").WithPrice(0, 300).WithType(TradeOfferType.Sell).Build());
        var offer = (await offerRepository.FindAlertMatchingTradeOffers(TimeSpan.FromMinutes(1))).Single();
        await sut.AddNotifications(new[]
        {
            A.Notification().WithScrapedOffer(offer).Build()
        });

        // Act
        var result = await act(sut);

        // Assert
        result.Single().ScrapedTradeOffer.TradeOffer.Item.ItemType.Should().Be(itemType);
    }

    [TestCaseSource(nameof(GetNotificationsActionsSource))]
    public async Task GetNotifications_should_return_notifications_with_ScrapedTradeOffer_TradeOffer_Item_Color_mapped(
        Func<NotificationPersistenceRepository, Task<IList<Notification>>> act)
    {
        // Arrange
        await offerRepository.AddTradeOffers(new[]
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
        var offer = (await offerRepository.FindAlertMatchingTradeOffers(TimeSpan.FromMinutes(1))).Single();
        await sut.AddNotifications(new[]
        {
            A.Notification().WithScrapedOffer(offer).Build()
        });

        // Act
        var result = await act(sut);

        // Assert
        result.Single().ScrapedTradeOffer.TradeOffer.Item.Color.Should().Be("Orange");
    }

    [TestCaseSource(nameof(GetNotificationsActionsSource))]
    public async Task GetNotifications_should_return_notifications_with_ScrapedTradeOffer_TradeOffer_Item_Certification_mapped(
        Func<NotificationPersistenceRepository, Task<IList<Notification>>> act)
    {
        // Arrange
        await offerRepository.AddTradeOffers(new[]
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
        var offer = (await offerRepository.FindAlertMatchingTradeOffers(TimeSpan.FromMinutes(1))).Single();
        await sut.AddNotifications(new[]
        {
            A.Notification().WithScrapedOffer(offer).Build()
        });

        // Act
        var result = await act(sut);

        // Assert
        result.Single().ScrapedTradeOffer.TradeOffer.Item.Certification.Should().Be("Sniper");
    }

    [TestCaseSource(nameof(GetNotificationsActionsSource))]
    public async Task GetNotifications_should_return_notifications_with_ScrapedTradeOffer_TradeOffer_Price_mapped(
        Func<NotificationPersistenceRepository, Task<IList<Notification>>> act)
    {
        // Arrange
        await offerRepository.AddTradeOffers(new[]
        {
            A.ScrapedOffer().WithOffer(
                A.TradeOffer().WithItem(
                    A.TradeItem().WithName("Fennec")
                ).WithPrice(300).WithType(TradeOfferType.Sell)
            ).Build()
        });
        await alertRepository.AddAlert(An.Alert().WithItemName("Fennec").WithPrice(0, 300).WithType(TradeOfferType.Sell).Build());
        var offer = (await offerRepository.FindAlertMatchingTradeOffers(TimeSpan.FromMinutes(1))).Single();
        await sut.AddNotifications(new[]
        {
            A.Notification().WithScrapedOffer(offer).Build()
        });

        // Act
        var result = await act(sut);

        // Assert
        result.Single().ScrapedTradeOffer.TradeOffer.Price.Should().Be(300);
    }

    [TestCaseSource(nameof(GetNotificationsActionsSource))]
    public async Task GetNotifications_should_return_notifications_with_ScrapedTradeOffer_TradeOffer_Link_mapped(
        Func<NotificationPersistenceRepository, Task<IList<Notification>>> act)
    {
        // Arrange
        var expectedLink = $"https://rocket-league.com/trade/{Guid.NewGuid()}";
        await offerRepository.AddTradeOffers(new[]
        {
            A.ScrapedOffer().WithOffer(
                A.TradeOffer().WithItem(
                    A.TradeItem().WithName("Fennec")
                ).WithPrice(300).WithType(TradeOfferType.Sell).WithLink(expectedLink)
            ).Build()
        });
        await alertRepository.AddAlert(An.Alert().WithItemName("Fennec").WithPrice(0, 300).WithType(TradeOfferType.Sell).Build());
        var offer = (await offerRepository.FindAlertMatchingTradeOffers(TimeSpan.FromMinutes(1))).Single();
        await sut.AddNotifications(new[]
        {
            A.Notification().WithScrapedOffer(offer).Build()
        });

        // Act
        var result = await act(sut);

        // Assert
        result.Single().ScrapedTradeOffer.TradeOffer.Link.Should().Be(expectedLink);
    }

    [TestCaseSource(nameof(GetNotificationsActionsSource))]
    public async Task GetNotifications_should_return_notifications_with_ScrapedTradeOffer_TradeOffer_Trader_mapped(
        Func<NotificationPersistenceRepository, Task<IList<Notification>>> act)
    {
        // Arrange
        var expectedTrader = new Trader(TradingSite.RocketLeagueGarage, Guid.NewGuid().ToString());
        await offerRepository.AddTradeOffers(new[]
        {
            A.ScrapedOffer().WithOffer(
                A.TradeOffer().WithItem(
                    A.TradeItem().WithName("Fennec")
                ).WithPrice(300).WithType(TradeOfferType.Sell).WithTrader(expectedTrader)
            ).Build()
        });
        await alertRepository.AddAlert(An.Alert().WithItemName("Fennec").WithPrice(0, 300).WithType(TradeOfferType.Sell).Build());
        var offer = (await offerRepository.FindAlertMatchingTradeOffers(TimeSpan.FromMinutes(1))).Single();
        await sut.AddNotifications(new[]
        {
            A.Notification().WithScrapedOffer(offer).Build()
        });

        // Act
        var result = await act(sut);

        // Assert
        result.Single().ScrapedTradeOffer.TradeOffer.Trader.Should().Be(expectedTrader);
    }

    [Test]
    public async Task GetNotifications_by_page_size_should_return_notifications_page()
    {
        // Arrange
        await offerRepository.AddTradeOffers(new[]
        {
            A.ScrapedOffer().WithOffer(
                A.TradeOffer().WithItem(
                    A.TradeItem().WithName("Fennec")
                ).WithPrice(300).WithType(TradeOfferType.Sell)
            ).Build()
        });
        await alertRepository.AddAlert(An.Alert().WithItemName("Fennec").WithPrice(0, 300).WithType(TradeOfferType.Sell).Build());
        var offer = (await offerRepository.FindAlertMatchingTradeOffers(TimeSpan.FromMinutes(1))).Single();
        await sut.AddNotifications(new[]
        {
            A.Notification().WithScrapedOffer(offer).Build(),
            A.Notification().WithScrapedOffer(offer).Build(),
            A.Notification().WithScrapedOffer(offer).Build()
        });

        // Act
        var result = await sut.GetNotifications(2);

        // Assert
        result.Count.Should().Be(2);
    }

    [Test]
    public async Task GetNotifications_by_page_size_should_return_notifications_ordered_by_created_date_newest_first()
    {
        // Arrange
        await offerRepository.AddTradeOffers(new[]
        {
            A.ScrapedOffer().WithOffer(
                A.TradeOffer().WithItem(
                    A.TradeItem().WithName("Fennec")
                ).WithPrice(300).WithType(TradeOfferType.Sell)
            ).Build(),
            A.ScrapedOffer().WithOffer(
                A.TradeOffer().WithItem(
                    A.TradeItem().WithName("Dueling Dragons")
                ).WithPrice(400).WithType(TradeOfferType.Sell)
            ).Build()
        });
        await alertRepository.AddAlert(An.Alert().WithItemName("Fennec").WithPrice(0, 300).WithType(TradeOfferType.Sell).Build());
        await alertRepository.AddAlert(An.Alert().WithItemName("Dueling Dragons").WithPrice(0, 400).WithType(TradeOfferType.Sell).Build());
        var offers = await offerRepository.FindAlertMatchingTradeOffers(TimeSpan.FromMinutes(1));
        // Older notification is for Fennec
        testContainer.NowIs(new DateTime(2022, 1, 1));
        await sut.AddNotifications(new[]
        {
            A.Notification().WithScrapedOffer(offers.Single(o => o.TradeOffer.Item.Name == "Fennec")).Build()
        });
        // Newer notification is for Dueling Dragons
        testContainer.NowIs(new DateTime(2022, 1, 2));
        await sut.AddNotifications(new[]
        {
            A.Notification().WithScrapedOffer(offers.Single(o => o.TradeOffer.Item.Name == "Dueling Dragons")).Build()
        });

        // Act
        var result = await sut.GetNotifications(2);

        // Assert
        result.Count.Should().Be(2);
        result[0].ScrapedTradeOffer.TradeOffer.Item.Name.Should().Be("Dueling Dragons");
        result[1].ScrapedTradeOffer.TradeOffer.Item.Name.Should().Be("Fennec");
    }

    [Test]
    public async Task GetNotifications_by_timespan_should_not_return_expired_notifications()
    {
        // Arrange
        var fakeNow = new DateTime(2022, 1, 1);
        await offerRepository.AddTradeOffers(new[]
        {
            A.ScrapedOffer().WithOffer(
                A.TradeOffer().WithItem(
                    A.TradeItem().WithName("Fennec")
                ).WithPrice(300).WithType(TradeOfferType.Sell)
            ).Build()
        });
        await alertRepository.AddAlert(An.Alert().WithItemName("Fennec").WithPrice(0, 300).WithType(TradeOfferType.Sell).Build());
        var offer = (await offerRepository.FindAlertMatchingTradeOffers(TimeSpan.FromMinutes(1))).Single();
        testContainer.NowIs(fakeNow.AddMinutes(-10));
        await sut.AddNotifications(new[]
        {
            A.Notification().WithScrapedOffer(offer).Build()
        });

        // Act
        testContainer.NowIs(fakeNow);
        var result = await sut.GetNotifications(TimeSpan.FromMinutes(9));

        // Assert
        result.Count.Should().Be(0);
    }

    [Test]
    public async Task GetNotificationsCount_should_return_total_notifications_count()
    {
        // Arrange
        await offerRepository.AddTradeOffers(new[]
        {
            A.ScrapedOffer().WithOffer(
                A.TradeOffer().WithItem(
                    A.TradeItem().WithName("Fennec")
                ).WithPrice(300).WithType(TradeOfferType.Sell)
            ).Build()
        });
        await alertRepository.AddAlert(An.Alert().WithItemName("Fennec").WithPrice(0, 300).WithType(TradeOfferType.Sell).Build());
        var offer = (await offerRepository.FindAlertMatchingTradeOffers(TimeSpan.FromMinutes(1))).Single();
        await sut.AddNotifications(new[]
        {
            A.Notification().WithScrapedOffer(offer).Build(),
            A.Notification().WithScrapedOffer(offer).Build(),
            A.Notification().WithScrapedOffer(offer).Build()
        });

        // Act
        var result = await sut.GetNotificationsCount();

        // Assert
        result.Should().Be(3);
    }

    [Test]
    public async Task AddNotifications_should_add_notifications_with_SeenDate_property_set_to_null()
    {
        // Arrange
        await offerRepository.AddTradeOffers(new[]
        {
            A.ScrapedOffer().WithOffer(
                A.TradeOffer().WithItem(
                    A.TradeItem().WithName("Fennec")
                ).WithPrice(300).WithType(TradeOfferType.Sell)
            ).Build()
        });
        await alertRepository.AddAlert(An.Alert().WithItemName("Fennec").WithPrice(0, 300).WithType(TradeOfferType.Sell).Build());
        var offer = (await offerRepository.FindAlertMatchingTradeOffers(TimeSpan.FromMinutes(1))).Single();

        // Act
        await sut.AddNotifications(new[]
        {
            A.Notification().WithScrapedOffer(offer).Build(),
        });

        // Assert
        var result = await sut.GetNotifications(10);
        result.Single().SeenDate.Should().BeNull();
    }

    [Test]
    public async Task MarkNotificationAsSeen_should_set_SeenDate_property_to_current_time_for_one_notification()
    {
        // Arrange
        var fakeNow = new DateTime(2022, 1, 1);
        testContainer.NowIs(fakeNow);
        await offerRepository.AddTradeOffers(new[]
        {
            A.ScrapedOffer().WithOffer(
                A.TradeOffer().WithItem(
                    A.TradeItem().WithName("Fennec")
                ).WithPrice(300).WithType(TradeOfferType.Sell)
            ).Build()
        });
        await alertRepository.AddAlert(An.Alert().WithItemName("Fennec").WithPrice(0, 300).WithType(TradeOfferType.Sell).Build());
        var offer = (await offerRepository.FindAlertMatchingTradeOffers(TimeSpan.FromMinutes(1))).Single();
        await sut.AddNotifications(new[]
        {
            A.Notification().WithScrapedOffer(offer).Build(),
        });
        var notification = (await sut.GetNotifications(10)).Single();

        // Act
        await sut.MarkNotificationAsSeen(notification.Id);

        // Assert
        var result = await sut.GetNotifications(10);
        result.Single().SeenDate.Should().Be(fakeNow);
    }

    [Test]
    public async Task MarkAllNotificationAsSeen_should_set_SeenDate_property_to_current_time_for_all_notification()
    {
        // Arrange
        var fakeNow = new DateTime(2022, 1, 1);
        testContainer.NowIs(fakeNow);
        await offerRepository.AddTradeOffers(new[]
        {
            A.ScrapedOffer().WithOffer(
                A.TradeOffer().WithItem(
                    A.TradeItem().WithName("Fennec")
                ).WithPrice(300).WithType(TradeOfferType.Sell)
            ).Build()
        });
        await alertRepository.AddAlert(An.Alert().WithItemName("Fennec").WithPrice(0, 300).WithType(TradeOfferType.Sell).Build());
        var offer = (await offerRepository.FindAlertMatchingTradeOffers(TimeSpan.FromMinutes(1))).Single();
        await sut.AddNotifications(new[]
        {
            A.Notification().WithScrapedOffer(offer).Build(),
            A.Notification().WithScrapedOffer(offer).Build(),
            A.Notification().WithScrapedOffer(offer).Build()
        });

        // Act
        await sut.MarkAllNotificationAsSeen();

        // Assert
        var result = await sut.GetNotifications(10);
        result.Count(n => n.SeenDate == fakeNow).Should().Be(3);
    }

    [Test]
    public async Task DeleteOldNotifications_should_delete_old_notifications_but_keep_related_offers()
    {
        // Arrange
        var fakeNow = new DateTime(2022, 1, 1);
        await offerRepository.AddTradeOffers(new[]
        {
            A.ScrapedOffer().WithOffer(
                A.TradeOffer().WithItem(
                    A.TradeItem().WithName("Fennec")
                ).WithPrice(300).WithType(TradeOfferType.Sell)
            ).Build()
        });
        await alertRepository.AddAlert(An.Alert().WithItemName("Fennec").WithPrice(0, 300).WithType(TradeOfferType.Sell).Build());
        var offer = (await offerRepository.FindAlertMatchingTradeOffers(TimeSpan.FromMinutes(1))).Single();
        testContainer.NowIs(fakeNow.AddMinutes(-10));
        await sut.AddNotifications(new[]
        {
            A.Notification().WithScrapedOffer(offer).Build()
        });

        // Act
        testContainer.NowIs(fakeNow);
        await sut.DeleteOldNotifications(TimeSpan.FromMinutes(9));

        // Assert
        var notifications = await sut.GetNotifications(10);
        var offers = await offerRepository.FindAlertMatchingTradeOffers(TimeSpan.FromMinutes(1));
        notifications.Count.Should().Be(0);
        offers.Count.Should().Be(1);
    }

    private static IEnumerable<Func<NotificationPersistenceRepository, Task<IList<Notification>>>> GetNotificationsActionsSource()
    {
        yield return sut => sut.GetNotifications(10);
        yield return sut => sut.GetNotifications(TimeSpan.FromMinutes(10));
    }
}