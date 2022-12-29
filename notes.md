# Notes

## Known issues

## To-do

### Core

- Notifications refresh should not be done every time notifications are requested with GET action. Instead, the refresh
  should be performed in a background running  task. <https://learn.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services>
  could be a way to implement this.

### Web

- Add test for Web API controllers.
- Add React tests for Notifications page component. There is plenty scenarios to test, will be a good exercise.

### Refactoring

#### Tasks

- [x] Take `MappingException` class into use.
- [x] Replace contract class types with records.
- [x] Split `PersistenceRepository` into smaller, application specific repositories.
- [x] Add more unit tests for persistence repository mapping.
- [x] Add `BlacklistedTrader` entity with id and return objects of it with BlacklistController GET action.

#### Things I want to be tested

- API controllers (Web)
  - Mapping DTO => domain
  - Mapping domain => DTO
  - Business logic. For example, refresh notifications before returning them.
- Persistence repositories (Infrastructure)
  - Mapping persistence => domain
  - Mapping domain => persistence
  - Business logic. For example, find matching offers query takes all required filtering properties into account.
- Trade offer repository (Infrastructure)
  - Parsing and maping to domain model
- Applications (Core)
  - Scrap. Re-trying rules.
  - DataRetention. Make sure we delete notifications before we delete offers.
  - Notification. Same notifications are not created again when refreshing.

#### Unit of testing questions

- How to test persistence repository layer? Mock the database or not?
- How to test application layer? Mock repositories or not?
- How to test API layer? Mock application layer or not?
 
## Projects to check out for inspiration/learning

Things to check:

- Tests naming convention and structure
- Web API contracts naming convention

Links:

- [Most starred repositories](https://github.com/search?l=C%23&o=desc&q=stars%3A%3E0&s=stars&type=Repositories)
- <https://github.com/jellyfin/jellyfin>
- [DDD examples](https://github.com/topics/ddd-example?l=c%23)

## Own custom logo

Ideas:

- TT, as for Trading Tools

## Keep the following concept naming in the solution

- Trade item
- Trade offer
- Alert (configuration rules)
- Alert notification (in the database)
- Notification pop-up (in the browser)

## Technical notes

To create migrations, run this command from Scraper project folder:

```bash
dotnet ef migrations add InitialCreate --project ../RocketLeagueTradingTools.Infrastructure --output-dir "Persistence/Migrations"
```
