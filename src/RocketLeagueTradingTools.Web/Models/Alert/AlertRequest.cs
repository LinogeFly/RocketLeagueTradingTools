using RocketLeagueTradingTools.Web.Models.Common;

namespace RocketLeagueTradingTools.Web.Models.Alert;

public sealed class AlertRequest
{
    public string ItemName { get; set; } = "";
    public OfferTypeDto OfferType { get; set; }
    public PriceRangeDto Price { get; set; } = new();
    public AlertItemTypeDto ItemType { get; set; }
    public string Color { get; set; } = "";
    public string Certification { get; set; } = "";
    public bool Enabled { get; set; } = true;
}

public sealed class AlertPatchRequest
{
    public bool? Enabled { get; set; }
}