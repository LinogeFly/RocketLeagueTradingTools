namespace RocketLeagueTradingTools.Core.Domain.Entities;

public sealed class TradeItem
{
    public string Name { get; set; }
    public string Color { get; set; } = "";
    public string Certification { get; set; } = "";

    public TradeItem(string name)
    {
        Name = name;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as TradeItem);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Name, Color, Certification);
    }

    private bool Equals(TradeItem? other)
    {
        return other != null &&
            Name == other.Name &&
            Color == other.Color &&
            Certification == other.Certification;
    }
}
