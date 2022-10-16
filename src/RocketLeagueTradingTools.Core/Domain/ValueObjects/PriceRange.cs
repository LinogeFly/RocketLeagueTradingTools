namespace RocketLeagueTradingTools.Core.Domain.ValueObjects;

public sealed class PriceRange
{
    public int PriceFrom { get; } = 0;
    private int PriceTo { get; }

    public PriceRange(int priceTo)
    {
        if (priceTo <= 0)
            throw new ArgumentException($"{nameof(PriceTo)} has to be more than 0.");

        PriceTo = priceTo;
    }

    public PriceRange(int priceFrom, int priceTo) : this(priceTo)
    {
        if (priceFrom <= 0)
            throw new ArgumentException($"{nameof(PriceFrom)} has to be more than 0.");

        if (priceTo <= 0)
            throw new ArgumentException($"{nameof(PriceTo)} has to be more than 0.");

        if (priceFrom >= priceTo)
            throw new ArgumentException($"{nameof(PriceFrom)} has to be less than {nameof(PriceTo)}.");

        PriceFrom = priceFrom;
    }
}