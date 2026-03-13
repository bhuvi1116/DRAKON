using DrakonNx.Core.Model;

namespace DrakonNx.Editor.Models;

public sealed class ConnectionItemViewModel
{
    public ConnectionItemViewModel(string id, string fromNodeId, PortKind fromPort, string toNodeId, PortKind toPort)
    {
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
    public string DisplayName => $"{Id}: {FromNodeId}.{FromPort} -> {ToNodeId}.{ToPort}";
}
