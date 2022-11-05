using RocketLeagueTradingTools.Core.Application.Interfaces;

namespace RocketLeagueTradingTools.Infrastructure.Common;

public class SystemDateTime : IDateTime
{
    public DateTime Now => DateTime.UtcNow;
}