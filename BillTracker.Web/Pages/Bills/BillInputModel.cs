using System.ComponentModel.DataAnnotations;
using BillTracker.Core.Models;

namespace BillTracker.Web.Pages.Bills;

public sealed class BillInputModel : IValidatableObject
{
    [Display(Name = "Utility")]
    public UtilityCategory Category { get; set; } = UtilityCategory.Electricity;

    [Range(typeof(decimal), "0.01", "79228162514264337593543950335", ErrorMessage = "Amount must be greater than zero.")]
    public decimal Amount { get; set; } = 0.01m;

    [Range(1, 9999)]
    [Display(Name = "Year")]
    public int Year { get; set; } = DateTime.Today.Year;

    [Range(1, 12)]
    [Display(Name = "Month")]
    public int Month { get; set; } = DateTime.Today.Month;

    [StringLength(500)]
    public string? Notes { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!Enum.IsDefined(Category))
        {
            yield return new ValidationResult("Choose a valid utility.", [nameof(Category)]);
        }
    }

    public Bill ToBill() => new()
    {
        Category = Category,
        Amount = Amount,
        BillingMonth = new BillingMonth(Year, Month),
        Notes = string.IsNullOrWhiteSpace(Notes) ? null : Notes.Trim()
    };

    public static BillInputModel FromBill(Bill bill) => new()
    {
        Category = bill.Category,
        Amount = bill.Amount,
        Year = bill.BillingMonth.Year,
        Month = bill.BillingMonth.Month,
        Notes = bill.Notes
    };
}