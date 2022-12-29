using FluentAssertions;
using Moq;
using RocketLeagueTradingTools.Core.Application.Alert;
using RocketLeagueTradingTools.Core.Application.Interfaces.Persistence;
using RocketLeagueTradingTools.Core.Application.Notification;
using RocketLeagueTradingTools.Core.Domain.Entities;
using RocketLeagueTradingTools.Core.UnitTests.Support;
using RocketLeagueTradingTools.Test;

namespace RocketLeagueTradingTools.Core.UnitTests.Application;

[TestFixture]
public class NotificationApplicationTests
{
    private NotificationApplication sut = null!;
    private TestContainer testContainer = null!;
    private AlertApplication alertApplication = null!;
    private Mock<INotificationPersistenceRepository> notificationPersistence = null!;
    private Mock<ITradeOfferPersistenceRepository> tradeOfferPersistence = null!;
    private Mock<INotificationApplicationSettings> config = null!;

    [SetUp]
    public void Setup()
    {
        testContainer = TestContainer.Create();

        notificationPersistence = testContainer.MockOf<INotificationPersistenceRepository>();
        tradeOfferPersistence = testContainer.MockOf<ITradeOfferPersistenceRepository>();
        alertApplication = testContainer.GetService<AlertApplication>();

        config = testContainer.MockOf<INotificationApplicationSettings>();
        config.SetupGet(c => c.NotificationsExpiration).Returns(TimeSpan.FromDays(1));

        testContainer.MockOf<INotificationSessionStorage>().SetupAllProperties();

        sut = testContainer.GetService<NotificationApplication>();
    }

    [Test]
    public async Task GetNotifications_should_return_old_notifications()
    {
        notificationPersistence.Setup(p => p.GetNotifications(It.IsAny<int>())).ReturnsAsync(new[]
        {
            A.Notification().Build()
        });

        var notifications = await sut.GetNotifications(20);

        notifications.Count.Should().Be(1);
    }

    [Test]
    public async Task RefreshNotifications_should_create_new_notifications_for_not_yet_notified_alert_matching_offers()
    {
        var scrapDate = new DateTime(2022, 1, 1);
        notificationPersistence.Setup(p => p.GetNotifications(It.IsAny<TimeSpan>())).ReturnsAsync(new[]
        {
            A.Notification()
                .WithScrapedOffer(A.ScrapedOffer()
                    .WithScrapedDate(scrapDate)
                    .WithOffer(A.TradeOffer().WithPrice(300)))
                .Build()
        });
        tradeOfferPersistence.Setup(p => p.FindAlertMatchingTradeOffers(It.IsAny<TimeSpan>())).ReturnsAsync(new[]
        {
            A.ScrapedOffer()
                .WithScrapedDate(scrapDate.AddMinutes(1))
                .WithOffer(A.TradeOffer().WithPrice(290))
                .Build()
        });

        await sut.RefreshNotifications();

        notificationPersistence.Verify(p => p.AddNotifications(It.Is<IList<Notification>>(n => n.Count == 1)), Times.Once);
    }

    [Test]
    public async Task RefreshNotifications_should_not_create_new_notifications_for_already_notified_alert_matching_offers()
    {
        var scrapDate = new DateTime(2022, 1, 1);
        notificationPersistence.Setup(p => p.GetNotifications(It.IsAny<TimeSpan>())).ReturnsAsync(new[]
        {
            A.Notification()
                .WithScrapedOffer(A.ScrapedOffer().WithScrapedDate(scrapDate))
                .Build()
        });
        tradeOfferPersistence.Setup(p => p.FindAlertMatchingTradeOffers(It.IsAny<TimeSpan>())).ReturnsAsync(new[]
        {
            A.ScrapedOffer()
                .WithScrapedDate(scrapDate.AddMinutes(1))
                .Build()
        });

        await sut.RefreshNotifications();

        notificationPersistence.Verify(p => p.AddNotifications(It.IsAny<IList<Notification>>()), Times.Never);
    }

    [Test]
    public async Task RefreshNotifications_should_do_a_full_refresh_when_called_for_the_first_time()
    {
        notificationPersistence.Setup(p => p.GetNotifications(It.IsAny<TimeSpan>())).ReturnsAsync(new List<Notification>());
        tradeOfferPersistence.Setup(p => p.FindAlertMatchingTradeOffers(It.IsAny<TimeSpan>())).ReturnsAsync(new List<ScrapedTradeOffer>());
        config.SetupGet(c => c.AlertOfferMaxAge).Returns(TimeSpan.FromMinutes(20));

        await sut.RefreshNotifications();

        tradeOfferPersistence.Verify(p => p.FindAlertMatchingTradeOffers(TimeSpan.FromMinutes(20)), Times.Once);
    }

