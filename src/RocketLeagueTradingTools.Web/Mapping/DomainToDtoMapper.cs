using RocketLeagueTradingTools.Common;
using RocketLeagueTradingTools.Common.Exceptions;
using RocketLeagueTradingTools.Core.Application.Interfaces;
using RocketLeagueTradingTools.Core.Domain.Entities;
using RocketLeagueTradingTools.Core.Domain.Enumerations;
using RocketLeagueTradingTools.Core.Domain.ValueObjects;
using RocketLeagueTradingTools.Web.Contracts.Alert;
using RocketLeagueTradingTools.Web.Contracts.Blacklist;
using RocketLeagueTradingTools.Web.Contracts.Common;
using RocketLeagueTradingTools.Web.Contracts.Notification;

namespace RocketLeagueTradingTools.Web.Mapping;

public class DomainToDtoMapper
{
    private readonly IDateTime dateTime;

    public DomainToDtoMapper(IDateTime dateTime)
    {
        this.dateTime = dateTime;
    }
    
    public static AlertResponse Map(Alert value)
    {
        return new AlertResponse
        {
            Id = value.Id,
            ItemName = value.ItemName,
            TradeOfferType = Map(value.OfferType),
            Price = Map(value.Price),
            ItemType = Map(value.ItemType),
            Color = value.Color,
            Certification = value.Certification,
            Enabled = value.Enabled
        };
    }
    
    public IList<NotificationDto> Map(IEnumerable<Notification> value)
    {
        return value.Select(Map).ToList();
    }
    
    public static BlacklistedTraderResponse Map(BlacklistedTrader value)
    {
        return new BlacklistedTraderResponse
        {
            Id = value.Id,
            Name = value.Trader.Name,
            TradingSite = Map(value.Trader.TradingSite)
        };
    }

    private static PriceRangeDto Map(PriceRange value)
    {
        return new PriceRangeDto(value.From, value.To);
    }

    private static TradeOfferTypeDto Map(TradeOfferType value)
    {
        return value switch
        {
            TradeOfferType.Buy => TradeOfferTypeDto.Buy,
            TradeOfferType.Sell => TradeOfferTypeDto.Sell,
            _ => throw new MappingException(value, typeof(AlertItemType), typeof(AlertItemTypeDto))
        };
    }

    private static AlertItemTypeDto Map(AlertItemType value)
    {
        return value switch
        {
            AlertItemType.Any => AlertItemTypeDto.Any,
            AlertItemType.Body => AlertItemTypeDto.Body,
            AlertItemType.Decal => AlertItemTypeDto.Decal,
            AlertItemType.PaintFinish => AlertItemTypeDto.PaintFinish,
            AlertItemType.Wheels => AlertItemTypeDto.Wheels,
            AlertItemType.RocketBoost => AlertItemTypeDto.RocketBoost,
            AlertItemType.Topper => AlertItemTypeDto.Topper,
            AlertItemType.Antenna => AlertItemTypeDto.Antenna,
            AlertItemType.GoalExplosion => AlertItemTypeDto.GoalExplosion,
            AlertItemType.Trail => AlertItemTypeDto.Trail,
            AlertItemType.Banner => AlertItemTypeDto.Banner,
            AlertItemType.AvatarBorder => AlertItemTypeDto.AvatarBorder,
            _ => throw new MappingException(value, typeof(AlertItemType), typeof(AlertItemTypeDto))
        };
    }
    
    private NotificationDto Map(Notification value)
    {
        return new NotificationDto
        {
            Id = value.Id,
            ItemName = value.ScrapedTradeOffer.TradeOffer.Item.Name,
            ItemPrice = value.ScrapedTradeOffer.TradeOffer.Price,
            TradeOfferAge = (dateTime.Now - value.ScrapedTradeOffer.ScrapedDate).ToHumanReadableString(),
            TradeOfferLink = value.ScrapedTradeOffer.TradeOffer.Link,
            IsNew = value.SeenDate is null
        };
    }
    
    private static string Map(TradingSite value)
    {
        return value switch
        {
            TradingSite.RocketLeagueGarage => "RLG",
            _ => throw new MappingException(value, typeof(TradingSite), typeof(string))
        };
    }
}