using DrakonNx.CodeGen.C;
using DrakonNx.Core.Services;

namespace DrakonNx.CodeGen.GoldenTests;

public sealed class CodeGeneratorGoldenTests
{
    [Fact]
    public void Generate_MinimalSample_MatchesGoldenFile()
    {
        var generator = new CodeGenerator();
        var actual = generator.Generate(DiagramFactory.CreateMinimalSample()).Replace("", "");
        var expected = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "TestData", "minimal-sample.golden.c")).Replace("", "");
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Generate_MaxOfTwo_MatchesGoldenFile()
    {
        var generator = new CodeGenerator();
        var actual = generator.Generate(DiagramFactory.CreateMaxOfTwoSample()).Replace("", "");
        var expected = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "TestData", "max-of-two.golden.c")).Replace("", "");
        Assert.Equal(expected, actual);
    }
}
