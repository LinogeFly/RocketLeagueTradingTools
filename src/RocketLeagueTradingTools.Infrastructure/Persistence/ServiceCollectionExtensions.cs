using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RocketLeagueTradingTools.Infrastructure.Common;

namespace RocketLeagueTradingTools.Infrastructure.Persistence;

public static class ServiceCollectionExtensions
{
    public static void AddSQLiteDbContext(this IServiceCollection services, IConfiguration config)
    {
        var homePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var appPath = Path.Combine(homePath, ".RocketLeagueTradingTools");
        var databaseFilename = config.GetRequiredValue<string>("DatabaseFilename");
        var databasePath = Path.Combine(appPath, databaseFilename);

        Directory.CreateDirectory(appPath);

        services.AddDbContext<PersistenceDbContext>(opt => opt.UseSqlite($"Data Source={databasePath};"));
    }
}