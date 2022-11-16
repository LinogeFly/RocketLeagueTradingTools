using RocketLeagueTradingTools.Common;
using RocketLeagueTradingTools.Core.Domain.Enumerations;

namespace RocketLeagueTradingTools.Core.Domain.Entities;

public sealed class TradeItem
{
    public string Name { get; }
    public TradeItemType ItemType { get; init; }
    public string Color { get; init; } = "";
    public string Certification { get; init; } = "";

    public TradeItem(string name)
    {
        if (name.IsEmpty())
            throw new ArgumentException($"{nameof(name)} is required.");
        
        Name = name;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as TradeItem);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Name, ItemType, Color, Certification);
    }

    private bool Equals(TradeItem? other)
    {
        return other != null &&
            Name == other.Name &&
            ItemType == other.ItemType &&
            Color == other.Color &&
            Certification == other.Certification;
    }
}
