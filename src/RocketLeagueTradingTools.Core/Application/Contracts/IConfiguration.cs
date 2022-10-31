namespace RocketLeagueTradingTools.Core.Application.Contracts;

public interface IConfiguration
{
    int ScrapRetryMaxAttempts { get; }
    TimeSpan ScrapIntervalMin { get; }
    TimeSpan ScrapIntervalMax { get; }
    TimeSpan HttpTimeout { get; }
    string HttpDefaultRequestUserAgent { get; }
    string HttpDefaultRequestCookie { get; }
    TimeSpan AlertOfferMaxAge { get; }
    int NotificationsPageSize { get; }
    TimeSpan NotificationsExpiration { get; }
}