using RocketLeagueTradingTools.Infrastructure.IntegrationTests.Support;

namespace RocketLeagueTradingTools.Infrastructure.IntegrationTests.Persistence;

[SetUpFixture]
public class PersistenceRepositoryFixture
{
    [OneTimeSetUp]
    public void RunBeforeAnyTests()
    {
        TestContainer.Create().CreateDatabase();
    }
}