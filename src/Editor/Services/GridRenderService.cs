namespace DrakonNx.Editor.Services;

public sealed class GridRenderService
{
    public IReadOnlyList<GridLine> CreateLines(double width, double height, double gridSize)
    {
        if (width < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(width));
        }

        if (height < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(height));
        }

        if (gridSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(gridSize), "Размер сетки должен быть больше нуля.");
        }

        var lines = new List<GridLine>();

        for (double x = 0; x <= width; x += gridSize)
        {
            lines.Add(new GridLine(x, 0, x, height, GridLineOrientation.Vertical));
        }

        for (double y = 0; y <= height; y += gridSize)
        {
            lines.Add(new GridLine(0, y, width, y, GridLineOrientation.Horizontal));
        }

        return lines;
    }
}

public sealed class GridLine
{
    public GridLine(double x1, double y1, double x2, double y2, GridLineOrientation orientation)
    {
        X1 = x1;
        Y1 = y1;
        X2 = x2;
        Y2 = y2;
        Orientation = orientation;
    }

    public double X1 { get; }
    public double Y1 { get; }
    public double X2 { get; }
    public double Y2 { get; }
    public GridLineOrientation Orientation { get; }
}

public enum GridLineOrientation
{
    Vertical = 0,
    Horizontal = 1
}
