namespace DrakonNx.Build.Toolchain;

public sealed record ProcessRunResult(int ExitCode, string StandardOutput, string StandardError)
{
    public string CombinedOutput => string.IsNullOrWhiteSpace(StandardError)
        ? StandardOutput
        : string.Concat(StandardOutput, Environment.NewLine, StandardError).Trim();
}
