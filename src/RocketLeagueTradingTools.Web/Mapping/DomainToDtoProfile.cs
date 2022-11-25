using AutoMapper;
using RocketLeagueTradingTools.Core.Application.Interfaces;
using RocketLeagueTradingTools.Core.Domain.Entities;
using RocketLeagueTradingTools.Core.Domain.Enumerations;
using RocketLeagueTradingTools.Core.Domain.ValueObjects;
using RocketLeagueTradingTools.Web.Models.Alert;
using RocketLeagueTradingTools.Web.Models.Blacklist;
using RocketLeagueTradingTools.Web.Models.Common;
using RocketLeagueTradingTools.Web.Models.Notification;

namespace RocketLeagueTradingTools.Web.Mapping;

public class DomainToDtoProfile : Profile
{
    public DomainToDtoProfile()
    {
        CreateMap<PriceRange, PriceRangeDto>();

        CreateMap<Alert, AlertDto>();

        CreateMap<Notification, NotificationDto>()
            .ForMember(dest => dest.ItemName, opt => opt.MapFrom(src => src.ScrapedTradeOffer.TradeOffer.Item.Name))
            .ForMember(dest => dest.ItemPrice, opt => opt.MapFrom(src => src.ScrapedTradeOffer.TradeOffer.Price))
            .ForMember(dest => dest.TradeOfferLink, opt => opt.MapFrom(src => src.ScrapedTradeOffer.TradeOffer.Link))
            .ForMember(dest => dest.TradeOfferAge, opt => opt.MapFrom<TradeOfferAgeResolver>())
            .ForMember(dest => dest.IsNew, opt => opt.MapFrom(src => src.SeenDate == null));

        CreateMap<Trader, BlacklistedTraderDto>()
            .ForMember(dest => dest.TradingSite, opt => opt.MapFrom(src => Map(src.TradingSite)))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name));
    }

    private string Map(TradingSite tradingSite)
    {
        switch (tradingSite)
        {
            case TradingSite.RocketLeagueGarage:
                return "RLG";
            default:
                throw new InvalidOperationException($"Invalid trading site '{tradingSite}'.");
        }
    }

    public class TradeOfferAgeResolver : IValueResolver<Notification, NotificationDto, string>
    {
        private readonly IDateTime dateTime;

        public TradeOfferAgeResolver(IDateTime dateTime)
        {
            this.dateTime = dateTime;
        }

        public string Resolve(Notification source, NotificationDto dest, string destMember, ResolutionContext context)
        {
            var age = dateTime.Now - source.ScrapedTradeOffer.ScrapedDate;

            if (age.Hours > 1)
                return $"{age.Hours} hours";
            if (age.Hours == 1)
                return $"{age.Hours} hour";
            if (age.Minutes > 1)
                return $"{age.Minutes} minutes";
            if (age.Minutes == 1)
                return $"{age.Minutes} minute";
            if (age.Seconds > 1)
                return $"{age.Seconds} seconds";

            // Combining one and less than one into "1 second", as we don't want to have "0 seconds" age.
            return "1 second";
        }
    }
}