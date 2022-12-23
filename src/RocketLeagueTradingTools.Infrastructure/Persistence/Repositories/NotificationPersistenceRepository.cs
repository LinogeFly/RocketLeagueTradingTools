using Microsoft.EntityFrameworkCore;
using RocketLeagueTradingTools.Core.Application.Interfaces;
using RocketLeagueTradingTools.Core.Application.Interfaces.Persistence;
using RocketLeagueTradingTools.Core.Domain.Entities;
using RocketLeagueTradingTools.Infrastructure.Persistence.Mapping;
using RocketLeagueTradingTools.Infrastructure.Persistence.Models;

namespace RocketLeagueTradingTools.Infrastructure.Persistence.Repositories;

public class NotificationPersistenceRepository : INotificationPersistenceRepository
{
    private readonly IDateTime dateTime;
    private readonly IDbContextFactory<PersistenceDbContext> dbContextFactory;

    public NotificationPersistenceRepository(
        IDbContextFactory<PersistenceDbContext> dbContextFactory,
        IDateTime dateTime)
    {
        this.dbContextFactory = dbContextFactory;
        this.dateTime = dateTime;
    }

    public async Task<IList<Notification>> GetNotifications(int pageSize)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        var items = await dbContext.Notifications
            .OrderByDescending(n => n.CreatedDate)
            .Include(n => n.TradeOffer)
            .Take(pageSize)
            .ToListAsync();

        return items.Select(PersistenceMapper.Map).ToList();
    }

    public async Task<IList<Notification>> GetNotifications(TimeSpan notOlderThan)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        var items = await dbContext.Notifications
            .Where(n => n.CreatedDate >= dateTime.Now - notOlderThan)
            .Include(n => n.TradeOffer)
            .ToListAsync();

        return items.Select(PersistenceMapper.Map).ToList();
    }

    public async Task<int> GetNotificationsCount()
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        return await dbContext.Notifications.CountAsync();
    }

    public async Task AddNotifications(IList<Notification> notifications)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        dbContext.Notifications.AddRange(notifications.Select(n =>
            new PersistedNotification
            {
                TradeOffer = dbContext.TradeOffers.Single(o => o.Id == n.ScrapedTradeOffer.Id),
                CreatedDate = dateTime.Now
            }
        ));

        await dbContext.SaveChangesAsync();
    }

    public async Task MarkNotificationAsSeen(int id)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        var persistedItem = await dbContext.Notifications.SingleOrDefaultAsync(x => x.Id == id);

        if (persistedItem == null)
            throw new InvalidOperationException("Notification not found.");

        if (persistedItem.SeenDate != null)
            return;

        persistedItem.SeenDate = dateTime.Now;

        await dbContext.SaveChangesAsync();
    }

    public async Task MarkAllNotificationAsSeen()
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        await dbContext.Notifications
            .Where(n => n.SeenDate == null)
            .ForEachAsync(n => n.SeenDate = dateTime.Now);

        await dbContext.SaveChangesAsync();
    }
}