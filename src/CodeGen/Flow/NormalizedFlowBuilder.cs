using DrakonNx.Core.IR;
using DrakonNx.Core.Model;
using DrakonNx.Validation.Services;

namespace DrakonNx.CodeGen.Flow;

public sealed class NormalizedFlowBuilder
{
    private readonly DiagramValidator _validator = new();

    public FlowNode Build(DiagramDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        var issues = _validator.Validate(document).Where(i => i.Severity == DrakonNx.Validation.Diagnostics.ValidationSeverity.Error).ToList();
        if (issues.Count > 0)
        {
            var message = string.Join(Environment.NewLine, issues.Select(i => $"{i.Code}: {i.Message}"));
            throw new InvalidOperationException($"Невозможно построить IR для некорректной диаграммы:{Environment.NewLine}{message}");
        }

        var start = document.Nodes.Single(n => n.Kind == NodeKind.Start);
        var next = document.Connections.Single(c => c.FromNodeId == start.Id).ToNodeId;
        return BuildFrom(document, next, null);
    }

    private FlowNode BuildFrom(DiagramDocument document, string currentNodeId, string? stopNodeId)
    {
        if (currentNodeId == stopNodeId)
        {
            return new FlowSequence(Array.Empty<FlowNode>());
        }

        var node = document.FindNode(currentNodeId)
            ?? throw new InvalidOperationException($"Узел не найден: {currentNodeId}");

        return node.Kind switch
        {
            NodeKind.Action => BuildAction(document, node, stopNodeId),
            NodeKind.Condition => BuildCondition(document, node, stopNodeId),
            NodeKind.End => new FlowEnd(),
            NodeKind.Start => throw new InvalidOperationException("Внутренний Start после точки входа не допускается."),
            _ => throw new InvalidOperationException($"Неподдерживаемый вид узла: {node.Kind}")
        };
    }

    private FlowNode BuildAction(DiagramDocument document, DiagramNode node, string? stopNodeId)
    {
        var items = new List<FlowNode>
        {
            new FlowStatement(node.Id, node.Text)
        };

        var next = document.Connections.SingleOrDefault(c => c.FromNodeId == node.Id);
        if (next is null || next.ToNodeId == stopNodeId)
        {
            return new FlowSequence(items);
        }

        var tail = BuildFrom(document, next.ToNodeId, stopNodeId);
        Append(items, tail);
        return new FlowSequence(items);
    }

    private FlowNode BuildCondition(DiagramDocument document, DiagramNode node, string? stopNodeId)
    {
        var outgoing = document.Connections.Where(c => c.FromNodeId == node.Id).ToList();
        var trueTarget = outgoing.Single(c => c.FromPort == PortKind.True).ToNodeId;
        var falseTarget = outgoing.Single(c => c.FromPort == PortKind.False).ToNodeId;
        var joinNodeId = FindJoinNodeId(document, trueTarget, falseTarget, stopNodeId);

        var trueBranch = BuildFrom(document, trueTarget, joinNodeId);
        var falseBranch = BuildFrom(document, falseTarget, joinNodeId);

        var items = new List<FlowNode>
        {
            new FlowIf(node.Id, node.Text, trueBranch, falseBranch)
        };

        if (joinNodeId is not null && joinNodeId != stopNodeId)
        {
            var tail = BuildFrom(document, joinNodeId, stopNodeId);
            Append(items, tail);
        }

        return new FlowSequence(items);
    }

    private static string? FindJoinNodeId(DiagramDocument document, string trueTarget, string falseTarget, string? stopNodeId)
    {
        var fromTrue = Reachable(document, trueTarget, stopNodeId);
        var fromFalse = Reachable(document, falseTarget, stopNodeId);
        var intersection = fromTrue.Intersect(fromFalse, StringComparer.Ordinal).ToList();
        if (intersection.Count == 0)
        {
            return null;
        }

        var distanceTrue = Distances(document, trueTarget, stopNodeId);
        var distanceFalse = Distances(document, falseTarget, stopNodeId);

        return intersection
            .OrderBy(id => distanceTrue.GetValueOrDefault(id, int.MaxValue) + distanceFalse.GetValueOrDefault(id, int.MaxValue))
            .ThenBy(id => id, StringComparer.Ordinal)
            .First();
    }

    private static HashSet<string> Reachable(DiagramDocument document, string startNodeId, string? stopNodeId)
    {
        var visited = new HashSet<string>(StringComparer.Ordinal);
        var queue = new Queue<string>();
        visited.Add(startNodeId);
        queue.Enqueue(startNodeId);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            foreach (var next in document.Connections.Where(c => c.FromNodeId == current).Select(c => c.ToNodeId))
            {
                if (next == stopNodeId)
                {
                    continue;
                }

                if (visited.Add(next))
                {
                    queue.Enqueue(next);
                }
            }
        }

        return visited;
    }

    private static Dictionary<string, int> Distances(DiagramDocument document, string startNodeId, string? stopNodeId)
    {
        var distances = new Dictionary<string, int>(StringComparer.Ordinal)
        {
            [startNodeId] = 0
        };
        var queue = new Queue<string>();
        queue.Enqueue(startNodeId);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            var currentDistance = distances[current];
            foreach (var next in document.Connections.Where(c => c.FromNodeId == current).Select(c => c.ToNodeId))
            {
                if (next == stopNodeId)
                {
                    continue;
                }

                if (!distances.ContainsKey(next))
                {
                    distances[next] = currentDistance + 1;
                    queue.Enqueue(next);
                }
            }
        }

        return distances;
    }

    private static void Append(List<FlowNode> items, FlowNode tail)
    {
        if (tail is FlowSequence sequence)
        {
            items.AddRange(sequence.Items);
            return;
        }

        items.Add(tail);
    }
}
