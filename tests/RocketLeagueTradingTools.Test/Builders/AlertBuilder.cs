using RocketLeagueTradingTools.Core.Domain.Entities;
using RocketLeagueTradingTools.Core.Domain.Enumerations;
using RocketLeagueTradingTools.Core.Domain.ValueObjects;

namespace RocketLeagueTradingTools.Test.Builders;

public class AlertBuilder
{
    private TradeOfferType offerType;
    private string itemName = default!;
    private string color = "";
    private string certification = "";
    private PriceRange price = null!;
    private AlertItemType itemType;

    public AlertBuilder WithType(TradeOfferType offerType)
    {
        this.offerType = offerType;

        return this;
    }

    public AlertBuilder WithItemName(string itemName)
    {
        this.itemName = itemName;

        return this;
    }
    
    public AlertBuilder WithItemType(AlertItemType itemType)
    {
        this.itemType = itemType;

        return this;
    }
    
    public AlertBuilder WithColor(string color)
    {
        this.color = color;

        return this;
    }
    
    public AlertBuilder WithCertification(string certification)
    {
        this.certification = certification;

        return this;
    }
    
    public AlertBuilder WithPrice(PriceRange price)
    {
        this.price = price;

        return this;
    }
    
    public AlertBuilder WithPrice(int priceFrom, int priceTo)
    {
        this.price = new PriceRange(priceFrom, priceTo);

        return this;
    }

    public AlertBuilder WithPriceTo(int priceTo)
    {
        this.price = new PriceRange(priceTo);

        return this;
    }

    public Alert Build()
    {
        return new Alert(offerType, itemName, price)
        {
            ItemType = itemType,
            Color = color,
            Certification = certification
        };
    }
}