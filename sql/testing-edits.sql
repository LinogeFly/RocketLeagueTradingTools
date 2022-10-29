DELETE FROM BuyOffers; DELETE FROM SellOffers


DELETE FROM Notifications
WHERE
	Id = 157

	
UPDATE Notifications
SET
	SeenDate = NULL
WHERE
	Id = 83
	
UPDATE Notifications
SET
	SeenDate = datetime('now')
WHERE
	Id = 83

	
INSERT INTO Notifications (TradeItemName, TradeItemColor, TradeItemCertification, TradeOfferPrice, TradeOfferSourceId, TradeOfferLink, TradeOfferScrapedDate, CreatedDate)
VALUES ("Sub-Zero", "", "", 50, "1d41df6e-f728-4c50-9291-782ac53e1915", "https://rocket-league.com/trade/1d41df6e-f728-4c50-9291-782ac53e1915", datetime('now'), datetime('now'))


DELETE FROM Alerts