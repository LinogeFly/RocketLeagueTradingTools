using FluentAssertions;
using Moq;
using RocketLeagueTradingTools.Core.Application.Scraping;
using RocketLeagueTradingTools.Core.Application.Common;
using RocketLeagueTradingTools.Core.Application.Interfaces;
using RocketLeagueTradingTools.Core.Domain.Entities;
using RocketLeagueTradingTools.Core.Domain.Exceptions;
using RocketLeagueTradingTools.Core.UnitTests.Support;

namespace RocketLeagueTradingTools.Core.UnitTests;

[TestFixture]
public class ScrapApplicationTests
{
    private ScrapApplication sut = null!;
    private CancellationToken cancellationToken;
    private Mock<IScrapApplicationSettings> config = null!;
    private Mock<ILog> log = null!;
    private Mock<ITradeOfferRepository> repository = null!;
    private Mock<IPersistenceRepository> persistence = null!;

    [SetUp]
    public void Setup()
    {
        cancellationToken = new CancellationToken();
        config = new Mock<IScrapApplicationSettings>();
        log = new Mock<ILog>();
        repository = new Mock<ITradeOfferRepository>();
        persistence = new Mock<IPersistenceRepository>();
    }

    [Test]
    public async Task GetTradeOffersPage_should_fail_after_number_of_unsuccessful_downloading_attempts()
    {
        var session = new SessionStorage.ScrapApplication();

        config.Setup(c => c.ScrapRetryMaxAttempts).Returns(3);
        repository.Setup(r => r.GetTradeOffersPage(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException());

        // Attempt 1
        sut = new SutBuilder(repository.Object, persistence.Object, log.Object, config.Object).Add(session).Build();
        await sut.ScrapPageAsync(cancellationToken);
        // Attempt 2
        sut = new SutBuilder(repository.Object, persistence.Object, log.Object, config.Object).Add(session).Build();
        await sut.ScrapPageAsync(cancellationToken);
        // Attempt 3
        var act = async () =>
        {
            sut = new SutBuilder(repository.Object, persistence.Object, log.Object, config.Object).Add(session).Build();
            await sut.ScrapPageAsync(cancellationToken);
        };

        await act.Should().ThrowAsync<PageScrapFailedAfterNumberOfRetriesException>();
    }

    [Test]
    public async Task GetTradeOffersPage_should_not_persist_buy_offers_from_previous_page_scrap()
    {
        var session = new SessionStorage.ScrapApplication();
        var firstOffer = new TradeOfferBuilder()
            .WithTradeItem(Build.TradeItem("Fennec"))
            .WithRlgId("1")
            .WithPrice(300)
            .Build();
        var secondOffer = new TradeOfferBuilder()
            .WithTradeItem(Build.TradeItem("Hellfire"))
            .WithRlgId("2")
            .WithPrice(100)
            .Build();

        config.Setup(c => c.ScrapRetryMaxAttempts).Returns(3);
        repository.SetupSequence(r => r.GetTradeOffersPage(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TradeOffersPage { BuyOffers = new[] { firstOffer } })
            .ReturnsAsync(new TradeOffersPage { BuyOffers = new[] { firstOffer, secondOffer } });

        // Scrap 1
        sut = new SutBuilder(repository.Object, persistence.Object, log.Object, config.Object).Add(session).Build();
        await sut.ScrapPageAsync(cancellationToken);
        // Scrap 2
        sut = new SutBuilder(repository.Object, persistence.Object, log.Object, config.Object).Add(session).Build();
        await sut.ScrapPageAsync(cancellationToken);

        persistence.Verify(p => p.AddBuyOffers(new[] { firstOffer }), Times.Once);
        persistence.Verify(p => p.AddBuyOffers(new[] { secondOffer }), Times.Once);
    }

    [Test]
    public async Task GetTradeOffersPage_should_not_persist_sell_offers_from_previous_page_scrap()
    {
        var session = new SessionStorage.ScrapApplication();
        var firstOffer = new TradeOfferBuilder()
            .WithTradeItem(Build.TradeItem("Fennec"))
            .WithRlgId("1")
            .WithPrice(350)
            .Build();
        var secondOffer = new TradeOfferBuilder()
            .WithTradeItem(Build.TradeItem("Hellfire"))
            .WithRlgId("2")
            .WithPrice(150)
            .Build();

        config.Setup(c => c.ScrapRetryMaxAttempts).Returns(3);
        repository.SetupSequence(r => r.GetTradeOffersPage(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TradeOffersPage { SellOffers = new[] { firstOffer } })
            .ReturnsAsync(new TradeOffersPage { SellOffers = new[] { firstOffer, secondOffer } });

        // Scrap 1
        sut = new SutBuilder(repository.Object, persistence.Object, log.Object, config.Object).Add(session).Build();
        await sut.ScrapPageAsync(cancellationToken);
        // Scrap 2
        sut = new SutBuilder(repository.Object, persistence.Object, log.Object, config.Object).Add(session).Build();
        await sut.ScrapPageAsync(cancellationToken);

        persistence.Verify(p => p.AddSellOffers(new[] { firstOffer }), Times.Once);
        persistence.Verify(p => p.AddSellOffers(new[] { secondOffer }), Times.Once);
    }

    [Test]
    public async Task GetTradeOffersPage_should_warn_if_there_is_no_offers_overlap_between_latest_and_previous_scraps()
    {
        var session = new SessionStorage.ScrapApplication();
        var firstScrapDate = new DateTime(2022, 1, 1);
        var secondScrapDate = firstScrapDate.AddSeconds(10);

        config.Setup(c => c.ScrapRetryMaxAttempts).Returns(3);
        repository.SetupSequence(r => r.GetTradeOffersPage(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TradeOffersPage
            {
                BuyOffers = new[]
                {
                    new TradeOfferBuilder()
                        .WithTradeItem(Build.TradeItem("Fennec"))
                        .WithRlgId("1")
                        .WithPrice(300)
                        .WithScrapedDate(firstScrapDate)
                        .Build()
                },
                SellOffers = new[]
                {
                    new TradeOfferBuilder()
                        .WithTradeItem(Build.TradeItem("Hellfire"))
                        .WithRlgId("1")
                        .WithPrice(150)
                        .WithScrapedDate(firstScrapDate)
                        .Build()
                }
            })
            .ReturnsAsync(new TradeOffersPage
            {
                BuyOffers = new[]
                {
                    new TradeOfferBuilder()
                        .WithTradeItem(Build.TradeItem("Dueling Dragons"))
                        .WithRlgId("3")
                        .WithPrice(500)
                        .WithScrapedDate(secondScrapDate)
                        .Build()
                },
                SellOffers = new[]
                {
                    new TradeOfferBuilder()
                        .WithTradeItem(Build.TradeItem("Supernova III"))
                        .WithRlgId("4")
                        .WithPrice(100)
                        .WithScrapedDate(secondScrapDate)
                        .Build()
                }
            });

        // Scrap 1
        sut = new SutBuilder(repository.Object, persistence.Object, log.Object, config.Object).Add(session).Build();
        await sut.ScrapPageAsync(cancellationToken);
        // Scrap 2
        sut = new SutBuilder(repository.Object, persistence.Object, log.Object, config.Object).Add(session).Build();
        await sut.ScrapPageAsync(cancellationToken);

        log.Verify(logger => logger.Warn(It.Is<string>(m => m.StartsWith("No offers overlap"))), Times.Once);
    }

    [Test]
    public async Task GetTradeOffersPage_should_not_warn_if_there_is_offers_overlap_between_latest_and_previous_scraps()
    {
        var session = new SessionStorage.ScrapApplication();
        var firstScrapDate = new DateTime(2022, 1, 1);
        var secondScrapDate = firstScrapDate.AddSeconds(10);

        config.Setup(c => c.ScrapRetryMaxAttempts).Returns(3);
        repository.SetupSequence(r => r.GetTradeOffersPage(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TradeOffersPage
            {
                BuyOffers = new[]
                {
                    new TradeOfferBuilder()
                        .WithTradeItem(Build.TradeItem("Fennec"))
                        .WithRlgId("1")
                        .WithPrice(300)
                        .WithScrapedDate(firstScrapDate)
                        .Build()
                },
                SellOffers = new[]
                {
                    new TradeOfferBuilder()
                        .WithTradeItem(Build.TradeItem("Hellfire"))
                        .WithRlgId("2")
                        .WithPrice(150)
                        .WithScrapedDate(firstScrapDate)
                        .Build()
                }
            })
            .ReturnsAsync(new TradeOffersPage
            {
                BuyOffers = new[]
                {
                    new TradeOfferBuilder()
                        .WithTradeItem(Build.TradeItem("Fennec"))
                        .WithRlgId("1")
                        .WithPrice(300)
                        .WithScrapedDate(secondScrapDate)
                        .Build()
                },
                SellOffers = new[]
                {
                    new TradeOfferBuilder()
                        .WithTradeItem(Build.TradeItem("Hellfire"))
                        .WithRlgId("2")
                        .WithPrice(150)
                        .WithScrapedDate(secondScrapDate)
                        .Build()
                }
            });

        // Scrap 1
        sut = new SutBuilder(repository.Object, persistence.Object, log.Object, config.Object).Add(session).Build();
        await sut.ScrapPageAsync(cancellationToken);
        // Scrap 2
        sut = new SutBuilder(repository.Object, persistence.Object, log.Object, config.Object).Add(session).Build();
        await sut.ScrapPageAsync(cancellationToken);

        log.Verify(logger => logger.Warn(It.Is<string>(m => m.StartsWith("No offers overlap"))), Times.Never);
    }

    [Test]
    public async Task GetTradeOffersPage_should_not_warn_if_there_is_buy_offers_overlap_but_no_sell_offers_overlap_between_latest_and_previous_scraps()
    {
        var session = new SessionStorage.ScrapApplication();
        var firstScrapDate = new DateTime(2022, 1, 1);
        var secondScrapDate = firstScrapDate.AddSeconds(10);

        config.Setup(c => c.ScrapRetryMaxAttempts).Returns(3);
        repository.SetupSequence(r => r.GetTradeOffersPage(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TradeOffersPage
            {
                BuyOffers = new[]
                {
                    new TradeOfferBuilder()
                        .WithTradeItem(Build.TradeItem("Fennec"))
                        .WithRlgId("1")
                        .WithPrice(300)
                        .WithScrapedDate(firstScrapDate)
                        .Build()
                },
                SellOffers = new[]
                {
                    new TradeOfferBuilder()
                        .WithTradeItem(Build.TradeItem("Hellfire"))
                        .WithRlgId("2")
                        .WithPrice(150)
                        .WithScrapedDate(firstScrapDate)
                        .Build()
                }
            })
            .ReturnsAsync(new TradeOffersPage
            {
                BuyOffers = new[]
                {
                    new TradeOfferBuilder()
                        .WithTradeItem(Build.TradeItem("Fennec"))
                        .WithRlgId("1")
                        .WithPrice(300)
                        .WithScrapedDate(secondScrapDate)
                        .Build()
                },
                SellOffers = new[]
                {
                    new TradeOfferBuilder()
                        .WithTradeItem(Build.TradeItem("Supernova III"))
                        .WithRlgId("3")
                        .WithPrice(100)
                        .WithScrapedDate(secondScrapDate)
                        .Build()
                }
            });

        // Scrap 1
        sut = new SutBuilder(repository.Object, persistence.Object, log.Object, config.Object).Add(session).Build();
        await sut.ScrapPageAsync(cancellationToken);
        // Scrap 2
        sut = new SutBuilder(repository.Object, persistence.Object, log.Object, config.Object).Add(session).Build();
        await sut.ScrapPageAsync(cancellationToken);

        log.Verify(logger => logger.Warn(It.Is<string>(m => m.StartsWith("No offers overlap"))), Times.Never);
    }

    [Test]
    public async Task GetTradeOffersPage_should_not_warn_if_there_is_sell_offers_overlap_but_no_buy_offers_overlap_between_latest_and_previous_scraps()
    {
        var session = new SessionStorage.ScrapApplication();
        var firstScrapDate = new DateTime(2022, 1, 1);
        var secondScrapDate = firstScrapDate.AddSeconds(10);

        config.Setup(c => c.ScrapRetryMaxAttempts).Returns(3);
        repository.SetupSequence(r => r.GetTradeOffersPage(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TradeOffersPage
            {
                BuyOffers = new[]
                {
                    new TradeOfferBuilder()
                        .WithTradeItem(Build.TradeItem("Fennec"))
                        .WithRlgId("1")
                        .WithPrice(300)
                        .WithScrapedDate(firstScrapDate)
                        .Build()
                },
                SellOffers = new[]
                {
                    new TradeOfferBuilder()
                        .WithTradeItem(Build.TradeItem("Hellfire"))
                        .WithRlgId("2")
                        .WithPrice(150)
                        .WithScrapedDate(firstScrapDate)
                        .Build()
                }
            })
            .ReturnsAsync(new TradeOffersPage
            {
                BuyOffers = new[]
                {
                    new TradeOfferBuilder()
                        .WithTradeItem(Build.TradeItem("Dueling Dragons"))
                        .WithRlgId("3")
                        .WithPrice(500)
                        .WithScrapedDate(secondScrapDate)
                        .Build()
                },
                SellOffers = new[]
                {
                    new TradeOfferBuilder()
                        .WithTradeItem(Build.TradeItem("Hellfire"))
                        .WithRlgId("2")
                        .WithPrice(150)
                        .WithScrapedDate(secondScrapDate)
                        .Build()
                }
            });

        // Scrap 1
        sut = new SutBuilder(repository.Object, persistence.Object, log.Object, config.Object).Add(session).Build();
        await sut.ScrapPageAsync(cancellationToken);
        // Scrap 2
        sut = new SutBuilder(repository.Object, persistence.Object, log.Object, config.Object).Add(session).Build();
        await sut.ScrapPageAsync(cancellationToken);

        log.Verify(logger => logger.Warn(It.Is<string>(m => m.StartsWith("No offers overlap"))), Times.Never);
    }

    [Test]
    public async Task GetTradeOffersPage_should_not_warn_if_there_is_no_offers_overlap_but_the_previous_scrap_failed()
    {
        var session = new SessionStorage.ScrapApplication();
        var firstScrapDate = new DateTime(2022, 1, 1);
        var secondScrapDate = firstScrapDate.AddSeconds(10);

        config.Setup(c => c.ScrapRetryMaxAttempts).Returns(3);
        repository.SetupSequence(r => r.GetTradeOffersPage(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TradeOffersPage
            {
                BuyOffers = new[]
                {
                    new TradeOfferBuilder()
                        .WithTradeItem(Build.TradeItem("Fennec"))
                        .WithRlgId("1")
                        .WithPrice(300)
                        .WithScrapedDate(firstScrapDate)
                        .Build()
                },
                SellOffers = new[]
                {
                    new TradeOfferBuilder()
                        .WithTradeItem(Build.TradeItem("Hellfire"))
                        .WithRlgId("2")
                        .WithPrice(150)
                        .WithScrapedDate(firstScrapDate)
                        .Build()
                }
            })
            .ThrowsAsync(new HttpRequestException())
            .ReturnsAsync(new TradeOffersPage
            {
                BuyOffers = new[]
                {
                    new TradeOfferBuilder()
                        .WithTradeItem(Build.TradeItem("Dueling Dragons"))
                        .WithRlgId("3")
                        .WithPrice(500)
                        .WithScrapedDate(secondScrapDate)
                        .Build()
                },
                SellOffers = new[]
                {
                    new TradeOfferBuilder()
                        .WithTradeItem(Build.TradeItem("Supernova III"))
                        .WithRlgId("4")
                        .WithPrice(100)
                        .WithScrapedDate(secondScrapDate)
                        .Build()
                }
            });

        // Scrap 1
        sut = new SutBuilder(repository.Object, persistence.Object, log.Object, config.Object).Add(session).Build();
        await sut.ScrapPageAsync(cancellationToken);
        // Scrap 2
        sut = new SutBuilder(repository.Object, persistence.Object, log.Object, config.Object).Add(session).Build();
        await sut.ScrapPageAsync(cancellationToken);
        // Scrap 3
        sut = new SutBuilder(repository.Object, persistence.Object, log.Object, config.Object).Add(session).Build();
        await sut.ScrapPageAsync(cancellationToken);

        log.Verify(logger => logger.Warn(It.Is<string>(m => m.StartsWith("No offers overlap"))), Times.Never);
    }

    private ScrapApplication BuildSut(SessionStorage.ScrapApplication session)
    {
        return new ScrapApplication(repository.Object, persistence.Object, session, log.Object, config.Object);
    }

    private class SutBuilder
    {
        private ITradeOfferRepository repository;
        private IPersistenceRepository persistence;
        private ILog log;
        private IScrapApplicationSettings config;
        private SessionStorage.ScrapApplication session = new SessionStorage.ScrapApplication();

        public SutBuilder(ITradeOfferRepository repository, IPersistenceRepository persistence, ILog log, IScrapApplicationSettings config)
        {
            this.repository = repository;
            this.persistence = persistence;
            this.log = log;
            this.config = config;
        }

        public SutBuilder Add(SessionStorage.ScrapApplication session)
        {
            this.session = session;

            return this;
        }

        public ScrapApplication Build()
        {
            return new ScrapApplication(repository, persistence, session, log, config);
        }
    }
}