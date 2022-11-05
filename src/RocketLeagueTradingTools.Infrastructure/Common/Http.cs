namespace RocketLeagueTradingTools.Infrastructure.Common;

public interface IHttp
{
    Task<string> GetStringAsync(string? requestUri, CancellationToken cancellationToken);
    TimeSpan Timeout { set; }
    string DefaultRequestUserAgent { set; }
    string DefaultRequestCookie { set; }
}

public class Http : IHttp
{
    private readonly HttpClient httpClient;

    public Http()
    {
        httpClient = new HttpClient();
    }

    public Task<string> GetStringAsync(string? requestUri, CancellationToken cancellationToken)
    {
        return httpClient.GetStringAsync(requestUri, cancellationToken);
    }

    public TimeSpan Timeout
    {
        set => httpClient.Timeout = value;
    }

    public string DefaultRequestUserAgent
    {
        set
        {
            if (string.IsNullOrEmpty(value))
                return;

            if (httpClient.DefaultRequestHeaders.Contains("user-agent"))
                httpClient.DefaultRequestHeaders.Remove("user-agent");

            httpClient.DefaultRequestHeaders.Add("user-agent", value);
        }
    }

    public string DefaultRequestCookie
    {
        set
        {
            if (string.IsNullOrEmpty(value))
                return;

            if (httpClient.DefaultRequestHeaders.Contains("cookie"))
                httpClient.DefaultRequestHeaders.Remove("cookie");

            httpClient.DefaultRequestHeaders.Add("cookie", value);
        }
    }
}