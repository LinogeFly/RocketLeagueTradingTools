using RocketLeagueTradingTools.Web.Models.Common;
using System.ComponentModel.DataAnnotations;

namespace RocketLeagueTradingTools.Web.Models.Testing;

public sealed class TradeOfferRequest
{
    [Required]
    public OfferTypeDto? OfferType { get; set; }

    [Required]
    public string Age { get; set; } = null!;

    [Required]
    public string Name { get; set; } = null!;

    [Required]
    [Range(1, 100000)]
    public int Price { get; set; }

    [Range(1, int.MaxValue)]
    public int Amount { get; set; } = 1;

    public OfferItemTypeDto ItemType { get; set; }
    public string Color { get; set; } = "";
    public string Certification { get; set; } = "";
}
