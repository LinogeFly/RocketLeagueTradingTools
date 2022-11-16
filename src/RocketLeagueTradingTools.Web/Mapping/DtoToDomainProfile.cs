using AutoMapper;
using RocketLeagueTradingTools.Core.Domain.Entities;
using RocketLeagueTradingTools.Core.Domain.Enumerations;
using RocketLeagueTradingTools.Core.Domain.ValueObjects;
using RocketLeagueTradingTools.Web.Models.Alert;
using RocketLeagueTradingTools.Web.Models.Blacklist;
using RocketLeagueTradingTools.Web.Models.Common;

namespace RocketLeagueTradingTools.Web.Mapping;

public class DtoToDomainProfile : Profile
{
    public DtoToDomainProfile()
    {
        CreateMap<PriceRangeDto, PriceRange>()
            .ConstructUsing(src => new PriceRange(src.From, src.To));

        CreateMap<OfferItemTypeDto, TradeItemType>();

        CreateMap<AlertRequest, Alert>();

        CreateMap<BlacklistedTraderRequest, BlacklistedTrader>()
            .ConstructUsing(src => new BlacklistedTrader(Map(src.TradingSite), src.Name));
    }

    private TradingSite Map(string tradingSite)
    {
        switch (tradingSite.ToLower())
        {
            case "rlg":
            case "rocketleaguegarage":
            case "rlgarage":
                return TradingSite.RocketLeagueGarage;        
            default:
                throw new InvalidOperationException($"Invalid trading site '{tradingSite}'.");
        }
    }
}