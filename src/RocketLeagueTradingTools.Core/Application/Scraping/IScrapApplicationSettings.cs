
namespace RocketLeagueTradingTools.Core.Application.Scraping;

public interface IScrapApplicationSettings
{
    TimeSpan ScrapDelayMin { get; }
    TimeSpan ScrapDelayMax { get; }
    int ScrapRetryMaxAttempts { get; }
}