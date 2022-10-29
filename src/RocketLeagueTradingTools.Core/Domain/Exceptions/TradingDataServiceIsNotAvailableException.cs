namespace RocketLeagueTradingTools.Core.Domain.Exceptions;

public sealed class TradingDataServiceIsNotAvailableException : Exception
{
    public string DataServiceName { get; }

    public TradingDataServiceIsNotAvailableException(string dataServiceName)
    {
        DataServiceName = dataServiceName;
    }
}