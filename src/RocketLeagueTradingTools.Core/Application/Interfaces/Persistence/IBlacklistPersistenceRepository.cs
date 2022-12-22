using RocketLeagueTradingTools.Core.Domain.Entities;
using RocketLeagueTradingTools.Core.Domain.ValueObjects;

namespace RocketLeagueTradingTools.Core.Application.Interfaces.Persistence;

public interface IBlacklistPersistenceRepository
{
    Task<IList<BlacklistedTrader>> GetBlacklistedTraders();
    Task AddBlacklistedTrader(Trader trader);
    Task DeleteBlacklistedTrader(int id);
}