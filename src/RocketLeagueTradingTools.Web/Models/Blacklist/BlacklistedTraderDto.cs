namespace RocketLeagueTradingTools.Web.Models.Blacklist;

public sealed class BlacklistedTraderDto
{
    public int Id { get; set; }
    public string TradingSite { get; set; } = "";
    public string Name { get; set; } = "";
}