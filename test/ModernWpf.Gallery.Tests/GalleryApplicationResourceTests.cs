using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Controls;

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
}
