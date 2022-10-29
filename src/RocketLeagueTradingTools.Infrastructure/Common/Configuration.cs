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
    public string HttpDefaultRequestUserAgent => config.GetValue<string>("HttpDefaultRequestUserAgent");
    public string HttpDefaultRequestCookie => config.GetValue<string>("HttpDefaultRequestCookie");
    public int AlertOfferMaxAgeInMinutes => config.GetValue<int>("AlertOfferMaxAgeInMinutes");
    public int NotificationsPageSize => config.GetValue<int>("NotificationsPageSize", 50);
    public int NotificationsExpirationInHours => config.GetValue<int>("NotificationsExpirationInHours", 48);
}