using Microsoft.Extensions.Configuration;
using RocketLeagueTradingTools.Common;

namespace RocketLeagueTradingTools.Infrastructure.Common;

public static class ConfigurationExtensions
{
    public static T GetRequiredValue<T>(this IConfiguration config, string key)
    {
        var value = config.GetValue<T>(key);

        if (typeof(T) == typeof(string))
            EnsureString(value as string, key);

        return value;
    }

    private static void EnsureString(string? value, string key)
    {
        if (value.IsEmpty())
            throw new Exception($"Required configuration value with '{key}' key is missing.");
    }
}