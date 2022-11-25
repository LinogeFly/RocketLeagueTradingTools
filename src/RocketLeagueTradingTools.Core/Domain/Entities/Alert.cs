using RocketLeagueTradingTools.Common;
using RocketLeagueTradingTools.Core.Domain.Enumerations;
using RocketLeagueTradingTools.Core.Domain.ValueObjects;

namespace RocketLeagueTradingTools.Core.Domain.Entities;

public sealed class Alert
{
    public int Id { get; set; }
    public TradeOfferType OfferType { get; set; }
    public string ItemName { get; set; }
    public PriceRange Price { get; set; }
    public AlertItemType ItemType { get; set; }
    public string Color { get; set; } = "";
    public string Certification { get; set; } = "";
    public DateTime CreatedDate { get; set; }
    public bool Enabled { get; set; } = true;

    public Alert(TradeOfferType offerType, string itemName, PriceRange price)
    {
        if (itemName.IsEmpty())
            throw new ArgumentException("The field is required.", nameof(itemName));
        
        OfferType = offerType;
        ItemName = itemName;
        Price = price;
    }

    public Alert(TradeOfferType offerType, string itemName, int priceTo)
    {
        OfferType = offerType;
        ItemName = itemName;
        Price = new PriceRange(priceTo);
    }
}