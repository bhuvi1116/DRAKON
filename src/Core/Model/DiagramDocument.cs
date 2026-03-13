namespace DrakonNx.Core.Model;

public sealed class DiagramDocument
{
    public const string CurrentVersion = "0.2";

    public DiagramDocument(string name)
    {
        Name = string.IsNullOrWhiteSpace(name) ? "Untitled" : name;
    }

    public string Version { get; init; } = CurrentVersion;

    public string Name { get; set; }

    public DiagramProfile Profile { get; set; } = DiagramProfile.ExecutableV0;

    public DiagramLayoutMode LayoutMode { get; set; } = DiagramLayoutMode.Primitive;

    public List<DiagramNode> Nodes { get; } = new();

    public List<DiagramConnection> Connections { get; } = new();

    public DiagramNode? FindNode(string id) => Nodes.FirstOrDefault(n => n.Id == id);
}
