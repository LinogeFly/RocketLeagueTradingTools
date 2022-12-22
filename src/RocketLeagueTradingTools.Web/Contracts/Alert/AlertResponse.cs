using RocketLeagueTradingTools.Web.Contracts.Common;

namespace RocketLeagueTradingTools.Web.Contracts.Alert;

public record AlertResponse
{
    public int Id { get; init; }
    public string ItemName { get; init; } = default!;
    public TradeOfferTypeDto TradeOfferType { get; init; }
    public PriceRangeDto Price { get; init; } = default!;
    public AlertItemTypeDto ItemType { get; init; }
    public string Color { get; init; } = "";
    public string Certification { get; init; } = "";
    public bool Enabled { get; init; } = true;
}