using RocketLeagueTradingTools.Common.Exceptions;
using RocketLeagueTradingTools.Core.Domain.Entities;
using RocketLeagueTradingTools.Core.Domain.Enumerations;
using RocketLeagueTradingTools.Core.Domain.ValueObjects;
using RocketLeagueTradingTools.Web.Contracts.Alert;
using RocketLeagueTradingTools.Web.Contracts.Blacklist;
using RocketLeagueTradingTools.Web.Contracts.Common;

namespace RocketLeagueTradingTools.Web.Mapping;

public static class DtoToDomainMapper
{
    public static Alert Map(AlertUpsertRequest value)
    {
        return new Alert(Map(value.OfferType), value.ItemName, Map(value.Price))
        {
            ItemType = Map(value.ItemType),
            Color = value.Color,
            Certification = value.Certification,
            Enabled = value.Enabled
        };
    }
    
    public static Trader Map(BlacklistedTraderRequest value)
    {
        return new Trader(Map(value.TradingSite), value.Name);
    }
    
    public static TradeItemType Map(TradeItemTypeDto value)
    {
        return value switch
        {
            TradeItemTypeDto.Body => TradeItemType.Body,
            TradeItemTypeDto.Decal => TradeItemType.Decal,
            TradeItemTypeDto.PaintFinish => TradeItemType.PaintFinish,
            TradeItemTypeDto.Wheels => TradeItemType.Wheels,
            TradeItemTypeDto.RocketBoost => TradeItemType.RocketBoost,
            TradeItemTypeDto.Topper => TradeItemType.Topper,
            TradeItemTypeDto.Antenna => TradeItemType.Antenna,
            TradeItemTypeDto.GoalExplosion => TradeItemType.GoalExplosion,
            TradeItemTypeDto.Trail => TradeItemType.Trail,
            TradeItemTypeDto.Banner => TradeItemType.Banner,
            TradeItemTypeDto.AvatarBorder => TradeItemType.AvatarBorder,
            TradeItemTypeDto.Unknown => TradeItemType.Unknown,
            _ => throw new MappingException(value, typeof(TradeItemTypeDto), typeof(TradeItemType))
        };
    }
    
    private static PriceRange Map(PriceRangeDto value)
    {
        return new PriceRange(value.From, value.To);
    }

    private static TradeOfferType Map(TradeOfferTypeDto? value)
    {
        return value switch
        {
            TradeOfferTypeDto.Buy => TradeOfferType.Buy,
            TradeOfferTypeDto.Sell => TradeOfferType.Sell,
            null => throw new ArgumentNullException(),
            _ => throw new MappingException(value, typeof(TradeOfferTypeDto), typeof(TradeOfferType))
        };
    }

    private static AlertItemType Map(AlertItemTypeDto value)
    {
        return value switch
        {
            AlertItemTypeDto.Any => AlertItemType.Any,
            AlertItemTypeDto.Body => AlertItemType.Body,
            AlertItemTypeDto.Decal => AlertItemType.Decal,
            AlertItemTypeDto.PaintFinish => AlertItemType.PaintFinish,
            AlertItemTypeDto.Wheels => AlertItemType.Wheels,
            AlertItemTypeDto.RocketBoost => AlertItemType.RocketBoost,
            AlertItemTypeDto.Topper => AlertItemType.Topper,
            AlertItemTypeDto.Antenna => AlertItemType.Antenna,
            AlertItemTypeDto.GoalExplosion => AlertItemType.GoalExplosion,
            AlertItemTypeDto.Trail => AlertItemType.Trail,
            AlertItemTypeDto.Banner => AlertItemType.Banner,
            AlertItemTypeDto.AvatarBorder => AlertItemType.AvatarBorder,
            _ => throw new MappingException(value, typeof(AlertItemTypeDto), typeof(AlertItemType))
        };
    }
    
    private static TradingSite Map(string value)
    {
        return value.ToLower() switch
        {
            "rlg" => TradingSite.RocketLeagueGarage,
            "rocketleaguegarage" => TradingSite.RocketLeagueGarage,
            "rlgarage" => TradingSite.RocketLeagueGarage,
            _ => throw new MappingException(value, typeof(string), typeof(TradingSite))
        };
    }
}