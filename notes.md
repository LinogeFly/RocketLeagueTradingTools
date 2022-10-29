# Notes

## To-do

### Core

- Address the performance issue coming from making one SQL query for every alert against BuyOffers/SellOffers tables. Instead of making multiple queries try to have one query against BuyOffers/SellOffers table and join Alerts table by all the filtering columns (Price, Color etc).
- Do more performance testing to see how fast/slow matching alert offers are found. Make sure database indexes are in place. For example, add for Alerts.Disabled, remove for TradeOffers.SourceId etc.
- Replace static fields in ScrapApplication with `ISessionStorage` singleton service. Make sure all tests in `ScrapApplicationTests` are passing when run all at once, not one at a time.
- Split `IConfiguration` interface into more specialized groups. Them pass them to the classes so they get the only settings they need.
- Add retention policy logic for offers so get cleaned away. For example, after 3 days.
- Fix to-dos in the code.

### Web

- Show offer type in notification boxes.

### Bash script

- Add `-s` option for the commands which will open appsettings.json file in the default editor.

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
