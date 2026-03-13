using DrakonNx.Core.Model;
using DrakonNx.Core.Services;

namespace DrakonNx.Core.Templates;

public static class DiagramTemplateCatalog
{
    private static readonly IReadOnlyDictionary<string, Func<DiagramDocument>> Factories =
        new Dictionary<string, Func<DiagramDocument>>(StringComparer.OrdinalIgnoreCase)
        {
            ["hello-world"] = DiagramFactory.CreateHelloWorldSample,
            ["minimal"] = DiagramFactory.CreateMinimalSample,
            ["simple-branch"] = DiagramFactory.CreateBranchSample,
            ["max-of-two"] = DiagramFactory.CreateMaxOfTwoSample,
            ["drakon-primitive-spec"] = DiagramFactory.CreateDrakonPrimitiveSpecSample,
            ["drakon-silhouette-spec"] = DiagramFactory.CreateDrakonSilhouetteSpecSample
        };

    public static IReadOnlyList<string> GetTemplateNames()
        => Factories.Keys.OrderBy(x => x, StringComparer.OrdinalIgnoreCase).ToList();

    public static bool TryCreate(string templateName, out DiagramDocument? document)
    {
        if (Factories.TryGetValue(templateName, out var factory))
        {
            document = factory();
            return true;
        }

        document = null;
        return false;
    }
}
