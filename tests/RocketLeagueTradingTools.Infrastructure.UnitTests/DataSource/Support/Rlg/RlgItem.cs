namespace RocketLeagueTradingTools.Infrastructure.UnitTests.DataSource.Support.Rlg;

static class RlgItem
{
    public static RlgItemBuilder Credits(int quantity) => new RlgItemBuilder("Credits", quantity);
    public static RlgItemBuilder Item(string name) => new RlgItemBuilder(name, 1);
    public static RlgItemBuilder Items(string name, int quantity) => new RlgItemBuilder(name, quantity);
}
