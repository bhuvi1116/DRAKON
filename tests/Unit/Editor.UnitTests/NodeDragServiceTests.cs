using DrakonNx.Editor.Services;

namespace DrakonNx.Editor.UnitTests;

public sealed class NodeDragServiceTests
{
    [Fact]
    public void Update_ReturnsTranslatedCoordinates()
    {
        var service = new NodeDragService();
        var session = service.Begin(100, 50, 10, 10);

        var result = service.Update(session, 30, 45);

        Assert.Equal(120, result.X);
        Assert.Equal(85, result.Y);
    }

    [Fact]
    public void Snap_RoundsToNearestGridPoint()
    {
        var service = new NodeDragService();

        var snapped = service.Snap((53, 67), 20);

        Assert.Equal(60, snapped.X);
        Assert.Equal(60, snapped.Y);
    }
}
