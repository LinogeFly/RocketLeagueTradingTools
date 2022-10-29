namespace RocketLeagueTradingTools.Core.Domain.ValueObjects;

public sealed class PriceRange
{
    public int From { get; }
    public int To { get; }

    public PriceRange(int to)
    {
        if (to <= 0)
            throw new ArgumentException($"{nameof(To)} has to be more than 0.");

        To = to;
    }

    public PriceRange(int from, int priceTo) : this(priceTo)
    {
        if (from < 0)
            throw new ArgumentException($"{nameof(From)} has to be more or equal 0.");

        if (priceTo <= 0)
            throw new ArgumentException($"{nameof(To)} has to be more than 0.");

        if (from >= priceTo)
            throw new ArgumentException($"{nameof(From)} has to be less than {nameof(To)}.");

        From = from;
    }
}