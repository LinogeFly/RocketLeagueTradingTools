using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using RocketLeagueTradingTools.Core.Application.Interfaces;
using RocketLeagueTradingTools.Infrastructure.Common;
using RocketLeagueTradingTools.Infrastructure.Persistence;
using RocketLeagueTradingTools.Infrastructure.Persistence.Repositories;
using RocketLeagueTradingTools.Infrastructure.TradeOfferSource;

namespace RocketLeagueTradingTools.Infrastructure.IntegrationTests.Support;

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
    
    public void NowIs(DateTime dateTime)
    {
        MockOf<IDateTime>().SetupGet(d => d.Now).Returns(dateTime);
    }

    public void CreateDatabase()
    {
        var dbContextFactory = GetService<IDbContextFactory<PersistenceDbContext>>();
        using var dbContext = dbContextFactory.CreateDbContext();

        dbContext.Database.EnsureDeleted();
        dbContext.Database.EnsureCreated();
    }

    public void ResetDatabase()
    {
        var dbContextFactory = GetService<IDbContextFactory<PersistenceDbContext>>();
        using var dbContext = dbContextFactory.CreateDbContext();

        dbContext.Database.ExecuteSqlRaw("DELETE FROM Alerts");
        dbContext.Database.ExecuteSqlRaw("DELETE FROM Blacklist");
        dbContext.Database.ExecuteSqlRaw("DELETE FROM Notifications");
        dbContext.Database.ExecuteSqlRaw("DELETE FROM TradeOffers");
    }

    private void AddServicesToBeTested(IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<RlgDataSource>();
        serviceCollection.AddScoped<NotificationPersistenceRepository>();
        serviceCollection.AddScoped<TradeOfferPersistenceRepository>();
        serviceCollection.AddScoped<AlertPersistenceRepository>();
        serviceCollection.AddScoped<BlacklistPersistenceRepository>();
        serviceCollection.AddDbContextFactory<PersistenceDbContext>(opt => opt.UseSqlite("Data Source=RocketLeagueTradingTools-test.sqlite.db;"));
    }

    private void AddServicesToBeMocked(IServiceCollection serviceCollection)
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