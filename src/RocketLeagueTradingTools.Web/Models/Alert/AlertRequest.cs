using System.ComponentModel.DataAnnotations;
using RocketLeagueTradingTools.Web.Models.Common;

namespace RocketLeagueTradingTools.Web.Models.Alert;

public sealed class AlertRequest
{
    [Required]
    public string? ItemName { get; set; }

    [Required]
    public OfferTypeDto? OfferType { get; set; }

    [Required]
    public PriceRangeDto? Price { get; set; }

    public AlertItemTypeDto ItemType { get; set; }
    public string Color { get; set; } = "";
    public string Certification { get; set; } = "";
    public bool Enabled { get; set; } = true;
}

public sealed class AlertPatchRequest
{
    public bool? Enabled { get; set; }
}