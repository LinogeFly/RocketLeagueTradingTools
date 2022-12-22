using System.ComponentModel.DataAnnotations;
using RocketLeagueTradingTools.Web.Contracts.Common;

namespace RocketLeagueTradingTools.Web.Contracts.Alert;

public record AlertUpsertRequest
{
    [Required]
    public string ItemName { get; init; } = default!;

    [Required]
    public TradeOfferTypeDto? OfferType { get; init; } = null!;

    [Required]
    public PriceRangeDto Price { get; init; } = default!;

    public AlertItemTypeDto ItemType { get; init; }
    public string Color { get; init; } = "";
    public string Certification { get; init; } = "";
    public bool Enabled { get; init; } = true;
}