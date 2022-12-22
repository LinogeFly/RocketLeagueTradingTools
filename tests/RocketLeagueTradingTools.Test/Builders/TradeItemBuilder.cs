using RocketLeagueTradingTools.Core.Domain.Enumerations;
using RocketLeagueTradingTools.Core.Domain.ValueObjects;

namespace RocketLeagueTradingTools.Test.Builders;

public class TradeItemBuilder
{
    private string name = null!;
    private TradeItemType itemType;
    private string color = "";
    private string certification = "";
    
    public TradeItemBuilder WithName(string name)
    {
        this.name = name;

        return this;
    }
    
    public TradeItemBuilder WithType(TradeItemType itemType)
    {
        this.itemType = itemType;

        return this;
    }
    
    public TradeItemBuilder WithColor(string color)
    {
        this.color = color;

        return this;
    }
    
    public TradeItemBuilder WithCertification(string certification)
    {
        this.certification = certification;

        return this;
    }

    public TradeItem Build()
    {
        return new TradeItem(name)
        {
            ItemType = itemType,
            Color = color,
            Certification = certification
        };
    }
}