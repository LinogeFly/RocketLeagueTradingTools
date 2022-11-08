-- Trending items with price range and price differences
SELECT Name, Color, ItemType, count(1) [Total], min(Price) || '-' || max(price) PriceRange, max(Price) - min(Price) PriceDif
FROM (
	-- Unique offers of all items
	SELECT
		Name, ItemType, Color, Price, ItemType, SourceId
	FROM SellOffers
	WHERE
		ScrapedDate > datetime('now', '-3 days')
	GROUP BY
		Name, ItemType, Color, Price, ItemType, SourceId
) ofr
GROUP BY
	Name, Color, ItemType
ORDER BY [Total] DESC
LIMIT 100


-- Unique offers of a certain item
SELECT
	Name, Color, Price, ItemType, SourceId
FROM SellOffers
WHERE
	ScrapedDate > datetime('now', '-5 days')
	AND Name = "Nomster"
	AND Color = "Lime"
	AND ItemType = "Goal Explosion"
GROUP BY
	Name, Color, Price, ItemType, SourceId
ORDER BY
	Price

	
-- Total amount of buy and sell offers
SELECT sum(cnt) as offers_count FROM (
	SELECT COUNT(1) as cnt FROM BuyOffers
	UNION
	SELECT COUNT(1) as cnt FROM SellOffers
)