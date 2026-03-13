namespace DrakonNx.Core.IR;

public sealed record FlowIf(string NodeId, string Condition, FlowNode TrueBranch, FlowNode FalseBranch) : FlowNode;
