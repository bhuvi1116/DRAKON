namespace DrakonNx.Build.Model;

public sealed record GeneratedProjectLayout(
    string ProjectName,
    string OutputDirectory,
    string MainSourcePath,
    string CMakeListsPath,
    string BinaryName,
    string BinaryPath);
