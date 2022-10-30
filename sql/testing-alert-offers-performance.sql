SELECT o.*
FROM SellOffers o
INNER JOIN Alerts a
ON
	o.Name = a.ItemName
	AND o.Price >= a.PriceFrom
	AND o.Price <= a.PriceTo

	
SELECT * FROM SellOffers
WHERE
	Name = "Fennec"
	
	
UPDATE SellOffers
SET
	ScrapedDate = datetime('now', '-2 hours')
	
	
SELECT sum(cnt) as offers_count FROM (
	SELECT COUNT(1) as cnt FROM BuyOffers
	UNION
	SELECT COUNT(1) as cnt FROM SellOffers
)