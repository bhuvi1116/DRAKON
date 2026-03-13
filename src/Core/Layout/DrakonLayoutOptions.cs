namespace DrakonNx.Core.Layout;

public sealed class DrakonLayoutOptions
{
    public double SpineX { get; init; } = 220;

    public double TopY { get; init; } = 80;

    public double RowHeight { get; init; } = 118;

    public double LaneWidth { get; init; } = 210;

    public bool AlignToSpine { get; init; } = true;

    public bool KeepRightwardProgression { get; init; } = true;
}
