using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using DrakonNx.Editor.Models;
using DrakonNx.Editor.ViewModels;

namespace DrakonNx.Editor.Controls;

public sealed class DiagramCanvasControl : Control
{
    private const double NodeWidth = 150;
    private const double NodeHeight = 56;
    private const double ConnectionHitTolerance = 8;
    private MainWindowViewModel? _viewModel;
    private bool _isDragging;
    private string? _dragNodeId;
    private Point? _connectionPreviewPoint;
    private string? _hoverConnectionTargetNodeId;

    public DiagramCanvasControl()
    {
        ClipToBounds = true;
        Focusable = true;
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        if (_viewModel is not null)
        {
            _viewModel.PropertyChanged -= ViewModelOnPropertyChanged;
        }

        _viewModel = DataContext as MainWindowViewModel;
        if (_viewModel is not null)
        {
            _viewModel.PropertyChanged += ViewModelOnPropertyChanged;
        }

        InvalidateVisual();
        base.OnDataContextChanged(e);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        var vm = _viewModel;
        var bounds = Bounds;
        context.FillRectangle(new SolidColorBrush(Color.FromRgb(250, 250, 252)), bounds);

        if (vm is null)
        {
            return;
        }

        DrawGrid(context, bounds, vm.GridSizeValue);
        DrawLaneGuides(context, bounds, vm.Nodes);
        DrawConnections(context, vm.Nodes, vm.Connections, vm.SelectedConnection?.Id);
        DrawPendingConnectionPreview(context, vm.Nodes, vm.ConnectionFromNode?.Id, vm.SelectedNode?.Id, _connectionPreviewPoint);
        DrawNodes(context, vm.Nodes, vm.SelectedNode?.Id, vm.ConnectionFromNode?.Id, _hoverConnectionTargetNodeId, vm);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        var vm = _viewModel;
        if (vm is null)
        {
            return;
        }

        if (e.Key == Key.Escape && vm.ConnectionFromNode is not null)
        {
            vm.CanvasCancelPendingConnection("Режим создания связи отменен клавишей Escape.");
            _connectionPreviewPoint = null;
            _hoverConnectionTargetNodeId = null;
            e.Handled = true;
            InvalidateVisual();
        }
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        var vm = _viewModel;
        if (vm is null)
        {
            return;
        }

        Focus();
        var point = e.GetPosition(this);
        var properties = e.GetCurrentPoint(this).Properties;
        var hitNode = HitTestNode(vm.Nodes, point);
        var hitConnection = hitNode is null ? HitTestConnection(vm.Nodes, vm.Connections, point) : null;

        if (properties.IsRightButtonPressed)
        {
            HandleContextPress(vm, hitNode, hitConnection, point);
            e.Handled = true;
            return;
        }

        vm.SelectConnectionById(null);
        vm.SelectNodeById(hitNode?.Id);

        if (hitNode is not null && vm.ConnectionFromNode is not null &&
            !string.Equals(vm.ConnectionFromNode.Id, hitNode.Id, StringComparison.Ordinal))
        {
            vm.CanvasConnectFromPendingSourceToNode(hitNode.Id);
            _connectionPreviewPoint = null;
            _dragNodeId = null;
            _isDragging = false;
            e.Handled = true;
            InvalidateVisual();
            return;
        }

        if (hitNode is not null)
        {
            _dragNodeId = hitNode.Id;
            _isDragging = vm.BeginCanvasDrag(hitNode.Id, point.X, point.Y);
            if (_isDragging)
            {
                e.Pointer.Capture(this);
            }
        }
        else
        {
            _dragNodeId = null;
            _isDragging = false;
            if (vm.ConnectionFromNode is not null)
            {
                vm.CanvasCancelPendingConnection("Режим создания связи отменен кликом в пустую область.");
            }
            _connectionPreviewPoint = null;
            _hoverConnectionTargetNodeId = null;
        }

        e.Handled = hitNode is not null || hitConnection is not null;
        InvalidateVisual();
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);

        var vm = _viewModel;
        if (vm is null)
        {
            return;
        }

        var point = e.GetPosition(this);

        if (_isDragging && !string.IsNullOrWhiteSpace(_dragNodeId))
        {
            vm.UpdateCanvasDrag(_dragNodeId, point.X, point.Y);
            e.Handled = true;
            InvalidateVisual();
            return;
        }

