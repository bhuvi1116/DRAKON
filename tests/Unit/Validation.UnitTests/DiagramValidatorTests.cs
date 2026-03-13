using DrakonNx.Core.Model;
using DrakonNx.Core.Services;
using DrakonNx.Validation.Services;

namespace DrakonNx.Validation.UnitTests;

public sealed class DiagramValidatorTests
{
    private readonly DiagramValidator _validator = new();

    [Fact]
    public void Validate_MinimalSample_HasNoIssues()
    {
        var document = DiagramFactory.CreateMinimalSample();
        var issues = _validator.Validate(document);
        Assert.Empty(issues);
    }

    [Fact]
    public void Validate_MissingEnd_ReturnsError()
    {
        var document = new DiagramDocument("Broken");
        document.Nodes.Add(new DiagramNode("start", NodeKind.Start, "Старт"));
        document.Nodes.Add(new DiagramNode("action", NodeKind.Action, "x = 1"));
        document.Connections.Add(new DiagramConnection("c1", "start", PortKind.Out, "action", PortKind.In));

        var issues = _validator.Validate(document);

        Assert.Contains(issues, i => i.Code == "VAL101");
    }

    [Fact]
    public void Validate_UnreachableNode_ReturnsError()
    {
        var document = DiagramFactory.CreateMinimalSample();
        document.Nodes.Add(new DiagramNode("orphan", NodeKind.Action, "y = 2"));

        var issues = _validator.Validate(document);

        Assert.Contains(issues, i => i.Code == "VAL300" && i.NodeId == "orphan");
    }

    [Fact]
    public void Validate_ConditionMissingFalse_ReturnsError()
    {
        var document = new DiagramDocument("Broken Condition");
        document.Nodes.Add(new DiagramNode("start", NodeKind.Start, "Старт"));
        document.Nodes.Add(new DiagramNode("condition", NodeKind.Condition, "x > 0"));
        document.Nodes.Add(new DiagramNode("end", NodeKind.End, "Конец"));
        document.Connections.Add(new DiagramConnection("c1", "start", PortKind.Out, "condition", PortKind.In));
        document.Connections.Add(new DiagramConnection("c2", "condition", PortKind.True, "end", PortKind.In));

        var issues = _validator.Validate(document);

        Assert.Contains(issues, i => i.Code == "VAL113");
    }
}
