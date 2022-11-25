using FluentAssertions;
using RocketLeagueTradingTools.Core.Domain.Enumerations;
using RocketLeagueTradingTools.Core.Domain.ValueObjects;

namespace RocketLeagueTradingTools.Core.UnitTests.Domain;

[TestFixture]
public class TradeItemTests
{
    [Test]
    public void Equals_should_return_true_when_two_objects_have_same_property_values()
    {
        var item1 = new TradeItem("Fennec")
        {
            ItemType = TradeItemType.Body,
            Color = "Orange",
            Certification = "Sniper"
        };
        var item2 = new TradeItem("Fennec")
        {
            ItemType = TradeItemType.Body,
            Color = "Orange",
            Certification = "Sniper"
        };

        item1.Should().BeEquivalentTo(item2);
        (item1 == item2).Should().Be(true);
    }

    [Test]
    public void Equals_should_return_false_when_two_objects_have_different_Name_property_value()
    {
        var item1 = new TradeItem("Fennec");
        var item2 = new TradeItem("Supernova III");

        item1.Should().NotBeEquivalentTo(item2);
        (item1 != item2).Should().Be(true);
    }

    [Test]
    public void Equals_should_return_false_when_two_objects_have_different_ItemType_property_value()
    {
        var item1 = new TradeItem("Nomster")
        {
            ItemType = TradeItemType.GoalExplosion
        };
        var item2 = new TradeItem("Nomster")
        {
            ItemType = TradeItemType.Wheels
        };

        item1.Should().NotBeEquivalentTo(item2);
        (item1 != item2).Should().Be(true);
    }

    [Test]
    public void Equals_should_return_false_when_two_objects_have_different_Color_property_value()
    {
        var item1 = new TradeItem("Fennec")
        {
            Color = "Orange"
        };
        var item2 = new TradeItem("Fennec")
        {
            Color = "Crimson"
        };

        item1.Should().NotBeEquivalentTo(item2);
        (item1 != item2).Should().Be(true);
    }

    [Test]
    public void Equals_should_return_false_when_two_objects_have_different_Certification_property_value()
    {
        var item1 = new TradeItem("Fennec")
        {
            Certification = "Sniper"
        };
        var item2 = new TradeItem("Fennec")
        {
            Certification = "Show-Off"
        };

        item1.Should().NotBeEquivalentTo(item2);
        (item1 != item2).Should().Be(true);
    }
}