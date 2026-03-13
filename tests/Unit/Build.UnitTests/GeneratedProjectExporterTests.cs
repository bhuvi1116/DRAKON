using DrakonNx.Build.Services;
using DrakonNx.Core.Services;

namespace DrakonNx.Build.UnitTests;

public sealed class GeneratedProjectExporterTests
{
    [Fact]
    public void Export_CreatesMainSourceAndCMakeLists()
    {
        var document = DiagramFactory.CreateMaxOfTwoSample();
        var exporter = new GeneratedProjectExporter();
        var tempDirectory = Path.Combine(Path.GetTempPath(), "drakon_nx_tests", Guid.NewGuid().ToString("N"));

        try
        {
            var result = exporter.Export(document, tempDirectory);

            Assert.True(File.Exists(result.Layout.MainSourcePath));
            Assert.True(File.Exists(result.Layout.CMakeListsPath));
            Assert.Contains("int main(void)", result.MainSource);
            Assert.Contains("cmake_minimum_required", result.CMakeLists);
            Assert.Contains(result.Layout.ProjectName, result.CMakeLists);
        }
        finally
        {
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, recursive: true);
            }
        }
    }
}
