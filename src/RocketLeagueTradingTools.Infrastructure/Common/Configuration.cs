using Microsoft.Extensions.Configuration;

namespace RocketLeagueTradingTools.Infrastructure.Common;

public class Configuration : Core.Application.Contracts.IConfiguration
{
    private readonly IConfiguration config;

    public Configuration(IConfiguration config)
    {
        this.config = config;
    }

    public int ScrapRetryMaxAttempts => config.GetValue<int>("ScrapRetryMaxAttempts");
    public int ScrapIntervalInSecondsMin => config.GetValue<int>("ScrapIntervalInSecondsMin");
    public int ScrapIntervalInSecondsMax => config.GetValue<int>("ScrapIntervalInSecondsMax");
    public int HttpTimeoutInSeconds => config.GetValue<int>("HttpTimeoutInSeconds");
    public int CacheExpirationInMinutes => config.GetValue<int>("CacheExpirationInMinutes");
}