using Microsoft.EntityFrameworkCore;
using RocketLeagueTradingTools.Core.Application.Interfaces;
using RocketLeagueTradingTools.Core.Application.Interfaces.Persistence;
using RocketLeagueTradingTools.Core.Domain.Entities;
using RocketLeagueTradingTools.Infrastructure.Persistence.Mapping;
using RocketLeagueTradingTools.Infrastructure.Persistence.Models;

namespace RocketLeagueTradingTools.Infrastructure.Persistence.Repositories;

public class AlertPersistenceRepository : IAlertPersistenceRepository
{
    private readonly IDateTime dateTime;
    private readonly IDbContextFactory<PersistenceDbContext> dbContextFactory;

    public AlertPersistenceRepository(
        IDbContextFactory<PersistenceDbContext> dbContextFactory,
        IDateTime dateTime)
    {
        this.dbContextFactory = dbContextFactory;
        this.dateTime = dateTime;
    }

    public async Task<IList<Alert>> GetAlerts()
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        var items = await dbContext.Alerts.ToListAsync();

        return items.Select(PersistenceMapper.Map).ToList();
    }

    public async Task<Alert?> GetAlert(int id)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        var persistedAlert = await dbContext.Alerts.SingleOrDefaultAsync(x => x.Id == id);

        if (persistedAlert == null)
            return null;

        return PersistenceMapper.Map(persistedAlert);
    }

    public async Task<int> AddAlert(Alert alert)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        var persistedAlert = new PersistedAlert
        {
            CreatedDate = dateTime.Now,
            ItemName = alert.ItemName,
            OfferType = PersistenceMapper.MapTradeOfferType(alert.OfferType),
            PriceFrom = alert.Price.From,
            PriceTo = alert.Price.To,
            ItemType = PersistenceMapper.MapAlertItemType(alert.ItemType),
            Color = alert.Color,
            Certification = alert.Certification
        };

        dbContext.Alerts.Add(persistedAlert);
        await dbContext.SaveChangesAsync();

        return persistedAlert.Id;
    }

    public async Task UpdateAlert(Alert alert)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        var persistedAlert = await dbContext.Alerts.SingleOrDefaultAsync(x => x.Id == alert.Id);

        if (persistedAlert == null)
            throw new InvalidOperationException("Alert not found.");

        persistedAlert.ItemName = alert.ItemName;
        persistedAlert.OfferType = PersistenceMapper.MapTradeOfferType(alert.OfferType);
        persistedAlert.ItemType = PersistenceMapper.MapAlertItemType(alert.ItemType);
        persistedAlert.PriceFrom = alert.Price.From;
        persistedAlert.PriceTo = alert.Price.To;
        persistedAlert.Color = alert.Color;
        persistedAlert.Certification = alert.Certification;
        persistedAlert.Enabled = PersistenceMapper.BoolToString(alert.Enabled);

        await dbContext.SaveChangesAsync();

        PersistenceMapper.Map(persistedAlert);
    }

    public async Task DeleteAlert(int id)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        var persistedAlert = await dbContext.Alerts.SingleOrDefaultAsync(x => x.Id == id);

        if (persistedAlert == null)
            throw new InvalidOperationException("Alert not found.");

        dbContext.Alerts.Remove(persistedAlert);

        await dbContext.SaveChangesAsync();
    }
}