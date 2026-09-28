namespace BillTracker.Core.Models;

public sealed class Bill
{
    public int Id { get; set; }

    public UtilityCategory Category { get; set; }

    public decimal Amount { get; set; }

    public BillingMonth BillingMonth { get; set; }

    public string? Notes { get; set; }
}