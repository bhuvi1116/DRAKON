using System.Text;
using System.Text.RegularExpressions;
using DrakonNx.Core.IR;

namespace DrakonNx.CodeGen.C;

public sealed class CPrinter
{
    private static readonly Regex AssignmentRegex = new(@"^\s*([A-Za-z_][A-Za-z0-9_]*)\s*=\s*(.+)$", RegexOptions.Compiled);
    private static readonly Regex PrintRegex = new(@"^\s*print\((.+)\)\s*$", RegexOptions.Compiled);

    public string Print(FlowNode root, IReadOnlyList<string> declarations)
    {
        ArgumentNullException.ThrowIfNull(root);
        ArgumentNullException.ThrowIfNull(declarations);

        var sb = new StringBuilder();
        sb.AppendLine("#include <stdio.h>");
        sb.AppendLine();
        sb.AppendLine("int main(void)");
        sb.AppendLine("{");

        foreach (var declaration in declarations)
        {
            AppendIndent(sb, 1);
            sb.Append("int ").Append(declaration).AppendLine(" = 0;");
        }

        if (declarations.Count > 0)
        {
            sb.AppendLine();
        }

        PrintNode(sb, root, 1);

        AppendIndent(sb, 1);
        sb.AppendLine("return 0;");
        sb.AppendLine("}");
        return sb.ToString();
    }

    private static void PrintNode(StringBuilder sb, FlowNode node, int indent)
    {
        switch (node)
        {
            case FlowSequence sequence:
                foreach (var item in sequence.Items)
                {
                    PrintNode(sb, item, indent);
                }
                break;
            case FlowStatement statement:
                AppendIndent(sb, indent);
                sb.Append("/* node: ").Append(statement.NodeId).AppendLine(" */");
                AppendIndent(sb, indent);
                sb.AppendLine(TranslateStatement(statement.Text));
                break;
            case FlowIf flowIf:
                AppendIndent(sb, indent);
                sb.Append("/* node: ").Append(flowIf.NodeId).AppendLine(" */");
                AppendIndent(sb, indent);
                sb.Append("if (").Append(flowIf.Condition).AppendLine(")");
                AppendIndent(sb, indent);
                sb.AppendLine("{");
                PrintNode(sb, flowIf.TrueBranch, indent + 1);
                AppendIndent(sb, indent);
                sb.AppendLine("}");
                AppendIndent(sb, indent);
                sb.AppendLine("else");
                AppendIndent(sb, indent);
                sb.AppendLine("{");
                PrintNode(sb, flowIf.FalseBranch, indent + 1);
                AppendIndent(sb, indent);
                sb.AppendLine("}");
                break;
            case FlowEnd:
                break;
            default:
                throw new InvalidOperationException($"Неподдерживаемый тип IR: {node.GetType().Name}");
        }
    }

    private static string TranslateStatement(string text)
    {
        var assignment = AssignmentRegex.Match(text);
        if (assignment.Success)
        {
            return $"{assignment.Groups[1].Value} = {assignment.Groups[2].Value.Trim()};";
        }

        var print = PrintRegex.Match(text);
        if (print.Success)
        {
            var value = print.Groups[1].Value.Trim();
            if (value.StartsWith('"') && value.EndsWith('"'))
            {
                return $"printf(\"%s\\n\", {value});";
            }

            return $"printf(\"%d\\n\", {value});";
        }

        var trimmed = text.Trim();
        return trimmed.EndsWith(';') ? trimmed : trimmed + ';';
    }

    private static void AppendIndent(StringBuilder sb, int indent)
    {
        sb.Append(' ', indent * 4);
    }
}
