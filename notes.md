# Notes

## To-do

## Known issues

- Console app doesn't terminate after number of failed scraping attempts, unless "Q" is pressed.

## Technical notes

To create migrations, run this command from Scraper project folder:

```bash
dotnet ef migrations add InitialCreate --project ../RocketLeagueTradingTools.Infrastructure --output-dir "Persistence/SQLite/Migrations"
```

## Projects structure

```
/src
	RocketLeagueTradingTools.Core
	RocketLeagueTradingTools.Infrastructure
	RocketLeagueTradingTools.Scraper (Console app)
	RocketLeagueTradingTools.UI (Web app with alerts)
/tests/
	RocketLeagueTradingTools.Infrastructure.UnitTests
		RlgDataSource (parsing, filtering)
	RocketLeagueTradingTools.Infrastructure.IntegrationTests
		RlgDataSource (Scrap one page from RLG and see that at least one row ends up in the database)
	RocketLeagueTradingTools.Core.UnitTests
		TradeOfferApplication (retry attempts)
		TradeOfferApplication (not adding duplicates)
/tests/
	RocketLeagueTradingTools.UnitTests
		/Core
			TradeOfferApplicationTests
		/Infrastructure
			/RlgDataSource
				RlgDataSourceTests
				/RlgPageBuilder
	RocketLeagueTradingTools.IntegrationTests
		/Infrastructure
			RlgDataSourceTests
```

## Architecture

```
Domain
	/Entities
		TradeOffer
			string Link
			string SourceId
			Item From
				Name
				Type (item, credits)
				Attributes (color, certificate)
			Item To
	/ValueObjects
	/Exceptions


Application
	TradeOfferApplication
		ScrapPage()
		Search()
		
	/Contracts
		IPersistance
			UpsertTradeOffer()
		IScraper
			ScrapPage()
		


Intfrastructure
	/Persistance
		/Types

		SQLitePersistance: IPersistance
	/Scrapers
		RlgScraper: IScraper


Console
	while (true)
	{
		TradeOfferApplication.ScrapPage();
		Task.Delay(10000);
	}
```
