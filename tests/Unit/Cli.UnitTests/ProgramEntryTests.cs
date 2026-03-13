using System.Reflection;

namespace DrakonNx.Tests.Unit.Cli;

public sealed class ProgramEntryTests
{
    [Fact]
    public async Task RunAsync_NewTemplate_CreatesOutputFile()
    {
        var tempDirectory = Path.Combine(Path.GetTempPath(), "drakon-nx-cli-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDirectory);
        var outputPath = Path.Combine(tempDirectory, "hello.drakon.json");

        var cliAssembly = Assembly.Load("Cli");
        var programEntry = cliAssembly.GetType("ProgramEntry", throwOnError: true)!;
        var runAsync = programEntry.GetMethod("RunAsync", BindingFlags.Public | BindingFlags.Static)!;

        var task = (Task<int>)runAsync.Invoke(null, new object[] { new[] { "new", "hello-world", outputPath } })!;
        var exitCode = await task;

        Assert.Equal(0, exitCode);
        Assert.True(File.Exists(outputPath));

        var text = await File.ReadAllTextAsync(outputPath);
        Assert.Contains("hello", text, StringComparison.OrdinalIgnoreCase);
    }
}
