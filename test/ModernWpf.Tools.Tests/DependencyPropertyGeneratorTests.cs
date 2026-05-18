using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.DependencyPropertyGenerator;

namespace ModernWpf.Tools.Tests;

[TestClass]
public class DependencyPropertyGeneratorTests
{
    [TestMethod]
    public void GeneratesReadOnlyTemplateSettingProperty()
    {
        var text = Generate(new DependencyPropertyManifest
        {
            Namespace = "ModernWpf.Controls.Primitives",
            Usings = { "System.Windows" },
            Type = new TypeManifest
            {
                Declaration = "public sealed partial class",
                Name = "SampleTemplateSettings"
            },
            Properties =
            {
                new DependencyPropertyEntry
                {
                    Name = "CurrentWidth",
                    Type = "double",
                    Kind = DependencyPropertyKind.RegisterReadOnly,
                    Metadata = "null",
                    SetterAccessibility = "internal"
                }
            }
        });

        StringAssert.Contains(text, "private static readonly DependencyPropertyKey CurrentWidthPropertyKey =");
        StringAssert.Contains(text, "DependencyProperty.RegisterReadOnly(");
        StringAssert.Contains(text, "nameof(CurrentWidth),");
        StringAssert.Contains(text, "public static readonly DependencyProperty CurrentWidthProperty =");
        StringAssert.Contains(text, "internal set => SetValue(CurrentWidthPropertyKey, value);");
    }

    [TestMethod]
    public void GeneratesMetadataOptionsCallbacksCoerceAndSetterGuard()
    {
        var text = Generate(new DependencyPropertyManifest
        {
            Namespace = "ModernWpf.Controls",
            Usings = { "System.Windows" },
            Type = new TypeManifest { Name = "NumberBox" },
            Properties =
            {
                new DependencyPropertyEntry
                {
                    Name = "Value",
                    Type = "double",
                    Default = "double.NaN",
                    MetadataType = "FrameworkPropertyMetadata",
                    Options = { "BindsTwoWayByDefault", "Journal" },
                    Changed = "OnValuePropertyChanged",
                    ChangedForwardTo = "OnValuePropertyChanged",
                    Coerce = "CoerceValue",
                    SetterGuard = "!double.IsNaN(value) || !double.IsNaN(Value)"
                }
            }
        });

        StringAssert.Contains(text, "new FrameworkPropertyMetadata(double.NaN, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal, OnValuePropertyChanged, CoerceValue)");
        StringAssert.Contains(text, "if (!double.IsNaN(value) || !double.IsNaN(Value))");
        StringAssert.Contains(text, "((NumberBox)sender).OnValuePropertyChanged(args);");
    }

    [TestMethod]
    public void GeneratesAttachedAndAddOwnerProperties()
    {
        var text = Generate(new DependencyPropertyManifest
        {
            Namespace = "ModernWpf.Controls.Primitives",
            Usings = { "System.Windows" },
            Type = new TypeManifest { Name = "SampleHelper" },
            Properties =
            {
                new DependencyPropertyEntry
                {
                    Name = "IsActive",
                    Type = "bool",
                    Kind = DependencyPropertyKind.RegisterAttached,
                    Default = "false"
                },
                new DependencyPropertyEntry
                {
                    Name = "CornerRadius",
                    Type = "CornerRadius",
                    Kind = DependencyPropertyKind.AddOwner,
                    SourceProperty = "ControlHelper.CornerRadiusProperty"
                }
            }
        });

        StringAssert.Contains(text, "DependencyProperty.RegisterAttached(");
        StringAssert.Contains(text, "\"IsActive\",");
        StringAssert.Contains(text, "public static bool GetIsActive(DependencyObject element)");
        StringAssert.Contains(text, "public static void SetIsActive(DependencyObject element, bool value)");
        StringAssert.Contains(text, "ControlHelper.CornerRadiusProperty.AddOwner(typeof(SampleHelper));");
    }

    [TestMethod]
    public void GeneratesCustomSetterAndChangedBodies()
    {
        var text = Generate(new DependencyPropertyManifest
        {
            Namespace = "ModernWpf.Controls",
            Usings = { "System.Windows" },
            Type = new TypeManifest { Name = "NumberBox" },
            Properties =
            {
                new DependencyPropertyEntry
                {
                    Name = "NumberFormatter",
                    Type = "INumberBoxNumberFormatter",
                    Changed = "OnNumberFormatterPropertyChanged",
                    SetterBody = "ValidateNumberFormatter(value);\nSetValue(NumberFormatterProperty, value);",
                    ChangedBody = "var owner = (NumberBox)sender;\nowner.OnNumberFormatterPropertyChanged(args);"
                }
            }
        });

        StringAssert.Contains(text, "ValidateNumberFormatter(value);");
        StringAssert.Contains(text, "SetValue(NumberFormatterProperty, value);");
        StringAssert.Contains(text, "var owner = (NumberBox)sender;");
        StringAssert.Contains(text, "owner.OnNumberFormatterPropertyChanged(args);");
    }

    [TestMethod]
    public void GeneratesExplicitRegistrationNameForNoWrapperProperties()
    {
        var text = Generate(new DependencyPropertyManifest
        {
            Namespace = "ModernWpf.Controls",
            Usings = { "System.Windows" },
            Type = new TypeManifest { Name = "SplitButton" },
            Properties =
            {
                new DependencyPropertyEntry
                {
                    Name = "PrimaryButtonIsPressed",
                    Type = "bool",
                    MetadataType = "FrameworkPropertyMetadata",
                    Changed = "OnVisualPropertyChanged",
                    FieldAccessibility = "private",
                    GenerateWrapper = false,
                    RegistrationName = "PrimaryButtonIsPressed"
                }
            }
        });

        StringAssert.Contains(text, "private static readonly DependencyProperty PrimaryButtonIsPressedProperty =");
        StringAssert.Contains(text, "\"PrimaryButtonIsPressed\",");
        Assert.IsFalse(text.Contains("nameof(PrimaryButtonIsPressed),", StringComparison.Ordinal));
        Assert.IsFalse(text.Contains("public bool PrimaryButtonIsPressed", StringComparison.Ordinal));
    }

    [TestMethod]
    public void CheckModeReportsStaleGeneratedFiles()
    {
        var directory = Path.Combine(Path.GetTempPath(), "ModernWpfDpGenTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);

        try
        {
            var manifestPath = Path.Combine(directory, "Sample.dprops.json");
            File.WriteAllText(
                manifestPath,
                """
                {
                  "usings": [ "System.Windows" ],
                  "namespace": "ModernWpf.Controls",
                  "type": {
                    "name": "Sample"
                  },
                  "properties": [
                    {
                      "name": "Value",
                      "type": "double",
                      "default": "0d"
                    }
                  ]
                }
                """);
            File.WriteAllText(Path.Combine(directory, "Sample.properties.g.cs"), "stale");

            var result = GeneratorRunner.Run(new GenerationOptions
            {
                Mode = GenerationMode.Check,
                RootPath = directory
            });

            Assert.AreEqual(1, result.ExitCode);
            CollectionAssert.Contains(result.StaleFiles.ToList(), "Sample.properties.g.cs");
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    private static string Generate(DependencyPropertyManifest manifest)
    {
        return DependencyPropertyCodeGenerator.Generate(manifest);
    }
}
