using EsperantOS.Helpers;

namespace EsperantOS.Tests.Helpers;

public class VagtHelpersTests
{
    private static DateTime GetNextFriday()
    {
        var d = DateTime.Today;
        while (d.DayOfWeek != DayOfWeek.Friday) d = d.AddDays(1);
        return d;
    }

    // ── GetShiftDateTime ──────────────────────────────────────

    [Fact]
    public void GetShiftDateTime_1600_ReturnsFridayAt1600()
    {
        var friday = GetNextFriday();
        var result = VagtHelpers.GetShiftDateTime(friday, "16:00:00");
        Assert.Equal(friday.Date.AddHours(16), result);
    }

    [Fact]
    public void GetShiftDateTime_2000_ReturnsFridayAt2000()
    {
        var friday = GetNextFriday();
        var result = VagtHelpers.GetShiftDateTime(friday, "20:00:00");
        Assert.Equal(friday.Date.AddHours(20), result);
    }

    [Fact]
    public void GetShiftDateTime_Midnight_ReturnsNextDayAt0000()
    {
        // 00:00-vagter gemmes som lørdag for at afspejle at de sker efter midnat
        var friday = GetNextFriday();
        var result = VagtHelpers.GetShiftDateTime(friday, "00:00:00");
        Assert.Equal(friday.Date.AddDays(1), result);
    }

    // ── GetUpcomingFridaysSelectList ──────────────────────────

    [Fact]
    public void GetUpcomingFridaysSelectList_Returns10Items()
    {
        var result = VagtHelpers.GetUpcomingFridaysSelectList();
        Assert.Equal(10, result.Count);
    }

    [Fact]
    public void GetUpcomingFridaysSelectList_AllDatesAreFridays()
    {
        var result = VagtHelpers.GetUpcomingFridaysSelectList();
        foreach (var item in result)
        {
            var date = DateTime.Parse(item.Value);
            Assert.Equal(DayOfWeek.Friday, date.DayOfWeek);
        }
    }

    [Fact]
    public void GetUpcomingFridaysSelectList_WithNoSelectedDate_NothingIsSelected()
    {
        var result = VagtHelpers.GetUpcomingFridaysSelectList(null);
        Assert.DoesNotContain(result, x => x.Selected);
    }

    [Fact]
    public void GetUpcomingFridaysSelectList_WithSelectedDate_CorrectItemIsSelected()
    {
        var friday = GetNextFriday().AddHours(20);
        var result = VagtHelpers.GetUpcomingFridaysSelectList(friday);

        var selected = result.Where(x => x.Selected).ToList();
        Assert.Single(selected);
        Assert.Equal(friday.Date.ToString("yyyy-MM-dd"), selected[0].Value);
    }

    [Fact]
    public void GetUpcomingFridaysSelectList_MidnightShift_SelectsTheFridayItBelongsTo()
    {
        // En 00:00-vagt gemmes som lørdag; dropdown'en bør forvælge fredagen
        var friday = GetNextFriday();
        var midnightShift = friday.AddDays(1); // Lørdag 00:00

        var result = VagtHelpers.GetUpcomingFridaysSelectList(midnightShift);

        var selected = result.Where(x => x.Selected).ToList();
        Assert.Single(selected);
        Assert.Equal(friday.ToString("yyyy-MM-dd"), selected[0].Value);
    }

    // ── GetShiftTimesSelectList ───────────────────────────────

    [Fact]
    public void GetShiftTimesSelectList_Returns3Items()
    {
        var result = VagtHelpers.GetShiftTimesSelectList();
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public void GetShiftTimesSelectList_NoDate_SelectsFirstItem()
    {
        var result = VagtHelpers.GetShiftTimesSelectList(null);
        Assert.True(result.Single(x => x.Value == "16:00:00").Selected);
        Assert.False(result.Single(x => x.Value == "20:00:00").Selected);
        Assert.False(result.Single(x => x.Value == "00:00:00").Selected);
    }

    [Theory]
    [InlineData(16, "16:00:00")]
    [InlineData(20, "20:00:00")]
    [InlineData(0,  "00:00:00")]
    public void GetShiftTimesSelectList_PreselectsCorrectItemBasedOnHour(int hour, string expectedValue)
    {
        var date = DateTime.Today.AddHours(hour);
        var result = VagtHelpers.GetShiftTimesSelectList(date);

        var selected = result.Single(x => x.Selected);
        Assert.Equal(expectedValue, selected.Value);
    }
}
