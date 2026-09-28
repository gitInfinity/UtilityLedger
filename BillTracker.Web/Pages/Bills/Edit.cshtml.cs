using BillTracker.Core.Models;
using BillTracker.Web.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BillTracker.Web.Pages.Bills;

public class EditModel : PageModel
{
    private readonly BillTrackerDbContext _dbContext;
    private readonly ILogger<EditModel> _logger;

    public EditModel(BillTrackerDbContext dbContext, ILogger<EditModel> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    [BindProperty]
    public BillInputModel Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var bill = await _dbContext.Bills.AsNoTracking().SingleOrDefaultAsync(bill => bill.Id == id);
        if (bill is null)
        {
            return NotFound();
        }

        Input = BillInputModel.FromBill(bill);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var bill = await _dbContext.Bills.SingleOrDefaultAsync(bill => bill.Id == id);
        if (bill is null)
        {
            return NotFound();
        }

        var updatedBill = Input.ToBill();
        if (await HasDuplicateAsync(updatedBill.Category, updatedBill.BillingMonth, id))
        {
            ModelState.AddModelError(string.Empty, "A bill for this utility and month already exists.");
            return Page();
        }

        bill.Category = updatedBill.Category;
        bill.Amount = updatedBill.Amount;
        bill.BillingMonth = updatedBill.BillingMonth;
        bill.Notes = updatedBill.Notes;

        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException exception)
        {
            _logger.LogWarning(exception, "Could not update bill {BillId}.", id);
            ModelState.AddModelError(string.Empty, "The bill could not be saved. Check that one does not already exist for this utility and month.");
            return Page();
        }

        return RedirectToPage("Index");
    }

    private Task<bool> HasDuplicateAsync(UtilityCategory category, BillingMonth billingMonth, int billId) =>
        _dbContext.Bills.AnyAsync(bill =>
            bill.Id != billId && bill.Category == category && bill.BillingMonth == billingMonth);
}