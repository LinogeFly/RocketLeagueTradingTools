namespace RocketLeagueTradingTools.Web.Contracts.Alert;

public record AlertPatchRequest
{
    public bool? Enabled { get; init; }
};