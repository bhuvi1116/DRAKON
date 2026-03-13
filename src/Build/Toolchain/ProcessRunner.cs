using System.ComponentModel;
using System.Diagnostics;

namespace DrakonNx.Build.Toolchain;

public sealed class ProcessRunner
{
    public async Task<ProcessRunResult> RunAsync(
        string fileName,
        string arguments,
        string workingDirectory,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        ArgumentNullException.ThrowIfNull(workingDirectory);

        var startInfo = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = startInfo };

        try
        {
            if (!process.Start())
            {
                throw new InvalidOperationException($"Не удалось запустить процесс: {fileName}");
            }
        }
        catch (Exception ex) when (ex is Win32Exception or InvalidOperationException)
        {
            throw new InvalidOperationException($"Не удалось запустить внешний инструмент '{fileName}'.", ex);
        }

        var stdOutTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var stdErrTask = process.StandardError.ReadToEndAsync(cancellationToken);

        await process.WaitForExitAsync(cancellationToken);

        return new ProcessRunResult(
            process.ExitCode,
            await stdOutTask,
            await stdErrTask);
    }
}
