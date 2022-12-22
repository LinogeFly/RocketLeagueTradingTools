using System.ComponentModel.DataAnnotations;
using RocketLeagueTradingTools.Web.Contracts.Common;

namespace RocketLeagueTradingTools.Web.Contracts.Testing;

public sealed class TradeOfferRequest
{
    [Required]
    public TradeOfferTypeDto? OfferType { get; init; }

    [Required]
    public string Age { get; init; } = default!;

    [Required]
    public string Name { get; init; } = default!;

    [Required]
    [Range(1, 100000)]
    public int Price { get; init; }

    [Range(1, int.MaxValue)]
    public int Amount { get; init; } = 1;

    public TradeItemTypeDto ItemType { get; init; }
    public string Color { get; init; } = "";
    public string Certification { get; init; } = "";
}