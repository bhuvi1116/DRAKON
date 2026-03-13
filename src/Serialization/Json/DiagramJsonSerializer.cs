using System.Text.Json;
using DrakonNx.Core.Model;
using DrakonNx.Serialization.Mapping;

namespace DrakonNx.Serialization.Json;

public sealed class DiagramJsonSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public string Serialize(DiagramDocument document)
    {
        var dto = DiagramMapper.ToDto(document);
        return JsonSerializer.Serialize(dto, Options);
    }

    public DiagramDocument Deserialize(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            throw new ArgumentException("JSON не должен быть пустым.", nameof(json));
        }

        var dto = JsonSerializer.Deserialize<Dto.DiagramDocumentDto>(json, Options)
            ?? throw new InvalidOperationException("Не удалось десериализовать диаграмму.");

        return DiagramMapper.FromDto(dto);
    }
}
