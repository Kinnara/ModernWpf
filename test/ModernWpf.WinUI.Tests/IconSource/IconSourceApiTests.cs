using System;
using System.Windows;
using System.Windows.Media;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Controls;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.IconSource;

[TestClass]
public class IconSourceApiTests
{
    [TestMethod]
    public void SymbolIconSourceTest()
    {
        WpfTestHost.Run(() =>
        {
            var iconSource = new SymbolIconSource();
            var symbolIcon = (SymbolIcon)iconSource.CreateIconElement();

            Assert.IsNull(iconSource.Foreground);

            var icon = new SymbolIcon();
            Assert.AreEqual(icon.Symbol, iconSource.Symbol);
            Assert.AreEqual(symbolIcon.Symbol, iconSource.Symbol);

            iconSource.Foreground = Brushes.Red;
            iconSource.Symbol = Symbol.HangUp;

            Assert.AreSame(Brushes.Red, iconSource.Foreground);
            Assert.AreSame(Brushes.Red, symbolIcon.Foreground);
            Assert.AreEqual(Symbol.HangUp, iconSource.Symbol);
            Assert.AreEqual(Symbol.HangUp, symbolIcon.Symbol);
        });
    }

    [TestMethod]
    public void FontIconSourceTest()
    {
        WpfTestHost.Run(() =>
        {
            var iconSource = new FontIconSource();
            var fontIcon = (FontIcon)iconSource.CreateIconElement();

            Assert.IsNull(iconSource.Foreground);

            var icon = new FontIcon();
            Assert.AreEqual(icon.Glyph, iconSource.Glyph);
            Assert.AreEqual(fontIcon.Glyph, iconSource.Glyph);
            Assert.AreEqual(icon.FontSize, iconSource.FontSize);
            Assert.AreEqual(fontIcon.FontSize, iconSource.FontSize);
            Assert.AreEqual(icon.FontStyle, iconSource.FontStyle);
            Assert.AreEqual(fontIcon.FontStyle, iconSource.FontStyle);
            Assert.AreEqual(icon.FontWeight, iconSource.FontWeight);
            Assert.AreEqual(fontIcon.FontWeight, iconSource.FontWeight);
            Assert.AreEqual(icon.FontFamily.Source, iconSource.FontFamily.Source);
            Assert.AreEqual(fontIcon.FontFamily.Source, iconSource.FontFamily.Source);

            iconSource.Foreground = Brushes.Red;
            iconSource.Glyph = "&#xE114;";
            iconSource.FontSize = 25;
            iconSource.FontStyle = FontStyles.Oblique;
            iconSource.FontWeight = FontWeights.ExtraLight;
            iconSource.FontFamily = new FontFamily("Segoe UI Symbol");

            Assert.AreSame(Brushes.Red, iconSource.Foreground);
            Assert.AreSame(Brushes.Red, fontIcon.Foreground);
            Assert.AreEqual("&#xE114;", iconSource.Glyph);
            Assert.AreEqual("&#xE114;", fontIcon.Glyph);
            Assert.AreEqual(25.0, iconSource.FontSize);
            Assert.AreEqual(25.0, fontIcon.FontSize);
            Assert.AreEqual(FontStyles.Oblique, iconSource.FontStyle);
            Assert.AreEqual(FontStyles.Oblique, fontIcon.FontStyle);
            Assert.AreEqual(FontWeights.ExtraLight, iconSource.FontWeight);
            Assert.AreEqual(FontWeights.ExtraLight, fontIcon.FontWeight);
            Assert.AreEqual("Segoe UI Symbol", iconSource.FontFamily.Source);
            Assert.AreEqual("Segoe UI Symbol", fontIcon.FontFamily.Source);
        });
    }

    [TestMethod]
    public void BitmapIconSourceTest()
    {
        WpfTestHost.Run(() =>
        {
            var uri = new Uri("pack://application:,,,/ModernWpf.WinUI.Tests;component/Assets/rating_set.png", UriKind.Absolute);
            var iconSource = new BitmapIconSource();
            var bitmapIcon = (BitmapIcon)iconSource.CreateIconElement();

            Assert.IsNull(iconSource.Foreground);

            var icon = new BitmapIcon();
            Assert.AreEqual(icon.UriSource, iconSource.UriSource);
            Assert.AreEqual(bitmapIcon.UriSource, iconSource.UriSource);
            Assert.AreEqual(icon.ShowAsMonochrome, iconSource.ShowAsMonochrome);
            Assert.AreEqual(bitmapIcon.ShowAsMonochrome, iconSource.ShowAsMonochrome);

            iconSource.Foreground = Brushes.Red;
            iconSource.UriSource = uri;
            iconSource.ShowAsMonochrome = false;

            Assert.AreSame(Brushes.Red, iconSource.Foreground);
            Assert.AreSame(Brushes.Red, bitmapIcon.Foreground);
            Assert.AreEqual(uri, iconSource.UriSource);
            Assert.AreEqual(uri, bitmapIcon.UriSource);
            Assert.IsFalse(iconSource.ShowAsMonochrome);
            Assert.IsFalse(bitmapIcon.ShowAsMonochrome);
        });
    }

    [TestMethod]
    public void PathIconSourceTest()
    {
        WpfTestHost.Run(() =>
        {
            var iconSource = new PathIconSource();
            var pathIcon = (PathIcon)iconSource.CreateIconElement();

            Assert.IsNull(iconSource.Foreground);

            var icon = new PathIcon();
            Assert.AreEqual(icon.Data, iconSource.Data);
            Assert.AreEqual(pathIcon.Data, iconSource.Data);

            var rectGeometry = new RectangleGeometry();
            iconSource.Foreground = Brushes.Red;
            iconSource.Data = rectGeometry;

            Assert.AreSame(Brushes.Red, iconSource.Foreground);
            Assert.AreSame(Brushes.Red, pathIcon.Foreground);
            Assert.AreSame(rectGeometry, iconSource.Data);
            Assert.AreSame(rectGeometry, pathIcon.Data);
        });
    }

    [TestMethod]
    public void VerifyFontWeightPropertyMetadata()
    {
        WpfTestHost.Run(() =>
        {
            Assert.AreEqual(typeof(FontWeight), FontIconSource.FontWeightProperty.PropertyType);
            Assert.AreEqual(typeof(ValueType), typeof(FontWeight).BaseType);
        });
    }
}
