using DrakonNx.Core.Model;
using DrakonNx.Serialization.Json;

namespace DrakonNx.Editor.Services;

public sealed class DiagramFileService
{
    private readonly DiagramJsonSerializer _serializer = new();

    public DiagramDocument Load(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("Файл диаграммы не найден.", filePath);
        }

        var json = File.ReadAllText(filePath);
        return _serializer.Deserialize(json);
    }

    public void Save(DiagramDocument document, string filePath)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = _serializer.Serialize(document);
        File.WriteAllText(filePath, json);
    }
}
