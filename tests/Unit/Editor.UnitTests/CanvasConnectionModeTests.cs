using DrakonNx.Editor.ViewModels;

namespace DrakonNx.Editor.UnitTests;

public sealed class CanvasConnectionModeTests
{
    [Fact]
    public void CanvasCancelPendingConnection_ClearsPendingSource()
    {
        var vm = new MainWindowViewModel();
        var source = vm.Nodes.First();

        vm.CanvasUseNodeAsConnectionSource(source.Id);
        vm.CanvasCancelPendingConnection("test cancel");

        Assert.Null(vm.ConnectionFromNode);
        Assert.Contains("test cancel", vm.CanvasSummary, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void CanvasCanConnectToNode_ReturnsFalseForSelfAndDuplicate()
    {
        var vm = new MainWindowViewModel();
        var source = vm.Nodes.First();
        var target = vm.Nodes.Last(n => !string.Equals(n.Id, source.Id, StringComparison.Ordinal));

        vm.CanvasUseNodeAsConnectionSource(source.Id);

        Assert.False(vm.CanvasCanConnectToNode(source.Id));

        vm.CanvasConnectFromPendingSourceToNode(target.Id);
        vm.CanvasUseNodeAsConnectionSource(source.Id);

        Assert.False(vm.CanvasCanConnectToNode(target.Id));
    }
}
