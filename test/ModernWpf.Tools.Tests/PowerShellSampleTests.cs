using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ModernWpf.Tools.Tests;

[TestClass]
public class PowerShellSampleTests
{
    [TestMethod]
    public void RunnerBuildsSupportedTargetsAndProvidesNonInteractiveValidation()
    {
        var repoRoot = FindRepoRoot();
        var sampleDirectory = Path.Combine(repoRoot, "samples", "PowerShellSample");
        var project = File.ReadAllText(Path.Combine(sampleDirectory, "PowerShellSample.csproj"));
        var runner = File.ReadAllText(Path.Combine(sampleDirectory, "run.ps1"));
        var readme = File.ReadAllText(Path.Combine(sampleDirectory, "README.md"));

        StringAssert.Contains(project, "<TargetFrameworks>net462;net8.0-windows7.0;net10.0-windows7.0</TargetFrameworks>");

        StringAssert.Contains(runner, "$targetFramework = 'net462'");
        StringAssert.Contains(runner, "$targetFramework = 'net8.0-windows7.0'");
        StringAssert.Contains(runner, "$targetFramework = 'net10.0-windows7.0'");
        StringAssert.Contains(runner, "& dotnet build $projectPath");
        StringAssert.Contains(runner, "@('ModernWpf.dll', 'ModernWpf.Controls.dll')");
        StringAssert.Contains(runner, "Join-Path $outputDirectory $assemblyName");
        StringAssert.Contains(runner, "if ($ValidateOnly)");
        StringAssert.Contains(runner, "[Windows.Markup.XamlReader]::Parse($xaml)");
        Assert.IsFalse(runner.Contains(@"bin\Debug\net45", StringComparison.Ordinal));

        StringAssert.Contains(readme, @".\run.ps1 -ValidateOnly");
        StringAssert.Contains(readme, "$window.FindName('Navigation')");
        StringAssert.Contains(readme, "GetAttribute('Name', $xamlNamespace)");
    }

    private static string FindRepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory != null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "ModernWpf.sln")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        Assert.Fail("Could not locate ModernWpf.sln from the test output directory.");
        return string.Empty;
    }
}
