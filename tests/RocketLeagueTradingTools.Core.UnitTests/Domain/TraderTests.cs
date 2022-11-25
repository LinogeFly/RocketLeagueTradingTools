using FluentAssertions;
using RocketLeagueTradingTools.Core.Domain.Enumerations;
using RocketLeagueTradingTools.Core.Domain.ValueObjects;

namespace RocketLeagueTradingTools.Core.UnitTests.Domain;

[TestFixture]
public class TraderTests
{
    [Test]
    public void Equals_should_return_true_when_two_objects_have_same_property_values()
    {
        var trader1 = new Trader(TradingSite.RocketLeagueGarage, "Trader");
        var trader2 = new Trader(TradingSite.RocketLeagueGarage, "Trader");

        trader1.Should().BeEquivalentTo(trader2);
        (trader1 == trader2).Should().Be(true);
    }

    [Test]
    public void Equals_should_return_false_when_two_objects_have_different_Name_property_value()
    {
        var trader1 = new Trader(TradingSite.RocketLeagueGarage, "Trader1");
        var trader2 = new Trader(TradingSite.RocketLeagueGarage, "Trader2");

        trader1.Should().NotBeEquivalentTo(trader2);
        (trader1 == trader2).Should().Be(false);
    }
}