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
    private Mock<IDataRetentionPersistenceRepository> repository = null!;
    private Mock<IDataRetentionApplicationSettings> config = null!;

    [SetUp]
    public void Setup()
    {
        testContainer = TestContainer.Create();
        
        repository = testContainer.MockOf<IDataRetentionPersistenceRepository>();
        config = testContainer.MockOf<IDataRetentionApplicationSettings>();

        sut = testContainer.GetService<DataRetentionApplication>();
    }
    
    [Test]
    public async Task DeleteOldData_should_delete_items_in_correct_order()
    {
        var calls = new List<string>();
        config.SetupGet(c => c.NotificationsMaxAge).Returns(TimeSpan.FromDays(30));
        config.SetupGet(c => c.TradeOffersMaxAge).Returns(TimeSpan.FromDays(5));
        repository.Setup(p => p.DeleteOldNotifications(It.IsAny<TimeSpan>())).Callback(() => calls.Add("notifications"));
        repository.Setup(p => p.DeleteOldTradeOffers(It.IsAny<TimeSpan>())).Callback(() => calls.Add("tradeOffers"));
        repository.Setup(p => p.Vacuum()).Callback(() => calls.Add("vacuum"));

        await sut.DeleteOldData();

        calls[0].Should().Be("notifications");
        calls[1].Should().Be("tradeOffers");
        calls[2].Should().Be("vacuum");
    }

    [Test]
    public async Task DeleteOldData_should_not_delete_notifications_if_max_age_is_not_set()
    {
        config.SetupGet(c => c.NotificationsMaxAge).Returns((TimeSpan?)null);
        config.SetupGet(c => c.TradeOffersMaxAge).Returns(TimeSpan.FromDays(5));
        
        await sut.DeleteOldData();
        
        repository.Verify(p => p.DeleteOldNotifications(It.IsAny<TimeSpan>()), Times.Never);
    }
}