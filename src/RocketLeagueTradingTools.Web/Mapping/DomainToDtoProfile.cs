using AutoMapper;
using RocketLeagueTradingTools.Web.Models.Common;
using RocketLeagueTradingTools.Web.Models.Alert;
using RocketLeagueTradingTools.Web.Models.Notification;
using RocketLeagueTradingTools.Core.Application.Interfaces;

namespace RocketLeagueTradingTools.Web.Mapping;

public class DomainToDtoProfile : Profile
{
    public DomainToDtoProfile()
    {
        CreateMap<Core.Domain.ValueObjects.PriceRange, PriceRangeDto>();

        CreateMap<Core.Domain.Entities.Alert, AlertDto>();

        CreateMap<Core.Domain.Entities.Notification, NotificationDto>()
            .ForMember(dest => dest.ItemName, opt => opt.MapFrom(src => src.TradeOffer.Item.Name))
            .ForMember(dest => dest.ItemPrice, opt => opt.MapFrom(src => src.TradeOffer.Price))
            .ForMember(dest => dest.IsNew, opt => opt.MapFrom(src => src.SeenDate == null))
            .ForMember(dest => dest.TradeOfferAge, opt => opt.MapFrom<TradeOfferAgeResolver>());
    }

    public class TradeOfferAgeResolver : IValueResolver<Core.Domain.Entities.Notification, NotificationDto, string>
    {
        private readonly IDateTime dateTime;

        public TradeOfferAgeResolver(IDateTime dateTime)
        {
            this.dateTime = dateTime;
        }

        public string Resolve(Core.Domain.Entities.Notification source, NotificationDto dest, string destMember, ResolutionContext context)
        {
            var age = dateTime.Now - source.TradeOffer.ScrapedDate;

            if (age.Hours == 1)
                return $"{age.Hours} hour";
            if (age.Hours > 1)
                return $"{age.Hours} hours";
            if (age.Minutes == 1)
                return $"{age.Minutes} minute";
            if (age.Minutes > 1)
                return $"{age.Minutes} minutes";
            if (age.Seconds == 1)
                return $"{age.Minutes} second";

            return $"{age.Seconds} seconds";
        }
    }
}