    [Test]
    public async Task RefreshNotifications_should_do_a_full_refresh_after_new_alert_was_added()
    {
        // Arrange
        var fakeNow = new DateTime(2022, 1, 1);
        var findMatchingOffersArgs = new List<TimeSpan>();
        notificationPersistence.Setup(p => p.GetNotifications(It.IsAny<TimeSpan>())).ReturnsAsync(new List<Notification>());
        tradeOfferPersistence.Setup(p => p.FindAlertMatchingTradeOffers(Capture.In(findMatchingOffersArgs))).ReturnsAsync(new List<ScrapedTradeOffer>());
        config.SetupGet(c => c.AlertOfferMaxAge).Returns(TimeSpan.FromMinutes(20));

        // Act
        //
        // First refresh
        testContainer.NowIs(fakeNow);
        sut = testContainer.GetService<NotificationApplication>();
        await sut.RefreshNotifications();
        // Add new alert
        testContainer.NowIs(fakeNow.AddSeconds(2));
        await alertApplication.AddAlert(An.Alert().Build());
        // Second refresh
        testContainer.NowIs(fakeNow.AddSeconds(10));
        sut = testContainer.GetService<NotificationApplication>();
        await sut.RefreshNotifications();

        // Assert
        findMatchingOffersArgs.Should().BeEquivalentTo(new[] {TimeSpan.FromMinutes(20), TimeSpan.FromMinutes(20)});
    }

    [Test]
    public async Task RefreshNotifications_should_do_a_full_refresh_after_existing_alert_was_modified()
    {
        // Arrange
        var fakeNow = new DateTime(2022, 1, 1);
        var findMatchingOffersArgs = new List<TimeSpan>();
        notificationPersistence.Setup(p => p.GetNotifications(It.IsAny<TimeSpan>())).ReturnsAsync(new List<Notification>());
        tradeOfferPersistence.Setup(p => p.FindAlertMatchingTradeOffers(Capture.In(findMatchingOffersArgs))).ReturnsAsync(new List<ScrapedTradeOffer>());
        config.SetupGet(c => c.AlertOfferMaxAge).Returns(TimeSpan.FromMinutes(20));

        // Act
        //
        // First refresh
        testContainer.NowIs(fakeNow);
        sut = testContainer.GetService<NotificationApplication>();
        await sut.RefreshNotifications();
        // Update alert
        testContainer.NowIs(fakeNow.AddSeconds(2));
        await alertApplication.UpdateAlert(An.Alert().Build());
        // Second refresh
        testContainer.NowIs(fakeNow.AddSeconds(10));
        sut = testContainer.GetService<NotificationApplication>();
        await sut.RefreshNotifications();

        // Assert
        findMatchingOffersArgs.Should().BeEquivalentTo(new[] {TimeSpan.FromMinutes(20), TimeSpan.FromMinutes(20)});
    }

    [Test]
    public async Task RefreshNotifications_should_do_a_full_refresh_after_existing_alert_was_deleted()
    {
        // Arrange
        var fakeNow = new DateTime(2022, 1, 1);
        var findMatchingOffersArgs = new List<TimeSpan>();
        notificationPersistence.Setup(p => p.GetNotifications(It.IsAny<TimeSpan>())).ReturnsAsync(new List<Notification>());
        tradeOfferPersistence.Setup(p => p.FindAlertMatchingTradeOffers(Capture.In(findMatchingOffersArgs))).ReturnsAsync(new List<ScrapedTradeOffer>());
        config.SetupGet(c => c.AlertOfferMaxAge).Returns(TimeSpan.FromMinutes(20));

        // Act
        //
        // First refresh
        testContainer.NowIs(fakeNow);
        sut = testContainer.GetService<NotificationApplication>();
        await sut.RefreshNotifications();
        // Delete alert
        testContainer.NowIs(fakeNow.AddSeconds(2));
        await alertApplication.DeleteAlert(1);
        // Second refresh
        testContainer.NowIs(fakeNow.AddSeconds(10));
        sut = testContainer.GetService<NotificationApplication>();
        await sut.RefreshNotifications();

        // Assert
        findMatchingOffersArgs.Should().BeEquivalentTo(new[] {TimeSpan.FromMinutes(20), TimeSpan.FromMinutes(20)});
    }

    [Test]
    public async Task RefreshNotifications_should_do_an_incremental_refresh_when_called_for_the_second_time()
    {
        // Arrange
        var fakeNow = new DateTime(2022, 1, 1);
        var findMatchingOffersArgs = new List<TimeSpan>();
        notificationPersistence.Setup(p => p.GetNotifications(It.IsAny<TimeSpan>())).ReturnsAsync(new List<Notification>());
        tradeOfferPersistence.Setup(p => p.FindAlertMatchingTradeOffers(Capture.In(findMatchingOffersArgs))).ReturnsAsync(new List<ScrapedTradeOffer>());
        config.SetupGet(c => c.AlertOfferMaxAge).Returns(TimeSpan.FromMinutes(20));

        // Act
        //
        // First refresh
        testContainer.NowIs(fakeNow);
        sut = testContainer.GetService<NotificationApplication>();
        await sut.RefreshNotifications();
        // Second refresh
        testContainer.NowIs(fakeNow.AddSeconds(10));
        sut = testContainer.GetService<NotificationApplication>();
        await sut.RefreshNotifications();

        // Assert
        findMatchingOffersArgs.Should().BeEquivalentTo(new[] {TimeSpan.FromMinutes(20), TimeSpan.FromSeconds(10)});
    }
}