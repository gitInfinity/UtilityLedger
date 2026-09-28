using BillTracker.Core.Models;

namespace BillTracker.Core.SpikeDetection;

public sealed class SpikeDetector
{
    private readonly int _lookbackMonths;
    private readonly decimal _threshold;

    public SpikeDetector(SpikeDetectionOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (options.LookbackMonths < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(options), "LookbackMonths must be at least 1.");
        }

        if (options.Threshold < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(options), "Threshold cannot be negative.");
        }

        _lookbackMonths = options.LookbackMonths;
        _threshold = options.Threshold;
    }

    public SpikeDetectionResult Analyze(Bill bill, IEnumerable<Bill> bills)
    {
        ArgumentNullException.ThrowIfNull(bill);
        ArgumentNullException.ThrowIfNull(bills);

        var currentMonthIndex = bill.BillingMonth.MonthIndex;
        var earliestMonthIndex = currentMonthIndex - _lookbackMonths;
        var previousBills = bills
            .Where(candidate => candidate.Category == bill.Category)
            .Where(candidate => !IsSameBill(candidate, bill))
            .Where(candidate => candidate.BillingMonth.MonthIndex < currentMonthIndex)
            .Where(candidate => candidate.BillingMonth.MonthIndex >= earliestMonthIndex)
            .ToArray();

        if (previousBills.Length == 0)
        {
            return new SpikeDetectionResult(
                false,
                null,
                null,
                $"Not enough data to compare {bill.Category} with bills from the previous {_lookbackMonths} months.");
        }

        var average = previousBills.Average(previousBill => previousBill.Amount);
        decimal? percentAboveAverage = average == 0
            ? null
            : (bill.Amount - average) / average * 100m;
        var isFlagged = bill.Amount > average * (1m + _threshold);
        var message = CreateMessage(bill.Category, average, percentAboveAverage, isFlagged);

        return new SpikeDetectionResult(isFlagged, average, percentAboveAverage, message);
    }

    private static bool IsSameBill(Bill candidate, Bill bill) =>
        ReferenceEquals(candidate, bill) || (bill.Id != 0 && candidate.Id == bill.Id);

    private string CreateMessage(
        UtilityCategory category,
        decimal average,
        decimal? percentAboveAverage,
        bool isFlagged)
    {
        if (percentAboveAverage is null)
        {
            return isFlagged
                ? $"{category} is above an average of zero; a percentage comparison is unavailable."
                : $"{category} is not flagged; its previous {_lookbackMonths}-month average is zero.";
        }

        var status = isFlagged ? "is" : "is not flagged; it is";
        return $"{category} {status} {percentAboveAverage:0.##}% above its previous {_lookbackMonths}-month average ({average:0.00}).";
    }
}