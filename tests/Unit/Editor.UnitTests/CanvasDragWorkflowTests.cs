using DrakonNx.Core.Services;
using DrakonNx.Editor.ViewModels;

namespace DrakonNx.Editor.UnitTests;

public sealed class CanvasDragWorkflowTests
{
    [Fact]
    public void CanvasDrag_UpdatesNodePositionAndAllowsSelection()
    {
        var vm = new MainWindowViewModel();
        var node = vm.Nodes.First();

        var started = vm.BeginCanvasDrag(node.Id, node.X, node.Y);
        vm.UpdateCanvasDrag(node.Id, node.X + 45, node.Y + 30);
        vm.CompleteCanvasDrag(node.Id);

        var moved = vm.Nodes.First(n => n.Id == node.Id);

        Assert.True(started);
        Assert.Equal(140, moved.X);
        Assert.Equal(80, moved.Y);
        Assert.Contains("привязан к сетке", vm.HistorySummary, StringComparison.OrdinalIgnoreCase);
    }
}
