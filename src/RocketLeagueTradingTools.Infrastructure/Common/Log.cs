using RocketLeagueTradingTools.Core.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace RocketLeagueTradingTools.Infrastructure.Common;

public class Log : ILog
{
    private readonly ILogger logger;

    public Log(ILogger logger)
    {
        this.logger = logger;
    }

    public void Trace(string message)
    {
        logger.LogTrace(message);
    }

    public void Error(string message)
    {
        logger.LogError(message);
    }

    public void Info(string message)
    {
        logger.LogInformation(message);
    }

    public void Warn(string message)
    {
        logger.LogWarning(message);
    }
}