using HtmlAgilityPack;
using HtmlAgilityPack.CssSelectors.NetCore;
using RocketLeagueTradingTools.Core.Application.Interfaces;
using RocketLeagueTradingTools.Core.Domain.Entities;
using RocketLeagueTradingTools.Core.Domain.Enumerations;
using RocketLeagueTradingTools.Core.Domain.Exceptions;
using RocketLeagueTradingTools.Infrastructure.Common;

namespace RocketLeagueTradingTools.Infrastructure.TradeOffers;

public class RlgDataSource
{
    // PC platform only,
    // items only (no blueprints),
    // credits trades only (no item for item)
    private const string PageUrl = "https://rocket-league.com/trading?filterPlatform%5B%5D=1&filterItemType=1&filterTradeType=2";

    private readonly IHttp client;
    private readonly ILog log;
    private readonly IDateTime dateTime;

    public RlgDataSource(
        IHttp client,
        ILog log,
        IDateTime dateTime)
    {
        this.client = client;
        this.log = log;
        this.dateTime = dateTime;
    }

    public async Task<TradeOffersPage> GetTradeOffersPage(CancellationToken cancellationToken)
    {
        // Download the latest trades page
        var tradesResponse = await client.GetStringAsync(PageUrl, cancellationToken);

        var tradesDocument = new HtmlDocument();
        tradesDocument.LoadHtml(tradesResponse);

        var tradeElements = tradesDocument.QuerySelectorAll("#trading-trades .rlg-trade");

        // If no trades are found it usually means that RLG website is responding with the error page
        if (tradeElements.Count == 0)
            throw new TradingDataServiceIsNotAvailableException("RLG");

        var filteredTradeOfferElements = tradeElements
            .Where(ContainsOnlyOneForOneTypeOfOffers)
            .Where(HasNoItemOrItemTypeOfOffers)
            .ToList();

        return new TradeOffersPage
        {
            BuyOffers = filteredTradeOfferElements
                .SelectMany(MapBuyOffersFromTradeNode)
                .Where(ContainsOnlySupportedItem)
                .Distinct()
                .ToList(),
            SellOffers = filteredTradeOfferElements
                .SelectMany(MapSellOffersFromTradeNode)
                .Where(ContainsOnlySupportedItem)
                .Distinct()
                .ToList()
        };
    }

    private IEnumerable<TradeOffer> MapBuyOffersFromTradeNode(HtmlNode tradeNode)
    {
        var hasItemsElements = GetHasItemsElementsFromTradeElement(tradeNode);
        var wantsItemsElements = GetWantsItemsElementsFromTradeElement(tradeNode);

        for (int i = 0; i < hasItemsElements.Count; i++)
        {
            var hasItemElement = hasItemsElements[i];
            var wantsItemElement = wantsItemsElements[i];

            if (IsNotCreditsTradeItemElement(hasItemElement))
                continue;

            if (IsNotOneItemTradeItemElement(wantsItemElement))
                continue;

            yield return new TradeOffer
            (
                GetTradeItemFromTradeItemElement(wantsItemElement),
                GetPriceFromTradeItemElement(hasItemElement),
                dateTime.Now,
                GetLinkFromTradeElement(tradeNode),
                TradingSite.RocketLeagueGarage,
                GetTraderNameFromTradeElement(tradeNode)
            );
        }
    }

    private IEnumerable<TradeOffer> MapSellOffersFromTradeNode(HtmlNode tradeNode)
    {
        var hasItemsElements = GetHasItemsElementsFromTradeElement(tradeNode);
        var wantsItemsElements = GetWantsItemsElementsFromTradeElement(tradeNode);

        for (int i = 0; i < hasItemsElements.Count; i++)
        {
            var hasItemElement = hasItemsElements[i];
            var wantsItemElement = wantsItemsElements[i];

            if (IsNotOneItemTradeItemElement(hasItemElement))
                continue;

            if (IsNotCreditsTradeItemElement(wantsItemElement))
                continue;

            yield return new TradeOffer
            (
                GetTradeItemFromTradeItemElement(hasItemElement),
                GetPriceFromTradeItemElement(wantsItemElement),
                dateTime.Now,
                GetLinkFromTradeElement(tradeNode),
                TradingSite.RocketLeagueGarage,
                GetTraderNameFromTradeElement(tradeNode)
            );
        }
    }

    private bool IsNotCreditsTradeItemElement(HtmlNode tradeItemNode)
    {
        var name = GetNameFromTradeItemElement(tradeItemNode);

        if (name.ToLower() != "credits")
            return true;

        // Credits element but without specified amount, which is parsed to 1.
        // Such offers can't be considered as valid price ones.
        if (GetAmountFromTradeItemElement(tradeItemNode) == 1)
            return true;

        return false;
    }

    private string GetLinkFromTradeElement(HtmlNode tradeNode)
    {
        return "https://rocket-league.com/" + tradeNode.QuerySelector(".rlg-trade__action.--comments").Attributes["href"].Value.TrimStart('/');
    }

    private static IList<HtmlNode> GetHasItemsElementsFromTradeElement(HtmlNode tradeNode)
    {
        return tradeNode.QuerySelectorAll(".rlg-trade__itemshas .rlg-item");
    }

    private static IList<HtmlNode> GetWantsItemsElementsFromTradeElement(HtmlNode tradeNode)
    {
        return tradeNode.QuerySelectorAll(".rlg-trade__itemswants .rlg-item");
    }
    
