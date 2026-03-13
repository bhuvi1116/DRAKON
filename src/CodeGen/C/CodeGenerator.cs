using System.Text.RegularExpressions;
using DrakonNx.CodeGen.Flow;
using DrakonNx.Core.IR;
using DrakonNx.Core.Model;

namespace DrakonNx.CodeGen.C;

public sealed class CodeGenerator
{
    private static readonly Regex AssignmentRegex = new(@"^\s*([A-Za-z_][A-Za-z0-9_]*)\s*=\s*(.+)$", RegexOptions.Compiled);
    private readonly NormalizedFlowBuilder _flowBuilder = new();

    public string Generate(DiagramDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        if (document.Profile == DrakonNx.Core.Model.DiagramProfile.DrakonVisualSpec)
        {
            return string.Join(Environment.NewLine, new[]
            {
                "/* Code generation is disabled for the visual DRAKON specification profile. */",
                "/* Use the editor in visual-first mode: primitive/silhouette authoring, branch layout and icon semantics. */",
                "/* Executable C generation remains available for the ExecutableV0 profile only. */"
            });
        }

        var flow = _flowBuilder.Build(document);
        var declarations = CollectAssignedVariables(flow);

        var printer = new CPrinter();
        return printer.Print(flow, declarations);
    }

    private static IReadOnlyList<string> CollectAssignedVariables(FlowNode root)
    {
        var result = new SortedSet<string>(StringComparer.Ordinal);
        Visit(root, result);
        return result.ToList();
    }

    private static void Visit(FlowNode node, ISet<string> symbols)
    {
        switch (node)
        {
            case FlowSequence sequence:
                foreach (var item in sequence.Items)
                {
                    Visit(item, symbols);
                }
                break;
            case FlowStatement statement:
                var match = AssignmentRegex.Match(statement.Text);
                if (match.Success)
                {
                    symbols.Add(match.Groups[1].Value);
                }
                break;
            case FlowIf flowIf:
                Visit(flowIf.TrueBranch, symbols);
                Visit(flowIf.FalseBranch, symbols);
                break;
            case FlowEnd:
                break;
            default:
                throw new InvalidOperationException($"Неподдерживаемый тип IR: {node.GetType().Name}");
        }
    }
}
