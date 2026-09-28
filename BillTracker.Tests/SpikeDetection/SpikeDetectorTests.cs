using BillTracker.Core.Models;
using BillTracker.Core.SpikeDetection;

namespace BillTracker.Tests.SpikeDetection;

public class SpikeDetectorTests
{
    [Fact]
    public void Analyze_WhenThereAreNoPriorBills_ReturnsNotEnoughData()
    {
        var detector = new SpikeDetector(new SpikeDetectionOptions());
        var bill = CreateBill(UtilityCategory.Electricity, 120m, 2025, 5);

        var result = detector.Analyze(bill, []);

        Assert.False(result.IsFlagged);
        Assert.Null(result.Average);
        Assert.Null(result.PercentAboveAverage);
        Assert.Contains("Not enough data", result.Message);
    }

    [Fact]
    public void Analyze_WhenAmountIsExactlyAtThreshold_DoesNotFlag()
    {
        var detector = new SpikeDetector(new SpikeDetectionOptions());
        var bill = CreateBill(UtilityCategory.Electricity, 130m, 2025, 5);
        var history = new[] { CreateBill(UtilityCategory.Electricity, 100m, 2025, 4) };

        var result = detector.Analyze(bill, history);

        Assert.False(result.IsFlagged);
        Assert.Equal(100m, result.Average);
        Assert.Equal(30m, result.PercentAboveAverage);
    }

    [Fact]
    public void Analyze_WhenAmountIsJustAboveThreshold_FlagsBill()
    {
        var detector = new SpikeDetector(new SpikeDetectionOptions());
        var bill = CreateBill(UtilityCategory.Electricity, 130.01m, 2025, 5);
        var history = new[] { CreateBill(UtilityCategory.Electricity, 100m, 2025, 4) };

        var result = detector.Analyze(bill, history);

        Assert.True(result.IsFlagged);
        Assert.Equal(30.01m, result.PercentAboveAverage);
    }

    [Fact]
    public void Analyze_WhenThereAreGaps_UsesOnlyBillsInsideCalendarLookback()
    {
        var detector = new SpikeDetector(new SpikeDetectionOptions { LookbackMonths = 3 });
        var bill = CreateBill(UtilityCategory.Electricity, 131m, 2025, 5);
        var history = new[]
        {
            CreateBill(UtilityCategory.Electricity, 1000m, 2025, 1),
            CreateBill(UtilityCategory.Electricity, 100m, 2025, 3),
            CreateBill(UtilityCategory.Electricity, 100m, 2025, 4)
        };

        var result = detector.Analyze(bill, history);

        Assert.True(result.IsFlagged);
        Assert.Equal(100m, result.Average);
    }

    [Fact]
    public void Analyze_WhenOtherCategoriesHaveBills_DoesNotUseThem()
    {
        var detector = new SpikeDetector(new SpikeDetectionOptions());
        var bill = CreateBill(UtilityCategory.Electricity, 120m, 2025, 5);
        var history = new[] { CreateBill(UtilityCategory.Gas, 1000m, 2025, 4) };

        var result = detector.Analyze(bill, history);

        Assert.False(result.IsFlagged);
        Assert.Null(result.Average);
        Assert.Contains("Not enough data", result.Message);
    }

    [Fact]
    public void Analyze_WhenHistoryContainsTheSameBillId_ExcludesItFromAverage()
    {
        var detector = new SpikeDetector(new SpikeDetectionOptions());
        var bill = CreateBill(UtilityCategory.Electricity, 131m, 2025, 5, id: 2);
        var history = new[]
        {
            CreateBill(UtilityCategory.Electricity, 100m, 2025, 4, id: 1),
            CreateBill(UtilityCategory.Electricity, 131m, 2025, 4, id: 2)
        };

        var result = detector.Analyze(bill, history);

        Assert.True(result.IsFlagged);
        Assert.Equal(100m, result.Average);
    }

    private static Bill CreateBill(
        UtilityCategory category,
        decimal amount,
        int year,
        int month,
        int id = 0) => new()
    {
        Id = id,
        Category = category,
        Amount = amount,
        BillingMonth = new BillingMonth(year, month)
    };
}