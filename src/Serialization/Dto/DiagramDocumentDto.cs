namespace DrakonNx.Serialization.Dto;

public sealed class DiagramDocumentDto
{
    public string Version { get; set; } = "0.2";
    public string Name { get; set; } = "Untitled";
    public string Profile { get; set; } = "ExecutableV0";
    public string LayoutMode { get; set; } = "Primitive";
    public List<DiagramNodeDto> Nodes { get; set; } = new();
    public List<DiagramConnectionDto> Connections { get; set; } = new();
}
