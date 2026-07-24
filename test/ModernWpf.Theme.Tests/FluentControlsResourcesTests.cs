using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Controls;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.Theme.Tests;

[TestClass]
public class FluentControlsResourcesTests
{
    private const string PlatformFluentThemePrefix =
        "pack://application:,,,/PresentationFramework.Fluent;component/Themes/";

    [TestMethod]
    public void FluentControlsResourcesUsesExpectedResourceLayers()
    {
        WpfTestHost.Run(() =>
        {
            var resources = new FluentControlsResources();

#if NET10_0_OR_GREATER
            Assert.AreEqual(
                0,
                CountPlatformFluentThemeDictionaries(resources),
                "The net10 Fluent entry should rely on WPF ThemeMode instead of nesting platform Fluent dictionaries.");
            AssertHasMergedSource(resources, "ModernWpfControlsResources.xaml");
#else
            Assert.AreEqual(0, CountPlatformFluentThemeDictionaries(resources));
            AssertHasMergedSource(resources, "ControlsResources.xaml");
#endif
            AssertHasMergedDictionaryType(resources, "UISettingsResources");
        });
    }

    [TestMethod]
    public void FluentControlsResourcesCompactResourcesCanBeToggled()
    {
        WpfTestHost.Run(() =>
        {
            var resources = new FluentControlsResources();
            AssertHasNoMergedSource(resources, "DensityStyles/Compact.xaml");

            resources.UseCompactResources = true;
            AssertHasMergedSource(resources, "DensityStyles/Compact.xaml");

            resources.UseCompactResources = false;
            AssertHasNoMergedSource(resources, "DensityStyles/Compact.xaml");
        });
    }

    [TestMethod]
    public void RecommendedResourceEntryAddsOnePlatformFluentDictionary()
    {
        WpfTestHost.Run(() =>
        {
            var app = ThemeTestApplication.EnsureInitialized();

#if NET10_0_OR_GREATER
#pragma warning disable WPF0001
            Assert.AreEqual("System", app.ThemeMode.Value);
#pragma warning restore WPF0001
            Assert.AreEqual(
                1,
                CountPlatformFluentThemeDictionaries(app.Resources),
                "The recommended entry should leave exactly one official WPF Fluent theme dictionary in application resources.");
#else
            Assert.AreEqual(0, CountPlatformFluentThemeDictionaries(app.Resources));
#endif
        });
    }

    [TestMethod]
    public void LegacyXamlControlsResourcesDoesNotMergePlatformFluent()
    {
        WpfTestHost.Run(() =>
        {
            var resources = new XamlControlsResources();

            Assert.AreEqual(0, CountPlatformFluentThemeDictionaries(resources));
            AssertHasMergedSource(resources, "ControlsResources.xaml");
            AssertHasMergedDictionaryType(resources, "UISettingsResources");
        });
    }

    [TestMethod]
    public void SymbolThemeFontFamilyMatchesOfficialFluentFallback()
    {
        WpfTestHost.Run(() =>
        {
            var element = new Border();
            element.Resources.MergedDictionaries.Add(new ModernWpf.ThemeResources());
            element.Resources.MergedDictionaries.Add(new FluentControlsResources());

            var fontFamily = (FontFamily)element.FindResource("SymbolThemeFontFamily");

            Assert.AreEqual("Segoe Fluent Icons, Segoe MDL2 Assets", fontFamily.Source);
        });
    }

#if NET10_0_OR_GREATER
    [TestMethod]
    public void ApplicationThemeMapsToPlatformThemeMode()
    {
        WpfTestHost.Run(() =>
        {
            var app = ThemeTestApplication.EnsureInitialized();
            var themeManager = ThemeManager.Current;
            var originalTheme = themeManager.ApplicationTheme;

            try
            {
#pragma warning disable WPF0001
                themeManager.ApplicationTheme = null;
                Assert.AreEqual("System", app.ThemeMode.Value);

                themeManager.ApplicationTheme = ApplicationTheme.Light;
                Assert.AreEqual("Light", app.ThemeMode.Value);

                themeManager.ApplicationTheme = ApplicationTheme.Dark;
                Assert.AreEqual("Dark", app.ThemeMode.Value);
#pragma warning restore WPF0001
            }
            finally
            {
                themeManager.ApplicationTheme = originalTheme;
            }
        });
    }

