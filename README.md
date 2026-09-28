# Utility Ledger

Utility Ledger is a small ASP.NET Core Razor Pages app for recording monthly electricity, gas, water, and internet bills. It compares each bill with the same category's previous calendar months and highlights unusually high amounts.

## Requirements

- .NET 10 SDK. The project was built with SDK `10.0.401`.

## Run the app

From the repository root:

```powershell
dotnet tool restore
dotnet restore
dotnet build
dotnet test
dotnet run --project BillTracker.Web
```

Open the local URL printed by `dotnet run` (the default HTTP profile uses `http://localhost:5122`). On startup, the app applies pending EF Core migrations. SQLite stores data in `bills.db`, relative to the application's working directory; database and journal files are excluded by `.gitignore`.

## Spike settings

Edit the `SpikeDetection` section in `BillTracker.Web/appsettings.json`:

```json
"SpikeDetection": {
  "LookbackMonths": 3,
  "Threshold": 0.3
}
```

`LookbackMonths` is the number of previous calendar months to consider. `Threshold` is a fraction, so `0.3` means 30%. A bill is flagged only when its amount is strictly greater than the previous-bill average multiplied by one plus the threshold. Bills from other categories are ignored, and a bill is never included in its own average. With no prior bill in the lookback window, the result is not flagged because there is not enough data.

## Database migrations

EF Core SQLite and the local `dotnet-ef` tool use the same version. To create and apply a migration from the repository root:

```powershell
dotnet ef migrations add MigrationName --project BillTracker.Web --startup-project BillTracker.Web
dotnet ef database update --project BillTracker.Web --startup-project BillTracker.Web
```

The database has a unique index on utility category and billing month, preventing duplicate bills even if concurrent requests pass the form validation at the same time.

## Project structure

- `BillTracker.Core` contains the bill and billing-month domain types and the dependency-free spike detector.
- `BillTracker.Web` contains the Razor Pages UI, EF Core SQLite context, configuration, and migrations.
- `BillTracker.Tests` contains spike-detector unit tests and SQLite persistence tests.