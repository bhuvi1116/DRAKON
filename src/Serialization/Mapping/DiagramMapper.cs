using DrakonNx.Core.Model;
using DrakonNx.Serialization.Dto;

namespace DrakonNx.Serialization.Mapping;

public static class DiagramMapper
{
    public static DiagramDocumentDto ToDto(DiagramDocument document)
    {
        return new DiagramDocumentDto
        {
            Version = document.Version,
            Name = document.Name,
            Profile = document.Profile.ToString(),
            LayoutMode = document.LayoutMode.ToString(),
            Nodes = document.Nodes.Select(n => new DiagramNodeDto
            {
                Id = n.Id,
                Kind = n.Kind.ToString(),
                Text = n.Text,
                X = n.X,
                Y = n.Y,
                Lane = n.Lane
            }).ToList(),
            Connections = document.Connections.Select(c => new DiagramConnectionDto
            {
                Id = c.Id,
                FromNodeId = c.FromNodeId,
                FromPort = c.FromPort.ToString(),
                ToNodeId = c.ToNodeId,
                ToPort = c.ToPort.ToString()
            }).ToList()
        };
    }

    public static DiagramDocument FromDto(DiagramDocumentDto dto)
    {
        var document = new DiagramDocument(dto.Name)
        {
            Version = dto.Version,
            Profile = Enum.TryParse<DiagramProfile>(dto.Profile, true, out var profile) ? profile : DiagramProfile.ExecutableV0,
            LayoutMode = Enum.TryParse<DiagramLayoutMode>(dto.LayoutMode, true, out var layout) ? layout : DiagramLayoutMode.Primitive
        };

        foreach (var node in dto.Nodes)
        {
            document.Nodes.Add(new DiagramNode(
                node.Id,
                Enum.Parse<NodeKind>(node.Kind, ignoreCase: true),
                node.Text,
                node.X,
                node.Y,
                node.Lane));
        }

        foreach (var connection in dto.Connections)
        {
            document.Connections.Add(new DiagramConnection(
                connection.Id,
                connection.FromNodeId,
                Enum.Parse<PortKind>(connection.FromPort, ignoreCase: true),
                connection.ToNodeId,
                Enum.Parse<PortKind>(connection.ToPort, ignoreCase: true)));
        }

        return document;
    }
}
