using EsperantOS.Helpers;

namespace EsperantOS.Tests.Helpers;

public class VagtHelpersTests
{
    private static DateTime NextFriday()
    {
        var d = DateTime.Today;
        while (d.DayOfWeek != DayOfWeek.Friday) d = d.AddDays(1);
        return d;
    }

    // ── GetShiftDateTime ──────────────────────────────────────

    [Theory]
    [InlineData("16:00:00", 16, 0)]
    [InlineData("20:00:00", 20, 0)]
    public void GetShiftDateTime_Test(string time, int expectedHour, int dayOffset)
    {
        var friday = NextFriday();
        var result = VagtHelpers.GetShiftDateTime(friday, time);
        Assert.Equal(friday.Date.AddDays(dayOffset).AddHours(expectedHour), result);
    }

    [Fact]
    public void GetShiftDateTime_Midnight_Test()
    {
        // 00:00-vagter gemmes som lørdag
        var friday = NextFriday();
        var result = VagtHelpers.GetShiftDateTime(friday, "00:00:00");
        Assert.Equal(friday.Date.AddDays(1), result);
    }

    // ── GetUpcomingFridaysSelectList ──────────────────────────

    [Fact]
    public void GetUpcomingFridaysList_Test()
    {
        var result = VagtHelpers.GetUpcomingFridaysSelectList();
        Assert.Equal(10, result.Count);
        Assert.All(result, item => Assert.Equal(DayOfWeek.Friday, DateTime.Parse(item.Value).DayOfWeek));
    }

    [Fact]
    public void GetUpcomingFridaysList_Midnight_Test()
    {
        // En 00:00-vagt gemmes som lørdag; dropdown skal forvælge fredagen
        var friday = NextFriday();
        var midnightShift = friday.AddDays(1); // Lørdag 00:00

        var result = VagtHelpers.GetUpcomingFridaysSelectList(midnightShift);

        var selected = result.Where(x => x.Selected).ToList();
        Assert.Single(selected);
        Assert.Equal(friday.ToString("yyyy-MM-dd"), selected[0].Value);
    }

    // ── GetShiftTimesSelectList ───────────────────────────────

    [Fact]
    public void GetShiftTimesList_Test()
    {
        Assert.Equal(3, VagtHelpers.GetShiftTimesSelectList().Count);
    }

    [Theory]
    [InlineData(16, "16:00:00")]
    [InlineData(20, "20:00:00")]
    [InlineData(0, "00:00:00")]
    public void GetShiftTimesList_PreselectsCorrectTime(int hour, string expectedValue)
    {
        var selected = VagtHelpers.GetShiftTimesSelectList(DateTime.Today.AddHours(hour)).Single(x => x.Selected);
        Assert.Equal(expectedValue, selected.Value);
    }
}
