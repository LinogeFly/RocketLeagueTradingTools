# Notes

## To-do

### Core

- Move `.sln` file to `src` folder.
- Move `StringExtensions` class to a separate project called SharedKernel.
- Scrap item type because some items have the same name and it's impossible to tell them apart. For example, an item called Shattered. There is a goal explosion and a banner called Shattered. Or Reaper. There is a goal explosion as well as wheels called like that.

### Web

- Fix the issue with notifications becoming "new" right after they are marked as seen by clicking "Mark as seen" link.
- Show offer type in notification boxes.
- Implement paging or infinite scroll for notifications.

### ScrapApplication that performs continuous scrapping

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
