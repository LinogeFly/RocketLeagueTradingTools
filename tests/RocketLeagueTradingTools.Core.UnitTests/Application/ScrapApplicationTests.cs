using FluentAssertions;
using FluentAssertions.Extensions;
using Moq;
using RocketLeagueTradingTools.Core.Application.Interfaces;
using RocketLeagueTradingTools.Core.Application.Interfaces.Persistence;
using RocketLeagueTradingTools.Core.Application.Scrap;
using RocketLeagueTradingTools.Core.UnitTests.Support;
using RocketLeagueTradingTools.Test;

namespace RocketLeagueTradingTools.Core.UnitTests.Application;

[TestFixture]
public class ScrapApplicationTests
{
    private ScrapApplication sut = null!;
    private TestContainer testContainer = null!;
    private readonly CancellationToken cancellationToken = default;
    private Mock<IScrapApplicationSettings> config = null!;
    private Mock<ILog> log = null!;
    private Mock<ITradeOfferRepository> repository = null!;
    private Mock<ITradeOfferPersistenceRepository> tradeOfferPersistence = null!;

    [SetUp]
    public void Setup()
    {
        testContainer = TestContainer.Create();
        
        log = testContainer.MockOf<ILog>();
        repository = testContainer.MockOf<ITradeOfferRepository>();
        tradeOfferPersistence = testContainer.MockOf<ITradeOfferPersistenceRepository>();

        config = testContainer.MockOf<IScrapApplicationSettings>();
        config.Setup(c => c.RetryMaxAttempts).Returns(0);
        config.Setup(c => c.DelayMin).Returns(TimeSpan.Zero);
        config.Setup(c => c.DelayMax).Returns(TimeSpan.Zero);

        sut = testContainer.GetService<ScrapApplication>();
    }

    [Test]
    public void InfiniteScrap_should_have_a_delay_between_scraps()
    {
        var expectedDelay = 200.Milliseconds();
        config.Setup(c => c.DelayMin).Returns(expectedDelay);
        config.Setup(c => c.DelayMax).Returns(expectedDelay);
        repository.SetupSequence(r => r.GetTradeOffersPage(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { A.ScrapedOffer().Build() })
            .ThrowsAsync(new OperationCanceledException());

        var act = async () => { await sut.InfiniteScrap(cancellationToken); };

        act.ExecutionTime().Should().BeCloseTo(expectedDelay, 50.Milliseconds());
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

        var act = () => { var _ = testContainer.GetService<ScrapApplication>(); };

        act.Should().Throw<InvalidOperationException>();
    }

    [Test]
    public void InfiniteScrap_should_have_exponentially_increased_delay_between_retries()
    {
        config.Setup(c => c.RetryMaxAttempts).Returns(2);
        config.Setup(c => c.RetryInterval).Returns(100.Milliseconds());
        config.Setup(c => c.RetryBackoffRate).Returns(2);
        repository.Setup(r => r.GetTradeOffersPage(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException());

        var act = async () => { await sut.InfiniteScrap(cancellationToken); };

        act.ExecutionTime().Should().BeCloseTo(300.Milliseconds(), 50.Milliseconds());
    }

    [Test]
    public async Task InfiniteScrap_should_not_persist_offers_from_previous_page_scrap()
    {
        var firstScrapDate = new DateTime(2022, 1, 1);
        var secondScrapDate = firstScrapDate.AddSeconds(10);
        var scrap1Offer1 = A.ScrapedOffer()
            .WithOffer(A.TradeOffer()
                .WithLink("1")
                .WithItem(A.TradeItem().WithName("Fennec"))
                .WithPrice(300))
            .WithScrapedDate(firstScrapDate)
            .Build();
        var scrap2Offer1 = A.ScrapedOffer()
            .WithOffer(A.TradeOffer()
                .WithLink("1")
                .WithItem(A.TradeItem().WithName("Fennec"))
                .WithPrice(300))
            .WithScrapedDate(secondScrapDate)
            .Build();
        var scrap2Offer2 = A.ScrapedOffer()
            .WithOffer(A.TradeOffer()
                .WithLink("2")
                .WithItem(A.TradeItem().WithName("Hellfire"))
                .WithPrice(100))
            .WithScrapedDate(secondScrapDate)
            .Build();
        repository.SetupSequence(r => r.GetTradeOffersPage(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { scrap1Offer1 })
            .ReturnsAsync(new[] { scrap2Offer1, scrap2Offer2 })
            .ThrowsAsync(new OperationCanceledException());

        await sut.InfiniteScrap(cancellationToken);

        tradeOfferPersistence.Verify(p => p.AddTradeOffers(new[] { scrap1Offer1 }), Times.Once);
        tradeOfferPersistence.Verify(p => p.AddTradeOffers(new[] { scrap2Offer2 }), Times.Once);
        tradeOfferPersistence.Verify(p => p.AddTradeOffers(new[] { scrap2Offer1, scrap2Offer2 }), Times.Never);
    }

    [Test]
    public async Task InfiniteScrap_should_warn_if_there_is_no_offers_overlap_between_latest_and_previous_scraps()
    {
        var firstScrapDate = new DateTime(2022, 1, 1);
        var secondScrapDate = firstScrapDate.AddSeconds(10);
        repository.SetupSequence(r => r.GetTradeOffersPage(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[]
            {
                A.ScrapedOffer()
                    .WithOffer(A.TradeOffer()
                        .WithLink("1")
                        .WithItem(A.TradeItem().WithName("Fennec"))
                        .WithPrice(300))
                    .WithScrapedDate(firstScrapDate)
                    .Build()
            })
            .ReturnsAsync(new[]
            {
                A.ScrapedOffer()
                    .WithOffer(A.TradeOffer()
                        .WithLink("2")
                        .WithItem(A.TradeItem().WithName("Hellfire"))
                        .WithPrice(100))
                    .WithScrapedDate(secondScrapDate)
                    .Build()
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
            .ReturnsAsync(new[]
            {
                A.ScrapedOffer()
                    .WithOffer(A.TradeOffer()
                        .WithLink("1")
                        .WithItem(A.TradeItem().WithName("Fennec"))
                        .WithPrice(300))
                    .WithScrapedDate(firstScrapDate)
                    .Build()
            })
            .ReturnsAsync(new[]
            {
                A.ScrapedOffer()
                    .WithOffer(A.TradeOffer()
                        .WithLink("1")
                        .WithItem(A.TradeItem().WithName("Fennec"))
                        .WithPrice(300))
                    .WithScrapedDate(secondScrapDate)
                    .Build()
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
            .ReturnsAsync(new[]
            {
                A.ScrapedOffer()
                    .WithOffer(A.TradeOffer()
                        .WithLink("1")
                        .WithItem(A.TradeItem().WithName("Fennec"))
                        .WithPrice(300))
                    .WithScrapedDate(firstScrapDate)
                    .Build()
            })
            .ThrowsAsync(new HttpRequestException())
            .ReturnsAsync(new[]
            {
                A.ScrapedOffer()
                    .WithOffer(A.TradeOffer()
                        .WithLink("2")
                        .WithItem(A.TradeItem().WithName("Hellfire"))
                        .WithPrice(100))
                    .WithScrapedDate(secondScrapDate)
                    .Build()
            })
            .ThrowsAsync(new HttpRequestException())
            .ThrowsAsync(new HttpRequestException());

        await sut.InfiniteScrap(cancellationToken);

        log.Verify(logger => logger.Warn(It.Is<string>(m => m.StartsWith("No offers overlap"))), Times.Never);
    }
}