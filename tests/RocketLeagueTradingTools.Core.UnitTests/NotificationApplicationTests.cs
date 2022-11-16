using FluentAssertions;
using Moq;
using RocketLeagueTradingTools.Core.Application.Interfaces;
using RocketLeagueTradingTools.Core.Application.Notifications;
using RocketLeagueTradingTools.Core.Domain.Entities;
using RocketLeagueTradingTools.Core.Domain.Enumerations;
using RocketLeagueTradingTools.Core.UnitTests.Support;

namespace RocketLeagueTradingTools.Core.UnitTests;

[TestFixture]
public class NotificationApplicationTests
{
    private NotificationApplication sut = null!;
    private Mock<IPersistenceRepository> persistence = null!;
    private Mock<INotificationApplicationSettings> config = null!;

    [SetUp]
    public void Setup()
    {
        persistence = new Mock<IPersistenceRepository>();

        config = new Mock<INotificationApplicationSettings>();
        config.SetupGet(c => c.NotificationsExpiration).Returns(TimeSpan.FromDays(1));

        sut = new NotificationApplication(persistence.Object, config.Object);
    }

    [Test]
    public async Task GetNotifications_should_return_old_notifications()
    {
        var tradeOffer = new TradeOfferBuilder()
            .WithTradeItem(Build.TradeItem("Hellfire"))
            .WithPrice(100)
            .WithRlgId("1")
            .Build();

        persistence.Setup(p => p.GetNotifications(It.IsAny<int>())).ReturnsAsync(new List<Notification>
        {
            new(tradeOffer)
        });

        var notifications = await sut.GetNotifications(20);

        notifications.Count.Should().Be(1);
    }

    [Test]
    public async Task RefreshNotifications_should_create_new_notifications()
    {
        var tradeOffer = new TradeOfferBuilder()
            .WithTradeItem(Build.TradeItem("Hellfire"))
            .WithPrice(100)
            .WithRlgId("1")
            .Build();

        persistence.Setup(p => p.GetNotifications(It.IsAny<TimeSpan>())).ReturnsAsync(new List<Notification>());
        persistence.Setup(p => p.FindAlertMatchingOffers(It.IsAny<TimeSpan>())).ReturnsAsync(new List<TradeOffer> { tradeOffer });

        await sut.RefreshNotifications();

        persistence.Verify(p => p.AddNotifications(It.Is<IList<Notification>>(n => n.Single().TradeOffer.Equals(tradeOffer))), Times.Once);
    }

