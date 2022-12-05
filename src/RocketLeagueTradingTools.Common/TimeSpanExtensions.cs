namespace RocketLeagueTradingTools.Common;

public static class TimeSpanExtensions
{
    public static string ToHumanReadableString(this TimeSpan span)
    {
        var totalDays = (int)Math.Floor(span.TotalDays);
        
        if (totalDays > 30)
            return "30+ days";
        if (totalDays > 1)
            return $"{totalDays} days";
        if (totalDays == 1)
            return "1 day";
        if (span.Hours > 1)
            return $"{span.Hours} hours";
        if (span.Hours == 1)
            return "1 hour";
        if (span.Minutes > 1)
            return $"{span.Minutes} minutes";
        if (span.Minutes == 1)
            return $"{span.Minutes} minute";
        if (span.Seconds > 1)
            return $"{span.Seconds} seconds";

        // Combining one second and less than one second into "1 second", as we don't want to have "0 seconds" value.
        // Basically rounding the span up from 0 to 1.
        return "1 second";
    }
}