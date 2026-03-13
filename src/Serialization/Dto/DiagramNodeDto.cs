namespace DrakonNx.Serialization.Dto;

public sealed class DiagramNodeDto
{
    public string Id { get; set; } = string.Empty;
    public string Kind { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public double X { get; set; }
    public double Y { get; set; }
    public int Lane { get; set; }
}
