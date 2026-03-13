using DrakonNx.Core.Layout;
using DrakonNx.Core.Model;
using DrakonNx.Core.Services;

namespace DrakonNx.Tests.Core.UnitTests;

public sealed class DrakonLayoutEngineTests
{
    [Fact]
    public void Apply_PrimitiveLayout_PlacesNodesOnSpine()
    {
        var document = DiagramFactory.CreateMinimalSample();
        var engine = new DrakonLayoutEngine();

        var report = engine.Apply(document);

        Assert.Equal(3, report.RepositionedNodes);
        Assert.All(document.Nodes, node => Assert.Equal(220, node.X));
        Assert.True(document.Nodes[1].Y > document.Nodes[0].Y);
    }

    [Fact]
    public void Apply_SilhouetteLayout_KeepsBranchLaneOnRight()
    {
        var document = new DiagramDocument("Silhouette")
        {
            Profile = DiagramProfile.DrakonVisualSpec,
            LayoutMode = DiagramLayoutMode.Silhouette
        };

        document.Nodes.Add(new DiagramNode("title", NodeKind.Title, "Алгоритм", 40, 40, 0));
        document.Nodes.Add(new DiagramNode("q", NodeKind.Question, "Условие?", 40, 140, 0));
        document.Nodes.Add(new DiagramNode("branch", NodeKind.BranchStart, "Да", 40, 240, 1));
        document.Nodes.Add(new DiagramNode("addr", NodeKind.Address, "Переход", 40, 340, 1));
        document.Nodes.Add(new DiagramNode("end", NodeKind.End, "Конец", 40, 440, 0));

        document.Connections.Add(new DiagramConnection("c1", "title", PortKind.Bottom, "q", PortKind.Top));
        document.Connections.Add(new DiagramConnection("c2", "q", PortKind.True, "branch", PortKind.Top));
        document.Connections.Add(new DiagramConnection("c3", "branch", PortKind.Bottom, "addr", PortKind.Top));
        document.Connections.Add(new DiagramConnection("c4", "addr", PortKind.Bottom, "end", PortKind.Top));

        var engine = new DrakonLayoutEngine();
        var report = engine.Apply(document);

        Assert.True(report.AppliedSilhouette);
        Assert.Equal(430, document.FindNode("branch")!.X);
        Assert.Equal(430, document.FindNode("addr")!.X);
        Assert.Equal(220, document.FindNode("q")!.X);
    }

    [Fact]
    public void Apply_SilhouetteLayout_FlagsNegativeLane()
    {
        var document = new DiagramDocument("Silhouette")
        {
            Profile = DiagramProfile.DrakonVisualSpec,
            LayoutMode = DiagramLayoutMode.Silhouette
        };

        document.Nodes.Add(new DiagramNode("title", NodeKind.Title, "Алгоритм", 0, 0, 0));
        document.Nodes.Add(new DiagramNode("bad", NodeKind.Action, "Левая ветвь", 0, 0, -1));
        document.Connections.Add(new DiagramConnection("c1", "title", PortKind.Bottom, "bad", PortKind.Top));

        var report = new DrakonLayoutEngine().Apply(document);

        Assert.Contains(report.Issues, issue => issue.Code == "DRAKON_LAYOUT_NEGATIVE_LANE");
        Assert.Equal(220, document.FindNode("bad")!.X);
    }
}
