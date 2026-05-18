using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ModernWpf.DependencyPropertyGenerator;

public enum GenerationMode
{
    Generate,
    Check
}

public sealed class GenerationOptions
{
    public GenerationMode Mode { get; init; }

    public string RootPath { get; init; } = Directory.GetCurrentDirectory();

    public IReadOnlyList<string> ManifestPaths { get; init; } = Array.Empty<string>();
}

public sealed class GenerationResult
{
    public GenerationResult(int exitCode, IReadOnlyList<string> staleFiles, IReadOnlyList<string> generatedFiles)
    {
        ExitCode = exitCode;
        StaleFiles = staleFiles;
        GeneratedFiles = generatedFiles;
    }

    public int ExitCode { get; }

    public IReadOnlyList<string> StaleFiles { get; }

    public IReadOnlyList<string> GeneratedFiles { get; }
}

public static class GeneratorRunner
{
    public static GenerationResult Run(GenerationOptions options, TextWriter? log = null)
    {
        ArgumentNullException.ThrowIfNull(options);

        var rootPath = Path.GetFullPath(options.RootPath);
        var manifestPaths = options.ManifestPaths.Count == 0
            ? FindManifestPaths(rootPath)
            : options.ManifestPaths.Select(path => Path.GetFullPath(path)).Order(StringComparer.OrdinalIgnoreCase).ToArray();

        var staleFiles = new List<string>();
        var generatedFiles = new List<string>();

        foreach (var manifestPath in manifestPaths)
        {
            var manifest = DependencyPropertyManifest.Load(manifestPath);
            var outputPath = GetOutputPath(manifestPath, manifest);
            var text = DependencyPropertyCodeGenerator.Generate(manifest);

            if (File.Exists(outputPath) && string.Equals(File.ReadAllText(outputPath), text, StringComparison.Ordinal))
            {
                continue;
            }

            if (options.Mode == GenerationMode.Check)
            {
                staleFiles.Add(Relativize(rootPath, outputPath));
                continue;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
            File.WriteAllText(outputPath, text, Encoding.UTF8);
            generatedFiles.Add(Relativize(rootPath, outputPath));
        }

        if (options.Mode == GenerationMode.Check && staleFiles.Count > 0)
        {
            log?.WriteLine("Stale dependency property generated files:");
            foreach (var staleFile in staleFiles)
            {
                log?.WriteLine("  " + staleFile);
            }
            return new GenerationResult(1, staleFiles, generatedFiles);
        }

        foreach (var generatedFile in generatedFiles)
        {
            log?.WriteLine("Generated " + generatedFile);
        }

        return new GenerationResult(0, staleFiles, generatedFiles);
    }

    public static string GetOutputPath(string manifestPath, DependencyPropertyManifest manifest)
    {
        if (!string.IsNullOrWhiteSpace(manifest.Output))
        {
            return Path.GetFullPath(Path.Combine(Path.GetDirectoryName(manifestPath)!, manifest.Output));
        }

        const string suffix = ".dprops.json";
        if (!manifestPath.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Manifest '{manifestPath}' must end with '{suffix}' or specify an output path.");
        }

        return manifestPath[..^suffix.Length] + ".properties.g.cs";
    }

    private static IReadOnlyList<string> FindManifestPaths(string rootPath)
    {
        return Directory
            .EnumerateFiles(rootPath, "*.dprops.json", SearchOption.AllDirectories)
            .Where(path => !IsInBuildOutput(path))
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static bool IsInBuildOutput(string path)
    {
        var normalized = path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
        return normalized.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase);
    }

    private static string Relativize(string rootPath, string path)
    {
        return Path.GetRelativePath(rootPath, path).Replace(Path.DirectorySeparatorChar, '/');
    }
}

public sealed class DependencyPropertyManifest
{
    private static readonly JsonSerializerOptions s_jsonOptions = new()
    {
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        PropertyNameCaseInsensitive = true,
        Converters =
        {
            new JsonStringEnumConverter()
        }
    };

    public string? Output { get; set; }

    public List<string> Usings { get; set; } = new();

    public string Namespace { get; set; } = string.Empty;

    public TypeManifest Type { get; set; } = new();

    public List<DependencyPropertyEntry> Properties { get; set; } = new();

    public static DependencyPropertyManifest Load(string path)
    {
        var json = File.ReadAllText(path);
        var manifest = JsonSerializer.Deserialize<DependencyPropertyManifest>(json, s_jsonOptions)
            ?? throw new InvalidOperationException($"Manifest '{path}' is empty.");

        manifest.Validate(path);
        return manifest;
    }

