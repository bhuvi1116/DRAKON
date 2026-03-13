using System.Text;
using DrakonNx.Build.Model;

namespace DrakonNx.Build.CMake;

public sealed class CMakeProjectWriter
{
    public string CreateMainSourceFileName() => "main.c";

    public string CreateCMakeLists(GeneratedProjectLayout layout)
    {
        ArgumentNullException.ThrowIfNull(layout);

        var sb = new StringBuilder();
        sb.AppendLine("cmake_minimum_required(VERSION 3.20)");
        sb.Append("project(").Append(layout.ProjectName).AppendLine(" LANGUAGES C)");
        sb.AppendLine();
        sb.AppendLine("set(CMAKE_C_STANDARD 99)");
        sb.AppendLine("set(CMAKE_C_STANDARD_REQUIRED ON)");
        sb.AppendLine("set(CMAKE_C_EXTENSIONS OFF)");
        sb.AppendLine();
        sb.Append("add_executable(").Append(layout.BinaryName).AppendLine();
        sb.AppendLine("    main.c");
        sb.AppendLine(")");
        sb.AppendLine();
        sb.Append("target_compile_definitions(").Append(layout.BinaryName).AppendLine(" PRIVATE)");
        sb.AppendLine("    DRAKON_NX_GENERATED=1");
        sb.AppendLine(")");
        return sb.ToString();
    }
}
