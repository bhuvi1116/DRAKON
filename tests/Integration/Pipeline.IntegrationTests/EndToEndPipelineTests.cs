using DrakonNx.CodeGen.C;
using DrakonNx.Core.Services;
using DrakonNx.Serialization.Json;
using DrakonNx.Validation.Services;

namespace DrakonNx.Tests.Integration;

public sealed class EndToEndPipelineTests
{
    [Fact]
    public void Sample_MaxOfTwo_ValidatesAndGeneratesC()
    {
        var document = DiagramFactory.CreateMaxOfTwoSample();
        var validator = new DiagramValidator();
        var issues = validator.Validate(document);

        Assert.DoesNotContain(issues, x => x.Severity == DrakonNx.Validation.Diagnostics.ValidationSeverity.Error);

        var code = new CodeGenerator().Generate(document);
        Assert.Contains("int main(void)", code, StringComparison.Ordinal);
        Assert.Contains("if (a > b)", code, StringComparison.Ordinal);
        Assert.Contains("printf", code, StringComparison.Ordinal);
    }

    [Fact]
    public void Sample_HelloWorld_RoundTripSerializationPreservesCodegen()
    {
        var document = DiagramFactory.CreateHelloWorldSample();
        var serializer = new DiagramJsonSerializer();
        var json = serializer.Serialize(document);
        var restored = serializer.Deserialize(json);

        var validator = new DiagramValidator();
        Assert.DoesNotContain(validator.Validate(restored), x => x.Severity == DrakonNx.Validation.Diagnostics.ValidationSeverity.Error);

        var code = new CodeGenerator().Generate(restored);
        Assert.Contains("printf", code, StringComparison.Ordinal);
        Assert.Contains("hello", json, StringComparison.OrdinalIgnoreCase);
    }
}
