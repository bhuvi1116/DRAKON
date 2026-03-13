using DrakonNx.Core.Model;

namespace DrakonNx.Editor.Services;

public sealed class DiagramEditService
{
    public DiagramNode AddNode(DiagramDocument document, NodeKind kind, string text)
    {
        ArgumentNullException.ThrowIfNull(document);

        var id = GenerateNodeId(document, kind);
        var node = new DiagramNode(id, kind, text, document.Nodes.Count * 40, document.Nodes.Count * 30);
        document.Nodes.Add(node);
        return node;
    }

    public bool DeleteNode(DiagramDocument document, string nodeId)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentException.ThrowIfNullOrWhiteSpace(nodeId);

        var node = document.FindNode(nodeId);
        if (node is null)
        {
            return false;
        }

        document.Nodes.Remove(node);
        document.Connections.RemoveAll(c => c.FromNodeId == nodeId || c.ToNodeId == nodeId);
        return true;
    }

    public bool UpdateNodeText(DiagramDocument document, string nodeId, string newText)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentException.ThrowIfNullOrWhiteSpace(nodeId);

        var node = document.FindNode(nodeId);
        if (node is null)
        {
            return false;
        }

        node.Text = newText ?? string.Empty;
        return true;
    }


    public bool UpdateNodePosition(DiagramDocument document, string nodeId, double x, double y)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentException.ThrowIfNullOrWhiteSpace(nodeId);

        var node = document.FindNode(nodeId);
        if (node is null)
        {
            return false;
        }

        node.X = x;
        node.Y = y;
        return true;
    }

    public bool SnapNodeToGrid(DiagramDocument document, string nodeId, double gridSize)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentException.ThrowIfNullOrWhiteSpace(nodeId);
        if (gridSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(gridSize), "Размер сетки должен быть больше нуля.");
        }

        var node = document.FindNode(nodeId);
        if (node is null)
        {
            return false;
        }

        node.X = Math.Round(node.X / gridSize) * gridSize;
        node.Y = Math.Round(node.Y / gridSize) * gridSize;
        return true;
    }

    public DiagramConnection AddConnection(DiagramDocument document, string fromNodeId, PortKind fromPort, string toNodeId, PortKind toPort)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentException.ThrowIfNullOrWhiteSpace(fromNodeId);
        ArgumentException.ThrowIfNullOrWhiteSpace(toNodeId);

        if (document.FindNode(fromNodeId) is null)
        {
            throw new InvalidOperationException($"Исходный узел не найден: {fromNodeId}");
        }

        if (document.FindNode(toNodeId) is null)
        {
            throw new InvalidOperationException($"Целевой узел не найден: {toNodeId}");
        }

        var connectionId = $"conn_{document.Connections.Count + 1}";
        var connection = new DiagramConnection(connectionId, fromNodeId, fromPort, toNodeId, toPort);
        document.Connections.Add(connection);
        return connection;
    }

    public bool DeleteConnection(DiagramDocument document, string connectionId)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionId);

        var connection = document.Connections.FirstOrDefault(c => c.Id == connectionId);
        if (connection is null)
        {
            return false;
        }

        document.Connections.Remove(connection);
        return true;
    }

    private static string GenerateNodeId(DiagramDocument document, NodeKind kind)
    {
        var prefix = kind switch
        {
            NodeKind.Start => "start",
            NodeKind.Action => "action",
            NodeKind.Condition => "cond",
            NodeKind.End => "end",
            _ => "node"
        };

        var index = 1;
        while (document.FindNode($"{prefix}_{index}") is not null)
        {
            index++;
        }

        return $"{prefix}_{index}";
    }
}
