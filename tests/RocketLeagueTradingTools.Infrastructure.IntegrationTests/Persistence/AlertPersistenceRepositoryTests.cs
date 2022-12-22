using FluentAssertions;
using RocketLeagueTradingTools.Core.Domain.Enumerations;
using RocketLeagueTradingTools.Core.Domain.ValueObjects;
using RocketLeagueTradingTools.Infrastructure.IntegrationTests.Support;
using RocketLeagueTradingTools.Infrastructure.Persistence.Repositories;
using RocketLeagueTradingTools.Test;

namespace RocketLeagueTradingTools.Infrastructure.IntegrationTests.Persistence;

[TestFixture]
public class AlertPersistenceRepositoryTests
{
    private AlertPersistenceRepository sut = null!;
    private TestContainer testContainer = null!;

    [SetUp]
    public void Setup()
    {
        testContainer = TestContainer.Create();
        testContainer.ResetDatabase();

        sut = testContainer.GetService<AlertPersistenceRepository>();
    }

    [Test]
    public async Task GetAlert_should_return_alert_with_Id_mapped()
    {
        var alertId = await sut.AddAlert(An.Alert().Build());

        var result = await sut.GetAlert(alertId);

        result?.Id.Should().Be(alertId);
    }

    [TestCase(TradeOfferType.Buy)]
    [TestCase(TradeOfferType.Sell)]
    public async Task GetAlert_should_return_alert_with_OfferType_mapped(TradeOfferType offerType)
    {
        var alertId = await sut.AddAlert(An.Alert()
            .WithType(offerType)
            .Build()
        );

        var result = await sut.GetAlert(alertId);

        result?.OfferType.Should().Be(offerType);
    }

    [Test]
    public async Task GetAlert_should_return_alert_with_ItemName_mapped()
    {
        var alertId = await sut.AddAlert(An.Alert()
            .WithItemName("Fennec")
            .Build()
        );

        var result = await sut.GetAlert(alertId);

        result?.ItemName.Should().Be("Fennec");
    }

    [Test]
    public async Task GetAlert_should_return_alert_with_Price_mapped()
    {
        var expectedPrice = new PriceRange(0, 300);
        var alertId = await sut.AddAlert(An.Alert()
            .WithPrice(expectedPrice)
            .Build()
        );

        var result = await sut.GetAlert(alertId);

        result?.Price.Should().Be(expectedPrice);
    }

    [TestCase(AlertItemType.Any)]
    [TestCase(AlertItemType.Body)]
    [TestCase(AlertItemType.Decal)]
    [TestCase(AlertItemType.PaintFinish)]
    [TestCase(AlertItemType.Wheels)]
    [TestCase(AlertItemType.RocketBoost)]
    [TestCase(AlertItemType.Antenna)]
    [TestCase(AlertItemType.GoalExplosion)]
    [TestCase(AlertItemType.Trail)]
    [TestCase(AlertItemType.Banner)]
    [TestCase(AlertItemType.AvatarBorder)]
    public async Task GetAlert_should_return_alert_with_AlertItemType_mapped(AlertItemType itemType)
    {
        var alertId = await sut.AddAlert(An.Alert()
            .WithItemType(itemType)
            .Build()
        );

        var result = await sut.GetAlert(alertId);

        result?.ItemType.Should().Be(itemType);
    }

    [Test]
    public async Task GetAlert_should_return_alert_with_Color_mapped()
    {
        var alertId = await sut.AddAlert(An.Alert()
            .WithColor("Orange")
            .Build()
        );

        var result = await sut.GetAlert(alertId);

        result?.Color.Should().Be("Orange");
    }

    [Test]
    public async Task GetAlert_should_return_alert_with_Certification_mapped()
    {
        var alertId = await sut.AddAlert(An.Alert()
            .WithCertification("Sniper")
            .Build()
        );

        var result = await sut.GetAlert(alertId);

        result?.Certification.Should().Be("Sniper");
    }

    [Test]
    public async Task GetAlert_should_return_alert_with_CreatedDate_mapped()
    {
        var fakeNow = new DateTime(2022, 12, 31);
        testContainer.NowIs(fakeNow);
        var alertId = await sut.AddAlert(An.Alert().Build());

        var result = await sut.GetAlert(alertId);

        result?.CreatedDate.Should().Be(fakeNow);
    }

    [Test]
    public async Task GetAlerts_should_return_alerts_with_Id_mapped()
    {
        var alert1Id = await sut.AddAlert(An.Alert().Build());
        var alert2Id = await sut.AddAlert(An.Alert().Build());

        var result = await sut.GetAlerts();

        result.Count.Should().Be(2);
        result[0].Id.Should().Be(alert1Id);
        result[1].Id.Should().Be(alert2Id);
    }

