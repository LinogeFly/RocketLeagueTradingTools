using RocketLeagueTradingTools.Infrastructure.Persistence.PersistedTypes;

namespace RocketLeagueTradingTools.Infrastructure.Persistence;

public static class PersistenceRepositoryExtensions
{
    public static IQueryable<T> WhereMatchingAlert<T>(this IQueryable<T> source, PersistedAlert alert) where T : PersistedTradeOffer
    {
        return source
            .Where(offer => offer.Name.ToLower() == alert.ItemName.ToLower())
            .Where(offer => offer.Price >= alert.PriceFrom && offer.Price <= alert.PriceTo)
            .Where(offer => alert.Color.ToLower() == "*" ||
                            offer.Color.ToLower() == alert.Color.ToLower())
            .Where(offer => alert.Certification.ToLower() == "*" ||
                            offer.Certification.ToLower() == alert.Certification.ToLower());
    }

    public static IQueryable<T> WhereNotOlderThan<T>(this IQueryable<T> source, int maxAgeInMinutes, DateTime dateTime) where T : PersistedTradeOffer
    {
        return source.Where(o => o.ScrapedDate.AddMinutes(maxAgeInMinutes) >= dateTime);
    }
}