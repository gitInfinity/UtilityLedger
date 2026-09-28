namespace BillTracker.Core.Models;

public readonly record struct BillingMonth
{
    public BillingMonth(int year, int month)
    {
        if (year is < 1 or > 9999)
        {
            throw new ArgumentOutOfRangeException(nameof(year), "Year must be between 1 and 9999.");
        }

        if (month is < 1 or > 12)
        {
            throw new ArgumentOutOfRangeException(nameof(month), "Month must be between 1 and 12.");
        }

        Year = year;
        Month = month;
    }

    public int Year { get; }

    public int Month { get; }

    public int MonthIndex => ((Year - 1) * 12) + Month - 1;

    public static BillingMonth FromMonthIndex(int monthIndex)
    {
        if (monthIndex is < 0 or >= 9999 * 12)
        {
            throw new ArgumentOutOfRangeException(nameof(monthIndex));
        }

        return new BillingMonth((monthIndex / 12) + 1, (monthIndex % 12) + 1);
    }

    public override string ToString() => $"{Year:D4}-{Month:D2}";
}