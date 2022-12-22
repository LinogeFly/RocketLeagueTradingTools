namespace RocketLeagueTradingTools.Web.Contracts.Blacklist;

public record BlacklistedTraderResponse
{
    public int Id { get; init; } = default!;
    public string TradingSite { get; init; } = default!;
    public string Name { get; init; } = default!;
}