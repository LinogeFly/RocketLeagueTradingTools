using FluentAssertions;
using Moq;
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
    private Mock<INotificationPersistenceRepository> notificationPersistence = null!;
    private Mock<ITradeOfferPersistenceRepository> tradeOfferPersistence = null!;
    private Mock<INotificationApplicationSettings> config = null!;

    [SetUp]
    public void Setup()
    {
        testContainer = TestContainer.Create();
        
        notificationPersistence = testContainer.MockOf<INotificationPersistenceRepository>();
        tradeOfferPersistence = testContainer.MockOf<ITradeOfferPersistenceRepository>();

        config = testContainer.MockOf<INotificationApplicationSettings>();
        config.SetupGet(c => c.NotificationsExpiration).Returns(TimeSpan.FromDays(1));

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
}