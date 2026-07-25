using System.Windows;
using System.Windows.Media;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ModernWpf.Theme.Tests;

[TestClass]
public class ColorsHelperTests
{
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
}
