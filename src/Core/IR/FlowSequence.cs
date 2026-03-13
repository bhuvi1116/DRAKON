namespace DrakonNx.Core.IR;

public sealed record FlowSequence(IReadOnlyList<FlowNode> Items) : FlowNode;
