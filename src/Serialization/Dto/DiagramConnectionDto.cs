namespace DrakonNx.Serialization.Dto;

public sealed class DiagramConnectionDto
{
    public string Id { get; set; } = string.Empty;
    public string FromNodeId { get; set; } = string.Empty;
    public string FromPort { get; set; } = string.Empty;
    public string ToNodeId { get; set; } = string.Empty;
    public string ToPort { get; set; } = string.Empty;
}
