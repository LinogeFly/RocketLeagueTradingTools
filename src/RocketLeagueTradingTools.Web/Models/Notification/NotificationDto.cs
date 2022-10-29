namespace RocketLeagueTradingTools.Web.Models.Notification;

public sealed class NotificationDto
{
    public int Id { get; set; }
    public string ItemName { get; set; } = "";
    public int ItemPrice { get; set; }

    // Represents how how old a trade is. For example, 6 minutes.
    public string TradeOfferAge { get; set; } = "";
    public string TradeOfferLink { get; set; } = "";
    public bool IsNew { get; set; }
}
