using Microsoft.EntityFrameworkCore;
using RocketLeagueTradingTools.Infrastructure.Persistence.SQLite;

static class ProgramExtensions
{
    public static void UseSQLiteDbContext(this IServiceCollection services)
    {
        var homePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var appPath = Path.Combine(homePath, ".RocketLeagueTradingTools");
        var databasePath = Path.Combine(appPath, "RocketLeagueTradingTools.sqlite.db");

        Directory.CreateDirectory(appPath);

        services.AddDbContext<SQLiteDbContext>(opt => opt.UseSqlite($"Data Source={databasePath};"));
    }
}