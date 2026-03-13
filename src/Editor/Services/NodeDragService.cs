namespace DrakonNx.Editor.Services;

public sealed class NodeDragService
{
    public DragSession Begin(double originX, double originY, double pointerX, double pointerY)
    {
        return new DragSession(originX, originY, pointerX, pointerY);
    }

    public (double X, double Y) Update(DragSession session, double pointerX, double pointerY)
    {
        ArgumentNullException.ThrowIfNull(session);
        return (session.OriginX + (pointerX - session.PointerStartX), session.OriginY + (pointerY - session.PointerStartY));
    }

    public (double X, double Y) Snap((double X, double Y) position, double gridSize)
    {
        if (gridSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(gridSize), "Размер сетки должен быть больше нуля.");
        }

        return (
            Math.Round(position.X / gridSize) * gridSize,
            Math.Round(position.Y / gridSize) * gridSize);
    }
}

public sealed class DragSession
{
    public DragSession(double originX, double originY, double pointerStartX, double pointerStartY)
    {
        OriginX = originX;
        OriginY = originY;
        PointerStartX = pointerStartX;
        PointerStartY = pointerStartY;
    }

    public double OriginX { get; }
    public double OriginY { get; }
    public double PointerStartX { get; }
    public double PointerStartY { get; }
}
