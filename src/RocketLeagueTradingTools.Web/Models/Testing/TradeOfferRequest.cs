namespace RocketLeagueTradingTools.Web.Models.Testing;

public sealed class TradeOfferRequest
{
    public OfferTypeRequest OfferType { get; set; }
    public int Amount { get; set; }
    public string Age { get; set; } = "";
    public string Name { get; set; } = "";
    public string Color { get; set; } = "";
    public string Certification { get; set; } = "";
    public int Price { get; set; }
}

public enum OfferTypeRequest
{
    None,
    Buy,
    Sell
}
