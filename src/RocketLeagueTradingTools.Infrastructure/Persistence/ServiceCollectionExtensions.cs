using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace RocketLeagueTradingTools.Infrastructure.Persistence;

public static class ServiceCollectionExtensions
{
    public static void AddSQLiteDbContext(this IServiceCollection services)
    {
        var homePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var appPath = Path.Combine(homePath, ".RocketLeagueTradingTools");
        var databasePath = Path.Combine(appPath, "RocketLeagueTradingTools.sqlite.db");

        Directory.CreateDirectory(appPath);

        services.AddDbContext<PersistenceDbContext>(opt => opt.UseSqlite($"Data Source={databasePath};"));
    }
}