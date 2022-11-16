using RocketLeagueTradingTools.Common;
using RocketLeagueTradingTools.Core.Domain.Enumerations;
using RocketLeagueTradingTools.Core.Domain.ValueObjects;

namespace RocketLeagueTradingTools.Core.Domain.Entities;

public sealed class Alert
{
    public int Id { get; set; }
    public DateTime CreatedDate { get; set; }
    public string ItemName { get; set; }
    public AlertOfferType OfferType { get; set; }
    public PriceRange Price { get; set; }
    public AlertItemType ItemType { get; set; }
    public string Color { get; set; } = "";
    public string Certification { get; set; } = "";
    public bool Enabled { get; set; } = true;

    public Alert(AlertOfferType offerType, string itemName, PriceRange price)
    {
        if (itemName.IsEmpty())
            throw new ArgumentException($"{nameof(itemName)} is required.");
        
        OfferType = offerType;
        ItemName = itemName;
        Price = price;
    }

    public Alert(AlertOfferType offerType, string itemName, int priceTo)
    {
        OfferType = offerType;
        ItemName = itemName;
        Price = new PriceRange(priceTo);
    }
}