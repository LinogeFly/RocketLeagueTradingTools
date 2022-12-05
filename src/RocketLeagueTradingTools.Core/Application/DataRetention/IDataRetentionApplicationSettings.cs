namespace RocketLeagueTradingTools.Core.Application.DataRetention;

public interface IDataRetentionApplicationSettings
{
    TimeSpan TradeOffersMaxAge { get; }
    TimeSpan? NotificationsMaxAge { get; }
}