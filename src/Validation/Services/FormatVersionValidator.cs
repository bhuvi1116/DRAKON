using DrakonNx.Core.Model;
using DrakonNx.Validation.Diagnostics;

namespace DrakonNx.Validation.Services;

public static class FormatVersionValidator
{
    public static ValidationIssue? Validate(DiagramDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        if (string.Equals(document.Version, DiagramDocument.CurrentVersion, StringComparison.Ordinal))
        {
            return null;
        }

        return new ValidationIssue(
            "VAL010",
            $"Версия формата проекта '{document.Version}' отличается от текущей поддерживаемой версии '{DiagramDocument.CurrentVersion}'. Проверьте совместимость схемы.",
            ValidationSeverity.Warning);
    }
}
