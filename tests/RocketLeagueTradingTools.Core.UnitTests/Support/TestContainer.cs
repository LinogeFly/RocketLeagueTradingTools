using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using RocketLeagueTradingTools.Core.Application.Alert;
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
        AddMockServices(serviceCollection);
        AddDependencyServices(serviceCollection);

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
        serviceCollection.AddTransient<NotificationApplication>();
        serviceCollection.AddTransient<DataRetentionApplication>();
        serviceCollection.AddTransient<ScrapApplication>();
        serviceCollection.AddTransient<AlertApplication>();
    }

    private static void AddMockServices(IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton(Mock.Of<ILog>());
        serviceCollection.AddSingleton(Mock.Of<IDateTime>());
        serviceCollection.AddSingleton(Mock.Of<INotificationPersistenceRepository>());
        serviceCollection.AddSingleton(Mock.Of<INotificationApplicationSettings>());
        serviceCollection.AddSingleton(Mock.Of<ITradeOfferPersistenceRepository>());
        serviceCollection.AddSingleton(Mock.Of<IDataRetentionPersistenceRepository>());
        serviceCollection.AddSingleton(Mock.Of<IDataRetentionApplicationSettings>());
        serviceCollection.AddSingleton(Mock.Of<ITradeOfferRepository>());
        serviceCollection.AddSingleton(Mock.Of<IAlertPersistenceRepository>());
        serviceCollection.AddSingleton(Mock.Of<IScrapApplicationSettings>());
        serviceCollection.AddSingleton(Mock.Of<INotificationSessionStorage>());
    }
    
    private static void AddDependencyServices(IServiceCollection serviceCollection)
    {
        serviceCollection.AddMediatR(typeof(AlertUpdateEvent));
    }
}