-- Trending items
SELECT ItemName, Color, ItemType, count(1) [Total], min(Price) || '-' || max(price) PriceRange, max(Price) - min(Price) PriceDif
FROM (
	-- Unique offers of all items
	SELECT
		ItemName, ItemType, Color, Price, ItemType, Link
	FROM TradeOffers
	WHERE
		ScrapedDate > datetime('now', '-3 days')
		AND OfferType = "Sell"
	GROUP BY
		ItemName, ItemType, Color, Price, ItemType, Link
) ofr
GROUP BY
	ItemName, Color, ItemType
ORDER BY [Total] DESC
LIMIT 100


-- Offers summary of a certain item
SELECT Price, count(1) Amount
FROM (
	-- Unique offers of a certain item
	SELECT
		ItemName, Color, Price, OfferType, ItemType, Link
	FROM TradeOffers
	WHERE
		ScrapedDate > datetime('now', '-3 days')
		AND OfferType = "Sell"
		AND ItemName = "Hellfire"
		AND Color = ""
	GROUP BY
		ItemName, Color, Price, OfferType, ItemType, Link
	ORDER BY
		Price
)
GROUP BY
	Price
ORDER BY
	Price

	
-- Unique offers of a certain item
SELECT
	ItemName, Color, Price, OfferType, ItemType, Link
FROM TradeOffers
WHERE
	ScrapedDate > datetime('now', '-3 days')
	AND OfferType = "Sell"
	AND ItemName = "Hellfire"
	AND Color = ""
GROUP BY
	ItemName, Color, Price, OfferType, ItemType, Link
ORDER BY
	Price

	
-- Total amount of buy and sell offers
SELECT COUNT(1) as cnt FROM TradeOffers
