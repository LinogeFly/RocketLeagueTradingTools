using FluentAssertions;
using RocketLeagueTradingTools.Core.Domain.Enumerations;
using RocketLeagueTradingTools.Infrastructure.IntegrationTests.Support;
using RocketLeagueTradingTools.Infrastructure.Persistence.Repositories;
using RocketLeagueTradingTools.Test;

namespace RocketLeagueTradingTools.Infrastructure.IntegrationTests.Persistence;

[TestFixture]
public class DataRetentionPersistenceRepositoryTests
{
    private DataRetentionPersistenceRepository sut = null!;
    private TradeOfferPersistenceRepository offerRepository = null!;
    private AlertPersistenceRepository alertRepository = null!;
    private NotificationPersistenceRepository notificationRepository = null!;
    private TestContainer testContainer = null!;

    [SetUp]
    public void Setup()
    {
        testContainer = TestContainer.Create();
        testContainer.ResetDatabase();

        offerRepository = testContainer.GetService<TradeOfferPersistenceRepository>();
        alertRepository = testContainer.GetService<AlertPersistenceRepository>();
        notificationRepository = testContainer.GetService<NotificationPersistenceRepository>();
        sut = testContainer.GetService<DataRetentionPersistenceRepository>();
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
        await notificationRepository.AddNotifications(new[]
        {
            A.Notification().WithScrapedOffer(offer).Build()
        });

        // Act
        testContainer.NowIs(fakeNow);
        await sut.DeleteOldNotifications(TimeSpan.FromMinutes(9));

        // Assert
        var notifications = await notificationRepository.GetNotifications(10);
        var offers = await offerRepository.FindAlertMatchingTradeOffers(TimeSpan.FromMinutes(1));
        notifications.Count.Should().Be(0);
        offers.Count.Should().Be(1);
    }
    
    [Test]
    public async Task DeleteOldOffers_should_delete_only_old_offers_that_do_not_have_related_notifications()
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
            ).WithScrapedDate(fakeNow.AddMinutes(-10)).Build(),
            A.ScrapedOffer().WithOffer(
                A.TradeOffer().WithItem(
                    A.TradeItem().WithName("Dueling Dragons")
                ).WithPrice(400).WithType(TradeOfferType.Sell)
            ).WithScrapedDate(fakeNow.AddMinutes(-10)).Build()
        });
        await alertRepository.AddAlert(An.Alert().WithItemName("Fennec").WithPrice(0, 300).WithType(TradeOfferType.Sell).Build());
        await alertRepository.AddAlert(An.Alert().WithItemName("Dueling Dragons").WithPrice(0, 400).WithType(TradeOfferType.Sell).Build());
        var fennecOffer = (await offerRepository.FindAlertMatchingTradeOffers(TimeSpan.FromMinutes(11))).Single(o => o.TradeOffer.Item.Name == "Fennec");
        // Add notification only for Fennec trade offer, but not for Dueling Dragons. 
        await notificationRepository.AddNotifications(new[]
        {
            A.Notification().WithScrapedOffer(fennecOffer).Build()
        });

        // Act
        await sut.DeleteOldTradeOffers(TimeSpan.FromMinutes(9));

        // Assert
        var offers = await offerRepository.FindAlertMatchingTradeOffers(TimeSpan.FromMinutes(11));
        var fennecOfferAfterAct = offers.SingleOrDefault(o => o.TradeOffer.Item.Name == "Fennec");
        var dragonsOfferAfterAct = offers.SingleOrDefault(o => o.TradeOffer.Item.Name == "Dueling Dragons");
        //
        fennecOfferAfterAct.Should().NotBeNull();
        dragonsOfferAfterAct.Should().BeNull();
    }

    [Test]
    public async Task Vacuum_should_execute_database_shrink_sql_query()
    {
        var act = () => sut.Vacuum();

        // Can't completely verify that the database gets vacuumed/shrank.
        // At least we can check that the execution doesn't throw exceptions.
        // For example, if SQL query is wrong and fails when executed.
        await act.Should().NotThrowAsync();
    }
}