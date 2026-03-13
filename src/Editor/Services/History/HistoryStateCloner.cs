using DrakonNx.Core.Model;

namespace DrakonNx.Editor.Services.History;

public static class HistoryStateCloner
{
    public static DocumentHistoryState Clone(DocumentHistoryState state)
    {
        return new DocumentHistoryState(
            CloneDocument(state.Document),
            state.CurrentFilePath,
            state.OpenFilePath,
            state.SaveAsFilePath,
            state.IsDirty,
            state.Description);
    }

    public static DiagramDocument CloneDocument(DiagramDocument source)
    {
        var clone = new DiagramDocument(source.Name)
        {
            Version = source.Version
        };

        foreach (var node in source.Nodes)
        {
            clone.Nodes.Add(new DiagramNode(node.Id, node.Kind, node.Text, node.X, node.Y, node.Lane));
        }

        foreach (var connection in source.Connections)
        {
            clone.Connections.Add(new DiagramConnection(
                connection.Id,
                connection.FromNodeId,
                connection.FromPort,
                connection.ToNodeId,
                connection.ToPort));
        }

        return clone;
    }
}
