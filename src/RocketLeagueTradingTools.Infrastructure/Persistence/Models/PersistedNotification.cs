namespace RocketLeagueTradingTools.Infrastructure.Persistence.Models;

public sealed class PersistedNotification
{
    public int Id { get; set; }
    public PersistedTradeOffer TradeOffer { get; set; } = null!;
    public DateTime CreatedDate { get; set; }
    public DateTime? SeenDate { get; set; }
}