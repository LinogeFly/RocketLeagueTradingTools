namespace RocketLeagueTradingTools.Core.Application.Contracts;

public interface ILog
{
    void Trace(string message);
    void Info(string message);
    void Warn(string message);
    void Error(string message);
}