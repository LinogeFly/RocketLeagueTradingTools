# Rocket League Trading Tools

Rocket League Trading Tools (RLTT for short) is a set of tools aiming to help with free market trading in the game of Rocket League.

The idea of this project is to practice coding with certain technologies, frameworks and principles, to have fun doing that, and eventually ending up with a useful tool for Rocket League trading.

## Features

- Scrap latest trade offers from [RL Garage](https://rocket-league.com/trading) website to a local database.
- Create infinite amount of trade offer alerts and get notified when matching trade offers are posted.

## Scraper

Scraper feature is implemented in a form of CLI program. When running, the program continuously scraps trade offers with a certain interval and stores them in a local database.

Scraping interval, retry attempts, data retention rules and other configuration settings can be changed by editing `appsettings.json` file.

To start Scraper CLI program do the following steps:

1. Make sure that Scraper CLI is built (see [Build](#build) section).
2. Open the terminal.
3. Change current directory to `src/RocketLeagueTradingTools.Scraper/bin/Release/net6.0/publish`.
4. Run in the terminal:

```
dotnet RocketLeagueTradingTools.Scraper.dll --environment Production
```

## Database

Scraper CLI program stores the data in SQLite database. The default location of the database is `~/.RocketLeagueTradingTools/RocketLeagueTradingTools.sqlite.db`.

The database has the following tables:

- TradeOffers, stores scraped trade offers.
- Alerts, stores trade offer alerts.
- Notifications, stores notifications for new trade offers that match active trade offer alerts.
- Blacklist, stores blacklisted traders. Notifications won't be created for trades posted by blacklisted traders, even if they match active trade offer alerts.

## Web UI

As of now, the sole purpose of Web UI is to notify about new trade offers that match active trade offer alerts. The use case goes like this:

1. Create alerts for trade offers you are interested in getting notified about. It can be done either in the database directly or using the [API](#api).
2. Start Scraper CLI program and keep it running.
3. Open Web UI and keep it open.
4. Get notified about alert matching trade offers shortly after they are posted.

To open Web UI do the following steps:

1. Make sure that Web UI is built (see [Build](#build) section).
2. Open the terminal.
3. Change current directory to `src/RocketLeagueTradingTools.Web/bin/Release/net6.0/publish`.
4. Run in the terminal:

```
dotnet RocketLeagueTradingTools.Web.dll --urls http://localhost:7070/ --environment Production
```

5. Open <http://localhost:7070/> in your web browser.

Default Web UI configuration settings can be changed by editing `appsettings.json` file.

## API

Things like alerts and blacklist are not editable in Web UI. Those can be managed either directly in the database or using the API.

### Alerts

```
GET http://localhost:7070/api/alerts
POST http://localhost:7070/api/alerts
PUT http://localhost:7070/api/alerts/{id}
PATCH http://localhost:7070/api/alerts/{id}
DELETE http://localhost:7070/api/alerts/{id}
```

### Blacklist

```
GET http://localhost:7070/api/blacklist
POST http://localhost:7070/api/blacklist
DELETE http://localhost:7070/api/blacklist/{id}
```

## Development Setup

### Build

**Important**: Commands in this section should all be run from `src` directory.

To build Scraper CLI run the following command:

```
dotnet publish -c Release RocketLeagueTradingTools.Scraper
```

To build Web UI run the following command:

```
dotnet publish -c Release RocketLeagueTradingTools.Web
```

### Test

To run the tests run the following command from the root directory:

```
dotnet test
```

## RLTT shortcut

There is a shortcut type of program available, called RLTT. It allows to run and configure Scraper CLI and Web UI easier in the terminal.

**Important**: RLTT program works only in Linux.

### Installation

Run the following commands from the root directory:

```
cp scripts/rltt ~/bin/rltt
chmod u+x ~/bin/rltt
```

### Usage

Start scraping:

```
rltt scrap
```

Open scraper configuration in a default text editor.

```
rltt scrap -s
```

Start Web UI application:

```
rltt web
```

Open Web UI configuration in a default text editor:

```
rltt web -s
```

Show the list of available commands and arguments:

```
rltt -h
```