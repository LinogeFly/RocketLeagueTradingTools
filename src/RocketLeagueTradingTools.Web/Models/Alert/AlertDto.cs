using RocketLeagueTradingTools.Web.Models.Common;

namespace RocketLeagueTradingTools.Web.Models.Alert;

public sealed class AlertDto
{
    public int Id { get; set; }
    public string ItemName { get; set; } = "";
    public OfferTypeDto OfferType { get; set; }
    public PriceRangeDto Price { get; set; } = new();
    public AlertItemTypeDto ItemType { get; set; }
    public string Color { get; set; } = "";
    public string Certification { get; set; } = "";
    public bool Enabled { get; set; } = true;
}

public enum AlertItemTypeDto
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