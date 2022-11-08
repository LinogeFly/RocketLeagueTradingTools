using RocketLeagueTradingTools.Common;
using RocketLeagueTradingTools.Infrastructure.Common;
using RocketLeagueTradingTools.Core.Application.Scraping;

namespace RocketLeagueTradingTools.Scraper.Infrastructure;

class ScrapApplicationSettings : IScrapApplicationSettings
{
    private readonly IConfiguration config;

    public ScrapApplicationSettings(IConfiguration config)
    {
        this.config = config;
    }

    public TimeSpan ScrapDelayMin => config.GetRequiredValue<string>("ScrapSettings:DelayMin").ToTimeSpan();
    public TimeSpan ScrapDelayMax => config.GetRequiredValue<string>("ScrapSettings:DelayMax").ToTimeSpan();
    public int ScrapRetryMaxAttempts => config.GetRequiredValue<int>("ScrapSettings:RetryMaxAttempts");
}