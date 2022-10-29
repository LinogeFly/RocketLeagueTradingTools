namespace RocketLeagueTradingTools.Core.Application.Contracts;

public interface IConfiguration
{
    int ScrapRetryMaxAttempts { get; }
    int ScrapIntervalInSecondsMin { get; }
    int ScrapIntervalInSecondsMax { get; }
    int HttpTimeoutInSeconds { get; }
    string HttpDefaultRequestUserAgent { get; }
    string HttpDefaultRequestCookie { get; }
    int AlertOfferMaxAgeInMinutes { get; }
    int NotificationsPageSize { get; }
    int NotificationsExpirationInHours { get; }
}