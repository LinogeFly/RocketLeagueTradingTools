namespace RocketLeagueTradingTools.Infrastructure.UnitTests.DataSource.Support.Rlg;

static class Build
{
    public static RlgItemBuilder Credits(int quantity) => new("Credits", quantity);
    public static RlgItemBuilder RlgItem(string name) => new(name, 1);
    public static RlgItemBuilder RlgItems(string name, int quantity) => new(name, quantity);
}
