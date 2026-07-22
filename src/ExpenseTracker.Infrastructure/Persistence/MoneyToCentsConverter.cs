namespace ExpenseTracker.Infrastructure.Persistence;

public static class MoneyToCentsConverter
{
    public static long ToCents(decimal amount)
    {
        return checked((long)(amount * 100m));
    }

    public static decimal FromCents(long cents)
    {
        return cents / 100m;
    }
}
