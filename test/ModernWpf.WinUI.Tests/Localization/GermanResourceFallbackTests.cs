using System.Globalization;
using System.Reflection;
using System.Resources;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Controls;

namespace ModernWpf.WinUI.Tests.Localization;

[TestClass]
public class GermanResourceFallbackTests
{
    [TestMethod]
    [DataRow("de-DE")]
    [DataRow("de-CH")]
    [DataRow("de")]
    public void GermanResourcesFallBackAcrossGermanCultures(string cultureName)
    {
        var culture = CultureInfo.GetCultureInfo(cultureName);

        foreach (var resource in GetGermanResources())
        {
            var resourceManager = new ResourceManager(resource.BaseName, resource.Assembly);
            Assert.AreEqual(
                resource.ExpectedValue,
                resourceManager.GetString(resource.Key, culture),
                $"{resource.BaseName}.{resource.Key} did not use German for {cultureName}.");
        }
    }

    private static GermanResource[] GetGermanResources()
    {
        var coreAssembly = typeof(TextContextMenu).Assembly;
        var controlsAssembly = typeof(global::ModernWpf.Controls.NumberBox).Assembly;

        return new[]
        {
            new GermanResource(
                coreAssembly,
                "ModernWpf.ProgressBar.Strings.Resources",
                "ProgressBarErrorStatus",
                "Fehler"),
            new GermanResource(
                coreAssembly,
                "ModernWpf.Resources.Strings",
                "AppBarMoreButtonClosedToolTip",
                "Weitere Infos"),
            new GermanResource(
                coreAssembly,
                "ModernWpf.TextContextMenu.Strings.Resources",
                "ProofingMenuItemLabel",
                "Dokumentprüfung"),
            new GermanResource(
                controlsAssembly,
                "ModernWpf.Controls.NavigationView.Strings.Resources",
                "NavigationButtonClosedName",
                "Navigation öffnen"),
            new GermanResource(
                controlsAssembly,
                "ModernWpf.Controls.NumberBox.Strings.Resources",
                "NumberBoxDownSpinButtonName",
                "Verringerung"),
            new GermanResource(
                controlsAssembly,
                "ModernWpf.Controls.PersonPicture.Strings.Resources",
                "BadgeItemPlural1",
                "{0}, {1:d} Elemente"),
            new GermanResource(
                controlsAssembly,
                "ModernWpf.Controls.ProgressRing.Strings.Resources",
                "ProgressRingIndeterminateStatus",
                "Beschäftigt"),
            new GermanResource(
                controlsAssembly,
                "ModernWpf.Controls.RatingControl.Strings.Resources",
                "BasicRatingString",
                "Bewertung, {0} von {1}"),
            new GermanResource(
                controlsAssembly,
                "ModernWpf.Controls.SplitButton.Strings.Resources",
                "SplitButtonSecondaryButtonName",
                "Weitere Optionen")
        };
    }

    private sealed class GermanResource
    {
        public GermanResource(Assembly assembly, string baseName, string key, string expectedValue)
        {
            Assembly = assembly;
            BaseName = baseName;
            Key = key;
            ExpectedValue = expectedValue;
        }

        public Assembly Assembly { get; }

        public string BaseName { get; }

        public string Key { get; }

        public string ExpectedValue { get; }
    }
}
