using ExpenseTracker.Infrastructure.Persistence;
using Xunit;

namespace ExpenseTracker.Infrastructure.UnitTests.Persistence;

public sealed class UtcDateTimeOffsetConverterTests
{
    [Fact]
    public void ToUtcTicks_WithUtcTimestamp_ReturnsUtcTicks()
    {
        var timestamp = new DateTimeOffset(
            2026,
            7,
            22,
            10,
            15,
            30,
            TimeSpan.Zero);

        var ticks = UtcDateTimeOffsetConverter.ToUtcTicks(timestamp);

        Assert.Equal(timestamp.UtcDateTime.Ticks, ticks);
    }

    [Fact]
    public void RoundTrip_WithNonUtcTimestamp_PreservesInstantAndReturnsUtcOffset()
    {
        var timestamp = new DateTimeOffset(
            2026,
            7,
            22,
            10,
            15,
            30,
            TimeSpan.FromHours(-5));

        var ticks = UtcDateTimeOffsetConverter.ToUtcTicks(timestamp);
        var convertedTimestamp = UtcDateTimeOffsetConverter.FromUtcTicks(ticks);

        Assert.Equal(timestamp, convertedTimestamp);
        Assert.Equal(TimeSpan.Zero, convertedTimestamp.Offset);
    }

    [Fact]
    public void ToUtcTicks_WithChronologicalTimestamps_PreservesOrdering()
    {
        var earlierTimestamp = new DateTimeOffset(
            2026,
            7,
            22,
            10,
            15,
            30,
            TimeSpan.Zero);
        var laterTimestamp = earlierTimestamp.AddTicks(1);

        var earlierTicks = UtcDateTimeOffsetConverter.ToUtcTicks(earlierTimestamp);
        var laterTicks = UtcDateTimeOffsetConverter.ToUtcTicks(laterTimestamp);

        Assert.True(earlierTicks < laterTicks);
    }
}
