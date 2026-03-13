namespace DrakonNx.Core.Layout;

public sealed class DrakonLayoutIssue
{
    public DrakonLayoutIssue(string code, string message, string? nodeId = null)
    {
        Code = code;
        Message = message;
        NodeId = nodeId;
    }

    public string Code { get; }

    public string Message { get; }

    public string? NodeId { get; }
}
