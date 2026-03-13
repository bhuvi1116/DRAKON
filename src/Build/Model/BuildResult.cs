namespace DrakonNx.Build.Model;

public sealed record BuildResult(
    bool Succeeded,
    string ConfigureLog,
    string BuildLog,
    string RunLog,
    string? BinaryPath,
    string? ErrorMessage);
