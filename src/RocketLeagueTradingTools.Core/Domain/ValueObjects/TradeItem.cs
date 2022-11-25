using RocketLeagueTradingTools.Common;
using RocketLeagueTradingTools.Core.Domain.Enumerations;

namespace RocketLeagueTradingTools.Core.Domain.ValueObjects;

public sealed class TradeItem : ValueObject
{
    public string Name { get; }
    public TradeItemType ItemType { get; init; }
    public string Color { get; init; } = "";
    public string Certification { get; init; } = "";

    public TradeItem(string name)
    {
        if (name.IsEmpty())
            throw new ArgumentException("The field is required.", nameof(name));

        Name = name;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Name;
        yield return ItemType;
        yield return Color;
        yield return Certification;
    }
}