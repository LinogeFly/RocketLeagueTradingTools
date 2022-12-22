using Microsoft.Extensions.DependencyInjection;
using Moq;
using RocketLeagueTradingTools.Core.Application.Interfaces;
using RocketLeagueTradingTools.Infrastructure.Common;
using RocketLeagueTradingTools.Infrastructure.TradeOfferSource;

namespace RocketLeagueTradingTools.Infrastructure.UnitTests.Support;

internal class TestContainer
{
    private readonly ServiceProvider serviceProvider;
    
    private TestContainer()
    {
        var serviceCollection = new ServiceCollection();

        AddServicesToBeTested(serviceCollection);
        AddServicesToBeMocked(serviceCollection);

        serviceProvider = serviceCollection.BuildServiceProvider();
        
        SetupMockServices();
    }

    public static TestContainer Create() => new();

    public T GetService<T>() where T : notnull => serviceProvider.GetRequiredService<T>();

    public Mock<T> MockOf<T>() where T : class
    {
        return Mock.Get(GetService<T>());
    }

    private static void AddServicesToBeTested(IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<RlgDataSource>();
    }

    private static void AddServicesToBeMocked(IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton(Mock.Of<ILog>());
        serviceCollection.AddSingleton(Mock.Of<IHttp>());
        serviceCollection.AddSingleton(Mock.Of<IDateTime>());
    }

    private void SetupMockServices()
    {
        MockOf<IDateTime>().SetupGet(d => d.Now).Returns(DateTime.UtcNow);
    }
}