using FluentAssertions;
using Moq;
using RocketLeagueTradingTools.Core.Application;
using RocketLeagueTradingTools.Core.Application.Contracts;
using RocketLeagueTradingTools.Core.Domain.Entities;
using RocketLeagueTradingTools.Core.UnitTests.Support;

namespace RocketLeagueTradingTools.Core.UnitTests;

[TestFixture]
public class NotificationApplicationTests
{
    private NotificationApplication sut = null!;
    private Mock<ILog> log = null!;
    private Mock<IPersistenceRepository> persistence = null!;
    private Mock<IDateTime> dateTime = null!;
    private Mock<IConfiguration> config = null!;

    [SetUp]
    public void Setup()
    {
        log = new Mock<ILog>();
        persistence = new Mock<IPersistenceRepository>();

        dateTime = new Mock<IDateTime>();
        dateTime.SetupGet(d => d.Now).Returns(DateTime.UtcNow);

        config = new Mock<IConfiguration>();
        config.SetupGet(c => c.NotificationsPageSize).Returns(50);
        config.SetupGet(c => c.NotificationsExpirationInHours).Returns(24);

        sut = new NotificationApplication(persistence.Object, dateTime.Object, config.Object, log.Object);
    }

    [Test]
    public async Task GetNotifications_should_return_old_notifications()
    {
        persistence.Setup(p => p.GetNotifications(It.IsAny<int>())).ReturnsAsync(new List<Notification>
        {
            new(new TradeOffer(Build.TradeItem("Hellfire")))
        });

        var notifications = await sut.GetNotifications();

        notifications.Count.Should().Be(1);
    }

    [Test]
    public async Task RefreshNotifications_should_create_new_notifications()
    {
        var tradeOffer = new TradeOffer(Build.TradeItem("Hellfire"));

        persistence.Setup(p => p.GetNotifications(It.IsAny<TimeSpan>())).ReturnsAsync(new List<Notification>());
        persistence.Setup(p => p.FindAlertMatchingOffers(It.IsAny<int>())).ReturnsAsync(new List<TradeOffer> { tradeOffer });

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
        persistence.Setup(p => p.FindAlertMatchingOffers(It.IsAny<int>())).ReturnsAsync(new List<TradeOffer>
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
        persistence.Setup(p => p.FindAlertMatchingOffers(It.IsAny<int>())).ReturnsAsync(new List<TradeOffer>
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
                yield return new object[]
                {
                    new TradeOffer(Build.TradeItem("Vortex"))
                    {
                        SourceId = "1",
                        Link = "https://rocket-league.com/trade/1",
                        Price = 100
                    },
                    new TradeOffer(Build.TradeItem("Vortex"))
                    {
                        SourceId = "2",
                        Link = "https://rocket-league.com/trade/2",
                        Price = 100
                    }
                };
                yield return new object[]
                {
                    new TradeOffer(Build.TradeItem("Vortex"))
                    {
                        SourceId = "1",
                        Link = "https://rocket-league.com/trade/1",
                        Price = 100
                    },
                    new TradeOffer(Build.TradeItem("Vortex"))
                    {
                        SourceId = "1",
                        Link = "https://rocket-league.com/trade/1",
                        Price = 110
                    }
                };
                yield return new object[]
                {
                    new TradeOffer(Build.TradeItem("Vortex"))
                    {
                        SourceId = "1",
                        Link = "https://rocket-league.com/trade/1",
                        Price = 100
                    },
                    new TradeOffer(Build.TradeItem("Vortex").WithColor("Orange"))
                    {
                        SourceId = "1",
                        Link = "https://rocket-league.com/trade/1",
                        Price = 100
                    }
                };
                yield return new object[]
                {
                    new TradeOffer(Build.TradeItem("Vortex"))
                    {
                        SourceId = "1",
                        Link = "https://rocket-league.com/trade/1",
                        Price = 100
                    },
                    new TradeOffer(Build.TradeItem("Vortex").WithCertification("Sniper"))
                    {
                        SourceId = "1",
                        Link = "https://rocket-league.com/trade/1",
                        Price = 100
                    }
                };
            }
        }

        public static IEnumerable<object[]> RefreshNotifications_should_not_create_new_notifications_for_already_existing_alert_matching_offer_notifications
        {
            get
            {
                yield return new object[]
                {
                    new TradeOffer(Build.TradeItem("Vortex"))
                    {
                        SourceId = "1",
                        Link = "https://rocket-league.com/trade/1",
                        Price = 100
                    },
                    new TradeOffer(Build.TradeItem("Vortex"))
                    {
                        SourceId = "1",
                        Link = "https://rocket-league.com/trade/1",
                        Price = 100
                    }
                };
                yield return new object[]
                {
                    new TradeOffer(Build.TradeItem("Vortex").WithColor("Orange"))
                    {
                        SourceId = "1",
                        Link = "https://rocket-league.com/trade/1",
                        Price = 100
                    },
                    new TradeOffer(Build.TradeItem("Vortex").WithColor("Orange"))
                    {
                        SourceId = "1",
                        Link = "https://rocket-league.com/trade/1",
                        Price = 100
                    }
                };
                yield return new object[]
                {
                    new TradeOffer(Build.TradeItem("Vortex").WithCertification("Sniper"))
                    {
                        SourceId = "1",
                        Link = "https://rocket-league.com/trade/1",
                        Price = 100
                    },
                    new TradeOffer(Build.TradeItem("Vortex").WithCertification("Sniper"))
                    {
                        SourceId = "1",
                        Link = "https://rocket-league.com/trade/1",
                        Price = 100
                    }
                };
            }
        }
    }
}