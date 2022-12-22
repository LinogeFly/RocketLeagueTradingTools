using System.ComponentModel.DataAnnotations;

namespace RocketLeagueTradingTools.Web.Contracts.Blacklist;

public record BlacklistedTraderRequest
{
    [Required]
    public string TradingSite { get; init; } = default!;
    
    [Required]
    public string Name { get; init; } = default!;
}