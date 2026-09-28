using BillTracker.Core.Models;
using BillTracker.Web.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace BillTracker.Web.Pages.Bills;

public class CreateModel : PageModel
{
    private readonly BillTrackerDbContext _dbContext;
    private readonly ILogger<CreateModel> _logger;

    public CreateModel(BillTrackerDbContext dbContext, ILogger<CreateModel> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    [BindProperty]
    public BillInputModel Input { get; set; } = new();

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var bill = Input.ToBill();
        if (await HasDuplicateAsync(bill.Category, bill.BillingMonth))
        {
            ModelState.AddModelError(string.Empty, "A bill for this utility and month already exists.");
            return Page();
        }

        _dbContext.Bills.Add(bill);
        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException exception)
        {
            _logger.LogWarning(exception, "Could not save a bill for {Category} in {BillingMonth}.", bill.Category, bill.BillingMonth);
            ModelState.AddModelError(string.Empty, "The bill could not be saved. Check that one does not already exist for this utility and month.");
            return Page();
        }

        return RedirectToPage("Index");
    }

    private Task<bool> HasDuplicateAsync(UtilityCategory category, BillingMonth billingMonth) =>
        _dbContext.Bills.AnyAsync(bill => bill.Category == category && bill.BillingMonth == billingMonth);
}