using RocketLeagueTradingTools.Common;
using RocketLeagueTradingTools.Core.Application.DataRetention;
using RocketLeagueTradingTools.Infrastructure.Common;

namespace RocketLeagueTradingTools.Scraper.Infrastructure;

class DataRetentionApplicationSettings : IDataRetentionApplicationSettings
{
    private readonly IConfiguration config;

    public DataRetentionApplicationSettings(IConfiguration config)
    {
        this.config = config;
    }

    public TimeSpan TradeOffersMaxAge => config.GetRequiredValue<string>("DataRetentionRules:DeleteTradeOffersAfter").ToTimeSpan();
    public TimeSpan? NotificationsMaxAge => config.GetValue<string>("DataRetentionRules:DeleteNotificationsAfter", "").ToTimeSpanOrNull();
}