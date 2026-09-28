using BillTracker.Core.Models;
using BillTracker.Core.SpikeDetection;
using BillTracker.Web.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BillTracker.Web.Pages;

public class IndexModel : PageModel
{
    private readonly BillTrackerDbContext _dbContext;
    private readonly SpikeDetector _spikeDetector;

    public IndexModel(BillTrackerDbContext dbContext, SpikeDetector spikeDetector)
    {
        _dbContext = dbContext;
        _spikeDetector = spikeDetector;
    }

    public IReadOnlyList<DashboardBillRow> RecentBills { get; private set; } = [];

    public int CurrentMonthBillCount { get; private set; }

    public decimal CurrentMonthTotal { get; private set; }

    public int FlaggedRecentCount { get; private set; }

    public async Task OnGetAsync()
    {
        var bills = await _dbContext.Bills.AsNoTracking().ToListAsync();
        var currentMonth = new BillingMonth(DateTime.Today.Year, DateTime.Today.Month);
        var currentMonthBills = bills
            .Where(bill => bill.BillingMonth == currentMonth)
            .ToArray();

        CurrentMonthBillCount = currentMonthBills.Length;
        CurrentMonthTotal = currentMonthBills.Sum(bill => bill.Amount);

        RecentBills = bills
            .OrderByDescending(bill => bill.BillingMonth.MonthIndex)
            .ThenByDescending(bill => bill.Id)
            .Take(8)
            .Select(bill => new DashboardBillRow(bill, _spikeDetector.Analyze(bill, bills)))
            .ToArray();

        FlaggedRecentCount = RecentBills.Count(row => row.Detection.IsFlagged);
    }

    public sealed record DashboardBillRow(Bill Bill, SpikeDetectionResult Detection);
}
