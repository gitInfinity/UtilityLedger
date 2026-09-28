namespace BillTracker.Core.SpikeDetection;

public sealed record SpikeDetectionResult(
    bool IsFlagged,
    decimal? Average,
    decimal? PercentAboveAverage,
    string Message);