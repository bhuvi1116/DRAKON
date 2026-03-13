using DrakonNx.Build.Model;
using DrakonNx.Build.Toolchain;

namespace DrakonNx.Build.Services;

public sealed class CMakeBuildService
{
    private readonly ProcessRunner _processRunner;

    public CMakeBuildService()
        : this(new ProcessRunner())
    {
    }

    public CMakeBuildService(ProcessRunner processRunner)
    {
        _processRunner = processRunner;
    }

    public async Task<BuildResult> ConfigureBuildAndRunAsync(
        GeneratedProjectLayout layout,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(layout);

        var buildDirectory = Path.Combine(layout.OutputDirectory, "build");
        Directory.CreateDirectory(buildDirectory);

        try
        {
            var versionResult = await _processRunner.RunAsync("cmake", "--version", layout.OutputDirectory, cancellationToken);
            if (versionResult.ExitCode != 0)
            {
                return new BuildResult(false, versionResult.CombinedOutput, string.Empty, string.Empty, null,
                    "Инструмент cmake недоступен или вернул ошибку.");
            }

            var configureArgs = $"-S \"{layout.OutputDirectory}\" -B \"{buildDirectory}\"";
            var configureResult = await _processRunner.RunAsync("cmake", configureArgs, layout.OutputDirectory, cancellationToken);
            if (configureResult.ExitCode != 0)
            {
                return new BuildResult(false, configureResult.CombinedOutput, string.Empty, string.Empty, null,
                    "Ошибка на этапе конфигурации CMake.");
            }

            var buildArgs = $"--build \"{buildDirectory}\" --config Release";
            var buildResult = await _processRunner.RunAsync("cmake", buildArgs, layout.OutputDirectory, cancellationToken);
            if (buildResult.ExitCode != 0)
            {
                return new BuildResult(false, configureResult.CombinedOutput, buildResult.CombinedOutput, string.Empty, null,
                    "Ошибка на этапе сборки generated C-кода.");
            }

            string runLog;
            string? binaryPath = FindBuiltBinary(layout, buildDirectory);
            if (binaryPath is null)
            {
                runLog = "Сборка завершилась, но исполняемый файл не найден в ожидаемых каталогах.";
                return new BuildResult(false, configureResult.CombinedOutput, buildResult.CombinedOutput, runLog, null,
                    "Исполняемый файл не найден.");
            }

            var binaryDirectory = Path.GetDirectoryName(binaryPath) ?? buildDirectory;
            var binaryName = Path.GetFileName(binaryPath);
            var runResult = await _processRunner.RunAsync(binaryPath, string.Empty, binaryDirectory, cancellationToken);
            runLog = runResult.CombinedOutput;

            return new BuildResult(runResult.ExitCode == 0,
                configureResult.CombinedOutput,
                buildResult.CombinedOutput,
                runLog,
                binaryPath,
                runResult.ExitCode == 0 ? null : $"Сгенерированный бинарник '{binaryName}' завершился с кодом {runResult.ExitCode}.");
        }
        catch (InvalidOperationException ex)
        {
            return new BuildResult(false, string.Empty, string.Empty, string.Empty, null, ex.Message);
        }
    }

    private static string? FindBuiltBinary(GeneratedProjectLayout layout, string buildDirectory)
    {
        var directPath = layout.BinaryPath;
        if (File.Exists(directPath))
        {
            return directPath;
        }

        var releasePath = Path.Combine(buildDirectory, "Release", Path.GetFileName(directPath));
        if (File.Exists(releasePath))
        {
            return releasePath;
        }

        var debugPath = Path.Combine(buildDirectory, "Debug", Path.GetFileName(directPath));
        if (File.Exists(debugPath))
        {
            return debugPath;
        }

        var allCandidates = Directory.EnumerateFiles(buildDirectory, Path.GetFileName(directPath), SearchOption.AllDirectories).ToList();
        return allCandidates.FirstOrDefault();
    }
}
