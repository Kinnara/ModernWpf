using ModernWpf.DependencyPropertyGenerator;

var parseResult = TryParse(args, out var options, out var error);
if (!parseResult)
{
    Console.Error.WriteLine(error);
    PrintUsage();
    return 2;
}

return GeneratorRunner.Run(options, Console.Out).ExitCode;

static bool TryParse(string[] args, out GenerationOptions options, out string error)
{
    options = new GenerationOptions();
    error = string.Empty;

    if (args.Length == 0)
    {
        error = "Missing command.";
        return false;
    }

    GenerationMode mode;
    switch (args[0].ToLowerInvariant())
    {
        case "generate":
            mode = GenerationMode.Generate;
            break;
        case "check":
            mode = GenerationMode.Check;
            break;
        default:
            error = $"Unknown command '{args[0]}'.";
            return false;
    }

    var rootPath = Directory.GetCurrentDirectory();
    var manifests = new List<string>();

    for (var i = 1; i < args.Length; i++)
    {
        switch (args[i])
        {
            case "--root":
                if (++i >= args.Length)
                {
                    error = "Missing value for --root.";
                    return false;
                }
                rootPath = args[i];
                break;
            case "--file":
                if (++i >= args.Length)
                {
                    error = "Missing value for --file.";
                    return false;
                }
                manifests.Add(args[i]);
                break;
            default:
                error = $"Unknown argument '{args[i]}'.";
                return false;
        }
    }

    options = new GenerationOptions
    {
        Mode = mode,
        RootPath = rootPath,
        ManifestPaths = manifests
    };
    return true;
}

static void PrintUsage()
{
    Console.Error.WriteLine("Usage:");
    Console.Error.WriteLine("  ModernWpf.DependencyPropertyGenerator generate [--root <path>] [--file <manifest>]");
    Console.Error.WriteLine("  ModernWpf.DependencyPropertyGenerator check [--root <path>] [--file <manifest>]");
}
