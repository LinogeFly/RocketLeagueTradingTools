namespace RocketLeagueTradingTools.Infrastructure.Persistence.PersistedTypes;

public sealed class PersistedAlert
{
    public int Id { get; set; }
    public DateTime CreatedDate { get; set; }
    public string ItemName { get; set; } = "";
    public PersistedAlertOfferType OfferType { get; set; }
    public int PriceFrom { get; set; }
    public int PriceTo { get; set; }
    public string Color { get; set; } = "";
    public string Certification { get; set; } = "";
    public bool Disabled { get; set; }
}

public enum PersistedAlertOfferType
{
    Buy,
    Sell
}