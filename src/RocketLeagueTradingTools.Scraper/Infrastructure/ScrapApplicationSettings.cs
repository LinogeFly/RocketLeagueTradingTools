using RocketLeagueTradingTools.Common;
using RocketLeagueTradingTools.Infrastructure.Common;
using RocketLeagueTradingTools.Core.Application.Scrap;

namespace RocketLeagueTradingTools.Scraper.Infrastructure;

class ScrapApplicationSettings : IScrapApplicationSettings
{
    private readonly IConfiguration config;

    public ScrapApplicationSettings(IConfiguration config)
    {
        this.config = config;
    }

    public TimeSpan DelayMin => config.GetRequiredValue<string>("ScrapSettings:DelayMin").ToTimeSpan();
    public TimeSpan DelayMax => config.GetRequiredValue<string>("ScrapSettings:DelayMax").ToTimeSpan();
    public int RetryMaxAttempts => config.GetRequiredValue<int>("ScrapSettings:Retry:MaxAttempts");
    public TimeSpan RetryInterval => config.GetRequiredValue<string>("ScrapSettings:Retry:Interval").ToTimeSpan();
    public int RetryBackoffRate => config.GetRequiredValue<int>("ScrapSettings:Retry:BackoffRate");
}