using DrakonNx.Core.Model;
using DrakonNx.Core.Services;
using DrakonNx.Editor.Services;

namespace DrakonNx.Editor.UnitTests;

public sealed class DocumentHistoryServiceTests
{
    [Fact]
    public void UndoRedo_RestoresPreviousSnapshots()
    {
        var history = new DocumentHistoryService();
        var original = DiagramFactory.CreateMinimalSample();
        var renamed = CloneWithName(original, "Renamed");

        history.Push(new DocumentHistoryState(original, "a.drakon.json", "a.drakon.json", "a.drakon.json", false, "initial"));

        var undo = history.Undo(new DocumentHistoryState(renamed, "a.drakon.json", "a.drakon.json", "a.drakon.json", true, "renamed"));
        Assert.Equal(original.Name, undo.Document.Name);
        Assert.True(history.CanRedo);

        var redo = history.Redo(undo);
        Assert.Equal("Renamed", redo.Document.Name);
    }

    [Fact]
    public void Push_ClearsRedoStack()
    {
        var history = new DocumentHistoryService();
        var first = DiagramFactory.CreateMinimalSample();
        var second = CloneWithName(first, "Second");
        var third = CloneWithName(first, "Third");

        history.Push(new DocumentHistoryState(first, string.Empty, string.Empty, string.Empty, false, "first"));
        var undo = history.Undo(new DocumentHistoryState(second, string.Empty, string.Empty, string.Empty, true, "second"));
        history.Push(new DocumentHistoryState(third, string.Empty, string.Empty, string.Empty, true, "third"));

        Assert.False(history.CanRedo);
        Assert.Equal(first.Name, undo.Document.Name);
    }

    private static DiagramDocument CloneWithName(DiagramDocument source, string name)
    {
        var clone = new DiagramDocument(name)
        {
            Version = source.Version
        };

        foreach (var node in source.Nodes)
        {
            clone.Nodes.Add(new DiagramNode(node.Id, node.Kind, node.Text, node.X, node.Y));
        }

        foreach (var connection in source.Connections)
        {
            clone.Connections.Add(new DiagramConnection(connection.Id, connection.FromNodeId, connection.FromPort, connection.ToNodeId, connection.ToPort));
        }

        return clone;
    }
}
