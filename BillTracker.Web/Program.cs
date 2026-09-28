using BillTracker.Web.Data;
using BillTracker.Core.SpikeDetection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddDbContext<BillTrackerDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("BillTracker")));
builder.Services.AddOptions<SpikeDetectionOptions>()
    .BindConfiguration("SpikeDetection")
    .Validate(options => options.LookbackMonths >= 1, "LookbackMonths must be at least 1.")
    .Validate(options => options.Threshold >= 0, "Threshold cannot be negative.")
    .ValidateOnStart();
builder.Services.AddSingleton(serviceProvider =>
    new SpikeDetector(serviceProvider.GetRequiredService<IOptions<SpikeDetectionOptions>>().Value));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<BillTrackerDbContext>();
    dbContext.Database.Migrate();
}

app.Run();
