namespace DrakonNx.Core.Model;

public sealed class DiagramNode
{
    public DiagramNode(string id, NodeKind kind, string text, double x = 0, double y = 0, int lane = 0)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Идентификатор узла не должен быть пустым.", nameof(id));
        }

        Id = id;
        Kind = kind;
        Text = text ?? string.Empty;
        X = x;
        Y = y;
        Lane = lane;
    }

    public string Id { get; }

    public NodeKind Kind { get; }

    public string Text { get; set; }

    public double X { get; set; }

    public double Y { get; set; }

    public int Lane { get; set; }
}
