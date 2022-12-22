using RocketLeagueTradingTools.Core.Domain.Enumerations;
using RocketLeagueTradingTools.Core.Domain.ValueObjects;

namespace RocketLeagueTradingTools.Test.Builders;

public class TraderBuilder
{
    private TradingSite tradingSite;
    private string name = default!;

    public TraderBuilder WithTradingSite(TradingSite tradingSite)
    {
        this.tradingSite = tradingSite;

        return this;
    }
    
    public TraderBuilder WithName(string name)
    {
        this.name = name;

        return this;
    }

    public Trader Build()
    {
        return new Trader(tradingSite, name);
    }
}