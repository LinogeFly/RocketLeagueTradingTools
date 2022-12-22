using FluentAssertions;
using RocketLeagueTradingTools.Core.Domain.Enumerations;
using RocketLeagueTradingTools.Test;

namespace RocketLeagueTradingTools.Core.UnitTests.Domain;

[TestFixture]
public class TradeOfferTests
{
    [Test]
    public void Equals_should_return_true_when_two_objects_have_same_property_values()
    {
        var offer1 = A.TradeOffer().Build();
        var offer2 = A.TradeOffer().Build();

        offer1.Should().BeEquivalentTo(offer2);
        (offer1 == offer2).Should().Be(true);
    }

    [Test]
    public void Equals_should_return_false_when_two_objects_have_different_OfferType_property_value()
    {
        var offer1 = A.TradeOffer().WithType(TradeOfferType.Buy).Build();
        var offer2 = A.TradeOffer().WithType(TradeOfferType.Sell).Build();

        offer1.Should().NotBeEquivalentTo(offer2);
        (offer1 != offer2).Should().Be(true);
    }

    [Test]
    public void Equals_should_return_false_when_two_objects_have_different_Item_property_value()
    {
        var offer1 = A.TradeOffer().WithItem(A.TradeItem().WithName("Fennec")).Build();
        var offer2 = A.TradeOffer().WithItem(A.TradeItem().WithName("Supernova III")).Build();

        offer1.Should().NotBeEquivalentTo(offer2);
        (offer1 != offer2).Should().Be(true);
    }

    [Test]
    public void Equals_should_return_false_when_two_objects_have_different_Price_property_value()
    {
        var offer1 = A.TradeOffer().WithPrice(300).Build();
        var offer2 = A.TradeOffer().WithPrice(290).Build();

        offer1.Should().NotBeEquivalentTo(offer2);
        (offer1 != offer2).Should().Be(true);
    }

    [Test]
    public void Equals_should_return_false_when_two_objects_have_different_Link_property_value()
    {
        var offer1 = A.TradeOffer().WithLink("https://rocket-league.com/trade/1").Build();
        var offer2 = A.TradeOffer().WithLink("https://rocket-league.com/trade/2").Build();

        offer1.Should().NotBeEquivalentTo(offer2);
        (offer1 != offer2).Should().Be(true);
    }

    [Test]
    public void Equals_should_return_false_when_two_objects_have_different_Trader_property_value()
    {
        var offer1 = A.TradeOffer().WithTrader(A.Trader().WithName("Trader1")).Build();
        var offer2 = A.TradeOffer().WithTrader(A.Trader().WithName("Trader2")).Build();

        offer1.Should().NotBeEquivalentTo(offer2);
        (offer1 != offer2).Should().Be(true);
    }
}