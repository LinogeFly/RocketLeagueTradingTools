namespace RocketLeagueTradingTools.Common;

public static class StringExtensions
{
    public static bool IsEmpty(this string? text) => string.IsNullOrWhiteSpace(text);

    public static bool IsNotEmpty(this string? text) => !text.IsEmpty();

    public static TimeSpan ToTimeSpan(this string input) => TimeSpan.ParseExact(input, "c", null);
}