    [TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.RefreshNotifications_should_create_new_notifications_for_alert_matching_offers))]
    public async Task RefreshNotifications_should_create_new_notifications_for_alert_matching_offers(TradeOffer oldNotificationOffer, TradeOffer alertMatchingOffer)
    {
        persistence.Setup(p => p.GetNotifications(It.IsAny<TimeSpan>())).ReturnsAsync(new List<Notification>
        {
            new(oldNotificationOffer)
        });
        persistence.Setup(p => p.FindAlertMatchingOffers(It.IsAny<TimeSpan>())).ReturnsAsync(new List<TradeOffer>
        {
            alertMatchingOffer
        });

        await sut.RefreshNotifications();

        persistence.Verify(p => p.AddNotifications(It.Is<IList<Notification>>(n => n.Single().TradeOffer.Equals(alertMatchingOffer))), Times.Once);
    }

    [TestCaseSource(typeof(TestCaseSources), nameof(TestCaseSources.RefreshNotifications_should_not_create_new_notifications_for_already_existing_alert_matching_offer_notifications))]
    public async Task RefreshNotifications_should_not_create_new_notifications_for_already_existing_alert_matching_offer_notifications(TradeOffer oldNotificationOffer, TradeOffer alertMatchingOffer)
    {
        persistence.Setup(p => p.GetNotifications(It.IsAny<TimeSpan>())).ReturnsAsync(new List<Notification>
        {
            new(oldNotificationOffer)
        });
        persistence.Setup(p => p.FindAlertMatchingOffers(It.IsAny<TimeSpan>())).ReturnsAsync(new List<TradeOffer>
        {
            alertMatchingOffer
        });

        await sut.RefreshNotifications();

        persistence.Verify(p => p.AddNotifications(It.IsAny<IList<Notification>>()), Times.Never);
    }

    class TestCaseSources
    {
        public static IEnumerable<object[]> RefreshNotifications_should_create_new_notifications_for_alert_matching_offers
        {
            get
            {
                yield return new object[] // Id
                {
                    new TradeOfferBuilder()
                        .WithTradeItem(Build.TradeItem("Vortex"))
                        .WithRlgId("1")
                        .WithPrice(100)
                        .Build()
                    ,
                    new TradeOfferBuilder()
                        .WithTradeItem(Build.TradeItem("Vortex"))
                        .WithRlgId("2")
                        .WithPrice(100)
                        .Build()
                };
                yield return new object[] // Price
                {
                    new TradeOfferBuilder()
                        .WithTradeItem(Build.TradeItem("Vortex"))
                        .WithRlgId("1")
                        .WithPrice(100)
                        .Build()
                    ,
                    new TradeOfferBuilder()
                        .WithTradeItem(Build.TradeItem("Vortex"))
                        .WithRlgId("1")
                        .WithPrice(110)
                        .Build()
                };
                yield return new object[] // Color
                {
                    new TradeOfferBuilder()
                        .WithTradeItem(Build.TradeItem("Vortex"))
                        .WithRlgId("1")
                        .WithPrice(100)
                        .Build()
                    ,
                    new TradeOfferBuilder()
                        .WithTradeItem(Build.TradeItem("Vortex").WithColor("Orange"))
                        .WithRlgId("1")
                        .WithPrice(100)
                        .Build()
                };
                yield return new object[] // Certification
                {
                    new TradeOfferBuilder()
                        .WithTradeItem(Build.TradeItem("Vortex"))
                        .WithRlgId("1")
                        .WithPrice(100)
                        .Build()
                    ,
                    new TradeOfferBuilder()
                        .WithTradeItem(Build.TradeItem("Vortex").WithCertification("Sniper"))
                        .WithRlgId("1")
                        .WithPrice(100)
                        .Build()
                };
                yield return new object[] // Item type
                {
                    new TradeOfferBuilder()
                        .WithTradeItem(Build.TradeItem("Nomster").WithType(TradeItemType.GoalExplosion))
                        .WithRlgId("1")
                        .WithPrice(50)
                        .Build()
                    ,
                    new TradeOfferBuilder()
                        .WithTradeItem(Build.TradeItem("Nomster").WithType(TradeItemType.Wheels))
                        .WithRlgId("1")
                        .WithPrice(50)
                        .Build()
                };
            }
        }

        public static IEnumerable<object[]> RefreshNotifications_should_not_create_new_notifications_for_already_existing_alert_matching_offer_notifications
        {
            get
            {
                yield return new object[] // Id, Price
                {
                    new TradeOfferBuilder()
                        .WithTradeItem(Build.TradeItem("Vortex"))
                        .WithRlgId("1")
                        .WithPrice(100)
                        .Build()
                    ,
                    new TradeOfferBuilder()
                        .WithTradeItem(Build.TradeItem("Vortex"))
                        .WithRlgId("1")
                        .WithPrice(100)
                        .Build()
                };
                yield return new object[] // Color
                {
                    new TradeOfferBuilder()
                        .WithTradeItem(Build.TradeItem("Vortex").WithColor("Orange"))
                        .WithRlgId("1")
                        .WithPrice(100)
                        .Build()
                    ,
                    new TradeOfferBuilder()
                        .WithTradeItem(Build.TradeItem("Vortex").WithColor("Orange"))
                        .WithRlgId("1")
                        .WithPrice(100)
                        .Build()
                };
                yield return new object[] // Certification
                {
                    new TradeOfferBuilder()
                        .WithTradeItem(Build.TradeItem("Vortex").WithCertification("Sniper"))
                        .WithRlgId("1")
                        .WithPrice(100)
                        .Build()
                    ,
                    new TradeOfferBuilder()
                        .WithTradeItem(Build.TradeItem("Vortex").WithCertification("Sniper"))
                        .WithRlgId("1")
                        .WithPrice(100)
                        .Build()
                };
                yield return new object[] // Item type
                {
                    new TradeOfferBuilder()
                        .WithTradeItem(Build.TradeItem("Nomster").WithType(TradeItemType.GoalExplosion))
                        .WithRlgId("1")
                        .WithPrice(100)
                        .Build()
                    ,
                    new TradeOfferBuilder()
                        .WithTradeItem(Build.TradeItem("Nomster").WithType(TradeItemType.GoalExplosion))
                        .WithRlgId("1")
                        .WithPrice(100)
                        .Build()
                };
            }
        }
    }
}