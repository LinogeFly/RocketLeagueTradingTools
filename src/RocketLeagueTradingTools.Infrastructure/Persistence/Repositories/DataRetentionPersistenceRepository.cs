using Microsoft.EntityFrameworkCore;
using RocketLeagueTradingTools.Core.Application.Interfaces;
using RocketLeagueTradingTools.Core.Application.Interfaces.Persistence;

namespace RocketLeagueTradingTools.Infrastructure.Persistence.Repositories;

public class DataRetentionPersistenceRepository : IDataRetentionPersistenceRepository
{
    private readonly IDateTime dateTime;
    private readonly IDbContextFactory<PersistenceDbContext> dbContextFactory;

    public DataRetentionPersistenceRepository(
        IDbContextFactory<PersistenceDbContext> dbContextFactory,
        IDateTime dateTime)
    {
        this.dbContextFactory = dbContextFactory;
        this.dateTime = dateTime;
    }
    public async Task DeleteOldTradeOffers(TimeSpan maxAge)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        var offersToDelete =
            from o in dbContext.TradeOffers
            // ReSharper disable once AccessToDisposedClosure
            from n in dbContext.Notifications.Where(n => n.TradeOffer.Id == o.Id).DefaultIfEmpty()
            where
                o.ScrapedDate < dateTime.Now.Add(-maxAge) &&
                n.TradeOffer == null
            select o;

        dbContext.TradeOffers.RemoveRange(offersToDelete);

        await dbContext.SaveChangesAsync();
    }

    public async Task DeleteOldNotifications(TimeSpan maxAge)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        var notificationsToDelete = dbContext.Notifications.Where(o => o.CreatedDate < dateTime.Now.Add(-maxAge));

        dbContext.Notifications.RemoveRange(notificationsToDelete);

        await dbContext.SaveChangesAsync();
    }

    public async Task Vacuum()
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();
        
        await dbContext.Database.ExecuteSqlRawAsync("VACUUM");
    }
}