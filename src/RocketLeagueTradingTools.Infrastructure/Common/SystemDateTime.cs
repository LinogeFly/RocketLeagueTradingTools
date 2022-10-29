using RocketLeagueTradingTools.Core.Application.Contracts;

namespace RocketLeagueTradingTools.Infrastructure.Common;

public class SystemDateTime : IDateTime
{
    public DateTime Now => DateTime.UtcNow;
}