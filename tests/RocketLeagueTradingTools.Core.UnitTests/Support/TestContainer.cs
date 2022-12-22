using Microsoft.Extensions.DependencyInjection;
using Moq;
using RocketLeagueTradingTools.Core.Application.DataRetention;
using RocketLeagueTradingTools.Core.Application.Interfaces;
using RocketLeagueTradingTools.Core.Application.Interfaces.Persistence;
using RocketLeagueTradingTools.Core.Application.Notification;
using RocketLeagueTradingTools.Core.Application.Scrap;

namespace RocketLeagueTradingTools.Core.UnitTests.Support;

internal class TestContainer
{
    private readonly ServiceProvider serviceProvider;
    
    private TestContainer()
    {
        var serviceCollection = new ServiceCollection();

        AddServicesToBeTested(serviceCollection);
        AddServicesToBeMocked(serviceCollection);

        serviceProvider = serviceCollection.BuildServiceProvider();
    }

    public static TestContainer Create() => new();

    public T GetService<T>() where T : notnull => serviceProvider.GetRequiredService<T>();

    public Mock<T> MockOf<T>() where T : class
    {
        return Mock.Get(GetService<T>());
    }
    
    public void NowIs(DateTime dateTime)
    {
        MockOf<IDateTime>().SetupGet(d => d.Now).Returns(dateTime);
    }

    private static void AddServicesToBeTested(IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<NotificationApplication>();
        serviceCollection.AddSingleton<DataRetentionApplication>();
        serviceCollection.AddTransient<ScrapApplication>();
    }

    private static void AddServicesToBeMocked(IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton(Mock.Of<ILog>());
        serviceCollection.AddSingleton(Mock.Of<INotificationPersistenceRepository>());
        serviceCollection.AddSingleton(Mock.Of<INotificationApplicationSettings>());
        serviceCollection.AddSingleton(Mock.Of<ITradeOfferPersistenceRepository>());
        serviceCollection.AddSingleton(Mock.Of<IDataRetentionApplicationSettings>());
        serviceCollection.AddSingleton(Mock.Of<ITradeOfferRepository>());
        serviceCollection.AddSingleton(Mock.Of<IScrapApplicationSettings>());
    }
}