using FluentAssertions;
using Moq;
using RocketLeagueTradingTools.Core.Application;
using RocketLeagueTradingTools.Core.Application.Contracts;
using RocketLeagueTradingTools.Core.Domain.Entities;
using RocketLeagueTradingTools.Core.Domain.Exceptions;

namespace RocketLeagueTradingTools.Core.UnitTests;

[TestFixture]
public class ScrapApplicationTests
{
    private ScrapApplication sut;
    private CancellationToken cancellationToken;
    private Mock<IConfiguration> config;
    private Mock<ILog> log;
    private Mock<ITradeOfferRepository> repository;
    private Mock<IPersistenceRepository> persistence;

    [SetUp]
    public void Setup()
    {
        cancellationToken = new CancellationToken();
        config = new Mock<IConfiguration>();
        log = new Mock<ILog>();
        repository = new Mock<ITradeOfferRepository>();
        persistence = new Mock<IPersistenceRepository>();

        sut = new ScrapApplication(repository.Object, persistence.Object, log.Object, config.Object);
    }

    [Test]
    public async Task GetTradeOffersPage_should_fail_after_number_of_unsuccessful_downloading_attempts()
    {
        config.Setup(c => c.ScrapRetryMaxAttempts).Returns(3);
        repository.Setup(r => r.GetTradeOffersPage(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException());

        // Attempt 1
        sut = new ScrapApplication(repository.Object, persistence.Object, log.Object, config.Object);
        await sut.ScrapPageAsync(cancellationToken);
        // Attempt 2
        sut = new ScrapApplication(repository.Object, persistence.Object, log.Object, config.Object);
        await sut.ScrapPageAsync(cancellationToken);
        // Attempt 3
        var act = async () =>
        {
            sut = new ScrapApplication(repository.Object, persistence.Object, log.Object, config.Object);
            await sut.ScrapPageAsync(cancellationToken);
        };

        await act.Should().ThrowAsync<PageScrapFailedAfterNumberOfRetriesException>();
    }

    [Test]
    public async Task GetTradeOffersPage_should_filter_away_buy_offers_from_previous_page_scrap()
    {
        var firstOffer = new TradeOffer
        {
            SourceId = "1",
            Link = "https://rocket-league.com/trade/1",
            Price = 300,
            Item = new TradeItem("Fennec")
        };

        var secondOffer = new TradeOffer
        {
            SourceId = "2",
            Link = "https://rocket-league.com/trade/2",
            Price = 100,
            Item = new TradeItem("Hellfire")
        };

        config.Setup(c => c.ScrapRetryMaxAttempts).Returns(3);
        repository.SetupSequence(r => r.GetTradeOffersPage(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TradeOffersPage { BuyOffers = new[] { firstOffer } })
            .ReturnsAsync(new TradeOffersPage { BuyOffers = new[] { firstOffer, secondOffer } });

        // Scrap 1
        sut = new ScrapApplication(repository.Object, persistence.Object, log.Object, config.Object);
        await sut.ScrapPageAsync(cancellationToken);
        // Scrap 2
        sut = new ScrapApplication(repository.Object, persistence.Object, log.Object, config.Object);
        await sut.ScrapPageAsync(cancellationToken);

        persistence.Verify(n => n.AddBuyOffers(new[] { firstOffer }), Times.Once);
        persistence.Verify(n => n.AddBuyOffers(new[] { secondOffer }), Times.Once);
    }

    [Test]
    public async Task GetTradeOffersPage_should_filter_away_sell_offers_from_previous_page_scrap()
    {
        var firstOffer = new TradeOffer
        {
            SourceId = "1",
            Link = "https://rocket-league.com/trade/1",
            Price = 350,
            Item = new TradeItem("Fennec")
        };

        var secondOffer = new TradeOffer
        {
            SourceId = "2",
            Link = "https://rocket-league.com/trade/2",
            Price = 150,
            Item = new TradeItem("Hellfire")
        };

        config.Setup(c => c.ScrapRetryMaxAttempts).Returns(3);
        repository.SetupSequence(r => r.GetTradeOffersPage(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TradeOffersPage { SellOffers = new[] { firstOffer } })
            .ReturnsAsync(new TradeOffersPage { SellOffers = new[] { firstOffer, secondOffer } });

        // Scrap 1
        sut = new ScrapApplication(repository.Object, persistence.Object, log.Object, config.Object);
        await sut.ScrapPageAsync(cancellationToken);
        // Scrap 2
        sut = new ScrapApplication(repository.Object, persistence.Object, log.Object, config.Object);
        await sut.ScrapPageAsync(cancellationToken);

        persistence.Verify(n => n.AddSellOffers(new[] { firstOffer }), Times.Once);
        persistence.Verify(n => n.AddSellOffers(new[] { secondOffer }), Times.Once);
    }

    [Test]
    public async Task GetTradeOffersPage_should_warn_if_there_is_no_offers_overlap_between_latest_and_previous_scraps()
    {
        config.Setup(c => c.ScrapRetryMaxAttempts).Returns(3);
        repository.SetupSequence(r => r.GetTradeOffersPage(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TradeOffersPage
            {
                BuyOffers = new[]
                {
                    new TradeOffer
                    {
                        SourceId = "1",
                        Link = "https://rocket-league.com/trade/1",
                        Price = 300,
                        Item = new TradeItem("Fennec")
                    },
                },
                SellOffers = new[]
                {
                    new TradeOffer
                    {
                        SourceId = "2",
                        Link = "https://rocket-league.com/trade/2",
                        Price = 150,
                        Item = new TradeItem("Hellfire")
                    },
                }
            })
            .ReturnsAsync(new TradeOffersPage
            {
                BuyOffers = new[]
                {
                    new TradeOffer
                    {
                        SourceId = "3",
                        Link = "https://rocket-league.com/trade/3",
                        Price = 500,
                        Item = new TradeItem("Dueling Dragons")
                    },
                },
                SellOffers = new[]
                {
                    new TradeOffer
                    {
                        SourceId = "4",
                        Link = "https://rocket-league.com/trade/4",
                        Price = 100,
                        Item = new TradeItem("Supernova III")
                    },
                }
            });

        // Scrap 1
        sut = new ScrapApplication(repository.Object, persistence.Object, log.Object, config.Object);
        await sut.ScrapPageAsync(cancellationToken);
        // Scrap 2
        sut = new ScrapApplication(repository.Object, persistence.Object, log.Object, config.Object);
        await sut.ScrapPageAsync(cancellationToken);

        log.Verify(logger => logger.Warn(It.Is<string>(m => m.StartsWith("No offers overlap"))), Times.Once);
    }

    [Test]
    public async Task GetTradeOffersPage_should_not_warn_if_there_is_offers_overlap_between_latest_and_previous_scraps()
    {
        config.Setup(c => c.ScrapRetryMaxAttempts).Returns(3);
        repository.SetupSequence(r => r.GetTradeOffersPage(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TradeOffersPage
            {
                BuyOffers = new[]
                {
                    new TradeOffer
                    {
                        SourceId = "1",
                        Link = "https://rocket-league.com/trade/1",
                        Price = 300,
                        Item = new TradeItem("Fennec")
                    },
                },
                SellOffers = new[]
                {
                    new TradeOffer
                    {
                        SourceId = "2",
                        Link = "https://rocket-league.com/trade/2",
                        Price = 150,
                        Item = new TradeItem("Hellfire")
                    },
                }
            })
            .ReturnsAsync(new TradeOffersPage
            {
                BuyOffers = new[]
                {
                    new TradeOffer
                    {
                        SourceId = "1",
                        Link = "https://rocket-league.com/trade/1",
                        Price = 300,
                        Item = new TradeItem("Fennec")
                    },
                },
                SellOffers = new[]
                {
                    new TradeOffer
                    {
                        SourceId = "2",
                        Link = "https://rocket-league.com/trade/2",
                        Price = 150,
                        Item = new TradeItem("Hellfire")
                    },
                }
            });

        // Scrap 1
        sut = new ScrapApplication(repository.Object, persistence.Object, log.Object, config.Object);
        await sut.ScrapPageAsync(cancellationToken);
        // Scrap 2
        sut = new ScrapApplication(repository.Object, persistence.Object, log.Object, config.Object);
        await sut.ScrapPageAsync(cancellationToken);

        log.Verify(logger => logger.Warn(It.Is<string>(m => m.StartsWith("No offers overlap"))), Times.Never);
    }

    [Test]
    public async Task GetTradeOffersPage_should_not_warn_if_there_is_buy_offers_overlap_but_no_sell_offers_overlap_between_latest_and_previous_scraps()
    {
        config.Setup(c => c.ScrapRetryMaxAttempts).Returns(3);
        repository.SetupSequence(r => r.GetTradeOffersPage(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TradeOffersPage
            {
                BuyOffers = new[]
                {
                    new TradeOffer
                    {
                        SourceId = "1",
                        Link = "https://rocket-league.com/trade/1",
                        Price = 300,
                        Item = new TradeItem("Fennec")
                    },
                },
                SellOffers = new[]
                {
                    new TradeOffer
                    {
                        SourceId = "2",
                        Link = "https://rocket-league.com/trade/2",
                        Price = 150,
                        Item = new TradeItem("Hellfire")
                    },
                }
            })
            .ReturnsAsync(new TradeOffersPage
            {
                BuyOffers = new[]
                {
                    new TradeOffer
                    {
                        SourceId = "1",
                        Link = "https://rocket-league.com/trade/1",
                        Price = 300,
                        Item = new TradeItem("Fennec")
                    },
                },
                SellOffers = new[]
                {
                    new TradeOffer
                    {
                        SourceId = "3",
                        Link = "https://rocket-league.com/trade/3",
                        Price = 100,
                        Item = new TradeItem("Supernova III")
                    },
                }
            });

        // Scrap 1
        sut = new ScrapApplication(repository.Object, persistence.Object, log.Object, config.Object);
        await sut.ScrapPageAsync(cancellationToken);
        // Scrap 2
        sut = new ScrapApplication(repository.Object, persistence.Object, log.Object, config.Object);
        await sut.ScrapPageAsync(cancellationToken);

        log.Verify(logger => logger.Warn(It.Is<string>(m => m.StartsWith("No offers overlap"))), Times.Never);
    }

    [Test]
    public async Task GetTradeOffersPage_should_not_warn_if_there_is_sell_offers_overlap_but_no_buy_offers_overlap_between_latest_and_previous_scraps()
    {
        config.Setup(c => c.ScrapRetryMaxAttempts).Returns(3);
        repository.SetupSequence(r => r.GetTradeOffersPage(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TradeOffersPage
            {
                BuyOffers = new[]
                {
                    new TradeOffer
                    {
                        SourceId = "1",
                        Link = "https://rocket-league.com/trade/1",
                        Price = 300,
                        Item = new TradeItem("Fennec")
                    },
                },
                SellOffers = new[]
                {
                    new TradeOffer
                    {
                        SourceId = "2",
                        Link = "https://rocket-league.com/trade/2",
                        Price = 150,
                        Item = new TradeItem("Hellfire")
                    },
                }
            })
            .ReturnsAsync(new TradeOffersPage
            {
                BuyOffers = new[]
                {
                    new TradeOffer
                    {
                        SourceId = "3",
                        Link = "https://rocket-league.com/trade/3",
                        Price = 500,
                        Item = new TradeItem("Dueling Dragons")
                    },
                },
                SellOffers = new[]
                {
                    new TradeOffer
                    {
                        SourceId = "2",
                        Link = "https://rocket-league.com/trade/2",
                        Price = 150,
                        Item = new TradeItem("Hellfire")
                    },
                }
            });

        // Scrap 1
        sut = new ScrapApplication(repository.Object, persistence.Object, log.Object, config.Object);
        await sut.ScrapPageAsync(cancellationToken);
        // Scrap 2
        sut = new ScrapApplication(repository.Object, persistence.Object, log.Object, config.Object);
        await sut.ScrapPageAsync(cancellationToken);

        log.Verify(logger => logger.Warn(It.Is<string>(m => m.StartsWith("No offers overlap"))), Times.Never);
    }
}