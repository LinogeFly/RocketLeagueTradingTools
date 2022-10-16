using RocketLeagueTradingTools.Core.Domain.ValueObjects;

namespace RocketLeagueTradingTools.Core.Domain.Entities;

public sealed class Alert
{
    public int Id { get; set; }
    public string ItemName { get; set; } = "";
    public AlertOfferType OfferType { get; set; }
    public PriceRange Price { get; set; } = new PriceRange(100000);
    public string Color { get; set; } = "";
    public string Certification { get; set; } = "";
    public bool Disabled { get; set; }

}

public enum AlertOfferType
{
    Buy,
    Sell
}
