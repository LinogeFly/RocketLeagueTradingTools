SELECT sum(cnt) as offers_count FROM (
	SELECT COUNT(1) as cnt FROM BuyOffers
	UNION
	SELECT COUNT(1) as cnt FROM SellOffers
)


SELECT * FROM Notifications
ORDER BY TradeOfferScrapedDate DESC


SELECT * FROM Alerts


SELECT COUNT(1) as cnt FROM BuyOffers
WHERE ScrapedDate < datetime('now', '-5 days')

SELECT COUNT(1) as cnt FROM SellOffers
WHERE ScrapedDate < datetime('now', '-5 days')