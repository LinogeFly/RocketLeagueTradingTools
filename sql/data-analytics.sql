-- Popular items with price range and price differences
SELECT Name, Color, count(1) cnt, min(Price) || '-' || max(price) rng, max(Price) - min(Price) dif
FROM (
	-- Unique offers of all items
	SELECT
		Name, Color, Price, SourceId
	FROM BuyOffers
	WHERE
		ScrapedDate > datetime('now', '-2 days')
	GROUP BY
		Name, Color, SourceId, Price
) ofr
GROUP BY
	Name, Color
ORDER BY cnt DESC
LIMIT 50



-- Unique offers of a certain item
SELECT
	Name, Color, Price, SourceId
FROM BuyOffers
WHERE
	ScrapedDate > datetime('now', '-7 days')
	AND Name = "Fennec"
	AND Color = ""
GROUP BY
	Name, Color, SourceId, Price
ORDER BY
	Price