    public void Validate(string sourceName)
    {
        if (string.IsNullOrWhiteSpace(Namespace))
        {
            throw new InvalidOperationException($"Manifest '{sourceName}' must specify a namespace.");
        }

        if (string.IsNullOrWhiteSpace(Type.Name))
        {
            throw new InvalidOperationException($"Manifest '{sourceName}' must specify a type name.");
        }

        if (Properties.Count == 0)
        {
            throw new InvalidOperationException($"Manifest '{sourceName}' must specify at least one property.");
        }

        foreach (var property in Properties)
        {
            property.ValidateEntry(sourceName);
        }
    }
}

public sealed class TypeManifest
{
    public string Declaration { get; set; } = "public partial class";

    public string Name { get; set; } = string.Empty;
}

public enum DependencyPropertyKind
{
    Register,
    RegisterReadOnly,
    RegisterAttached,
    RegisterAttachedReadOnly,
    AddOwner
}

public sealed class DependencyPropertyEntry
{
    public string Name { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public DependencyPropertyKind Kind { get; set; } = DependencyPropertyKind.Register;

    public string? SourceProperty { get; set; }

    public string? Metadata { get; set; }

    public string? MetadataType { get; set; }

    public string? Default { get; set; }

    public List<string> Options { get; set; } = new();

    public string? Changed { get; set; }

    public string? ChangedForwardTo { get; set; }

    public string? ChangedBody { get; set; }

    public string? Coerce { get; set; }

    public string? Validate { get; set; }

    public string FieldAccessibility { get; set; } = "public";

    public string KeyAccessibility { get; set; } = "private";

    public string PropertyAccessibility { get; set; } = "public";

    public string? SetterAccessibility { get; set; }

    public string? GetterAccessibility { get; set; }

    public string? AttachedSetterAccessibility { get; set; }

    public string? SetterGuard { get; set; }

    public bool GenerateWrapper { get; set; } = true;

    public void ValidateEntry(string sourceName)
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            throw new InvalidOperationException($"Manifest '{sourceName}' contains a property without a name.");
        }

        if (string.IsNullOrWhiteSpace(Type))
        {
            throw new InvalidOperationException($"Property '{Name}' in '{sourceName}' must specify a type.");
        }

        if (Kind == DependencyPropertyKind.AddOwner && string.IsNullOrWhiteSpace(SourceProperty))
        {
            throw new InvalidOperationException($"AddOwner property '{Name}' in '{sourceName}' must specify sourceProperty.");
        }

        if (ChangedBody != null && string.IsNullOrWhiteSpace(Changed))
        {
            throw new InvalidOperationException($"Property '{Name}' in '{sourceName}' specifies changedBody without changed.");
        }
    }
}

public static class DependencyPropertyCodeGenerator
{
    public static string Generate(DependencyPropertyManifest manifest)
    {
        manifest.Validate("<in-memory>");

        var writer = new CodeWriter();
        writer.WriteLine("// Copyright (c) Microsoft Corporation. All rights reserved.");
        writer.WriteLine("// Licensed under the MIT License. See LICENSE in the project root for license information.");
        writer.WriteLine("// <auto-generated />");
        writer.WriteLine();

        foreach (var @using in manifest.Usings.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal))
        {
            writer.WriteLine($"using {@using};");
        }

        if (manifest.Usings.Count > 0)
        {
            writer.WriteLine();
        }

        writer.WriteLine($"namespace {manifest.Namespace}");
        writer.OpenBlock();
        writer.WriteLine($"{manifest.Type.Declaration} {manifest.Type.Name}");
        writer.OpenBlock();

        for (var i = 0; i < manifest.Properties.Count; i++)
        {
            if (i > 0)
            {
                writer.WriteLine();
            }

            WriteProperty(writer, manifest.Type.Name, manifest.Properties[i]);
        }

        writer.CloseBlock();
        writer.CloseBlock();

