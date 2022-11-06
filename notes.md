# Notes

## To-do

### Core

- Move `.sln` file to `src` folder.
- Make the database name configurable per environment.
- Add a constructor to `Alert` class with all class required properties. Those are `ItemName`, `OfferType` and `Price`.
- Move `StringExtensions` class to a separate project called SharedKernel.
- Add a possibility to create alerts for painted only items.
- Add more unit tests for persistance repository mapping.
- Add a possibility to blacklist traders, so notifications for their trades are not created.

### Web

- Add model validation for the API requests.
- Fix the issue with notifications becoming "new" right after they are marked as seen by clicking "Mark as seen" link.
- Show offer type in notification boxes.
- Implement paging or infinite scroll for notifications.

### Own custom logo

Ideas:

- TT, as for Trading Tools

### ScrapApplication that performs continuous scrapping

- Prototype DbContext factory approach. For example, <https://github.com/vany0114/EF.DbContextFactory>
- Move `ScrapRetryMaxAttempts`, `ScrapIntervalMin` and `ScrapIntervalMax` configurations here.

### Keep the following concept naming in the solution
  
- Trade item
- Trade offer
- Alert (configuration rules)
- Alert notification (in the database)
- Notification pop-up (in the browser)

## Known issues

## Technical notes

To create migrations, run this command from Scraper project folder:

```bash
dotnet ef migrations add InitialCreate --project ../RocketLeagueTradingTools.Infrastructure --output-dir "Persistence/Migrations"
```
