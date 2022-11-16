namespace RocketLeagueTradingTools.Infrastructure.Persistence.Models;

public sealed class PersistedNotification
{
    public int Id { get; set; }
    public string TradeItemName { get; set; } = "";
    public string TradeItemType { get; set; } = "";
    public string TradeItemColor { get; set; } = "";
    public string TradeItemCertification { get; set; } = "";
    public int TradeOfferPrice { get; set; }
    public string TradeOfferLink { get; set; } = "";
    public DateTime TradeOfferScrapedDate { get; set; }
    public string TradingSite { get; set; } = "";
    public string TraderName { get; set; } = "";
    public DateTime CreatedDate { get; set; }
    public DateTime? SeenDate { get; set; }
}
