namespace RocketLeagueTradingTools.Infrastructure.Persistence.Models;

public sealed class PersistedBlacklistedTrader
{
    public int Id { get; set; }
    public string TradingSite { get; set; } = "";
    public string TraderName { get; set; } = "";
}