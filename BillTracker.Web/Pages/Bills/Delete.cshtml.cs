using BillTracker.Core.Models;
using BillTracker.Web.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BillTracker.Web.Pages.Bills;

public class DeleteModel : PageModel
{
    private readonly BillTrackerDbContext _dbContext;

    public DeleteModel(BillTrackerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Bill? Bill { get; private set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Bill = await _dbContext.Bills.AsNoTracking().SingleOrDefaultAsync(bill => bill.Id == id);
        return Bill is null ? NotFound() : Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var bill = await _dbContext.Bills.SingleOrDefaultAsync(bill => bill.Id == id);
        if (bill is null)
        {
            return NotFound();
        }

        _dbContext.Bills.Remove(bill);
        await _dbContext.SaveChangesAsync();
        return RedirectToPage("Index");
    }
}