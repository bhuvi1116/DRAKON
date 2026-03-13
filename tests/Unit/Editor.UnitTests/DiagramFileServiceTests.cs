using DrakonNx.Core.Services;
using DrakonNx.Editor.Services;

namespace DrakonNx.Editor.UnitTests;

public sealed class DiagramFileServiceTests
{
    [Fact]
    public void SaveThenLoad_PreservesDocumentIdentity()
    {
        var service = new DiagramFileService();
        var document = DiagramFactory.CreateBranchSample();
        var tempRoot = Path.Combine(Path.GetTempPath(), "drakon-nx-editor-tests", Guid.NewGuid().ToString("N"));
        var filePath = Path.Combine(tempRoot, "sample.drakon.json");

        service.Save(document, filePath);
        var loaded = service.Load(filePath);

        Assert.Equal(document.Name, loaded.Name);
        Assert.Equal(document.Nodes.Count, loaded.Nodes.Count);
        Assert.Equal(document.Connections.Count, loaded.Connections.Count);
    }
}
