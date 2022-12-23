using Microsoft.EntityFrameworkCore;
using RocketLeagueTradingTools.Core.Application.Interfaces;
using RocketLeagueTradingTools.Core.Application.Interfaces.Persistence;
using RocketLeagueTradingTools.Core.Domain.Entities;
using RocketLeagueTradingTools.Infrastructure.Persistence.Mapping;
using RocketLeagueTradingTools.Infrastructure.Persistence.Models;

namespace RocketLeagueTradingTools.Infrastructure.Persistence.Repositories;

public class TradeOfferPersistenceRepository : ITradeOfferPersistenceRepository
{
    private readonly IDateTime dateTime;
    private readonly IDbContextFactory<PersistenceDbContext> dbContextFactory;

    public TradeOfferPersistenceRepository(
        IDbContextFactory<PersistenceDbContext> dbContextFactory,
        IDateTime dateTime)
    {
        this.dbContextFactory = dbContextFactory;
        this.dateTime = dateTime;
    }

    public async Task AddTradeOffers(IList<ScrapedTradeOffer> offers)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        await dbContext.TradeOffers.AddRangeAsync(offers.Select(PersistenceMapper.Map));

        await dbContext.SaveChangesAsync();
    }

    public async Task<IList<ScrapedTradeOffer>> FindAlertMatchingTradeOffers(TimeSpan alertOfferMaxAge)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        var offers = await GetAlertMatchingOffersQuery(dbContext, alertOfferMaxAge).ToListAsync();

        return offers.Select(PersistenceMapper.Map).ToList();
    }

    private IQueryable<PersistedTradeOffer> GetAlertMatchingOffersQuery(PersistenceDbContext dbContext, TimeSpan alertOfferMaxAge)
    {
        return
            from o in dbContext.TradeOffers
            join a in dbContext.Alerts on o.ItemName.ToLower() equals a.ItemName.ToLower()
            where
                a.Enabled == PersistenceMapper.BoolToString(true) &&
                a.OfferType == o.OfferType &&
                o.ScrapedDate >= dateTime.Now.Add(-alertOfferMaxAge) &&
                o.Price >= a.PriceFrom &&
                o.Price <= a.PriceTo &&
                (a.ItemType == "*" || o.ItemType.ToLower() == a.ItemType.ToLower()) &&
                (a.Color == "*" || o.Color.ToLower() == a.Color.ToLower() || (a.Color == "+" && o.Color != "")) &&
                (a.Certification == "*" || o.Certification.ToLower() == a.Certification.ToLower() || (a.Certification == "+" && o.Certification != "")) &&
                !(
                    from b in dbContext.BlacklistedTraders
                    where b.TradingSite == o.TradingSite
                    select b.TraderName.ToLower()
                ).Contains(o.TraderName.ToLower())
            select o;
    }
}