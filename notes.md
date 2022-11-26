# Notes

## To-do

### Core

- Add more unit tests for persistence repository mapping.
- Split `PersistenceRepository` into smaller, application specific repositories.
- Add DI for tests. Wrap the DI container into `Given` and `Then` classes. Check MyPages for inspiration.
  Check `TestContainer.cs` in SM project as well.
- Implement incremental notifications refresh, instead of doing full refresh every time.

### Web

- Remove AutoMapper usage.
- Move API classes to a separate namespace called Contracts with Requests and Responses child namespaces. Use record
  type for API model, instead of classes.
- Implement paging or infinite scroll for notifications.

### Projects to check out for inspiration/learning

Things to check:

- Tests naming convention and structure

Links:

- (Most starred repositories)[https://github.com/search?l=C%23&o=desc&q=stars%3A%3E0&s=stars&type=Repositories]
- [https://github.com/jellyfin/jellyfin]

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
