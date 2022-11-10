
namespace RocketLeagueTradingTools.Core.Application.Scraping;

public interface IScrapApplicationSettings
{
    TimeSpan DelayMin { get; }
    TimeSpan DelayMax { get; }
    int RetryMaxAttempts { get; }
    TimeSpan RetryInterval { get; }
    int RetryBackoffRate { get; }
}