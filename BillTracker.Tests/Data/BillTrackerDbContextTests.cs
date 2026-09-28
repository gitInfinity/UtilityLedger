using BillTracker.Core.Models;
using BillTracker.Web.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace BillTracker.Tests.Data;

public class BillTrackerDbContextTests
{
    [Fact]
    public async Task Bills_EnforceOneBillPerCategoryAndMonth_AndRoundTripBillingMonth()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<BillTrackerDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var dbContext = new BillTrackerDbContext(options);
        await dbContext.Database.EnsureCreatedAsync();

        var bill = new Bill
        {
            Category = UtilityCategory.Electricity,
            Amount = 125.50m,
            BillingMonth = new BillingMonth(2025, 6)
        };
        dbContext.Bills.Add(bill);
        await dbContext.SaveChangesAsync();

        dbContext.ChangeTracker.Clear();
        var savedBill = await dbContext.Bills.SingleAsync();

        Assert.Equal(new BillingMonth(2025, 6), savedBill.BillingMonth);
        Assert.Equal(125.50m, savedBill.Amount);

        dbContext.Bills.Add(new Bill
        {
            Category = UtilityCategory.Electricity,
            Amount = 80m,
            BillingMonth = new BillingMonth(2025, 6)
        });

        await Assert.ThrowsAsync<DbUpdateException>(() => dbContext.SaveChangesAsync());
    }
}