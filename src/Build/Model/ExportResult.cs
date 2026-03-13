namespace DrakonNx.Build.Model;

public sealed record ExportResult(
    GeneratedProjectLayout Layout,
    string MainSource,
    string CMakeLists,
    IReadOnlyList<string> CreatedFiles);
