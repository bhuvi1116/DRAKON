using DrakonNx.Core.Model;
using DrakonNx.Editor.Services;
using DrakonNx.Editor.Services.History;

namespace DrakonNx.Editor.UnitTests;

public sealed class CommandHistoryServiceTests
{
    [Fact]
    public void ExecuteUndoRedo_ForDelegateAction_WorksInOrder()
    {
        var history = new CommandHistoryService();
        var value = 0;

        history.Execute(new DelegateHistoryAction(
            "increment",
            () => value--,
            () => value++));

        Assert.Equal(1, value);
        Assert.True(history.CanUndo);

        var undoDescription = history.Undo();
        Assert.Equal("increment", undoDescription);
        Assert.Equal(0, value);
        Assert.True(history.CanRedo);

        var redoDescription = history.Redo();
        Assert.Equal("increment", redoDescription);
        Assert.Equal(1, value);
    }

    [Fact]
    public void HistoryStateCloner_CloneDocument_CreatesDeepCopy()
    {
        var original = new DiagramDocument("sample");
        original.Nodes.Add(new DiagramNode("action_1", NodeKind.Action, "x = 1", 10, 20));

        var clone = HistoryStateCloner.CloneDocument(original);
        clone.Name = "copy";
        clone.Nodes[0].Text = "x = 2";

        Assert.Equal("sample", original.Name);
        Assert.Equal("x = 1", original.Nodes[0].Text);
    }
}
