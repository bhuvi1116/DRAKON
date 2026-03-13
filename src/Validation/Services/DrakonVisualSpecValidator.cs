using DrakonNx.Core.Model;
using DrakonNx.Validation.Diagnostics;

namespace DrakonNx.Validation.Services;

public static class DrakonVisualSpecValidator
{
    public static void Validate(DiagramDocument document, List<ValidationIssue> issues)
    {
        var titles = document.Nodes.Where(n => n.Kind == NodeKind.Title).ToList();
        if (titles.Count != 1)
        {
            issues.Add(new ValidationIssue("DVS100", "Для визуальной ДРАКОН-схемы нужен ровно один заголовок.", ValidationSeverity.Error));
        }

        if (!document.Nodes.Any(n => n.Kind == NodeKind.End))
        {
            issues.Add(new ValidationIssue("DVS101", "Для визуальной ДРАКОН-схемы нужна икона 'конец'.", ValidationSeverity.Error));
        }

        if (document.LayoutMode == DiagramLayoutMode.Silhouette)
        {
            var branches = document.Nodes.Where(n => n.Kind == NodeKind.BranchStart).OrderBy(n => n.X).ToList();
            if (branches.Count == 0)
            {
                issues.Add(new ValidationIssue("DVS110", "Силуэт должен содержать хотя бы одну икону 'имя ветки'.", ValidationSeverity.Error));
            }

            for (var i = 1; i < branches.Count; i++)
            {
                if (branches[i].X <= branches[i - 1].X)
                {
                    issues.Add(new ValidationIssue("DVS111", "Для силуэта должно выполняться правило 'чем правее — тем позже'.", ValidationSeverity.Warning, branches[i].Id));
                }
            }

            foreach (var branch in branches)
            {
                var hasExit = document.Connections.Any(c => c.FromNodeId == branch.Id);
                if (!hasExit)
                {
                    issues.Add(new ValidationIssue("DVS112", "Икона 'имя ветки' должна вести в тело ветки.", ValidationSeverity.Error, branch.Id));
                }
            }
        }

        foreach (var node in document.Nodes.Where(n => n.Kind == NodeKind.Question || n.Kind == NodeKind.Condition))
        {
            var outgoing = document.Connections.Where(c => c.FromNodeId == node.Id).ToList();
            if (outgoing.Count < 2)
            {
                issues.Add(new ValidationIssue("DVS120", "Икона 'вопрос' должна иметь не менее двух выходов.", ValidationSeverity.Warning, node.Id));
            }
        }
    }
}
