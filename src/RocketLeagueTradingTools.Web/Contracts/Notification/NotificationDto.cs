namespace RocketLeagueTradingTools.Web.Contracts.Notification;

public record NotificationDto
{
    public int Id { get; init; }
    public string ItemName { get; init; } = default!;
    public int ItemPrice { get; init; }
    public string TradeOfferAge { get; init; } = default!;
    public string TradeOfferLink { get; init; } = default!;
    public bool IsNew { get; init; }
}