    private static string GetTraderNameFromTradeElement(HtmlNode tradeNode)
    {
        return tradeNode.QuerySelector(".rlg-trade__meta .rlg-trade__username").GetDirectInnerText().Trim();
    }

    private bool IsNotOneItemTradeItemElement(HtmlNode tradeItemNode)
    {
        return GetAmountFromTradeItemElement(tradeItemNode) > 1;
    }

    private int GetPriceFromTradeItemElement(HtmlNode tradeItemNode)
    {
        if (IsNotCreditsTradeItemElement(tradeItemNode))
            throw new InvalidOperationException("Can't fetch the price from a non credits item element.");

        return GetAmountFromTradeItemElement(tradeItemNode);
    }

    private static string GetNameFromTradeItemElement(HtmlNode tradeItemNode)
    {
        return tradeItemNode.QuerySelector(".rlg-item__text .rlg-item__name").InnerText.Trim();
    }

    private TradeItem GetTradeItemFromTradeItemElement(HtmlNode tradeItemNode)
    {
        var name = GetNameFromTradeItemElement(tradeItemNode);

        return new TradeItem(name)
        {
            Color = GetColorFromTradeItemElement(tradeItemNode),
            Certification = GetCertificationFromTradeItemElement(tradeItemNode),
            ItemType = GetItemTypeFromTradeItemElement(tradeItemNode)
        };
    }

    private int GetAmountFromTradeItemElement(HtmlNode tradeItemNode)
    {
        var quantityElement = tradeItemNode.QuerySelector(".rlg-item__quantity");

        if (quantityElement == null)
            return 1;

        var quantityValue = quantityElement.InnerText.Trim();

        if (int.TryParse(quantityValue, out var quantity))
            return quantity;

        log.Error($"Unable to parse trade item amount from '{quantityValue}' string.");

        return 1;
    }

    private string GetColorFromTradeItemElement(HtmlNode tradeItemNode)
    {
        var paintElement = tradeItemNode.QuerySelector(".rlg-item__paint");
        
        return paintElement != null ? paintElement.InnerText.Trim() : "";
    }

    private string GetCertificationFromTradeItemElement(HtmlNode tradeItemNode)
    {
        var certElement = tradeItemNode.QuerySelector(".rlg-item__text .rlg-item__cert");
        
        return certElement != null ? certElement.InnerText.Trim() : "";
    }

    private TradeItemType GetItemTypeFromTradeItemElement(HtmlNode tradeItemNode)
    {
        var linkElement = tradeItemNode.QuerySelector(".rlg-item-links a.rlg-btn-primary");
        var link = linkElement?.Attributes["href"]?.Value;

        if (string.IsNullOrEmpty(link))
        {
            log.Error("Unable to parse trade item type.");

            return TradeItemType.Unknown;
        }

        switch (link.ToLower())
        {
            case var s when s.StartsWith("/items/bodies/"):
                return TradeItemType.Body;
            case var s when s.StartsWith("/items/decals/"):
                return TradeItemType.Decal;
            case var s when s.StartsWith("/items/paints/"):
                return TradeItemType.PaintFinish;
            case var s when s.StartsWith("/items/wheels/"):
                return TradeItemType.Wheels;
            case var s when s.StartsWith("/items/boosts/"):
                return TradeItemType.RocketBoost;
            case var s when s.StartsWith("/items/toppers/"):
                return TradeItemType.Topper;
            case var s when s.StartsWith("/items/antennas/"):
                return TradeItemType.Antenna;
            case var s when s.StartsWith("/items/explosions/"):
                return TradeItemType.GoalExplosion;
            case var s when s.StartsWith("/items/trails/"):
                return TradeItemType.Trail;
            case var s when s.StartsWith("/items/banners/"):
                return TradeItemType.Banner;
            case var s when s.StartsWith("/items/borders/"):
                return TradeItemType.AvatarBorder;
            default:
                return TradeItemType.Unknown;
        }        
    }

    private static bool ContainsOnlyOneForOneTypeOfOffers(HtmlNode tradeNode)
    {
        // Skip trades that are not 1 for 1 type of trades. Those are difficult to analyze
        // and usually are not specific trades, but those that expect offers for listed items.

        var hasItemsElements = GetHasItemsElementsFromTradeElement(tradeNode);
        var wantsItemsElements = GetWantsItemsElementsFromTradeElement(tradeNode);

        return hasItemsElements.Count == wantsItemsElements.Count;
    }

    private static bool HasNoItemOrItemTypeOfOffers(HtmlNode tradeNode)
    {
        // Skip trades that contain "OR" type of offers.

        var hasItemsElements = GetHasItemsElementsFromTradeElement(tradeNode);
        var wantsItemsElements = GetWantsItemsElementsFromTradeElement(tradeNode);

        if (hasItemsElements.Any(i => GetNameFromTradeItemElement(i).ToLower() == "or"))
            return false;

        if (wantsItemsElements.Any(i => GetNameFromTradeItemElement(i).ToLower() == "or"))
            return false;

        return true;
    }

    private static bool ContainsOnlySupportedItem(TradeOffer offer)
    {
        if (offer.Item.Name.ToLower().Contains("offer"))
            return false;

        if (offer.Item.Name.ToLower() == "overpay")
            return false;

        if (offer.Item.Name.ToLower() == "placeholder")
            return false;

        if (offer.Item.Name.ToLower().EndsWith("blueprint"))
            return false;

        if (offer.Item.Color.ToLower() == "painted set")
            return false;

        return true;
    }
}