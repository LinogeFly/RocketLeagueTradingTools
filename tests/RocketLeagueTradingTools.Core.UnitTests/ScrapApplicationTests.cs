using FluentAssertions;
using Moq;
using RocketLeagueTradingTools.Core.Application.Scraping;
using RocketLeagueTradingTools.Core.Application.Interfaces;
using RocketLeagueTradingTools.Core.Domain.Entities;
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
        log = new Mock<ILog>();
        repository = new Mock<ITradeOfferRepository>();
        persistence = new Mock<IPersistenceRepository>();

        config = new Mock<IScrapApplicationSettings>();
        config.Setup(c => c.ScrapRetryMaxAttempts).Returns(1);
        config.Setup(c => c.ScrapDelayMin).Returns(TimeSpan.Zero);
        config.Setup(c => c.ScrapDelayMax).Returns(TimeSpan.Zero);

        sut = new ScrapApplication(repository.Object, persistence.Object, log.Object, config.Object);
    }

    [Test]
    public async Task InfiniteScrap_should_retry_downloading_after_number_for_unsuccessful_attempts_before_stopping()
    {
        config.Setup(c => c.ScrapRetryMaxAttempts).Returns(3);
        repository.Setup(r => r.GetTradeOffersPage(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException());

        await sut.InfiniteScrap(cancellationToken);

        repository.Verify(r => r.GetTradeOffersPage(It.IsAny<CancellationToken>()), Times.Exactly(3));
    }

    [Test]
    public async Task InfiniteScrap_should_not_persist_buy_offers_from_previous_page_scrap()
    {
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
            .Build();
        var secondOffer = new TradeOfferBuilder()
            .WithTradeItem(Build.TradeItem("Hellfire"))
            .WithRlgId("2")
            .WithPrice(150)
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

        config.Setup(c => c.ScrapRetryMaxAttempts).Returns(2);
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