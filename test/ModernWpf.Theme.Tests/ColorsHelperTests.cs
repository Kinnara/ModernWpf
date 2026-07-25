using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.Theme.Tests;

[TestClass]
public class ColorsHelperTests
{
    [TestMethod]
    public void FrozenThemeResourcesRefreshWhenAccentChanges()
    {
        WpfTestHost.Run(() =>
        {
            ThemeTestApplication.EnsureInitialized();
            Assert.IsTrue(ThemeResources.Current.CanBeAccessedAcrossThreads);

            var themeManager = ThemeManager.Current;
            var originalTheme = themeManager.ApplicationTheme;
            var originalAccent = themeManager.AccentColor;

            try
            {
                themeManager.ApplicationTheme = ApplicationTheme.Light;
                var themeResources = ThemeResources.Current;

                var target = new Border();
                target.SetResourceReference(
                    Border.BackgroundProperty,
                    "SystemControlForegroundAccentBrush");

                using var host = new TestWindowHost(target, width: 120, height: 80);
                host.UpdateLayout();
                WpfTestHost.DoEvents();

                var updatedAccent = Color.FromRgb(0x12, 0xA4, 0x6C);
                themeManager.AccentColor = updatedAccent;
                WpfTestHost.DoEvents();

                var accentBrush = (SolidColorBrush)target.Background;
                Assert.AreEqual(updatedAccent, accentBrush.Color);
                Assert.IsTrue(accentBrush.IsFrozen);
                Assert.AreEqual(
                    updatedAccent,
                    Task.Run(() => accentBrush.Color).GetAwaiter().GetResult());

                AssertFrozenBrushColor(
                    themeResources.GetThemeDictionary(ThemeManager.LightKey),
                    "SystemControlForegroundAccentBrush",
                    updatedAccent);
                AssertFrozenBrushColor(
                    themeResources.GetThemeDictionary(ThemeManager.DarkKey),
                    "SystemControlForegroundAccentBrush",
                    updatedAccent);
                AssertFrozenBrushColor(
                    themeResources.GetThemeDictionary(ThemeManager.HighContrastKey),
                    "SystemAccentColorDark1Brush",
                    (Color)ColorsHelper.Current.Colors["SystemAccentColorDark1"]);

                var focusedBorder = (LinearGradientBrush)themeResources
                    .GetThemeDictionary(ThemeManager.LightKey)["TextControlElevationBorderFocusedBrush"];
                Assert.IsTrue(focusedBorder.IsFrozen);
                Assert.AreEqual(
                    focusedBorder.GradientStops[0].Color,
                    Task.Run(() => focusedBorder.GradientStops[0].Color).GetAwaiter().GetResult());
            }
            finally
            {
                themeManager.AccentColor = originalAccent;
                themeManager.ApplicationTheme = originalTheme;
                WpfTestHost.DoEvents();
            }
        });
    }

    [TestMethod]
    public void UpdateBrushesTraversesMergedDictionaries()
    {
        var accent = Color.FromRgb(0x30, 0x80, 0xD0);
        var accentBrush = new SolidColorBrush();
        ThemeResourceHelper.SetColorKey(accentBrush, "SystemAccentColor");

        var child = new ResourceDictionary
        {
            ["SystemControlForegroundAccentBrush"] = accentBrush
        };
        var root = new ResourceDictionary();
        root.MergedDictionaries.Add(child);

        ColorsHelper.UpdateBrushes(
            root,
            new ResourceDictionary { ["SystemAccentColor"] = accent });

        Assert.AreEqual(accent, accentBrush.Color);
    }

    [TestMethod]
    public void TargetedColorPaletteKeepsCustomAccentAcrossGlobalPaletteRefresh()
    {
        WpfTestHost.Run(() =>
        {
            ThemeTestApplication.EnsureInitialized();

            var themeManager = ThemeManager.Current;
            var originalAccent = themeManager.AccentColor;
            var customAccent = Color.FromRgb(0xE0, 0x20, 0x30);
            var palette = new ColorPaletteResources
            {
                TargetTheme = ApplicationTheme.Light,
                Accent = customAccent
            };
            var accentBrush =
                (SolidColorBrush)palette["SystemControlForegroundAccentBrush"];
            Assert.AreEqual(customAccent, accentBrush.Color);

            try
            {
                themeManager.AccentColor = Color.FromRgb(0x20, 0x60, 0xC0);
                WpfTestHost.DoEvents();

                Assert.AreEqual(customAccent, accentBrush.Color);
            }
            finally
            {
                themeManager.AccentColor = originalAccent;
                WpfTestHost.DoEvents();
            }
        });
    }

