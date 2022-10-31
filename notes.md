# Notes

## To-do

### Core

- Filter 1 credit offers.
- Split `IConfiguration` interface into more specialized groups. Them pass them to the classes so they get the only settings they need.
- Add retention policy for offers so they get cleaned away. For example, after 1 day.
- Add retention policy for notifications so they get cleaned away. For example, after 7 days.
- Fix to-dos in the code.

### Web

- When publishing, make sure that appsettings.json doesn't override existing file if not newer.
- "Loading..." text doesn't show up if navigating back to Notifications page from another page.
- Show offer type in notification boxes.

### Bash script

- Add `-s` option for the commands which will open appsettings.json file in the default editor.
- Implement paging or infinite scroll for notifications.

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
