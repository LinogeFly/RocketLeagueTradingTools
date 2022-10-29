namespace RocketLeagueTradingTools.Infrastructure.Persistence.PersistedTypes;

public abstract class PersistedTradeOffer
{
    public int Id { get; set; }
    public string Link { get; set; } = "";
    public string SourceId { get; set; } = "";
    public DateTime ScrapedDate { get; set; }
    public string Name { get; set; } = "";
    public int Price { get; set; }
    public string Color { get; set; } = "";
    public string Certification { get; set; } = "";

    public override bool Equals(object? obj)
    {
        return Equals(obj as PersistedTradeOffer);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(SourceId, Name, Price, Color, Certification);
    }

    private bool Equals(PersistedTradeOffer? other)
    {
        return other != null &&
               SourceId == other.SourceId &&
               Name == other.Name &&
               Price == other.Price &&
               Color == other.Color &&
               Certification == other.Certification;
    }
}

public sealed class PersistedBuyOffer : PersistedTradeOffer
{
}

public sealed class PersistedSellOffer : PersistedTradeOffer
{
}