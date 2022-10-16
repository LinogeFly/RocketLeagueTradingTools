namespace RocketLeagueTradingTools.Core.Application.Contracts;

public interface IConfiguration
{
    int ScrapRetryMaxAttempts { get; }
    int ScrapIntervalInSecondsMin { get; }
    int ScrapIntervalInSecondsMax { get; }
    int HttpTimeoutInSeconds { get; }
    int CacheExpirationInMinutes { get; }
}