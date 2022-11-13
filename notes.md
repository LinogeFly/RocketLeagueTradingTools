# Notes

## To-do

### Core

- Add more unit tests for persistence repository mapping.
- Add a possibility to blacklist traders, so notifications for their trades are not created.

### Web

- Show offer type in notification boxes.
- Implement paging or infinite scroll for notifications.

### Own custom logo

Ideas:

- TT, as for Trading Tools

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
