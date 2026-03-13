using DrakonNx.Core.Model;

namespace DrakonNx.Core.Layout;

public sealed class DrakonLayoutEngine
{
    public DrakonLayoutReport Apply(DiagramDocument document, DrakonLayoutOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(document);
        options ??= new DrakonLayoutOptions();

        var report = new DrakonLayoutReport
        {
            AppliedSilhouette = document.LayoutMode == DiagramLayoutMode.Silhouette
        };

        var ordered = OrderNodes(document);
        var indexById = ordered.Select((node, index) => new { node.Id, Index = index })
            .ToDictionary(x => x.Id, x => x.Index, StringComparer.Ordinal);

        for (var i = 0; i < ordered.Count; i++)
        {
            var node = ordered[i];
            var lane = NormalizeLane(node, document, report);
            var targetX = options.SpineX + lane * options.LaneWidth;
            var targetY = options.TopY + i * options.RowHeight;

            if (!NearlyEqual(node.X, targetX) || !NearlyEqual(node.Y, targetY))
            {
                node.X = targetX;
                node.Y = targetY;
                report.RepositionedNodes++;
            }
        }

        if (options.KeepRightwardProgression)
        {
            foreach (var connection in document.Connections)
            {
                if (!indexById.TryGetValue(connection.FromNodeId, out var fromIndex) ||
                    !indexById.TryGetValue(connection.ToNodeId, out var toIndex))
                {
                    continue;
                }

                if (toIndex < fromIndex)
                {
                    report.Issues.Add(new DrakonLayoutIssue(
                        "DRAKON_LAYOUT_REVERSE_FLOW",
                        "Обнаружено обратное движение по шампуру. Для visual-first профиля соединения должны читаться сверху вниз и слева направо.",
                        connection.ToNodeId));
                }
            }
        }

        return report;
    }

    private static List<DiagramNode> OrderNodes(DiagramDocument document)
    {
        if (document.Nodes.Count == 0)
        {
            return new List<DiagramNode>();
        }

        var start = document.Nodes.FirstOrDefault(n => n.Kind is NodeKind.Start or NodeKind.Title) ?? document.Nodes[0];
        var visited = new HashSet<string>(StringComparer.Ordinal);
        var ordered = new List<DiagramNode>();
        Visit(start, document, visited, ordered);

        foreach (var node in document.Nodes.OrderBy(n => n.Y).ThenBy(n => n.X).ThenBy(n => n.Id, StringComparer.Ordinal))
        {
            if (visited.Add(node.Id))
            {
                ordered.Add(node);
            }
        }

        return ordered;
    }

    private static void Visit(DiagramNode node, DiagramDocument document, HashSet<string> visited, List<DiagramNode> ordered)
    {
        if (!visited.Add(node.Id))
        {
            return;
        }

        ordered.Add(node);

        var outgoing = document.Connections
            .Where(c => c.FromNodeId == node.Id)
            .OrderBy(c => PortPriority(c.FromPort))
            .ThenBy(c => c.Id, StringComparer.Ordinal)
            .ToList();

        foreach (var connection in outgoing)
        {
            var next = document.FindNode(connection.ToNodeId);
            if (next is not null)
            {
                Visit(next, document, visited, ordered);
            }
        }
    }

    private static int NormalizeLane(DiagramNode node, DiagramDocument document, DrakonLayoutReport report)
    {
        if (node.Kind == NodeKind.BranchStart)
        {
            return Math.Max(node.Lane, 1);
        }

        if (node.Kind is NodeKind.Question or NodeKind.Condition)
        {
            return 0;
        }

        if (node.Kind == NodeKind.Address)
        {
            if (node.Lane == 0)
            {
                report.Issues.Add(new DrakonLayoutIssue(
                    "DRAKON_LAYOUT_ADDRESS_LANE",
                    "Икона 'Адрес' должна быть вынесена в правую ветвь, а не оставаться на шампуре.",
                    node.Id));
                return 1;
            }
        }

        if (document.LayoutMode == DiagramLayoutMode.Silhouette && node.Lane < 0)
        {
            report.Issues.Add(new DrakonLayoutIssue(
                "DRAKON_LAYOUT_NEGATIVE_LANE",
                "Силуэтный layout не поддерживает отрицательные ветви. Левее шампура иконы располагаться не должны.",
                node.Id));
            return 0;
        }

        return node.Lane;
    }

    private static int PortPriority(PortKind port) => port switch
    {
        PortKind.Top => 0,
        PortKind.Right => 1,
        PortKind.True => 2,
        PortKind.Bottom => 3,
        PortKind.Left => 4,
        PortKind.False => 5,
        _ => 10
    };

    private static bool NearlyEqual(double left, double right) => Math.Abs(left - right) < 0.001;
}
