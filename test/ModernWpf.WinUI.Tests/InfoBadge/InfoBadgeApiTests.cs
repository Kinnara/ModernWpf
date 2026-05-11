using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Controls;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.InfoBadge;

[TestClass]
public class InfoBadgeApiTests
{
    [TestMethod]
    public void InfoBadgeDisplayKindTest()
    {
        WpfTestHost.Run(() =>
        {
            var infoBadge = new ModernWpf.Controls.InfoBadge();
            var symbolIconSource = new SymbolIconSource
            {
                Symbol = Symbol.Setting
            };

            using var host = new TestWindowHost(infoBadge, width: 100, height: 100);

            var textBlock = FindNamedDescendant<TextBlock>(infoBadge, "ValueTextBlock");
            var iconPresenter = FindNamedDescendant<FrameworkElement>(infoBadge, "IconPresenter");

            Assert.AreEqual(Visibility.Collapsed, textBlock.Visibility);
            Assert.AreEqual(Visibility.Collapsed, iconPresenter.Visibility);

            infoBadge.IconSource = symbolIconSource;
            host.UpdateLayout();

            Assert.AreEqual(Visibility.Collapsed, textBlock.Visibility);
            Assert.AreEqual(Visibility.Visible, iconPresenter.Visibility);

            infoBadge.Value = 10;
            host.UpdateLayout();

            Assert.AreEqual(Visibility.Visible, textBlock.Visibility);
            Assert.AreEqual(Visibility.Collapsed, iconPresenter.Visibility);

            infoBadge.IconSource = null;
            host.UpdateLayout();

            Assert.AreEqual(Visibility.Visible, textBlock.Visibility);
            Assert.AreEqual(Visibility.Collapsed, iconPresenter.Visibility);

            infoBadge.Value = -1;
            host.UpdateLayout();

            Assert.AreEqual(Visibility.Collapsed, textBlock.Visibility);
            Assert.AreEqual(Visibility.Collapsed, iconPresenter.Visibility);
        });
    }

    [TestMethod]
    public void InfoBadgeSupportsWpfIconTypes()
    {
        WpfTestHost.Run(() =>
        {
            var infoBadge = new ModernWpf.Controls.InfoBadge();
            using var host = new TestWindowHost(infoBadge, width: 100, height: 100);

            infoBadge.IconSource = new SymbolIconSource { Symbol = Symbol.Setting };
            host.UpdateLayout();
            Assert.IsInstanceOfType(infoBadge.TemplateSettings.IconElement, typeof(SymbolIcon));

            infoBadge.IconSource = new PathIconSource
            {
                Data = new RectangleGeometry(new Rect(0, 0, 5, 2))
            };
            host.UpdateLayout();
            Assert.IsInstanceOfType(infoBadge.TemplateSettings.IconElement, typeof(PathIcon));

            infoBadge.IconSource = new FontIconSource
            {
                Glyph = "99+",
                FontFamily = new FontFamily("Segoe UI Symbol")
            };
            host.UpdateLayout();
            Assert.IsInstanceOfType(infoBadge.TemplateSettings.IconElement, typeof(FontIcon));

            infoBadge.IconSource = new BitmapIconSource
            {
                UriSource = new Uri("pack://application:,,,/ModernWpf.WinUI.Tests;component/Assets/rating_set.png", UriKind.Absolute)
            };
            host.UpdateLayout();
            Assert.IsInstanceOfType(infoBadge.TemplateSettings.IconElement, typeof(BitmapIcon));

            infoBadge.IconSource = new ImageIconSource
            {
                ImageSource = CreateTestImageSource()
            };
            host.UpdateLayout();
            Assert.IsInstanceOfType(infoBadge.TemplateSettings.IconElement, typeof(ImageIcon));
        });
    }

    [TestMethod]
    public void InfoBadgeValueLessThanNegativeOneThrows()
    {
        WpfTestHost.Run(() =>
        {
            var infoBadge = new ModernWpf.Controls.InfoBadge();
            using var host = new TestWindowHost(infoBadge, width: 100, height: 100);

            Assert.ThrowsException<ArgumentException>(() => infoBadge.Value = -10);
        });
    }

    private static DrawingImage CreateTestImageSource()
    {
        return new DrawingImage(
            new GeometryDrawing(
                Brushes.Blue,
                null,
                new RectangleGeometry(new Rect(0, 0, 16, 16))));
    }

    private static T FindNamedDescendant<T>(DependencyObject root, string name)
        where T : FrameworkElement
    {
        foreach (var descendant in VisualTreeTestHelper.EnumerateDescendants(root))
        {
            if (descendant is T element && element.Name == name)
            {
                return element;
            }
        }

        throw new InvalidOperationException($"Could not find descendant named '{name}'.");
    }
}