    [TestMethod]
    public void WindowRequestedThemeMapsToPlatformThemeMode()
    {
        WpfTestHost.Run(() =>
        {
            ThemeTestApplication.EnsureInitialized();

            var window = CreateHiddenWindow();
            try
            {
                window.Show();
                WpfTestHost.DoEvents();

                ThemeManager.SetIsThemeAware(window, true);
#pragma warning disable WPF0001
                Assert.AreEqual("None", window.ThemeMode.Value);

                ThemeManager.SetRequestedTheme(window, ElementTheme.Light);
                Assert.AreEqual("Light", window.ThemeMode.Value);

                ThemeManager.SetRequestedTheme(window, ElementTheme.Dark);
                Assert.AreEqual("Dark", window.ThemeMode.Value);

                ThemeManager.SetRequestedTheme(window, ElementTheme.Default);
                Assert.AreEqual("None", window.ThemeMode.Value);
#pragma warning restore WPF0001
            }
            finally
            {
                window.Close();
                WpfTestHost.DoEvents();
            }
        });
    }

    [TestMethod]
    public void ElementRequestedThemeUsesModernWpfThemeDictionaries()
    {
        WpfTestHost.Run(() =>
        {
            ThemeTestApplication.EnsureInitialized();

            var element = new Border();
            var window = CreateHiddenWindow(element);
            try
            {
                window.Show();
                WpfTestHost.DoEvents();

                ThemeManager.SetRequestedTheme(element, ElementTheme.Dark);

                Assert.AreEqual(ElementTheme.Dark, ThemeManager.GetActualTheme(element));
                AssertHasMergedSource(element.Resources, "ThemeResources/Dark.xaml");
                Assert.AreEqual(0, CountPlatformFluentThemeDictionaries(element.Resources));
            }
            finally
            {
                window.Content = null;
                window.Close();
                WpfTestHost.DoEvents();
            }
        });
    }
#endif

    private static Window CreateHiddenWindow(object? content = null)
    {
        return new Window
        {
            Width = 200,
            Height = 120,
            Left = -32000,
            Top = -32000,
            ShowInTaskbar = false,
            WindowStartupLocation = WindowStartupLocation.Manual,
            Content = content
        };
    }

    private static void AssertHasMergedSource(ResourceDictionary resources, string sourceSuffix)
    {
        Assert.IsTrue(
            ContainsMergedSource(resources, sourceSuffix),
            $"Expected a merged dictionary ending with '{sourceSuffix}'.");
    }

    private static void AssertHasNoMergedSource(ResourceDictionary resources, string sourceSuffix)
    {
        Assert.IsFalse(
            ContainsMergedSource(resources, sourceSuffix),
            $"Did not expect a merged dictionary ending with '{sourceSuffix}'.");
    }

    private static void AssertHasMergedDictionaryType(ResourceDictionary resources, string typeName)
    {
        Assert.IsTrue(
            resources.MergedDictionaries.Any(dictionary => dictionary.GetType().Name == typeName),
            $"Expected a merged dictionary of type {typeName}.");
    }

    private static bool ContainsMergedSource(ResourceDictionary resources, string sourceSuffix)
    {
        foreach (var dictionary in resources.MergedDictionaries)
        {
            if (dictionary.Source?.ToString().EndsWith(sourceSuffix, StringComparison.OrdinalIgnoreCase) == true)
            {
                return true;
            }

            if (ContainsMergedSource(dictionary, sourceSuffix))
            {
                return true;
            }
        }

        return false;
    }

    private static int CountPlatformFluentThemeDictionaries(ResourceDictionary resources)
    {
        var count = IsPlatformFluentThemeDictionary(resources) ? 1 : 0;

        foreach (var dictionary in resources.MergedDictionaries)
        {
            count += CountPlatformFluentThemeDictionaries(dictionary);
        }

        return count;
    }

    private static bool IsPlatformFluentThemeDictionary(ResourceDictionary dictionary)
    {
        return dictionary.Source?.ToString().StartsWith(PlatformFluentThemePrefix, StringComparison.OrdinalIgnoreCase) == true;
    }
}
