using FluentAssertions;
using RocketLeagueTradingTools.Core.Domain.Enumerations;
using RocketLeagueTradingTools.Infrastructure.IntegrationTests.Support;
using RocketLeagueTradingTools.Infrastructure.Persistence.Repositories;
using RocketLeagueTradingTools.Test;

namespace RocketLeagueTradingTools.Infrastructure.IntegrationTests.Persistence;

[TestFixture]
public class BlacklistPersistenceRepositoryTests
{
    private BlacklistPersistenceRepository sut = null!;
    private TestContainer testContainer = null!;

    [SetUp]
    public void Setup()
    {
        testContainer = TestContainer.Create();
        testContainer.ResetDatabase();

        sut = testContainer.GetService<BlacklistPersistenceRepository>();
    }
    
    [Test]
    public async Task GetBlacklistedTraders_should_return_blacklisted_traders_with_Id_mapped()
    {
        await sut.AddBlacklistedTrader(A.Trader().Build());

        var result = await sut.GetBlacklistedTraders();

        result.Count.Should().Be(1);
        result.Single().Id.Should().NotBe(default);
    }

    [TestCase(TradingSite.RocketLeagueGarage)]
    public async Task GetBlacklistedTraders_should_return_blacklisted_traders_with_Trader_TradingSite_mapped(TradingSite tradingSite)
    {
        await sut.AddBlacklistedTrader(A.Trader()
            .WithTradingSite(tradingSite)
            .Build()
        );

        var result = await sut.GetBlacklistedTraders();

        result.Count.Should().Be(1);
        result.Single().Trader.TradingSite.Should().Be(tradingSite);
    }

    [Test]
    public async Task GetBlacklistedTraders_should_return_blacklisted_traders_with_Trader_Name_mapped()
    {
        var expectedName = Guid.NewGuid().ToString();
        await sut.AddBlacklistedTrader(A.Trader()
            .WithName(expectedName)
            .Build()
        );

        var result = await sut.GetBlacklistedTraders();

        result.Count.Should().Be(1);
        result.Single().Trader.Name.Should().Be(expectedName);
    }

    [Test]
    public async Task DeleteBlacklistedTrader_should_delete_blacklisted_trader()
    {
        await sut.AddBlacklistedTrader(A.Trader().Build());
        var trader = (await sut.GetBlacklistedTraders()).Single();

        await sut.DeleteBlacklistedTrader(trader.Id);

        var traders = await sut.GetBlacklistedTraders();
        traders.Count.Should().Be(0);
    }
}