        if (vm.ConnectionFromNode is not null)
        {
            _connectionPreviewPoint = point;
            var hoverNode = HitTestNode(vm.Nodes, point);
            _hoverConnectionTargetNodeId = hoverNode is not null && vm.CanvasCanConnectToNode(hoverNode.Id)
                ? hoverNode.Id
                : null;
            e.Handled = true;
            InvalidateVisual();
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        FinishDrag(e);
    }

    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        base.OnPointerCaptureLost(e);
        FinishDrag(null);
    }

    private void ViewModelOnPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        InvalidateVisual();
    }

    private void HandleContextPress(MainWindowViewModel vm, NodeItemViewModel? hitNode, ConnectionItemViewModel? hitConnection, Point point)
    {
        if (hitNode is not null)
        {
            vm.SelectConnectionById(null);
            vm.SelectNodeById(hitNode.Id);
            OpenNodeContextMenu(vm, hitNode);
            return;
        }

        if (hitConnection is not null)
        {
            vm.SelectNodeById(null);
            vm.SelectConnectionById(hitConnection.Id);
            OpenConnectionContextMenu(vm, hitConnection);
            return;
        }

        vm.SelectNodeById(null);
        vm.SelectConnectionById(null);
    }

    private void OpenNodeContextMenu(MainWindowViewModel vm, NodeItemViewModel node)
    {
        var menu = new ContextMenu();
        menu.Items.Add(CreateMenuItem("Использовать как источник связи", (_, _) =>
        {
            vm.CanvasUseNodeAsConnectionSource(node.Id);
            InvalidateVisual();
        }));
        menu.Items.Add(CreateMenuItem("Связать с выбранного источника", (_, _) =>
        {
            vm.CanvasConnectFromPendingSourceToNode(node.Id);
            InvalidateVisual();
        }));
        menu.Items.Add(CreateMenuItem("Удалить узел", (_, _) =>
        {
            vm.CanvasDeleteSelectedNode();
            InvalidateVisual();
        }));
        /*{
            Items = new object[]
            {
                CreateMenuItem("Использовать как источник связи", (_, _) =>
                {
                    vm.CanvasUseNodeAsConnectionSource(node.Id);
                    InvalidateVisual();
                }),
                CreateMenuItem("Связать с выбранного источника", (_, _) =>
                {
                    vm.CanvasConnectFromPendingSourceToNode(node.Id);
                    InvalidateVisual();
                }),
                CreateMenuItem("Удалить узел", (_, _) =>
                {
                    vm.CanvasDeleteSelectedNode();
                    InvalidateVisual();
                })
            }
        };*/

        ContextMenu = menu;
        menu.Open(this);
    }

    private void OpenConnectionContextMenu(MainWindowViewModel vm, ConnectionItemViewModel connection)
    {
        var menu = new ContextMenu();
        menu.Items.Add(CreateMenuItem("Удалить связь", (_, _) =>
        {
            vm.CanvasDeleteSelectedConnection();
            InvalidateVisual();
        }));
        /*{
            Items = new object[]
            {
                CreateMenuItem("Удалить связь", (_, _) =>
                {
                    vm.CanvasDeleteSelectedConnection();
                    InvalidateVisual();
                })
            }
        };*/

        ContextMenu = menu;
        menu.Open(this);
    }

    private static MenuItem CreateMenuItem(string header, EventHandler<RoutedEventArgs> action)
    {
        var item = new MenuItem { Header = header };
        item.Click += action;
        return item;
    }

    private void FinishDrag(PointerReleasedEventArgs? e)
    {
        var vm = _viewModel;
        if (vm is null || !_isDragging || string.IsNullOrWhiteSpace(_dragNodeId))
        {
            _isDragging = false;
            _dragNodeId = null;
            return;
        }

        vm.CompleteCanvasDrag(_dragNodeId);
        e?.Pointer.Capture(null);
        _isDragging = false;
        _dragNodeId = null;
        _connectionPreviewPoint = null;
        _hoverConnectionTargetNodeId = null;
        InvalidateVisual();
    }

    private static void DrawGrid(DrawingContext context, Rect bounds, double gridSize)
    {
        if (gridSize <= 0)
        {
            return;
        }

        var minorPen = new Pen(new SolidColorBrush(Color.FromArgb(18, 110, 110, 118)), 1);
        var majorPen = new Pen(new SolidColorBrush(Color.FromArgb(28, 90, 90, 96)), 1);

        for (double x = 0; x <= bounds.Width; x += gridSize)
        {
            var pen = Math.Abs((x / gridSize) % 4) < 0.01 ? majorPen : minorPen;
            context.DrawLine(pen, new Point(x, 0), new Point(x, bounds.Height));
        }

        for (double y = 0; y <= bounds.Height; y += gridSize)
        {
            var pen = Math.Abs((y / gridSize) % 4) < 0.01 ? majorPen : minorPen;
            context.DrawLine(pen, new Point(0, y), new Point(bounds.Width, y));
        }
    }

    private static void DrawConnections(DrawingContext context, IEnumerable<NodeItemViewModel> nodes, IEnumerable<ConnectionItemViewModel> connections, string? selectedConnectionId)
    {
        var nodeMap = nodes.ToDictionary(n => n.Id, StringComparer.Ordinal);

        foreach (var connection in connections)
        {
            if (!nodeMap.TryGetValue(connection.FromNodeId, out var fromNode) || !nodeMap.TryGetValue(connection.ToNodeId, out var toNode))
            {
                continue;
            }

            var isSelected = string.Equals(connection.Id, selectedConnectionId, StringComparison.Ordinal);
            var pen = isSelected
                ? new Pen(new SolidColorBrush(Color.FromRgb(0, 122, 204)), 3)
                : new Pen(new SolidColorBrush(Color.FromRgb(124, 124, 132)), 2);

            var from = GetConnectionAnchor(fromNode, toNode, true);
            var to = GetConnectionAnchor(toNode, fromNode, false);
            var geometry = CreateConnectionGeometry(from, to);
            context.DrawGeometry(null, pen, geometry);
            DrawArrowHead(context, pen.Brush, from, to);
        }
    }

    private static void DrawPendingConnectionPreview(DrawingContext context, IEnumerable<NodeItemViewModel> nodes, string? sourceNodeId, string? selectedNodeId, Point? pointerPoint)
    {
        if (string.IsNullOrWhiteSpace(sourceNodeId) || pointerPoint is null)
        {
            return;
        }

        var source = nodes.FirstOrDefault(n => string.Equals(n.Id, sourceNodeId, StringComparison.Ordinal));
        if (source is null)
        {
            return;
        }

        var start = new Point(source.X + NodeWidth, source.Y + NodeHeight / 2);
        var pen = new Pen(new SolidColorBrush(Color.FromRgb(0, 122, 204)), 2, dashStyle: DashStyle.Dash);
        var geometry = CreateConnectionGeometry(start, pointerPoint.Value);
        context.DrawGeometry(null, pen, geometry);

        var previewBrush = new SolidColorBrush(Color.FromArgb(160, 0, 122, 204));
        context.DrawEllipse(previewBrush, null, pointerPoint.Value, 4, 4);
    }

    private static void DrawLaneGuides(DrawingContext context, Rect bounds, IEnumerable<NodeItemViewModel> nodes)
    {
        var branches = nodes.Where(n => n.Kind == DrakonNx.Core.Model.NodeKind.BranchStart)
            .OrderBy(n => n.X)
            .ToList();

        if (branches.Count == 0)
        {
            return;
        }

        var pen = new Pen(new SolidColorBrush(Color.FromArgb(48, 70, 70, 78)), 1, dashStyle: DashStyle.Dash);
        foreach (var branch in branches)
        {
            var x = branch.X + NodeWidth / 2;
            context.DrawLine(pen, new Point(x, 0), new Point(x, bounds.Height));
        }
    }

    private static void DrawNodes(
        DrawingContext context,
        IEnumerable<NodeItemViewModel> nodes,
        string? selectedNodeId,
        string? pendingSourceNodeId,
        string? hoverConnectionTargetNodeId,
        MainWindowViewModel viewModel)
    {
        foreach (var node in nodes)
        {
            var rect = new Rect(node.X, node.Y, NodeWidth, NodeHeight);
            var isSelected = string.Equals(node.Id, selectedNodeId, StringComparison.Ordinal);
            var isPendingSource = string.Equals(node.Id, pendingSourceNodeId, StringComparison.Ordinal);
            var isHoverConnectionTarget = string.Equals(node.Id, hoverConnectionTargetNodeId, StringComparison.Ordinal)
                                          && viewModel.CanvasCanConnectToNode(node.Id);
            var fill = GetNodeFill(node.Kind, isSelected, isPendingSource, isHoverConnectionTarget);
            var border = isSelected
                ? new Pen(new SolidColorBrush(Color.FromRgb(0, 120, 215)), 3)
                : isPendingSource
                    ? new Pen(new SolidColorBrush(Color.FromRgb(46, 125, 50)), 3)
                    : isHoverConnectionTarget
                        ? new Pen(new SolidColorBrush(Color.FromRgb(251, 192, 45)), 3)
                        : new Pen(new SolidColorBrush(Color.FromRgb(110, 110, 110)), 1.5);

            DrawNodeShape(context, node, rect, fill, border);

            var text = new FormattedText(
                $"{node.KindDisplayName}: {node.Text}",
                System.Globalization.CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                new Typeface("Inter, Segoe UI, Arial"),
                12,
                Brushes.Black)
            {
                MaxTextWidth = NodeWidth - 18
            };

            context.DrawText(text, new Point(node.X + 9, node.Y + 8));

            var idText = new FormattedText(
                $"{node.Id} · lane {node.Lane}",
                System.Globalization.CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                new Typeface("Inter, Segoe UI, Arial"),
                10,
                new SolidColorBrush(Color.FromRgb(90, 90, 90)));

            context.DrawText(idText, new Point(node.X + 9, node.Y + NodeHeight - 18));

            if (isHoverConnectionTarget)
            {
                var hintText = new FormattedText(
                    "допустимая цель",
                    System.Globalization.CultureInfo.InvariantCulture,
                    FlowDirection.LeftToRight,
                    new Typeface("Inter, Segoe UI, Arial"),
                    10,
                    new SolidColorBrush(Color.FromRgb(130, 100, 0)));

                context.DrawText(hintText, new Point(node.X + 52, node.Y + NodeHeight - 18));
            }
        }
    }

    private static IBrush GetNodeFill(DrakonNx.Core.Model.NodeKind kind, bool isSelected, bool isPendingSource, bool isHoverConnectionTarget)
    {
        if (isSelected)
        {
            return new SolidColorBrush(Color.FromRgb(220, 239, 255));
        }

        if (isPendingSource)
        {
            return new SolidColorBrush(Color.FromRgb(231, 245, 236));
        }

        if (isHoverConnectionTarget)
        {
            return new SolidColorBrush(Color.FromRgb(255, 245, 196));
        }

        return kind switch
        {
            DrakonNx.Core.Model.NodeKind.Title => new SolidColorBrush(Color.FromRgb(232, 234, 246)),
            DrakonNx.Core.Model.NodeKind.BranchStart => new SolidColorBrush(Color.FromRgb(227, 242, 253)),
            DrakonNx.Core.Model.NodeKind.Address => new SolidColorBrush(Color.FromRgb(243, 229, 245)),
            DrakonNx.Core.Model.NodeKind.Question or DrakonNx.Core.Model.NodeKind.Condition => new SolidColorBrush(Color.FromRgb(255, 243, 224)),
            DrakonNx.Core.Model.NodeKind.End => new SolidColorBrush(Color.FromRgb(255, 235, 238)),
            DrakonNx.Core.Model.NodeKind.Select or DrakonNx.Core.Model.NodeKind.Case => new SolidColorBrush(Color.FromRgb(232, 245, 233)),
            _ => new SolidColorBrush(Color.FromRgb(250, 250, 250))
        };
    }

    private static void DrawNodeShape(DrawingContext context, NodeItemViewModel node, Rect rect, IBrush fill, Pen border)
    {
        var shadowRect = new Rect(rect.X, rect.Y + 2, rect.Width, rect.Height);
        var shadowBrush = new SolidColorBrush(Color.FromArgb(18, 0, 0, 0));
        context.DrawRectangle(shadowBrush, null, shadowRect, 10, 10);

        switch (node.Kind)
        {
            case DrakonNx.Core.Model.NodeKind.Title:
                context.DrawRectangle(fill, border, rect, 12, 12);
                break;
            case DrakonNx.Core.Model.NodeKind.Question:
            case DrakonNx.Core.Model.NodeKind.Condition:
                var q = new StreamGeometry();
                using (var g = q.Open())
                {
                    g.BeginFigure(new Point(rect.X + rect.Width / 2, rect.Y + 2), true);
                    g.LineTo(new Point(rect.Right - 8, rect.Y + rect.Height / 2));
                    g.LineTo(new Point(rect.X + rect.Width / 2, rect.Bottom - 2));
                    g.LineTo(new Point(rect.X + 8, rect.Y + rect.Height / 2));
                    g.EndFigure(true);
                }
                context.DrawGeometry(fill, border, q);
                break;
            case DrakonNx.Core.Model.NodeKind.Address:
                var addr = new StreamGeometry();
                using (var g = addr.Open())
                {
                    g.BeginFigure(new Point(rect.X + 14, rect.Y), true);
                    g.LineTo(new Point(rect.Right - 10, rect.Y));
                    g.LineTo(new Point(rect.Right, rect.Y + rect.Height / 2));
                    g.LineTo(new Point(rect.Right - 10, rect.Bottom));
                    g.LineTo(new Point(rect.X + 14, rect.Bottom));
                    g.LineTo(new Point(rect.X, rect.Y + rect.Height / 2));
                    g.EndFigure(true);
                }
                context.DrawGeometry(fill, border, addr);
                break;
            case DrakonNx.Core.Model.NodeKind.End:
                context.DrawRectangle(fill, border, rect, 28, 28);
                break;
            default:
                context.DrawRectangle(fill, border, rect, 10, 10);
                break;
        }
    }

    private static StreamGeometry CreateConnectionGeometry(Point from, Point to)
    {
        var dx = Math.Abs(to.X - from.X);
        var controlOffset = Math.Max(42, dx * 0.45);
        var c1 = new Point(from.X + controlOffset, from.Y);
        var c2 = new Point(to.X - controlOffset, to.Y);

        var geometry = new StreamGeometry();
        using var g = geometry.Open();
        g.BeginFigure(from, false);
        g.CubicBezierTo(c1, c2, to);
        return geometry;
    }

    private static Point GetConnectionAnchor(NodeItemViewModel node, NodeItemViewModel other, bool fromNode)
    {
        var centerY = node.Y + NodeHeight / 2;
        if (other.X >= node.X)
        {
            return new Point(node.X + NodeWidth - 4, centerY);
        }

        return new Point(node.X + 4, centerY);
    }

    private static void DrawArrowHead(DrawingContext context, IBrush? brush, Point from, Point to)
    {
        brush ??= Brushes.Gray;
        var vx = to.X - from.X;
        var vy = to.Y - from.Y;
        var length = Math.Sqrt(vx * vx + vy * vy);
        if (length < 0.001)
        {
            return;
        }

        vx /= length;
        vy /= length;
        var size = 8.0;
        var px = -vy;
        var py = vx;
        var p1 = new Point(to.X - vx * size + px * 4, to.Y - vy * size + py * 4);
        var p2 = new Point(to.X - vx * size - px * 4, to.Y - vy * size - py * 4);

        var head = new StreamGeometry();
        using var g = head.Open();
        g.BeginFigure(to, true);
        g.LineTo(p1);
        g.LineTo(p2);
        g.EndFigure(true);
        context.DrawGeometry(brush, null, head);
    }

    private static NodeItemViewModel? HitTestNode(IEnumerable<NodeItemViewModel> nodes, Point point)
    {
        foreach (var node in nodes.Reverse())
        {
            if (new Rect(node.X, node.Y, NodeWidth, NodeHeight).Contains(point))
            {
                return node;
            }
        }

        return null;
    }

    private static ConnectionItemViewModel? HitTestConnection(IEnumerable<NodeItemViewModel> nodes, IEnumerable<ConnectionItemViewModel> connections, Point point)
    {
        var nodeMap = nodes.ToDictionary(n => n.Id, StringComparer.Ordinal);

        foreach (var connection in connections.Reverse())
        {
            if (!nodeMap.TryGetValue(connection.FromNodeId, out var fromNode) || !nodeMap.TryGetValue(connection.ToNodeId, out var toNode))
            {
                continue;
            }

            var from = new Point(fromNode.X + NodeWidth / 2, fromNode.Y + NodeHeight / 2);
            var to = new Point(toNode.X + NodeWidth / 2, toNode.Y + NodeHeight / 2);
            if (DistanceToSegment(point, from, to) <= ConnectionHitTolerance)
            {
                return connection;
            }
        }

        return null;
    }

    private static double DistanceToSegment(Point point, Point start, Point end)
    {
        var dx = end.X - start.X;
        var dy = end.Y - start.Y;
        if (Math.Abs(dx) < double.Epsilon && Math.Abs(dy) < double.Epsilon)
        {
            return Math.Sqrt(Math.Pow(point.X - start.X, 2) + Math.Pow(point.Y - start.Y, 2));
        }

        var t = ((point.X - start.X) * dx + (point.Y - start.Y) * dy) / (dx * dx + dy * dy);
        t = Math.Clamp(t, 0, 1);
        var projection = new Point(start.X + t * dx, start.Y + t * dy);
        return Math.Sqrt(Math.Pow(point.X - projection.X, 2) + Math.Pow(point.Y - projection.Y, 2));
    }
}
