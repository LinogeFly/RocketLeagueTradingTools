# Notes

## To-do

### Core

- Replace static fields in ScrapApplication with `ISessionStorage` singleton service. Make sure all tests in `ScrapApplicationTests` are passing when run all at once, not one at a time.
- Split `IConfiguration` interface into more specialized groups. Them pass them to the classes so they get the only settings they need.
- Add retention policy for offers so they get cleaned away. For example, after 1 day.
- Add retention policy for notifications so they get cleaned away. For example, after 7 days.
- Allow to configure all time related settings using human readable format. For example, "2 hours" or "7 days".
- Fix to-dos in the code.

### Web

- When publishing, make sure that appsettings.json doesn't override existing file if not newer.
- Add "(X)" prefix to the page title, where X is a number of new unseen notifications.
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
