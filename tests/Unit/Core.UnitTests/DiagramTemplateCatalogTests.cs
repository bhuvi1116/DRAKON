using DrakonNx.Core.Templates;

namespace DrakonNx.Tests.Unit.Core;

public sealed class DiagramTemplateCatalogTests
{
    [Fact]
    public void GetTemplateNames_ContainsExpectedTemplates()
    {
        var names = DiagramTemplateCatalog.GetTemplateNames();

        Assert.Contains("hello-world", names);
        Assert.Contains("minimal", names);
        Assert.Contains("simple-branch", names);
        Assert.Contains("max-of-two", names);
    }

    [Fact]
    public void TryCreate_KnownTemplate_ReturnsDocument()
    {
        var created = DiagramTemplateCatalog.TryCreate("hello-world", out var document);

        Assert.True(created);
        Assert.NotNull(document);
        Assert.NotEmpty(document!.Nodes);
        Assert.NotEmpty(document.Connections);
    }
}
