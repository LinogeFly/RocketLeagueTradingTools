using RocketLeagueTradingTools.Common;
using RocketLeagueTradingTools.Core.Domain.Enumerations;

namespace RocketLeagueTradingTools.Core.Domain.ValueObjects;

public class Trader : ValueObject
{
    public TradingSite TradingSite { get; }
    public string Name { get; }

    public Trader(TradingSite tradingSite, string name)
    {
        if (name.IsEmpty())
            throw new ArgumentException("The field is required.", nameof(name));

        TradingSite = tradingSite;
        Name = name;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return TradingSite;
        yield return Name;
    }
}