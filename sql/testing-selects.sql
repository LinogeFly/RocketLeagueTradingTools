SELECT sum(cnt) as offers_count FROM (
	SELECT COUNT(1) as cnt FROM BuyOffers
	UNION
	SELECT COUNT(1) as cnt FROM SellOffers
)


SELECT * FROM Notifications
ORDER BY TradeOfferScrapedDate DESC


SELECT * FROM Alerts