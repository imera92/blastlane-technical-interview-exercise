namespace ExpenseTracker.Infrastructure.Persistence;

public static class UtcDateTimeOffsetConverter
{
    public static long ToUtcTicks(DateTimeOffset value)
    {
        return value.UtcDateTime.Ticks;
    }

    public static DateTimeOffset FromUtcTicks(long ticks)
    {
        return new DateTimeOffset(ticks, TimeSpan.Zero);
    }
}
