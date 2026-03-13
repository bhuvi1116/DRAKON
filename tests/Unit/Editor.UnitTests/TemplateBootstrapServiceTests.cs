using DrakonNx.Editor.Services;

namespace DrakonNx.Tests.Unit.Editor;

public sealed class TemplateBootstrapServiceTests
{
    [Fact]
    public void CreateAndSave_WritesDiagramFileAndAppliesProjectName()
    {
        var service = new TemplateBootstrapService();
        var tempDirectory = Path.Combine(Path.GetTempPath(), "drakon-nx-editor-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDirectory);
        var outputPath = Path.Combine(tempDirectory, "branch.drakon.json");

        var result = service.CreateAndSave("simple-branch", "branch-demo", outputPath);

        Assert.True(File.Exists(outputPath));
        Assert.Equal("branch-demo", result.Document.Name);
        Assert.Equal(outputPath, result.OutputPath);

        var text = File.ReadAllText(outputPath);
        Assert.Contains("branch-demo", text, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("version", text, StringComparison.OrdinalIgnoreCase);
    }
}
