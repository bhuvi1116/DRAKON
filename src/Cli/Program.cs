using DrakonNx.Build.Services;
using DrakonNx.Core.Templates;
using DrakonNx.Serialization.Json;
using DrakonNx.Validation.Diagnostics;
using DrakonNx.Validation.Services;

var exitCode = await ProgramEntry.RunAsync(args);
return exitCode;

internal static class ProgramEntry
{
    public static async Task<int> RunAsync(string[] args)
    {
        if (args.Length == 0 || HasHelp(args))
        {
            PrintHelp();
            return 0;
        }

        var command = args[0].Trim().ToLowerInvariant();
        return command switch
        {
            "new" => await NewAsync(args.Skip(1).ToArray()),
            "validate" => await ValidateAsync(args.Skip(1).ToArray()),
            "generate" => await GenerateAsync(args.Skip(1).ToArray()),
            "build" => await BuildAsync(args.Skip(1).ToArray()),
            _ => Fail($"Неизвестная команда: {args[0]}")
        };
    }


    private static Task<int> NewAsync(string[] args)
    {
        if (args.Length < 2)
        {
            return Task.FromResult(Fail("Для new требуется имя шаблона и путь к создаваемому файлу"));
        }

        var templateName = args[0];
        var outputPath = args[1];

        if (!DiagramTemplateCatalog.TryCreate(templateName, out var document) || document is null)
        {
            Console.Error.WriteLine($"Неизвестный шаблон: {templateName}");
            Console.Error.WriteLine("Доступные шаблоны: " + string.Join(", ", DiagramTemplateCatalog.GetTemplateNames()));
            return Task.FromResult(1);
        }

        var directory = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var serializer = new DiagramJsonSerializer();
        File.WriteAllText(outputPath, serializer.Serialize(document));
        Console.WriteLine(outputPath);
        return Task.FromResult(0);
    }

    private static async Task<int> ValidateAsync(string[] args)
    {
        if (args.Length < 1)
        {
            return Fail("Для validate требуется путь к файлу .drakon.json");
        }

        var document = Load(args[0]);
        var validator = new DiagramValidator();
        var issues = validator.Validate(document);

        if (issues.Count == 0)
        {
            Console.WriteLine("Validation succeeded: ошибок нет.");
            return 0;
        }

        foreach (var issue in issues.OrderBy(x => x.Severity).ThenBy(x => x.Code, StringComparer.Ordinal))
        {
            Console.WriteLine($"{issue.Severity}: {issue.Code}: {issue.Message} (node={issue.NodeId ?? "-"}, connection={issue.ConnectionId ?? "-"})");
        }

        return issues.Any(x => x.Severity == ValidationSeverity.Error) ? 2 : 0;
    }

    private static async Task<int> GenerateAsync(string[] args)
    {
        if (args.Length < 2)
        {
            return Fail("Для generate требуется путь к файлу и каталог экспорта");
        }

        var document = Load(args[0]);
        var validator = new DiagramValidator();
        var issues = validator.Validate(document);
        if (issues.Any(x => x.Severity == ValidationSeverity.Error))
        {
            Console.WriteLine("Генерация отменена: диаграмма содержит ошибки.");
            foreach (var issue in issues)
            {
                Console.WriteLine($"{issue.Severity}: {issue.Code}: {issue.Message}");
            }
            return 2;
        }

        var exporter = new GeneratedProjectExporter();
        var result = exporter.Export(document, args[1]);
        Console.WriteLine(result.Layout.OutputDirectory);
        Console.WriteLine(result.Layout.MainSourcePath);
        Console.WriteLine(result.Layout.CMakeListsPath);
        return 0;
    }

    private static async Task<int> BuildAsync(string[] args)
    {
        if (args.Length < 2)
        {
            return Fail("Для build требуется путь к файлу и каталог экспорта");
        }

        var document = Load(args[0]);
        var validator = new DiagramValidator();
        var issues = validator.Validate(document);
        if (issues.Any(x => x.Severity == ValidationSeverity.Error))
        {
            Console.WriteLine("Сборка отменена: диаграмма содержит ошибки.");
            foreach (var issue in issues)
            {
                Console.WriteLine($"{issue.Severity}: {issue.Code}: {issue.Message}");
            }
            return 2;
        }

        var exporter = new GeneratedProjectExporter();
        var export = exporter.Export(document, args[1]);
        var buildService = new CMakeBuildService();
        var result = await buildService.ConfigureBuildAndRunAsync(export.Layout);

        Console.WriteLine("[configure]");
        Console.WriteLine(result.ConfigureLog);
        Console.WriteLine("[build]");
        Console.WriteLine(result.BuildLog);
        Console.WriteLine("[run]");
        Console.WriteLine(result.RunLog);
        Console.WriteLine("[binary]");
        Console.WriteLine(result.BinaryPath ?? "not-found");

        return result.Succeeded ? 0 : 3;
    }

    private static DrakonNx.Core.Model.DiagramDocument Load(string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException("Файл диаграммы не найден.", path);
        }

        var serializer = new DiagramJsonSerializer();
        return serializer.Deserialize(File.ReadAllText(path));
    }

    private static bool HasHelp(string[] args)
        => args.Any(a => string.Equals(a, "--help", StringComparison.OrdinalIgnoreCase) ||
                         string.Equals(a, "-h", StringComparison.OrdinalIgnoreCase));

    private static void PrintHelp()
    {
        Console.WriteLine("DRAKON-NX CLI");
        Console.WriteLine();
        Console.WriteLine("Команды:");
        Console.WriteLine("  new <template> <diagram.json>");
        Console.WriteLine("  validate <diagram.json>");
        Console.WriteLine("  generate <diagram.json> <output-dir>");
        Console.WriteLine("  build <diagram.json> <output-dir>");
        Console.WriteLine();
        Console.WriteLine("Шаблоны:");
        foreach (var template in DiagramTemplateCatalog.GetTemplateNames())
        {
            Console.WriteLine($"  - {template}");
        }
    }

    private static int Fail(string message)
    {
        Console.Error.WriteLine(message);
        Console.Error.WriteLine("Используйте --help для справки.");
        return 1;
    }
}
