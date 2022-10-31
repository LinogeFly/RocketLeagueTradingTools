using RocketLeagueTradingTools.Core.Domain.Entities;

namespace RocketLeagueTradingTools.Application.Common;

public abstract class SessionStorage
{
    private Dictionary<string, object> storage = new Dictionary<string, object>();

    protected void SetOrAdd<T>(string key, T value)
    {
        if (value == null)
            throw new ArgumentException(nameof(value));

        if (string.IsNullOrEmpty(key))
            throw new ArgumentException(nameof(key));

        if (storage.ContainsKey(key))
            storage[key] = value;
        else
            storage.Add(key, value);
    }

    protected T Get<T>(string key)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentException(nameof(key));

        if (!storage.ContainsKey(key))
            throw new InvalidOperationException($"No item in the session storage with '{key}' key.");

        return (T)storage[key];
    }

    public class ScrapApplication : SessionStorage
    {
        public ScrapApplication()
        {
            SetOrAdd(nameof(NumberOfFailedTries), 0);
            SetOrAdd(nameof(PreviousScrapBuyOffers), new List<TradeOffer>());
            SetOrAdd(nameof(PreviousScrapSellOffers), new List<TradeOffer>());
        }

        public int NumberOfFailedTries
        {
            get
            {
                return Get<int>(nameof(NumberOfFailedTries));
            }
            set
            {
                SetOrAdd(nameof(NumberOfFailedTries), value);
            }
        }

        public IList<TradeOffer> PreviousScrapBuyOffers
        {
            get
            {
                return Get<IList<TradeOffer>>(nameof(PreviousScrapBuyOffers));
            }
            set
            {
                SetOrAdd(nameof(PreviousScrapBuyOffers), value);
            }
        }

        public IList<TradeOffer> PreviousScrapSellOffers
        {
            get
            {
                return Get<IList<TradeOffer>>(nameof(PreviousScrapSellOffers));
            }
            set
            {
                SetOrAdd(nameof(PreviousScrapSellOffers), value);
            }
        }
    }
}