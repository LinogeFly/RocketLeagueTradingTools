namespace RocketLeagueTradingTools.Infrastructure.Persistence.Models;

public class PersistedTradeOffer
{
    public int Id { get; set; }
    public string Link { get; set; } = "";
    public DateTime ScrapedDate { get; set; }
    public string OfferType { get; set; } = "";
    public string ItemName { get; set; } = "";
    public string ItemType { get; set; } = "";
    public int Price { get; set; }
    public string Color { get; set; } = "";
    public string Certification { get; set; } = "";
    public string TradingSite { get; set; } = "";
    public string TraderName { get; set; } = "";
}
