namespace RocketLeagueTradingTools.Core.Domain.Exceptions;

public sealed class TradingDataSourceIsDownException : Exception
{
    public string DataSourceName { get; }

    public TradingDataSourceIsDownException(string dataSourceName)
    {
        DataSourceName = dataSourceName;
    }
}