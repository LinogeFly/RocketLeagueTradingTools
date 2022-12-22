using RocketLeagueTradingTools.Core.Domain.ValueObjects;

namespace RocketLeagueTradingTools.Core.Domain.Entities;

public class BlacklistedTrader
{
    public int Id { get; init; }
    public Trader Trader { get; }

    public BlacklistedTrader(Trader trader)
    {
        Trader = trader;
    }
}