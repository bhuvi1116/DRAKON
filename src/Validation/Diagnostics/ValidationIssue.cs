namespace DrakonNx.Validation.Diagnostics;

public sealed record ValidationIssue(
    string Code,
    string Message,
    ValidationSeverity Severity,
    string? NodeId = null,
    string? ConnectionId = null);
