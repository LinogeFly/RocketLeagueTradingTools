using Microsoft.Extensions.Configuration;

namespace RocketLeagueTradingTools.Infrastructure.Common;

public class Configuration : Core.Application.Contracts.IConfiguration
{
    private readonly IConfiguration config;

    public Configuration(IConfiguration config)
    {
        this.config = config;
    }

    public int ScrapRetryMaxAttempts => config.GetValue<int>("ScrapRetryMaxAttempts", 6);
    public TimeSpan ScrapIntervalMin => config.GetValue<string>("ScrapIntervalMin", "00:00:08").ToTimeSpan();
    public TimeSpan ScrapIntervalMax => config.GetValue<string>("ScrapIntervalMax", "00:00:10").ToTimeSpan();
    public TimeSpan HttpTimeout => config.GetValue<string>("HttpTimeout", "00:00:20").ToTimeSpan();
    public string HttpDefaultRequestUserAgent => config.GetValue<string>("HttpDefaultRequestUserAgent");
    public string HttpDefaultRequestCookie => config.GetValue<string>("HttpDefaultRequestCookie");
    public TimeSpan AlertOfferMaxAge => config.GetValue<string>("AlertOfferMaxAge", "01:00:00").ToTimeSpan();
    public int NotificationsPageSize => config.GetValue<int>("NotificationsPageSize", 50);
    public TimeSpan NotificationsExpiration => config.GetValue<string>("NotificationsExpiration", "2.00:00:00").ToTimeSpan();
}