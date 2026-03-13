using DrakonNx.Core.Model;
using DrakonNx.Validation.Diagnostics;

namespace DrakonNx.Validation.Services;

public sealed class DiagramValidator
{
    public IReadOnlyList<ValidationIssue> Validate(DiagramDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        var issues = new List<ValidationIssue>();

        var versionIssue = FormatVersionValidator.Validate(document);
        if (versionIssue is not null)
        {
            issues.Add(versionIssue);
        }

        ValidateNodeIds(document, issues);
        ValidateConnectionIds(document, issues);
        if (document.Profile == DiagramProfile.DrakonVisualSpec)
        {
            ValidateConnections(document, issues);
            DrakonVisualSpecValidator.Validate(document, issues);
            return issues;
        }

        ValidateStructuralRules(document, issues);
        ValidateConnections(document, issues);
        ValidateReachability(document, issues);

        return issues;
    }

    private static void ValidateNodeIds(DiagramDocument document, List<ValidationIssue> issues)
    {
        foreach (var duplicate in document.Nodes.GroupBy(n => n.Id).Where(g => g.Count() > 1))
        {
            issues.Add(new ValidationIssue("VAL001", $"Дублирующийся идентификатор узла: {duplicate.Key}", ValidationSeverity.Error, duplicate.Key));
        }
    }

    private static void ValidateConnectionIds(DiagramDocument document, List<ValidationIssue> issues)
    {
        foreach (var duplicate in document.Connections.GroupBy(c => c.Id).Where(g => g.Count() > 1))
        {
            issues.Add(new ValidationIssue("VAL002", $"Дублирующийся идентификатор связи: {duplicate.Key}", ValidationSeverity.Error, ConnectionId: duplicate.Key));
        }
    }

    private static void ValidateStructuralRules(DiagramDocument document, List<ValidationIssue> issues)
    {
        var starts = document.Nodes.Where(n => n.Kind == NodeKind.Start).ToList();
        if (starts.Count != 1)
        {
            issues.Add(new ValidationIssue("VAL100", "Диаграмма должна содержать ровно один узел Start.", ValidationSeverity.Error));
        }

        var ends = document.Nodes.Where(n => n.Kind == NodeKind.End).ToList();
        if (ends.Count == 0)
        {
            issues.Add(new ValidationIssue("VAL101", "Диаграмма должна содержать хотя бы один узел End.", ValidationSeverity.Error));
        }

        foreach (var node in document.Nodes)
        {
            var incoming = document.Connections.Count(c => c.ToNodeId == node.Id);
            var outgoing = document.Connections.Where(c => c.FromNodeId == node.Id).ToList();

            switch (node.Kind)
            {
                case NodeKind.Start:
                    if (incoming > 0)
                    {
                        issues.Add(new ValidationIssue("VAL110", "Узел Start не должен иметь входящих связей.", ValidationSeverity.Error, node.Id));
                    }

                    if (outgoing.Count != 1)
                    {
                        issues.Add(new ValidationIssue("VAL111", "Узел Start должен иметь ровно одну исходящую связь.", ValidationSeverity.Error, node.Id));
                    }
                    break;

                case NodeKind.End:
                    if (outgoing.Count > 0)
                    {
                        issues.Add(new ValidationIssue("VAL112", "Узел End не должен иметь исходящих связей.", ValidationSeverity.Error, node.Id));
                    }
                    break;

                case NodeKind.Condition:
                    var trueCount = outgoing.Count(c => c.FromPort == PortKind.True);
                    var falseCount = outgoing.Count(c => c.FromPort == PortKind.False);
                    if (trueCount != 1 || falseCount != 1 || outgoing.Count != 2)
                    {
                        issues.Add(new ValidationIssue("VAL113", "Узел Condition должен иметь ровно две ветви: True и False.", ValidationSeverity.Error, node.Id));
                    }
                    break;
            }
        }
    }

    private static void ValidateConnections(DiagramDocument document, List<ValidationIssue> issues)
    {
        foreach (var connection in document.Connections)
        {
            var from = document.FindNode(connection.FromNodeId);
            var to = document.FindNode(connection.ToNodeId);

            if (from is null)
            {
                issues.Add(new ValidationIssue("VAL200", $"Связь ссылается на отсутствующий исходный узел: {connection.FromNodeId}", ValidationSeverity.Error, ConnectionId: connection.Id));
                continue;
            }

            if (to is null)
            {
                issues.Add(new ValidationIssue("VAL201", $"Связь ссылается на отсутствующий целевой узел: {connection.ToNodeId}", ValidationSeverity.Error, ConnectionId: connection.Id));
                continue;
            }

            if (connection.ToPort != PortKind.In)
            {
                issues.Add(new ValidationIssue("VAL202", "Целевой порт связи должен быть входным.", ValidationSeverity.Error, ConnectionId: connection.Id));
            }

            if (from.Kind != NodeKind.Condition && connection.FromPort is PortKind.True or PortKind.False)
            {
                issues.Add(new ValidationIssue("VAL203", "Только узел Condition может иметь исходящие порты True/False.", ValidationSeverity.Error, ConnectionId: connection.Id));
            }

            if (from.Kind == NodeKind.Condition && connection.FromPort == PortKind.Out)
            {
                issues.Add(new ValidationIssue("VAL204", "Узел Condition не использует обычный выходной порт Out, только True/False.", ValidationSeverity.Error, ConnectionId: connection.Id));
            }
        }
    }

    private static void ValidateReachability(DiagramDocument document, List<ValidationIssue> issues)
    {
        var start = document.Nodes.SingleOrDefault(n => n.Kind == NodeKind.Start);
        if (start is null)
        {
            return;
        }

        var reachable = new HashSet<string>(StringComparer.Ordinal);
        var queue = new Queue<string>();
        reachable.Add(start.Id);
        queue.Enqueue(start.Id);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            foreach (var next in document.Connections.Where(c => c.FromNodeId == current).Select(c => c.ToNodeId))
            {
                if (reachable.Add(next))
                {
                    queue.Enqueue(next);
                }
            }
        }

        foreach (var node in document.Nodes.Where(n => !reachable.Contains(n.Id)))
        {
            issues.Add(new ValidationIssue("VAL300", "Обнаружен недостижимый узел.", ValidationSeverity.Error, node.Id));
        }
    }
}
