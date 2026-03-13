using DrakonNx.Core.Model;

namespace DrakonNx.Editor.Services;

public sealed class DocumentHistoryService
{
    private readonly Stack<DocumentHistoryState> _undo = new();
    private readonly Stack<DocumentHistoryState> _redo = new();

    public bool CanUndo => _undo.Count > 0;

    public bool CanRedo => _redo.Count > 0;

    public void Reset(DocumentHistoryState current)
    {
        _undo.Clear();
        _redo.Clear();
    }

    public void Push(DocumentHistoryState current)
    {
        _undo.Push(Clone(current));
        _redo.Clear();
    }

    public DocumentHistoryState Undo(DocumentHistoryState current)
    {
        if (!CanUndo)
        {
            throw new InvalidOperationException("Нет доступных изменений для Undo.");
        }

        _redo.Push(Clone(current));
        return Clone(_undo.Pop());
    }

    public DocumentHistoryState Redo(DocumentHistoryState current)
    {
        if (!CanRedo)
        {
            throw new InvalidOperationException("Нет доступных изменений для Redo.");
        }

        _undo.Push(Clone(current));
        return Clone(_redo.Pop());
    }

    private static DocumentHistoryState Clone(DocumentHistoryState state)
    {
        return new DocumentHistoryState(
            CloneDocument(state.Document),
            state.CurrentFilePath,
            state.OpenFilePath,
            state.SaveAsFilePath,
            state.IsDirty,
            state.Description);
    }

    private static DiagramDocument CloneDocument(DiagramDocument source)
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