        return writer.ToString();
    }

    private static void WriteProperty(CodeWriter writer, string ownerType, DependencyPropertyEntry property)
    {
        writer.WriteLine($"#region {property.Name}");
        writer.WriteLine();

        switch (property.Kind)
        {
            case DependencyPropertyKind.Register:
            case DependencyPropertyKind.RegisterAttached:
                WriteDependencyPropertyField(writer, ownerType, property);
                break;
            case DependencyPropertyKind.RegisterReadOnly:
            case DependencyPropertyKind.RegisterAttachedReadOnly:
                WriteReadOnlyDependencyPropertyFields(writer, ownerType, property);
                break;
            case DependencyPropertyKind.AddOwner:
                WriteAddOwnerField(writer, ownerType, property);
                break;
            default:
                throw new InvalidOperationException($"Unsupported dependency property kind '{property.Kind}'.");
        }

        if (property.GenerateWrapper)
        {
            writer.WriteLine();
            if (IsAttached(property.Kind))
            {
                WriteAttachedWrapper(writer, property);
            }
            else
            {
                WriteInstanceWrapper(writer, property);
            }
        }

        if (!string.IsNullOrWhiteSpace(property.ChangedBody) ||
            (!string.IsNullOrWhiteSpace(property.Changed) && !string.IsNullOrWhiteSpace(property.ChangedForwardTo)))
        {
            writer.WriteLine();
            WriteChangedCallback(writer, ownerType, property);
        }

        writer.WriteLine();
        writer.WriteLine("#endregion");
    }

    private static void WriteDependencyPropertyField(CodeWriter writer, string ownerType, DependencyPropertyEntry property)
    {
        var registrationMethod = property.Kind == DependencyPropertyKind.RegisterAttached
            ? "RegisterAttached"
            : "Register";
        var propertyName = property.Kind == DependencyPropertyKind.RegisterAttached
            ? Quote(property.Name)
            : $"nameof({property.Name})";
        var validateArgument = string.IsNullOrWhiteSpace(property.Validate) ? string.Empty : $", {property.Validate}";

        writer.WriteLine($"{property.FieldAccessibility} static readonly DependencyProperty {property.Name}Property =");
        writer.Indent();
        writer.WriteLine("DependencyProperty." + registrationMethod + "(");
        writer.Indent();
        writer.WriteLine($"{propertyName},");
        writer.WriteLine($"typeof({property.Type}),");
        writer.WriteLine($"typeof({ownerType}),");
        writer.WriteLine(BuildMetadata(property) + validateArgument + ");");
        writer.Unindent();
        writer.Unindent();
    }

    private static void WriteReadOnlyDependencyPropertyFields(CodeWriter writer, string ownerType, DependencyPropertyEntry property)
    {
        var registrationMethod = property.Kind == DependencyPropertyKind.RegisterAttachedReadOnly
            ? "RegisterAttachedReadOnly"
            : "RegisterReadOnly";
        var propertyName = property.Kind == DependencyPropertyKind.RegisterAttachedReadOnly
            ? Quote(property.Name)
            : $"nameof({property.Name})";
        var validateArgument = string.IsNullOrWhiteSpace(property.Validate) ? string.Empty : $", {property.Validate}";

        writer.WriteLine($"{property.KeyAccessibility} static readonly DependencyPropertyKey {property.Name}PropertyKey =");
        writer.Indent();
        writer.WriteLine("DependencyProperty." + registrationMethod + "(");
        writer.Indent();
        writer.WriteLine($"{propertyName},");
        writer.WriteLine($"typeof({property.Type}),");
        writer.WriteLine($"typeof({ownerType}),");
        writer.WriteLine(BuildMetadata(property) + validateArgument + ");");
        writer.Unindent();
        writer.Unindent();
        writer.WriteLine();
        writer.WriteLine($"{property.FieldAccessibility} static readonly DependencyProperty {property.Name}Property =");
        writer.Indent();
        writer.WriteLine($"{property.Name}PropertyKey.DependencyProperty;");
        writer.Unindent();
    }

    private static void WriteAddOwnerField(CodeWriter writer, string ownerType, DependencyPropertyEntry property)
    {
        writer.WriteLine($"{property.FieldAccessibility} static readonly DependencyProperty {property.Name}Property =");
        writer.Indent();
        if (HasMetadata(property))
        {
            writer.WriteLine($"{property.SourceProperty}.AddOwner(");
            writer.Indent();
            writer.WriteLine($"typeof({ownerType}),");
            writer.WriteLine(BuildMetadata(property) + ");");
            writer.Unindent();
        }
        else
        {
            writer.WriteLine($"{property.SourceProperty}.AddOwner(typeof({ownerType}));");
        }
        writer.Unindent();
    }

    private static string BuildMetadata(DependencyPropertyEntry property)
    {
        if (!string.IsNullOrWhiteSpace(property.Metadata))
        {
            return property.Metadata;
        }

        var metadataType = string.IsNullOrWhiteSpace(property.MetadataType)
            ? property.Options.Count > 0
                ? "FrameworkPropertyMetadata"
                : "PropertyMetadata"
            : property.MetadataType;

        var arguments = new List<string>();
        if (!string.IsNullOrWhiteSpace(property.Default))
        {
            arguments.Add(property.Default);
        }

        if (property.Options.Count > 0)
        {
            arguments.Add(string.Join(" | ", property.Options.Select(option => "FrameworkPropertyMetadataOptions." + option)));
        }

        if (!string.IsNullOrWhiteSpace(property.Changed))
        {
            arguments.Add(property.Changed);
        }

        if (!string.IsNullOrWhiteSpace(property.Coerce))
        {
            arguments.Add(property.Coerce);
        }

        return arguments.Count == 0
            ? "null"
            : $"new {metadataType}({string.Join(", ", arguments)})";
    }

    private static bool HasMetadata(DependencyPropertyEntry property)
    {
        return !string.IsNullOrWhiteSpace(property.Metadata)
            || !string.IsNullOrWhiteSpace(property.Default)
            || property.Options.Count > 0
            || !string.IsNullOrWhiteSpace(property.Changed)
            || !string.IsNullOrWhiteSpace(property.Coerce);
    }

    private static void WriteInstanceWrapper(CodeWriter writer, DependencyPropertyEntry property)
    {
        writer.WriteLine($"{property.PropertyAccessibility} {property.Type} {property.Name}");
        writer.OpenBlock();
        writer.WriteLine($"{WithAccessorAccessibility(property.GetterAccessibility)}get => ({property.Type})GetValue({property.Name}Property);");

        var setterAccessibility = property.SetterAccessibility;
        if (setterAccessibility == null && property.Kind == DependencyPropertyKind.RegisterReadOnly)
        {
            setterAccessibility = "internal";
        }

        if (!string.Equals(setterAccessibility, "none", StringComparison.OrdinalIgnoreCase))
        {
            var setterPrefix = WithAccessorAccessibility(setterAccessibility);
            var setValueTarget = property.Kind == DependencyPropertyKind.RegisterReadOnly
                ? $"{property.Name}PropertyKey"
                : $"{property.Name}Property";

            if (string.IsNullOrWhiteSpace(property.SetterGuard))
            {
                writer.WriteLine($"{setterPrefix}set => SetValue({setValueTarget}, value);");
            }
            else
            {
                writer.WriteLine($"{setterPrefix}set");
                writer.OpenBlock();
                writer.WriteLine($"if ({property.SetterGuard})");
                writer.OpenBlock();
                writer.WriteLine($"SetValue({setValueTarget}, value);");
                writer.CloseBlock();
                writer.CloseBlock();
            }
        }

        writer.CloseBlock();
    }

    private static void WriteAttachedWrapper(CodeWriter writer, DependencyPropertyEntry property)
    {
        writer.WriteLine($"{property.PropertyAccessibility} static {property.Type} Get{property.Name}(DependencyObject element)");
        writer.OpenBlock();
        writer.WriteLine($"return ({property.Type})element.GetValue({property.Name}Property);");
        writer.CloseBlock();

        var setterAccessibility = property.AttachedSetterAccessibility
            ?? (property.Kind == DependencyPropertyKind.RegisterAttachedReadOnly ? "internal" : property.PropertyAccessibility);

        if (!string.Equals(setterAccessibility, "none", StringComparison.OrdinalIgnoreCase))
        {
            var setValueTarget = property.Kind == DependencyPropertyKind.RegisterAttachedReadOnly
                ? $"{property.Name}PropertyKey"
                : $"{property.Name}Property";

            writer.WriteLine();
            writer.WriteLine($"{setterAccessibility} static void Set{property.Name}(DependencyObject element, {property.Type} value)");
            writer.OpenBlock();
            writer.WriteLine($"element.SetValue({setValueTarget}, value);");
            writer.CloseBlock();
        }
    }

    private static void WriteChangedCallback(CodeWriter writer, string ownerType, DependencyPropertyEntry property)
    {
        writer.WriteLine($"private static void {property.Changed}(DependencyObject sender, DependencyPropertyChangedEventArgs args)");
        writer.OpenBlock();
        if (!string.IsNullOrWhiteSpace(property.ChangedBody))
        {
            writer.WriteLine(property.ChangedBody);
        }
        else
        {
            writer.WriteLine($"(({ownerType})sender).{property.ChangedForwardTo}(args);");
        }
        writer.CloseBlock();
    }

    private static bool IsAttached(DependencyPropertyKind kind)
    {
        return kind is DependencyPropertyKind.RegisterAttached or DependencyPropertyKind.RegisterAttachedReadOnly;
    }

    private static string WithAccessorAccessibility(string? accessibility)
    {
        return string.IsNullOrWhiteSpace(accessibility) ? string.Empty : accessibility + " ";
    }

    private static string Quote(string value)
    {
        return "\"" + value.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
    }
}

internal sealed class CodeWriter
{
    private readonly StringBuilder _builder = new();
    private int _indentLevel;

    public void Indent()
    {
        _indentLevel++;
    }

    public void Unindent()
    {
        _indentLevel--;
    }

    public void OpenBlock()
    {
        WriteLine("{");
        Indent();
    }

    public void CloseBlock()
    {
        Unindent();
        WriteLine("}");
    }

    public void WriteLine()
    {
        _builder.AppendLine();
    }

    public void WriteLine(string value)
    {
        if (value.Length > 0)
        {
            _builder.Append(' ', _indentLevel * 4);
        }
        _builder.AppendLine(value);
    }

    public override string ToString()
    {
        return _builder.ToString();
    }
}
