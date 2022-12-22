using FluentAssertions;

namespace RocketLeagueTradingTools.Common.UnitTests;

public class TimeSpanExtensionsTests
{
    [TestCaseSource(nameof(ToHumanReadableString_ForGivenTimeSpan_ReturnsCorrespondingText_TestCases))]
    public void ToHumanReadableString_ForGivenTimeSpan_ReturnsCorrespondingText(TimeSpan time, string expected)
    {
        time.ToHumanReadableString().Should().Be(expected);
    }
    
    // ReSharper disable once InconsistentNaming
    private static object[] ToHumanReadableString_ForGivenTimeSpan_ReturnsCorrespondingText_TestCases =
    {
        new object[] { TimeSpan.FromSeconds(0), "1 second" },
        new object[] { TimeSpan.FromSeconds(1), "1 second" },
        new object[] { TimeSpan.FromSeconds(2), "2 seconds" },
        new object[] { TimeSpan.FromSeconds(59), "59 seconds" },
        new object[] { TimeSpan.FromSeconds(60), "1 minute" },
        new object[] { TimeSpan.FromSeconds(119), "1 minute" },
        new object[] { TimeSpan.FromSeconds(120), "2 minutes" },
        new object[] { TimeSpan.FromMinutes(59), "59 minutes" },
        new object[] { TimeSpan.FromMinutes(60), "1 hour" },
        new object[] { TimeSpan.FromMinutes(119), "1 hour" },
        new object[] { TimeSpan.FromMinutes(120), "2 hours" },
        new object[] { TimeSpan.FromHours(23), "23 hours" },
        new object[] { TimeSpan.FromHours(24), "1 day" },
        new object[] { TimeSpan.FromHours(47), "1 day" },
        new object[] { TimeSpan.FromHours(48), "2 days" },
        new object[] { TimeSpan.FromDays(30), "30 days" },
        new object[] { TimeSpan.FromDays(31), "30+ days" }
    };
}