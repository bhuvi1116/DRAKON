using DrakonNx.Core.Model;
using DrakonNx.Core.Services;
using DrakonNx.Editor.Services;

namespace DrakonNx.Editor.UnitTests;

public sealed class DiagramEditServiceTests
{
    private readonly DiagramEditService _service = new();

    [Fact]
    public void AddNode_AppendsNodeToDocument()
    {
        var document = DiagramFactory.CreateMinimalSample();

        var node = _service.AddNode(document, NodeKind.Action, "z = 42");

        Assert.Contains(document.Nodes, n => n.Id == node.Id && n.Text == "z = 42");
    }

    [Fact]
    public void DeleteNode_RemovesConnectedEdges()
    {
        var document = DiagramFactory.CreateMaxOfTwoSample();
        var target = document.Nodes.Single(n => n.Kind == NodeKind.Condition);

        var removed = _service.DeleteNode(document, target.Id);

        Assert.True(removed);
        Assert.DoesNotContain(document.Nodes, n => n.Id == target.Id);
        Assert.DoesNotContain(document.Connections, c => c.FromNodeId == target.Id || c.ToNodeId == target.Id);
    }

    [Fact]
    public void UpdateNodeText_ChangesExistingNode()
    {
        var document = DiagramFactory.CreateMinimalSample();
        var action = document.Nodes.Single(n => n.Kind == NodeKind.Action);

        var updated = _service.UpdateNodeText(document, action.Id, "value = 10");

        Assert.True(updated);
        Assert.Equal("value = 10", action.Text);
    }

    [Fact]
    public void AddConnection_CreatesNewConnection()
    {
        var document = new DiagramDocument("test");
        document.Nodes.Add(new DiagramNode("start_1", NodeKind.Start, "start"));
        document.Nodes.Add(new DiagramNode("end_1", NodeKind.End, "end"));

        var connection = _service.AddConnection(document, "start_1", PortKind.Out, "end_1", PortKind.In);

        Assert.Contains(document.Connections, c => c.Id == connection.Id);
        Assert.Equal("start_1", connection.FromNodeId);
        Assert.Equal("end_1", connection.ToNodeId);
    }

    [Fact]
    public void DeleteConnection_RemovesConnection()
    {
        var document = DiagramFactory.CreateMinimalSample();
        var connection = document.Connections.First();

        var removed = _service.DeleteConnection(document, connection.Id);

        Assert.True(removed);
        Assert.DoesNotContain(document.Connections, c => c.Id == connection.Id);
    }

    [Fact]
    public void UpdateNodePosition_ChangesCoordinates()
    {
        var document = DiagramFactory.CreateMinimalSample();
        var action = document.Nodes.Single(n => n.Kind == NodeKind.Action);

        var updated = _service.UpdateNodePosition(document, action.Id, 120, 80);

        Assert.True(updated);
        Assert.Equal(120, action.X);
        Assert.Equal(80, action.Y);
    }

    [Fact]
    public void SnapNodeToGrid_RoundsCoordinates()
    {
        var document = DiagramFactory.CreateMinimalSample();
        var action = document.Nodes.Single(n => n.Kind == NodeKind.Action);
        action.X = 53;
        action.Y = 67;

        var updated = _service.SnapNodeToGrid(document, action.Id, 20);

        Assert.True(updated);
        Assert.Equal(60, action.X);
        Assert.Equal(60, action.Y);
    }

}
