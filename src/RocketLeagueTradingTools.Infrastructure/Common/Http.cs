using RocketLeagueTradingTools.Core.Application.Contracts;

namespace RocketLeagueTradingTools.Infrastructure.Common;

public interface IHttp
{
    Task<string> GetStringAsync(string? requestUri, CancellationToken cancellationToken);
}

public class Http : IHttp
{
    private readonly HttpClient httpClient;

    public Http(IConfiguration config)
    {
        httpClient = new HttpClient();

        if (!string.IsNullOrEmpty(config.HttpDefaultRequestUserAgent))
            httpClient.DefaultRequestHeaders.Add("user-agent", config.HttpDefaultRequestUserAgent);

        if (!string.IsNullOrEmpty(config.HttpDefaultRequestCookie))
            httpClient.DefaultRequestHeaders.Add("cookie", config.HttpDefaultRequestCookie);

        if (config.HttpTimeoutInSeconds != 0)
            httpClient.Timeout = TimeSpan.FromMinutes(config.HttpTimeoutInSeconds);
    }

    public Task<string> GetStringAsync(string? requestUri, CancellationToken cancellationToken)
    {
        return httpClient.GetStringAsync(requestUri, cancellationToken);
    }
}