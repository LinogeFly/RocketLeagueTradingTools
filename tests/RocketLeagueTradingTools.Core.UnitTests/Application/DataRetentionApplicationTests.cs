using FluentAssertions;
using Moq;
using RocketLeagueTradingTools.Core.Application.DataRetention;
using RocketLeagueTradingTools.Core.Application.Interfaces.Persistence;
using RocketLeagueTradingTools.Core.UnitTests.Support;

namespace RocketLeagueTradingTools.Core.UnitTests.Application;

[TestFixture]
public class DataRetentionApplicationTests
{
    private DataRetentionApplication sut = null!;
    private TestContainer testContainer = null!;
    private Mock<INotificationPersistenceRepository> notificationPersistence = null!;
    private Mock<ITradeOfferPersistenceRepository> tradeOfferPersistence = null!;
    private Mock<IDataRetentionApplicationSettings> config = null!;

    [SetUp]
    public void Setup()
    {
        testContainer = TestContainer.Create();
        
        notificationPersistence = testContainer.MockOf<INotificationPersistenceRepository>();
        tradeOfferPersistence = testContainer.MockOf<ITradeOfferPersistenceRepository>();
        config = testContainer.MockOf<IDataRetentionApplicationSettings>();

        sut = testContainer.GetService<DataRetentionApplication>();
    }
    
    [Test]
    public async Task DeleteOldData_should_delete_items_in_correct_order()
    {
        var deleteCalls = new List<string>();
        config.SetupGet(c => c.NotificationsMaxAge).Returns(TimeSpan.FromDays(30));
        config.SetupGet(c => c.TradeOffersMaxAge).Returns(TimeSpan.FromDays(5));
        notificationPersistence.Setup(p => p.DeleteOldNotifications(It.IsAny<TimeSpan>())).Callback(() => deleteCalls.Add("notifications"));
        tradeOfferPersistence.Setup(p => p.DeleteOldTradeOffers(It.IsAny<TimeSpan>())).Callback(() => deleteCalls.Add("tradeOffers"));

        await sut.DeleteOldData();

        deleteCalls[0].Should().Be("notifications");
        deleteCalls[1].Should().Be("tradeOffers");
    }

    [Test]
    public async Task DeleteOldData_should_not_delete_notifications_if_max_age_is_not_set()
    {
        config.SetupGet(c => c.NotificationsMaxAge).Returns((TimeSpan?)null);
        config.SetupGet(c => c.TradeOffersMaxAge).Returns(TimeSpan.FromDays(5));
        
        await sut.DeleteOldData();
        
        notificationPersistence.Verify(p => p.DeleteOldNotifications(It.IsAny<TimeSpan>()), Times.Never);
    }
}