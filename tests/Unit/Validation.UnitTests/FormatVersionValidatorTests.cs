using DrakonNx.Core.Model;
using DrakonNx.Validation.Diagnostics;
using DrakonNx.Validation.Services;

namespace DrakonNx.Tests.Unit.Validation;

public sealed class FormatVersionValidatorTests
{
    [Fact]
    public void Validate_CurrentVersion_ReturnsNoWarning()
    {
        var document = new DiagramDocument("Current") { Version = DiagramDocument.CurrentVersion };

        var issue = FormatVersionValidator.Validate(document);

        Assert.Null(issue);
    }

    [Fact]
    public void Validate_OtherVersion_ReturnsWarning()
    {
        var document = new DiagramDocument("Legacy") { Version = "0.0" };

        var issue = FormatVersionValidator.Validate(document);

        Assert.NotNull(issue);
        Assert.Equal(ValidationSeverity.Warning, issue!.Severity);
        Assert.Equal("VAL010", issue.Code);
    }
}
