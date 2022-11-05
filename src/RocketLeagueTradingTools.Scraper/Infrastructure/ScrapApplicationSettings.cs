using Microsoft.Extensions.Configuration;
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

    public int ScrapRetryMaxAttempts => config.GetRequiredValue<int>("ScrapRetryMaxAttempts");
}