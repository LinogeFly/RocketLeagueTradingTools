namespace RocketLeagueTradingTools.Infrastructure.Persistence.Models;

public abstract class PersistedTradeOffer
{
    public int Id { get; set; }
    public string Link { get; set; } = "";
    public DateTime ScrapedDate { get; set; }
    public string ItemName { get; set; } = "";
    public string ItemType { get; set; } = "";
    public int Price { get; set; }
    public string Color { get; set; } = "";
    public string Certification { get; set; } = "";
    public string TradingSite { get; set; } = "";
    public string TraderName { get; set; } = "";
}

public sealed class PersistedBuyOffer : PersistedTradeOffer
{
}

public sealed class PersistedSellOffer : PersistedTradeOffer
{
}