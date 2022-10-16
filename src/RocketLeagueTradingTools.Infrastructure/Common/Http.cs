namespace RocketLeagueTradingTools.Infrastructure.Common;

public interface IHttp
{
    Task<string> GetStringAsync(string? requestUri, CancellationToken cancellationToken);
    void SetTimeout(TimeSpan timeout);
}

public class Http: IHttp
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

    public void SetTimeout(TimeSpan timeout)
    {
        httpClient.Timeout = timeout;
    }
}