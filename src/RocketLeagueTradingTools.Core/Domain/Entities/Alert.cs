using RocketLeagueTradingTools.Core.Domain.ValueObjects;

namespace RocketLeagueTradingTools.Core.Domain.Entities;

public sealed class Alert
{
    public int Id { get; set; }
    public DateTime CreatedDate { get; set; }
    public string ItemName { get; set; } = "";
    public AlertOfferType OfferType { get; set; }
    public PriceRange Price { get; set; } = new(100000);
    public AlertItemType ItemType { get; set; }
    public string Color { get; set; } = "";
    public string Certification { get; set; } = "";
    public bool Enabled { get; set; } = true;

}

public enum AlertOfferType
{
    Buy,
    Sell
}

public enum AlertItemType
{
    Any,
    Body,
    Decal,
    PaintFinish,
    Wheels,
    RocketBoost,
    Topper,
    Antenna,
    GoalExplosion,
    Trail,
    Banner,
    AvatarBorder
}