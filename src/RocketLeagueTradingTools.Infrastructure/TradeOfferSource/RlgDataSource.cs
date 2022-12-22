using HtmlAgilityPack;
using HtmlAgilityPack.CssSelectors.NetCore;
using RocketLeagueTradingTools.Core.Application.Interfaces;
using RocketLeagueTradingTools.Core.Domain.Entities;
using RocketLeagueTradingTools.Core.Domain.Enumerations;
using RocketLeagueTradingTools.Core.Domain.Exceptions;
using RocketLeagueTradingTools.Core.Domain.ValueObjects;
using RocketLeagueTradingTools.Infrastructure.Common;

namespace RocketLeagueTradingTools.Infrastructure.TradeOfferSource;

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

    public async Task<IList<ScrapedTradeOffer>> GetTradeOffersPage(CancellationToken cancellationToken)
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

        return filteredTradeOfferElements
            .SelectMany(MapTradeOffersFromTradeNode)
            .Where(ContainsOnlySupportedItem)
            .Distinct()
            .Select(o => new ScrapedTradeOffer(o, dateTime.Now))
            .ToList();
    }

    private IEnumerable<TradeOffer> MapTradeOffersFromTradeNode(HtmlNode tradeNode)
    {
        var hasItems = GetHasItemsElementsFromTradeElement(tradeNode);
        var wantsItems = GetWantsItemsElementsFromTradeElement(tradeNode);

        for (int i = 0; i < hasItems.Count; i++)
        {
            var hasItem = hasItems[i];
            var wantsItem = wantsItems[i];
            var isBuyOffer = IsBuyOfferTradeItemElements(hasItem, wantsItem);
            var isSellOffer = IsSellOfferTradeItemElements(hasItem, wantsItem);

            if (!isBuyOffer && !isSellOffer)
                continue;

            yield return new TradeOffer
            (
                isBuyOffer ? TradeOfferType.Buy : TradeOfferType.Sell,
                isBuyOffer ? GetTradeItemFromTradeItemElement(wantsItem) : GetTradeItemFromTradeItemElement(hasItem),
                isBuyOffer ? GetPriceFromTradeItemElement(hasItem) : GetPriceFromTradeItemElement(wantsItem),
                GetLinkFromTradeElement(tradeNode),
                new Trader(TradingSite.RocketLeagueGarage, GetTraderNameFromTradeElement(tradeNode))
            );
        }
    }

    private bool IsBuyOfferTradeItemElements(HtmlNode hasItemNode, HtmlNode wantsItemNode)
    {
        return IsCreditsTradeItemElement(hasItemNode) && IsOneItemTradeItemElement(wantsItemNode);
    }

    private bool IsSellOfferTradeItemElements(HtmlNode hasItemNode, HtmlNode wantsItemNode)
    {
        return IsOneItemTradeItemElement(hasItemNode) && IsCreditsTradeItemElement(wantsItemNode);
    }

    private bool IsCreditsTradeItemElement(HtmlNode tradeItemNode)
    {
        var name = GetNameFromTradeItemElement(tradeItemNode);

        if (name.ToLower() != "credits")
            return false;

        // Credits element but without specified amount, which is parsed to 1.
        // Such offers can't be considered as valid price ones.
        if (GetAmountFromTradeItemElement(tradeItemNode) == 1)
            return false;

        return true;
    }

    private bool IsOneItemTradeItemElement(HtmlNode tradeItemNode)
    {
        return GetAmountFromTradeItemElement(tradeItemNode) == 1;
    }

    private string GetLinkFromTradeElement(HtmlNode tradeNode)
    {
        return "https://rocket-league.com/" + tradeNode.QuerySelector(".rlg-trade__action.--comments").Attributes["href"].Value.TrimStart('/');
    }

    private IList<HtmlNode> GetHasItemsElementsFromTradeElement(HtmlNode tradeNode)
    {
        return tradeNode.QuerySelectorAll(".rlg-trade__itemshas .rlg-item");
    }

    private IList<HtmlNode> GetWantsItemsElementsFromTradeElement(HtmlNode tradeNode)
    {
        return tradeNode.QuerySelectorAll(".rlg-trade__itemswants .rlg-item");
    }

    private string GetTraderNameFromTradeElement(HtmlNode tradeNode)
    {
        return tradeNode.QuerySelector(".rlg-trade__meta .rlg-trade__username").GetDirectInnerText().Trim();
    }

    private int GetPriceFromTradeItemElement(HtmlNode tradeItemNode)
    {
        if (!IsCreditsTradeItemElement(tradeItemNode))
            throw new InvalidOperationException("Can't fetch the price from a non credits item element.");

        return GetAmountFromTradeItemElement(tradeItemNode);
    }

    private string GetNameFromTradeItemElement(HtmlNode tradeItemNode)
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

        return link.ToLower() switch
        {
            var s when s.StartsWith("/items/bodies/") => TradeItemType.Body,
            var s when s.StartsWith("/items/decals/") => TradeItemType.Decal,
            var s when s.StartsWith("/items/paints/") => TradeItemType.PaintFinish,
            var s when s.StartsWith("/items/wheels/") => TradeItemType.Wheels,
            var s when s.StartsWith("/items/boosts/") => TradeItemType.RocketBoost,
            var s when s.StartsWith("/items/toppers/") => TradeItemType.Topper,
            var s when s.StartsWith("/items/antennas/") => TradeItemType.Antenna,
            var s when s.StartsWith("/items/explosions/") => TradeItemType.GoalExplosion,
            var s when s.StartsWith("/items/trails/") => TradeItemType.Trail,
            var s when s.StartsWith("/items/banners/") => TradeItemType.Banner,
            var s when s.StartsWith("/items/borders/") => TradeItemType.AvatarBorder,
            _ => TradeItemType.Unknown
        };
    }

    private bool ContainsOnlyOneForOneTypeOfOffers(HtmlNode tradeNode)
    {
        // Skip trades that are not 1 for 1 type of trades. Those are difficult to analyze
        // and usually are not specific trades, but those that expect offers for listed items.

        var hasItemsElements = GetHasItemsElementsFromTradeElement(tradeNode);
        var wantsItemsElements = GetWantsItemsElementsFromTradeElement(tradeNode);

        return hasItemsElements.Count == wantsItemsElements.Count;
    }

    private bool HasNoItemOrItemTypeOfOffers(HtmlNode tradeNode)
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

    private bool ContainsOnlySupportedItem(TradeOffer offer)
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