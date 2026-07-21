namespace Antigen.Models.Settings;

public sealed record GuiSettings
{
    public int WindowX { get; init; }
    public int WindowY { get; init; }
    public double ExpandedHeight { get; init; } = 500;
    public double WorkerThreadPercentage { get; init; } = 0.5;
}
