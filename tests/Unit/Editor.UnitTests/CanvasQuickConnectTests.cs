using DrakonNx.Editor.ViewModels;

namespace DrakonNx.Editor.UnitTests;

public sealed class CanvasQuickConnectTests
{
    [Fact]
    public void CanvasQuickConnect_CreatesConnectionFromPendingSource()
    {
        var vm = new MainWindowViewModel();
        var source = vm.Nodes.First();
        var target = vm.Nodes.Last(n => !string.Equals(n.Id, source.Id, StringComparison.Ordinal));
        var initialCount = vm.Connections.Count;

        vm.CanvasUseNodeAsConnectionSource(source.Id);
        vm.CanvasConnectFromPendingSourceToNode(target.Id);

        Assert.NotNull(vm.ConnectionFromNode);
        Assert.Equal(source.Id, vm.ConnectionFromNode!.Id);
        Assert.Equal(initialCount + 1, vm.Connections.Count);
        Assert.Contains("создание связи", vm.CanvasSummary, StringComparison.OrdinalIgnoreCase);
    }
}
