using RocketLeagueTradingTools.Common;
using RocketLeagueTradingTools.Core.Domain.Enumerations;

namespace RocketLeagueTradingTools.Core.Domain.Entities;

public sealed class BlacklistedTrader
{
    public int Id { get; set; }
    public TradingSite TradingSite { get; }
    public string Name { get; }

    public BlacklistedTrader(TradingSite tradingSite, string name)
    {
        if (name.IsEmpty())
            throw new ArgumentException($"{nameof(name)} is required.");
        
        Name = name;
        TradingSite = tradingSite;
    }
}