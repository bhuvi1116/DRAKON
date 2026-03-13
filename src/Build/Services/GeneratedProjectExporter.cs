using System.Text.RegularExpressions;
using DrakonNx.Build.CMake;
using DrakonNx.Build.Model;
using DrakonNx.CodeGen.C;
using DrakonNx.Core.Model;

namespace DrakonNx.Build.Services;

public sealed class GeneratedProjectExporter
{
    private readonly CodeGenerator _codeGenerator = new();
    private readonly CMakeProjectWriter _cmakeWriter = new();

    public ExportResult Export(DiagramDocument document, string outputDirectory)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputDirectory);

        Directory.CreateDirectory(outputDirectory);

        var projectName = SanitizeIdentifier(document.Name, "drakon_nx_program");
        var binaryName = $"{projectName}_app";
        var layout = new GeneratedProjectLayout(
            projectName,
            outputDirectory,
            Path.Combine(outputDirectory, _cmakeWriter.CreateMainSourceFileName()),
            Path.Combine(outputDirectory, "CMakeLists.txt"),
            binaryName,
            Path.Combine(outputDirectory, "build", binaryName + GetExecutableSuffix()));

        var mainSource = _codeGenerator.Generate(document);
        var cmakeLists = _cmakeWriter.CreateCMakeLists(layout);

        File.WriteAllText(layout.MainSourcePath, mainSource);
        File.WriteAllText(layout.CMakeListsPath, cmakeLists);

        var createdFiles = new[] { layout.MainSourcePath, layout.CMakeListsPath };
        return new ExportResult(layout, mainSource, cmakeLists, createdFiles);
    }

    private static string GetExecutableSuffix()
    {
        return OperatingSystem.IsWindows() ? ".exe" : string.Empty;
    }

    private static string SanitizeIdentifier(string value, string fallback)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return fallback;
        }

        var normalized = Regex.Replace(value.ToLowerInvariant(), "[^a-z0-9]+", "_").Trim('_');
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return fallback;
        }

        if (char.IsDigit(normalized[0]))
        {
            normalized = "p_" + normalized;
        }

        return normalized;
    }
}
