using DrakonNx.Core.Model;
using DrakonNx.Core.Services;

namespace DrakonNx.Tests.Unit.Core;

public sealed class DiagramFactoryTests
{
    [Fact]
    public void CreateMinimalSample_CreatesExpectedNodesAndConnections()
    {
        var document = DiagramFactory.CreateMinimalSample();

        Assert.Equal("Minimal Sample", document.Name);
        Assert.Equal(3, document.Nodes.Count);
        Assert.Equal(2, document.Connections.Count);
        Assert.Contains(document.Nodes, n => n.Kind == NodeKind.Start);
        Assert.Contains(document.Nodes, n => n.Kind == NodeKind.End);
    }
}