    [TestMethod]
    [DataRow("Light", "SystemAccentColorDark1")]
    [DataRow("Dark", "SystemAccentColorLight2")]
    [DataRow("HighContrast", "SystemAccentColorLight2")]
    public void FocusedTextControlBorderTracksDynamicAccentColor(
        string themeName,
        string accentColorKey)
    {
        WpfTestHost.Run(() =>
        {
            var themeDictionary = new ResourceDictionary
            {
                Source = new Uri(
                    $"pack://application:,,,/ModernWpf;component/ThemeResources/{themeName}.xaml",
                    UriKind.Absolute)
            };
            var focusedBorder =
                (LinearGradientBrush)themeDictionary["TextControlElevationBorderFocusedBrush"];
            var expectedAccent = Color.FromRgb(0xC2, 0x39, 0xB3);
            var palette = new ResourceDictionary
            {
                [accentColorKey] = expectedAccent
            };

            ColorsHelper.UpdateBrushes(themeDictionary, palette);

            Assert.AreEqual(expectedAccent, focusedBorder.GradientStops[0].Color);
        });
    }

    [TestMethod]
    public void TransparentSystemAccentSnapshotDoesNotReplaceDynamicColorBrush()
    {
        var palette = new ResourceDictionary();
        var accent = Color.FromRgb(0x10, 0x70, 0xC0);
        var accentDark1 = Color.FromRgb(0x0E, 0x60, 0xA0);
        var accentDark2 = Color.FromRgb(0x0C, 0x50, 0x80);
        var accentDark3 = Color.FromRgb(0x0A, 0x40, 0x60);
        var accentLight1 = Color.FromRgb(0x38, 0x88, 0xC8);
        var accentLight2 = Color.FromRgb(0x60, 0xA0, 0xD0);
        var accentLight3 = Color.FromRgb(0x88, 0xB8, 0xD8);

        Assert.IsTrue(ColorsHelper.TryApplySystemAccentPalette(
            palette,
            accent,
            accentDark1,
            accentDark2,
            accentDark3,
            accentLight1,
            accentLight2,
            accentLight3));

        var accentBrush = new SolidColorBrush();
        ThemeResourceHelper.SetColorKey(accentBrush, "SystemAccentColor");
        var themeDictionary = new ResourceDictionary
        {
            ["SystemControlForegroundAccentBrush"] = accentBrush
        };

        ColorsHelper.UpdateBrushes(themeDictionary, palette);
        Assert.AreEqual(accent, accentBrush.Color);

        Assert.IsFalse(ColorsHelper.TryApplySystemAccentPalette(
            palette,
            Colors.Transparent,
            Colors.Transparent,
            Colors.Transparent,
            Colors.Transparent,
            Colors.Transparent,
            Colors.Transparent,
            Colors.Transparent));

        ColorsHelper.UpdateBrushes(themeDictionary, palette);
        Assert.AreEqual(accent, accentBrush.Color);
        Assert.AreEqual(accent, palette["SystemAccentColor"]);
        Assert.AreEqual(accentDark1, palette["SystemAccentColorDark1"]);
        Assert.AreEqual(accentDark2, palette["SystemAccentColorDark2"]);
        Assert.AreEqual(accentDark3, palette["SystemAccentColorDark3"]);
        Assert.AreEqual(accentLight1, palette["SystemAccentColorLight1"]);
        Assert.AreEqual(accentLight2, palette["SystemAccentColorLight2"]);
        Assert.AreEqual(accentLight3, palette["SystemAccentColorLight3"]);
    }

    private static void AssertFrozenBrushColor(
        ResourceDictionary resources,
        object key,
        Color expected)
    {
        var brush = (SolidColorBrush)resources[key];
        Assert.AreEqual(expected, brush.Color, key.ToString());
        Assert.IsTrue(brush.IsFrozen, key.ToString());
        Assert.AreEqual(
            expected,
            Task.Run(() => ((SolidColorBrush)resources[key]).Color).GetAwaiter().GetResult(),
            key.ToString());
    }
}
