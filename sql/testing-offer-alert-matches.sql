SELECT o.*
FROM SellOffers o
INNER JOIN Alerts a
ON
	o.Name = a.ItemName
	AND o.Color = a.Color
	AND o.Price >= a.PriceFrom
	AND o.Price <= a.PriceTo

	
SELECT * FROM SellOffers
WHERE
	Name = "Fennec"