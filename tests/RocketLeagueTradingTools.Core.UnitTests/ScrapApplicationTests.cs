using FluentAssertions;
using Moq;
using RocketLeagueTradingTools.Core.Application.Scraping;
using RocketLeagueTradingTools.Core.Application.Interfaces;
using RocketLeagueTradingTools.Core.Domain.Entities;
using RocketLeagueTradingTools.Core.UnitTests.Support;
using FluentAssertions.Extensions;

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
        log = new Mock<ILog>();
        repository = new Mock<ITradeOfferRepository>();
        persistence = new Mock<IPersistenceRepository>();

        config = new Mock<IScrapApplicationSettings>();
        config.Setup(c => c.RetryMaxAttempts).Returns(0);
        config.Setup(c => c.DelayMin).Returns(TimeSpan.Zero);
        config.Setup(c => c.DelayMax).Returns(TimeSpan.Zero);

        sut = new ScrapApplication(repository.Object, persistence.Object, log.Object, config.Object);
    }

    [Test]
    public void InfiniteScrap_should_have_a_delay_between_scraps()
    {
        var expectedDelay = 200.Milliseconds();

        config.Setup(c => c.DelayMin).Returns(expectedDelay);
        config.Setup(c => c.DelayMax).Returns(expectedDelay);
        repository.SetupSequence(r => r.GetTradeOffersPage(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TradeOffersPage())
            .ThrowsAsync(new OperationCanceledException());

        var act = async () =>
        {
            await sut.InfiniteScrap(cancellationToken);
        };

        act.ExecutionTime().Should().BeCloseTo(expectedDelay, 100.Milliseconds());
    }

    [TestCase(0)]
    [TestCase(1)]
    public async Task InfiniteScrap_should_retry_number_of_times_before_stopping(int maxRetryAttempts)
    {
        config.Setup(c => c.RetryMaxAttempts).Returns(maxRetryAttempts);
        repository.Setup(r => r.GetTradeOffersPage(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException());

        await sut.InfiniteScrap(cancellationToken);

        repository.Verify(r => r.GetTradeOffersPage(It.IsAny<CancellationToken>()), Times.Exactly(maxRetryAttempts + 1));
        log.Verify(logger => logger.Error(It.IsAny<string>()), Times.Exactly(maxRetryAttempts + 1));
    }

    [Test]
    public void InfiniteScrap_should_only_allow_zero_or_more_max_retry_attempts_configuration()
    {
        config.Setup(c => c.RetryMaxAttempts).Returns(-1);
        repository.Setup(r => r.GetTradeOffersPage(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException());

        var act = () =>
        {
            sut = new ScrapApplication(repository.Object, persistence.Object, log.Object, config.Object);
        };

        act.Should().Throw<InvalidOperationException>();
    }

    [Test]
    public void InfiniteScrap_should_have_exponentially_increased_delay_between_retries()
    {
        config.Setup(c => c.RetryMaxAttempts).Returns(2);
        config.Setup(c => c.RetryInterval).Returns(200.Milliseconds());
        config.Setup(c => c.RetryBackoffRate).Returns(2);

        repository.Setup(r => r.GetTradeOffersPage(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException());

        var act = async () =>
        {
            await sut.InfiniteScrap(cancellationToken);
        };

        act.ExecutionTime().Should().BeCloseTo(600.Milliseconds(), 100.Milliseconds());
    }

    [Test]
    public async Task InfiniteScrap_should_not_persist_buy_offers_from_previous_page_scrap()
    {
        var firstOffer = new TradeOfferBuilder()
            .WithTradeItem(Build.TradeItem("Fennec"))
            .WithRlgId("1")
            .WithPrice(300)
            .WithScrapedDate(DateTime.UtcNow.AddSeconds(-20))
            .Build();
        var secondOffer = new TradeOfferBuilder()
            .WithTradeItem(Build.TradeItem("Hellfire"))
            .WithRlgId("2")
            .WithPrice(100)
            .WithScrapedDate(DateTime.UtcNow)
            .Build();

        repository.SetupSequence(r => r.GetTradeOffersPage(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TradeOffersPage { BuyOffers = new[] { firstOffer } })
            .ReturnsAsync(new TradeOffersPage { BuyOffers = new[] { firstOffer, secondOffer } })
            .ThrowsAsync(new OperationCanceledException());

        await sut.InfiniteScrap(cancellationToken);

        persistence.Verify(p => p.AddBuyOffers(new[] { firstOffer }), Times.Once);
        persistence.Verify(p => p.AddBuyOffers(new[] { secondOffer }), Times.Once);
    }

    [Test]
    public async Task InfiniteScrap_should_not_persist_sell_offers_from_previous_page_scrap()
    {
        var firstOffer = new TradeOfferBuilder()
            .WithTradeItem(Build.TradeItem("Fennec"))
            .WithRlgId("1")
            .WithPrice(350)
            .WithScrapedDate(DateTime.UtcNow.AddSeconds(-20))
            .Build();
        var secondOffer = new TradeOfferBuilder()
            .WithTradeItem(Build.TradeItem("Hellfire"))
            .WithRlgId("2")
            .WithPrice(150)
            .WithScrapedDate(DateTime.UtcNow)
            .Build();

        repository.SetupSequence(r => r.GetTradeOffersPage(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TradeOffersPage { SellOffers = new[] { firstOffer } })
            .ReturnsAsync(new TradeOffersPage { SellOffers = new[] { firstOffer, secondOffer } })
            .ThrowsAsync(new HttpRequestException());

        await sut.InfiniteScrap(cancellationToken);

        persistence.Verify(p => p.AddSellOffers(new[] { firstOffer }), Times.Once);
        persistence.Verify(p => p.AddSellOffers(new[] { secondOffer }), Times.Once);
    }

    [Test]
    public async Task InfiniteScrap_should_warn_if_there_is_no_offers_overlap_between_latest_and_previous_scraps()
    {
        var firstScrapDate = new DateTime(2022, 1, 1);
        var secondScrapDate = firstScrapDate.AddSeconds(10);

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
            })
            .ThrowsAsync(new HttpRequestException());

        await sut.InfiniteScrap(cancellationToken);

        log.Verify(logger => logger.Warn(It.Is<string>(m => m.StartsWith("No offers overlap"))), Times.Once);
    }

    [Test]
    public async Task InfiniteScrap_should_not_warn_if_there_is_offers_overlap_between_latest_and_previous_scraps()
    {
        var firstScrapDate = new DateTime(2022, 1, 1);
        var secondScrapDate = firstScrapDate.AddSeconds(10);

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
            })
            .ThrowsAsync(new HttpRequestException());

        await sut.InfiniteScrap(cancellationToken);

        log.Verify(logger => logger.Warn(It.Is<string>(m => m.StartsWith("No offers overlap"))), Times.Never);
    }

    [Test]
    public async Task InfiniteScrap_should_not_warn_if_there_is_buy_offers_overlap_but_no_sell_offers_overlap_between_latest_and_previous_scraps()
    {
        var firstScrapDate = new DateTime(2022, 1, 1);
        var secondScrapDate = firstScrapDate.AddSeconds(10);

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
            })
            .ThrowsAsync(new HttpRequestException());

        await sut.InfiniteScrap(cancellationToken);

        log.Verify(logger => logger.Warn(It.Is<string>(m => m.StartsWith("No offers overlap"))), Times.Never);
    }

    [Test]
    public async Task InfiniteScrap_should_not_warn_if_there_is_sell_offers_overlap_but_no_buy_offers_overlap_between_latest_and_previous_scraps()
    {
        var firstScrapDate = new DateTime(2022, 1, 1);
        var secondScrapDate = firstScrapDate.AddSeconds(10);

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
            })
            .ThrowsAsync(new HttpRequestException());

        await sut.InfiniteScrap(cancellationToken);

        log.Verify(logger => logger.Warn(It.Is<string>(m => m.StartsWith("No offers overlap"))), Times.Never);
    }

    [Test]
    public async Task InfiniteScrap_should_not_warn_if_there_is_no_offers_overlap_but_the_previous_scrap_failed()
    {
        var firstScrapDate = new DateTime(2022, 1, 1);
        var secondScrapDate = firstScrapDate.AddSeconds(10);

        config.Setup(c => c.RetryMaxAttempts).Returns(1);
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
            })
            .ThrowsAsync(new HttpRequestException())
            .ThrowsAsync(new HttpRequestException());

        await sut.InfiniteScrap(cancellationToken);

        log.Verify(logger => logger.Warn(It.Is<string>(m => m.StartsWith("No offers overlap"))), Times.Never);
    }
}