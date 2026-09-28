using BillTracker.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace BillTracker.Web.Data;

public sealed class BillTrackerDbContext(DbContextOptions<BillTrackerDbContext> options) : DbContext(options)
{
    public DbSet<Bill> Bills => Set<Bill>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Bill>(entity =>
        {
            entity.Property(bill => bill.BillingMonth)
                .HasConversion(
                    month => month.MonthIndex,
                    monthIndex => BillingMonth.FromMonthIndex(monthIndex));

            entity.HasIndex(bill => new { bill.Category, bill.BillingMonth })
                .IsUnique();
        });
    }
}