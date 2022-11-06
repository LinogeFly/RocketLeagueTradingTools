using AutoMapper;
using RocketLeagueTradingTools.Core.Domain.ValueObjects;
using RocketLeagueTradingTools.Web.Models.Alert;
using RocketLeagueTradingTools.Web.Models.Common;

namespace RocketLeagueTradingTools.Web.Mapping;

public class DtoToDomainProfile : Profile
{
    public DtoToDomainProfile()
    {
        CreateMap<PriceRangeDto, PriceRange>()
            .ConstructUsing(src => new PriceRange(src.From, src.To));

        CreateMap<OfferItemTypeDto, Core.Domain.Entities.TradeItemType>();

        CreateMap<AlertDto, Core.Domain.Entities.Alert>();
        CreateMap<AlertRequest, Core.Domain.Entities.Alert>();
    }
}