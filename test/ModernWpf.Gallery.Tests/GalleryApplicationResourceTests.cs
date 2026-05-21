using System;
using System.Linq;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Controls;
using ModernWpf.Gallery.Pages;

namespace ModernWpf.Gallery.Tests
{
    [TestClass]
    public class GalleryApplicationResourceTests
    {
        private const string PlatformFluentThemePrefix =
            "pack://application:,,,/PresentationFramework.Fluent;component/Themes/";

        [TestMethod]
        public void GalleryUsesRecommendedFluentResourceEntry()
        {
            WpfTestHost.Run(() =>
            {
                var app = Application.Current;
                Assert.IsNotNull(app);

                Assert.IsTrue(app.Resources.MergedDictionaries.OfType<ThemeResources>().Any());
                Assert.IsTrue(app.Resources.MergedDictionaries.OfType<FluentControlsResources>().Any());
                Assert.IsFalse(app.Resources.MergedDictionaries.OfType<XamlControlsResources>().Any());

                var fontFamily = (FontFamily)app.FindResource("SymbolThemeFontFamily");
                Assert.AreEqual("Segoe Fluent Icons, Segoe MDL2 Assets", fontFamily.Source);

#if NET10_0_OR_GREATER
#pragma warning disable WPF0001
                Assert.AreEqual("System", app.ThemeMode.Value);
#pragma warning restore WPF0001
                Assert.AreEqual(1, CountPlatformFluentThemeDictionaries(app.Resources));
#else
                Assert.AreEqual(0, CountPlatformFluentThemeDictionaries(app.Resources));
#endif
            });
        }

        [TestMethod]
        public void HomeHeaderTilesMatchWpfGalleryReferenceSlotGeometry()
        {
            WpfTestHost.Run(() =>
            {
                var page = new HomePage();
                var tilesPanel = (StackPanel)page.FindName("TilesPanel");
                var buttons = tilesPanel.Children.OfType<Button>().ToArray();

                Assert.AreEqual(5, buttons.Length);

                foreach (var button in buttons)
                {
                    Assert.AreEqual(186d, button.Width);
                    Assert.AreEqual(208d, button.Height);
                }

                Assert.AreEqual(new Thickness(6, 6, 12, 6), buttons[0].Margin);

                foreach (var button in buttons.Skip(1))
                {
                    Assert.AreEqual(new Thickness(6), button.Margin);
                }

                CollectionAssert.AreEqual(
                    new[] { "Getting started", "Windows design", "WPF GitHub", "Code samples", "Partner Center" },
                    buttons.Select(AutomationProperties.GetName).ToArray());

                Assert.AreEqual("Scroll left", AutomationProperties.GetName((Button)page.FindName("ScrollBackButton")));
                Assert.AreEqual("Scroll right", AutomationProperties.GetName((Button)page.FindName("ScrollForwardButton")));
            });
        }

        [TestMethod]
        public void HomeHeaderTilesUseWpfGalleryAcrylicFillResources()
        {
            WpfTestHost.Run(() =>
            {
                var style = (Style)Application.Current.FindResource("GalleryHeaderTileButtonStyle");
                var acrylicBrush = Application.Current.FindResource("AcrylicBackgroundFillColorDefaultBrush");

                AssertHeaderTileBrush(style.Resources["ButtonBackground"], acrylicBrush, 0.8);
                AssertHeaderTileBrush(style.Resources["ButtonBackgroundPointerOver"], acrylicBrush, 0.9);
                AssertHeaderTileBrush(style.Resources["ButtonBackgroundPressed"], acrylicBrush, 1.0);
            });
        }

        [TestMethod]
        public void ControlExampleSourceCodeTextStyleUsesWpfGalleryReferenceResources()
        {
            WpfTestHost.Run(() =>
            {
                var style = (Style)Application.Current.FindResource("SelectionTextBox");

                AssertDynamicResourceSetter(style, Control.ForegroundProperty, "TextControlForeground");
                AssertDynamicResourceSetter(style, Control.FontSizeProperty, "ControlContentThemeFontSize");
            });
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

        private static void AssertHeaderTileBrush(object resource, object acrylicBrush, double opacity)
        {
            var brush = resource as SolidColorBrush;
            Assert.IsNotNull(brush);
            Assert.AreEqual(opacity, brush.Opacity, 0.001);

            var binding = BindingOperations.GetBinding(brush, SolidColorBrush.ColorProperty);
            Assert.IsNotNull(binding);
            Assert.AreEqual("Color", binding.Path.Path);
            Assert.AreSame(acrylicBrush, binding.Source);
        }

        private static void AssertDynamicResourceSetter(Style style, DependencyProperty property, object resourceKey)
        {
            var setter = style.Setters.OfType<Setter>().Single(item => item.Property == property);
            var dynamicResource = setter.Value as DynamicResourceExtension;

            Assert.IsNotNull(dynamicResource);
            Assert.AreEqual(resourceKey, dynamicResource.ResourceKey);
        }
    }
}
