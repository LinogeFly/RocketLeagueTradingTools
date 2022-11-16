namespace RocketLeagueTradingTools.Infrastructure.Persistence.Models;

public sealed class PersistedAlert
{
    public int Id { get; set; }
    public DateTime CreatedDate { get; set; }
    public string OfferType { get; set; } = "";
    public string ItemName { get; set; } = "";
    public string ItemType { get; set; } = "";
    public int PriceFrom { get; set; }
    public int PriceTo { get; set; }
    public string Color { get; set; } = "";
    public string Certification { get; set; } = "";
    public string Enabled { get; set; } = "Yes";
}
