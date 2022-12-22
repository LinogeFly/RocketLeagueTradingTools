
namespace RocketLeagueTradingTools.Core.Application.Scrap;

public interface IScrapApplicationSettings
{
    TimeSpan DelayMin { get; }
    TimeSpan DelayMax { get; }
    int RetryMaxAttempts { get; }
    TimeSpan RetryInterval { get; }
    int RetryBackoffRate { get; }
}