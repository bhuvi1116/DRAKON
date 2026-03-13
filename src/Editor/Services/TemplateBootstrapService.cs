using DrakonNx.Core.Templates;
using DrakonNx.Serialization.Json;

namespace DrakonNx.Editor.Services;

public sealed class TemplateBootstrapService
{
    private readonly DiagramJsonSerializer _serializer = new();

    public IReadOnlyList<string> GetTemplateNames()
        => DiagramTemplateCatalog.GetTemplateNames();

    public TemplateBootstrapResult CreateAndSave(string templateName, string projectName, string outputPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(templateName);
        ArgumentException.ThrowIfNullOrWhiteSpace(projectName);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);

        if (!DiagramTemplateCatalog.TryCreate(templateName, out var document) || document is null)
        {
            throw new InvalidOperationException($"Неизвестный шаблон: {templateName}");
        }

        document.Name = projectName.Trim();

        var directory = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(outputPath, _serializer.Serialize(document));
        return new TemplateBootstrapResult(document, outputPath);
    }
}
