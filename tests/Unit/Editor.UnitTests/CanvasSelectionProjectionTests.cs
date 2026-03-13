using DrakonNx.Core.Services;
using DrakonNx.Editor.ViewModels;
using Xunit;

namespace DrakonNx.Editor.UnitTests;

public sealed class CanvasSelectionProjectionTests
{
    [Fact]
    public void SelectNodeById_UpdatesSelectedNodeAndCanvasSummary()
    {
        var viewModel = new MainWindowViewModel();
        var targetId = viewModel.Nodes.First().Id;

        viewModel.SelectNodeById(targetId);

        Assert.NotNull(viewModel.SelectedNode);
        Assert.Equal(targetId, viewModel.SelectedNode!.Id);
        Assert.Contains(targetId, viewModel.CanvasSummary);
    }

    [Fact]
    public void GridSizeValue_InvalidText_FallsBackToDefault()
    {
        var viewModel = new MainWindowViewModel();
        viewModel.GridSize = "abc";

        Assert.Equal(20d, viewModel.GridSizeValue);
    }
}
