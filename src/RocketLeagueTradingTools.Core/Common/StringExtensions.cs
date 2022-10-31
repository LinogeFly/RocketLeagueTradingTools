public static class StringExtensions
{
    public static TimeSpan ToTimeSpan(this string input)
    {
        return TimeSpan.ParseExact(input, "c", null);
    }
}