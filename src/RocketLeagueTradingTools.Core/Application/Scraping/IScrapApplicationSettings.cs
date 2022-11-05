
namespace RocketLeagueTradingTools.Core.Application.Scraping;

public interface IScrapApplicationSettings
{
    int ScrapRetryMaxAttempts { get; }
}