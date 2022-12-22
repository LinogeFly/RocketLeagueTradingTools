using Microsoft.EntityFrameworkCore;
using RocketLeagueTradingTools.Core.Application.Interfaces.Persistence;
using RocketLeagueTradingTools.Core.Domain.Entities;
using RocketLeagueTradingTools.Core.Domain.ValueObjects;
using RocketLeagueTradingTools.Infrastructure.Persistence.Mapping;
using RocketLeagueTradingTools.Infrastructure.Persistence.Models;

namespace RocketLeagueTradingTools.Infrastructure.Persistence.Repositories;

public class BlacklistPersistenceRepository : IBlacklistPersistenceRepository
{
    private readonly IDbContextFactory<PersistenceDbContext> dbContextFactory;

    public BlacklistPersistenceRepository(IDbContextFactory<PersistenceDbContext> dbContextFactory)
    {
        this.dbContextFactory = dbContextFactory;
    }

    public async Task<IList<BlacklistedTrader>> GetBlacklistedTraders()
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        var items = await dbContext.BlacklistedTraders.ToListAsync();

        return items.Select(PersistenceMapper.MapToBlacklistedTrader).ToList();
    }

    public async Task AddBlacklistedTrader(Trader trader)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        var persistedTrader = new PersistedBlacklistedTrader
        {
            TraderName = trader.Name,
            TradingSite = PersistenceMapper.MapTradingSite(trader.TradingSite)
        };

        dbContext.BlacklistedTraders.Add(persistedTrader);
        await dbContext.SaveChangesAsync();
    }

    public async Task DeleteBlacklistedTrader(int id)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        var itemToDelete = await dbContext.BlacklistedTraders.SingleOrDefaultAsync(x => x.Id == id);

        if (itemToDelete == null)
            throw new InvalidOperationException("Blacklisted trader not found.");

        dbContext.BlacklistedTraders.Remove(itemToDelete);

        await dbContext.SaveChangesAsync();
    }
}