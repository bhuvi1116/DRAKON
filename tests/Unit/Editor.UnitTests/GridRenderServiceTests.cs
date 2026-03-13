using DrakonNx.Editor.Services;

namespace DrakonNx.Editor.UnitTests;

public sealed class GridRenderServiceTests
{
    [Fact]
    public void CreateLines_CreatesExpectedVerticalAndHorizontalLines()
    {
        var service = new GridRenderService();

        var lines = service.CreateLines(100, 60, 20);

        Assert.Equal(10, lines.Count);
        Assert.Equal(6, lines.Count(l => l.Orientation == GridLineOrientation.Vertical));
        Assert.Equal(4, lines.Count(l => l.Orientation == GridLineOrientation.Horizontal));
    }

    [Fact]
    public void CreateLines_WithInvalidGrid_Throws()
    {
        var service = new GridRenderService();

        Assert.Throws<ArgumentOutOfRangeException>(() => service.CreateLines(100, 100, 0));
    }
}
