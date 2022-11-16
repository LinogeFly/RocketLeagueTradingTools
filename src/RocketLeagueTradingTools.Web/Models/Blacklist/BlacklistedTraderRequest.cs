using System.ComponentModel.DataAnnotations;

namespace RocketLeagueTradingTools.Web.Models.Blacklist;

public sealed class BlacklistedTraderRequest
{
    [Required]
    public string TradingSite { get; set; } = null!;
    
    [Required]
    public string Name { get; set; } = null!;
}