    [TestCase(TradeOfferType.Buy)]
    [TestCase(TradeOfferType.Sell)]
    public async Task GetAlerts_should_return_alerts_with_OfferType_mapped(TradeOfferType offerType)
    {
        var alert1 = An.Alert()
            .WithType(offerType)
            .Build();
        var alert2 = An.Alert()
            .WithType(offerType)
            .Build();
        await sut.AddAlert(alert1);
        await sut.AddAlert(alert2);

        var result = await sut.GetAlerts();

        result.Count.Should().Be(2);
        result[0].OfferType.Should().Be(offerType);
        result[1].OfferType.Should().Be(offerType);
    }

    [Test]
    public async Task GetAlerts_should_return_alerts_with_ItemName_mapped()
    {
        var alert1 = An.Alert()
            .WithItemName("Fennec")
            .Build();
        var alert2 = An.Alert()
            .WithItemName("Dueling Dragons")
            .Build();
        await sut.AddAlert(alert1);
        await sut.AddAlert(alert2);

        var result = await sut.GetAlerts();

        result.Count.Should().Be(2);
        result[0].ItemName.Should().Be("Fennec");
        result[1].ItemName.Should().Be("Dueling Dragons");
    }

    [Test]
    public async Task GetAlerts_should_return_alerts_with_Price_mapped()
    {
        var expectedPrice1 = new PriceRange(0, 300);
        var expectedPrice2 = new PriceRange(100, 300);
        var alert1 = An.Alert()
            .WithPrice(expectedPrice1)
            .Build();
        var alert2 = An.Alert()
            .WithPrice(expectedPrice2)
            .Build();
        await sut.AddAlert(alert1);
        await sut.AddAlert(alert2);

        var result = await sut.GetAlerts();

        result.Count.Should().Be(2);
        result[0].Price.Should().Be(expectedPrice1);
        result[1].Price.Should().Be(expectedPrice2);
    }

    [TestCase(AlertItemType.Any)]
    [TestCase(AlertItemType.Body)]
    [TestCase(AlertItemType.Decal)]
    [TestCase(AlertItemType.PaintFinish)]
    [TestCase(AlertItemType.Wheels)]
    [TestCase(AlertItemType.RocketBoost)]
    [TestCase(AlertItemType.Antenna)]
    [TestCase(AlertItemType.GoalExplosion)]
    [TestCase(AlertItemType.Trail)]
    [TestCase(AlertItemType.Banner)]
    [TestCase(AlertItemType.AvatarBorder)]
    public async Task GetAlerts_should_return_alerts_with_AlertItemType_mapped(AlertItemType itemType)
    {
        var alert1 = An.Alert()
            .WithItemType(itemType)
            .Build();
        var alert2 = An.Alert()
            .WithItemType(itemType)
            .Build();
        await sut.AddAlert(alert1);
        await sut.AddAlert(alert2);

        var result = await sut.GetAlerts();

        result.Count.Should().Be(2);
        result[0].ItemType.Should().Be(itemType);
        result[1].ItemType.Should().Be(itemType);
    }

    [Test]
    public async Task GetAlerts_should_return_alerts_with_Color_mapped()
    {
        var alert1 = An.Alert()
            .WithColor("Orange")
            .Build();
        var alert2 = An.Alert()
            .WithColor("*")
            .Build();
        await sut.AddAlert(alert1);
        await sut.AddAlert(alert2);

        var result = await sut.GetAlerts();

        result.Count.Should().Be(2);
        result[0].Color.Should().Be("Orange");
        result[1].Color.Should().Be("*");
    }

    [Test]
    public async Task GetAlerts_should_return_alerts_with_Certification_mapped()
    {
        var alert1 = An.Alert()
            .WithCertification("Sniper")
            .Build();
        var alert2 = An.Alert()
            .WithCertification("*")
            .Build();
        await sut.AddAlert(alert1);
        await sut.AddAlert(alert2);

        var result = await sut.GetAlerts();

        result.Count.Should().Be(2);
        result[0].Certification.Should().Be("Sniper");
        result[1].Certification.Should().Be("*");
    }

    [Test]
    public async Task GetAlerts_should_return_alerts_with_CreatedDate_mapped()
    {
        var fakeNow = new DateTime(2022, 12, 31);
        var alert1 = An.Alert().Build();
        var alert2 = An.Alert().Build();
        testContainer.NowIs(fakeNow);
        await sut.AddAlert(alert1);
        await sut.AddAlert(alert2);

        var result = await sut.GetAlerts();

        result.Count.Should().Be(2);
        result[0].CreatedDate.Should().Be(fakeNow);
        result[1].CreatedDate.Should().Be(fakeNow);
    }

