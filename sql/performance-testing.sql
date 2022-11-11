SELECT o.*
FROM SellOffers o
INNER JOIN Alerts a
ON
	o.Name = a.ItemName
	AND o.Price >= a.PriceFrom
	AND o.Price <= a.PriceTo

	
UPDATE SellOffers
SET
	ScrapedDate = datetime('now')
WHERE
	Name = "Hellfire"
	
	
UPDATE Alerts
SET
	Enabled = "No"
WHERE
	ItemName = "Fennec"
	
	
DELETE FROM SellOffers
WHERE
	Name = "Dingo"
	
	
DELETE FROM Alerts
WHERE
	Name = "Dingo"
	
	
SELECT sum(cnt) as offers_count FROM (
	SELECT COUNT(1) as cnt FROM BuyOffers
	UNION
	SELECT COUNT(1) as cnt FROM SellOffers
)
