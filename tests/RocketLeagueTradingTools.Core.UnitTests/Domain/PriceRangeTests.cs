using FluentAssertions;
using RocketLeagueTradingTools.Core.Domain.ValueObjects;

namespace RocketLeagueTradingTools.Core.UnitTests.Domain;

[TestFixture]
public class PriceRangeTests
{
    [Test]
    public void Equals_should_return_true_when_two_objects_have_same_property_values()
    {
        var price1 = new PriceRange(0, 300);
        var price2 = new PriceRange(0, 300);

        price1.Should().BeEquivalentTo(price2);
        (price1 == price2).Should().Be(true);
    }

    [Test]
    public void Equals_should_return_false_when_two_objects_have_different_From_property_value()
    {
        var price1 = new PriceRange(0, 300);
        var price2 = new PriceRange(10, 300);

        price1.Should().NotBeEquivalentTo(price2);
        (price1 == price2).Should().Be(false);
    }

    [Test]
    public void Equals_should_return_false_when_two_objects_have_different_To_property_value()
    {
        var price1 = new PriceRange(0, 300);
        var price2 = new PriceRange(0, 290);

        price1.Should().NotBeEquivalentTo(price2);
        (price1 == price2).Should().Be(false);
    }
}