    [Test]
    public async Task AddAlert_should_add_alert_with_Enabled_property_set_to_true()
    {
        var alertId = await sut.AddAlert(An.Alert().Build());

        var result = await sut.GetAlert(alertId);

        result?.Enabled.Should().Be(true);
    }
    
    [Test]
    public async Task UpdateAlert_should_update_OfferType()
    {
        var alertId = await sut.AddAlert(An.Alert()
            .WithType(TradeOfferType.Buy)
            .Build()
        );
        
        var alert = await sut.GetAlert(alertId);
        alert!.OfferType = TradeOfferType.Sell;
        await sut.UpdateAlert(alert);

        var updatedAlert = await sut.GetAlert(alertId);
        updatedAlert?.OfferType.Should().Be(TradeOfferType.Sell);
    }

    [Test]
    public async Task UpdateAlert_should_update_ItemName()
    {
        var alertId = await sut.AddAlert(An.Alert()
            .WithItemName("Fennec")
            .Build());

        var alert = await sut.GetAlert(alertId);
        alert!.ItemName = "Merc";
        await sut.UpdateAlert(alert);

        var updatedAlert = await sut.GetAlert(alertId);
        updatedAlert?.ItemName.Should().Be("Merc");
    }

    [Test]
    public async Task UpdateAlert_should_update_Price()
    {
        var oldPrice = new PriceRange(0, 300);
        var newPrice = new PriceRange(200, 350);
        var alertId = await sut.AddAlert(An.Alert()
            .WithPrice(oldPrice)
            .Build()
        );

        var alert = await sut.GetAlert(alertId);
        alert!.Price = newPrice;
        await sut.UpdateAlert(alert);

        var updatedAlert = await sut.GetAlert(alertId);
        updatedAlert?.Price.Should().Be(newPrice);
    }

    [TestCase(AlertItemType.Any)]
    [TestCase(AlertItemType.Body)]
    [TestCase(AlertItemType.Decal)]
    [TestCase(AlertItemType.PaintFinish)]
    [TestCase(AlertItemType.Wheels)]
    [TestCase(AlertItemType.RocketBoost)]
    [TestCase(AlertItemType.Antenna)]
    [TestCase(AlertItemType.GoalExplosion)]
    [TestCase(AlertItemType.Trail)]
    [TestCase(AlertItemType.Banner)]
    [TestCase(AlertItemType.AvatarBorder)]
    public async Task UpdateAlert_should_update_AlertItemType(AlertItemType itemType)
    {
        var alertId = await sut.AddAlert(An.Alert().Build());

        var alert = await sut.GetAlert(alertId);
        alert!.ItemType = itemType;
        await sut.UpdateAlert(alert);

        var updatedAlert = await sut.GetAlert(alertId);
        updatedAlert?.ItemType.Should().Be(itemType);
    }

    [Test]
    public async Task UpdateAlert_should_update_Color()
    {
        var alertId = await sut.AddAlert(An.Alert()
            .WithColor("*")
            .Build()
        );

        var alert = await sut.GetAlert(alertId);
        alert!.Color = "Orange";
        await sut.UpdateAlert(alert);

        var updatedAlert = await sut.GetAlert(alertId);
        updatedAlert?.Color.Should().Be("Orange");
    }

    [Test]
    public async Task UpdateAlert_should_update_Certification()
    {
        var alertId = await sut.AddAlert(An.Alert()
            .WithCertification("*")
            .Build()
        );

        var alert = await sut.GetAlert(alertId);
        alert!.Certification = "Sniper";
        await sut.UpdateAlert(alert);

        var updatedAlert = await sut.GetAlert(alertId);
        updatedAlert?.Certification.Should().Be("Sniper");
    }

    [Test]
    public async Task UpdateAlert_should_update_Enabled()
    {
        var alertId = await sut.AddAlert(An.Alert().Build());

        var alert = await sut.GetAlert(alertId);
        alert!.Enabled = false;
        await sut.UpdateAlert(alert);

        var updatedAlert = await sut.GetAlert(alertId);
        updatedAlert?.Enabled.Should().Be(false);
    }

    [Test]
    public async Task DeleteAlert_should_delete_alert()
    {
        var alertId = await sut.AddAlert(An.Alert().Build());

        await sut.DeleteAlert(alertId);

        var alerts = await sut.GetAlerts();
        alerts.Count.Should().Be(0);
    }
}