using HtmlAgilityPack;
using HtmlAgilityPack.CssSelectors.NetCore;
using RocketLeagueTradingTools.Core.Application.Contracts;
using RocketLeagueTradingTools.Core.Domain.Entities;
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

            yield return new TradeOffer(GetTradeItemFromTradeItemElement(wantsItemElement))
            {
                SourceId = GetIdFromTradeElement(tradeNode),
                Link = GetLinkFromTradeElement(tradeNode),
                Price = GetPriceFromTradeItemElement(hasItemElement),
                ScrapedDate = dateTime.Now
            };
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

            yield return new TradeOffer(GetTradeItemFromTradeItemElement(hasItemElement))
            {
                SourceId = GetIdFromTradeElement(tradeNode),
                Link = GetLinkFromTradeElement(tradeNode),
                Price = GetPriceFromTradeItemElement(wantsItemElement),
                ScrapedDate = dateTime.Now
            };
        }
    }

    private bool IsNotCreditsTradeItemElement(HtmlNode tradeItemNode)
    {
        var name = GetNameFromTradeItemElement(tradeItemNode);

        if (name.ToLower() != "credits")
            return true;

        return false;
    }

    private string GetIdFromTradeElement(HtmlNode tradeNode)
    {
        return tradeNode.QuerySelector(".rlg-trade__bookmark").Attributes["data-alias"].Value;
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

        log.Error($"Unable to parse item amount from '{quantityValue}' string.");

        return 1;
    }

    private string GetColorFromTradeItemElement(HtmlNode tradeItemNode)
    {
        var paintElement = tradeItemNode.QuerySelector(".rlg-item__paint");
        if (paintElement != null)
            return paintElement.InnerText.Trim();

        return "";
    }

    private string GetCertificationFromTradeItemElement(HtmlNode tradeItemNode)
    {
        var certElement = tradeItemNode.QuerySelector(".rlg-item__text .rlg-item__cert");
        if (certElement != null)
            return certElement.InnerText.Trim();

        return "";
    }

    private static bool ContainsOnlyOneForOneTypeOfOffers(HtmlNode tradeNode)
    {
        // Skip trades that are not 1 for 1 type of trades. Those are difficult to analyze
        // and usually are not specific trades, but those that expect offers for listed items.

        var hasItemsElements = GetHasItemsElementsFromTradeElement(tradeNode);
        var wantsItemsElements = GetWantsItemsElementsFromTradeElement(tradeNode);

        if (hasItemsElements.Count == wantsItemsElements.Count)
            return true;

        return false;
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

        if (offer.Item.Color.ToLower() == ("painted set"))
            return false;

        return true;
    }
}