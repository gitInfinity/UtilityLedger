using BillTracker.Core.Models;
using BillTracker.Web.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BillTracker.Web.Pages.Bills;

public class IndexModel : PageModel
{
    private readonly BillTrackerDbContext _dbContext;

    public IndexModel(BillTrackerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IReadOnlyList<Bill> Bills { get; private set; } = [];

    public async Task OnGetAsync()
    {
        Bills = await _dbContext.Bills
            .AsNoTracking()
            .OrderByDescending(bill => bill.BillingMonth)
            .ThenBy(bill => bill.Category)
            .ToListAsync();
    }
}