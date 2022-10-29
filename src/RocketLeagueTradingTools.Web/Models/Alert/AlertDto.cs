using RocketLeagueTradingTools.Web.Models.Common;

namespace RocketLeagueTradingTools.Web.Models.Alert;

public sealed class AlertDto
{
    public int Id { get; set; }
    public string ItemName { get; set; } = "";
    public AlertOfferTypeDto OfferType { get; set; }
    public PriceRangeDto Price { get; set; } = new ();
    public string Color { get; set; } = "";
    public string Certification { get; set; } = "";
    public bool Disabled { get; set; }
}

public enum AlertOfferTypeDto
{
    Buy,
    Sell
}