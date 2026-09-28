namespace BillTracker.Core.SpikeDetection;

public sealed class SpikeDetectionOptions
{
    public int LookbackMonths { get; set; } = 3;

    public decimal Threshold { get; set; } = 0.30m;
}