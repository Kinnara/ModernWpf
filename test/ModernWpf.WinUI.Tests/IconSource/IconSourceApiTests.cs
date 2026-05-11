using System;
using System.Windows;
using System.Windows.Controls;
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
    public void ImageIconSourceTest()
    {
        WpfTestHost.Run(() =>
        {
            var imageSource = CreateTestImageSource(Brushes.Blue);
            var updatedImageSource = CreateTestImageSource(Brushes.Green);
            var iconSource = new ImageIconSource();
            var imageIcon = (ImageIcon)iconSource.CreateIconElement();

            Assert.IsNull(iconSource.Foreground);

            var icon = new ImageIcon();
            Assert.AreEqual(icon.Source, iconSource.ImageSource);
            Assert.AreEqual(imageIcon.Source, iconSource.ImageSource);

            iconSource.Foreground = Brushes.Red;
            iconSource.ImageSource = imageSource;

            Assert.AreSame(Brushes.Red, iconSource.Foreground);
            Assert.AreSame(Brushes.Red, imageIcon.Foreground);
            Assert.AreSame(imageSource, iconSource.ImageSource);
            Assert.AreSame(imageSource, imageIcon.Source);

            iconSource.ImageSource = updatedImageSource;

            Assert.AreSame(updatedImageSource, iconSource.ImageSource);
            Assert.AreSame(updatedImageSource, imageIcon.Source);
        });
    }

    [TestMethod]
    public void ImageIconTest()
    {
        WpfTestHost.Run(() =>
        {
            var imageSource = CreateTestImageSource(Brushes.Blue);
            var imageIcon = new ImageIcon
            {
                Foreground = Brushes.Red,
                Source = imageSource
            };

            using var host = new TestWindowHost(imageIcon, width: 64, height: 64);

            Assert.AreSame(Brushes.Red, imageIcon.Foreground);
            Assert.AreSame(imageSource, imageIcon.Source);

            var image = VisualTreeTestHelper.FindDescendant<Image>(imageIcon);
            Assert.IsNotNull(image);
            Assert.IsTrue(image!.IsLoaded);
            Assert.AreSame(imageSource, image.Source);
        });
    }

    [TestMethod]
    public void CreateIconElementReturnsCorrectTypeTest()
    {
        WpfTestHost.Run(() =>
        {
            Assert.IsInstanceOfType(new BitmapIconSource().CreateIconElement(), typeof(BitmapIcon));
            Assert.IsInstanceOfType(new FontIconSource().CreateIconElement(), typeof(FontIcon));
            Assert.IsInstanceOfType(new SymbolIconSource().CreateIconElement(), typeof(SymbolIcon));
            Assert.IsInstanceOfType(new PathIconSource().CreateIconElement(), typeof(PathIcon));
            Assert.IsInstanceOfType(new ImageIconSource().CreateIconElement(), typeof(ImageIcon));
        });
    }

    [TestMethod]
    public void CreateIconElementForegroundTest()
    {
        WpfTestHost.Run(() =>
        {
            var iconSourceWithForeground = new FontIconSource
            {
                Foreground = Brushes.Blue
            };
            var iconSourceWithoutForeground = new FontIconSource();

            var iconWithForeground = (FontIcon)iconSourceWithForeground.CreateIconElement();
            var iconWithoutForeground = (FontIcon)iconSourceWithoutForeground.CreateIconElement();

            Assert.AreSame(Brushes.Blue, iconWithForeground.Foreground);
            Assert.IsNotNull(iconWithoutForeground.Foreground);
        });
    }

    [TestMethod]
    public void PropertyChangePropagationToCreatedElements()
    {
        WpfTestHost.Run(() =>
        {
            var iconSource = new FontIconSource();
            var firstIcon = (FontIcon)iconSource.CreateIconElement();
            var secondIcon = (FontIcon)iconSource.CreateIconElement();

            Assert.IsNotNull(firstIcon.Foreground);
            Assert.IsNotNull(secondIcon.Foreground);

            iconSource.Foreground = Brushes.Red;
            iconSource.Glyph = "\uE001";
            iconSource.FontSize = 24;

            Assert.AreSame(Brushes.Red, firstIcon.Foreground);
            Assert.AreSame(Brushes.Red, secondIcon.Foreground);
            Assert.AreEqual("\uE001", firstIcon.Glyph);
            Assert.AreEqual("\uE001", secondIcon.Glyph);
            Assert.AreEqual(24.0, firstIcon.FontSize);
            Assert.AreEqual(24.0, secondIcon.FontSize);
        });
    }

    [TestMethod]
    public void CreateIconElementPreservesIconSourceProperties()
    {
        WpfTestHost.Run(() =>
        {
            var fontIconSource = new FontIconSource
            {
                Glyph = "\uE001",
                FontSize = 24,
                FontFamily = new FontFamily("Segoe UI Symbol"),
                Foreground = Brushes.Purple
            };

            var fontIcon = (FontIcon)fontIconSource.CreateIconElement();

            Assert.AreEqual("\uE001", fontIcon.Glyph);
            Assert.AreEqual(24.0, fontIcon.FontSize);
            Assert.AreEqual("Segoe UI Symbol", fontIcon.FontFamily.Source);
            Assert.AreSame(Brushes.Purple, fontIcon.Foreground);
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

    private static DrawingImage CreateTestImageSource(Brush brush)
    {
        return new DrawingImage(
            new GeometryDrawing(
                brush,
                null,
                new RectangleGeometry(new Rect(0, 0, 16, 16))));
    }
}
