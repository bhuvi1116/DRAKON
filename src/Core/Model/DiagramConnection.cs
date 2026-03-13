namespace DrakonNx.Core.Model;

public sealed class DiagramConnection
{
    public DiagramConnection(string id, string fromNodeId, PortKind fromPort, string toNodeId, PortKind toPort)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Идентификатор связи не должен быть пустым.", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(fromNodeId))
        {
            throw new ArgumentException("Идентификатор исходного узла не должен быть пустым.", nameof(fromNodeId));
        }

        if (string.IsNullOrWhiteSpace(toNodeId))
        {
            throw new ArgumentException("Идентификатор целевого узла не должен быть пустым.", nameof(toNodeId));
        }

        Id = id;
        FromNodeId = fromNodeId;
        FromPort = fromPort;
        ToNodeId = toNodeId;
        ToPort = toPort;
    }

    public string Id { get; }

    public string FromNodeId { get; }

    public PortKind FromPort { get; }

    public string ToNodeId { get; }

    public PortKind ToPort { get; }
}
