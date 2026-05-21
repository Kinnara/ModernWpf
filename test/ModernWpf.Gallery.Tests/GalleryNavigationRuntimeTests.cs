using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Controls;
using ModernWpf.Gallery.Models;
using ModernWpf.Gallery.Pages;
using ModernWpf.Gallery.Shell;
using ModernWpf.Gallery.Testing;

namespace ModernWpf.Gallery.Tests
{
    [TestClass]
    public class GalleryNavigationRuntimeTests
    {
        [TestMethod]
        public void ShellCanNavigateEveryCatalogRouteWithoutExceptions()
        {
            WpfTestHost.Run(() =>
            {
                GalleryDiagnostics.Configure(GalleryLaunchOptions.Parse(new[] { "--visual-test" }));
                var failures = new List<string>();
                var page = new NavigationRootPage();
                var window = new Window
                {
                    Width = 1180,
                    Height = 820,
                    Left = -32000,
                    Top = -32000,
                    ShowInTaskbar = false,
                    WindowStartupLocation = WindowStartupLocation.Manual,
                    Content = page
                };

                try
                {
                    window.Show();
                    WpfTestHost.DoEvents();
                    window.UpdateLayout();
                    WpfTestHost.DoEvents();

                    foreach (var route in CatalogRoutes())
                    {
                        try
                        {
                            page.NavigateTo(route);
                            WpfTestHost.DoEvents();
                            window.UpdateLayout();
                            WpfTestHost.DoEvents();

                            if (!string.IsNullOrWhiteSpace(GalleryDiagnostics.LastException))
                            {
                                failures.Add(route + ": " + GalleryDiagnostics.LastException);
                            }
                        }
                        catch (Exception ex)
                        {
                            failures.Add(route + ": " + ex.GetType().Name + ": " + ex.Message);
                        }
                    }
                }
                finally
                {
                    window.Content = null;
                    window.Close();
                    WpfTestHost.DoEvents();
                    GalleryDiagnostics.ResetForTests();
                }

                Assert.AreEqual(0, failures.Count, string.Join(Environment.NewLine, failures.Take(20)));
            });
        }

        [TestMethod]
        public void ShellNavigationMenuMatchesWpfGalleryReferenceChrome()
        {
            WpfTestHost.Run(() =>
            {
                var page = new NavigationRootPage();
                var navigation = (NavigationView)page.FindName("Navigation");
                var topLevelItems = navigation.MenuItems.OfType<NavigationViewItem>().ToList();

                CollectionAssert.AreEqual(
                    new[] { "Home", "What's New", "Design Guidance", "Samples", "All controls", "Basic Input" },
                    topLevelItems.Take(6).Select(GetNavigationItemText).ToArray());

                AssertFontIconGlyph(topLevelItems[0], "\uE80F");
                AssertFontIconGlyph(topLevelItems[1], "\uEB51");
                AssertFontIconGlyph(topLevelItems[2], "\uEB3C");
                AssertFontIconGlyph(topLevelItems[3], "\uEF58");
                AssertFontIconGlyph(topLevelItems[4], "\uE71D");
                AssertFontIconGlyph(topLevelItems[5], "\uE73A");

                var designGuidanceItems = topLevelItems[2].MenuItems.OfType<NavigationViewItem>().ToList();
                CollectionAssert.AreEqual(
                    new[] { "Colors", "Typography", "Spacing", "Geometry", "Icons" },
                    designGuidanceItems.Select(GetNavigationItemText).ToArray());
                AssertFontIconGlyph(designGuidanceItems[0], "\uE790");

                var basicInputItems = topLevelItems[5].MenuItems.OfType<NavigationViewItem>().ToList();
                Assert.IsNull(basicInputItems[0].Icon);
            });
        }

        [TestMethod]
        public void HomePageOverviewUsesWpfReferenceGroupFilter()
        {
            WpfTestHost.Run(() =>
            {
                var page = new HomePage();
                var expected = GalleryCatalog.OverviewGroups.Select(group => group.UniqueId).ToArray();
                var actual = ((IEnumerable<GalleryGroup>)page.Groups).Select(group => group.UniqueId).ToArray();

                CollectionAssert.AreEqual(expected, actual);
                Assert.IsFalse(actual.Contains("DesignGuidance"));
                Assert.IsFalse(actual.Contains("Samples"));
            });
        }

        private static string GetNavigationItemText(NavigationViewItem item)
        {
            return item.Content as string;
        }

        private static void AssertFontIconGlyph(NavigationViewItem item, string expectedGlyph)
        {
            var icon = item.Icon as FontIcon;

            Assert.IsNotNull(icon);
            Assert.AreEqual(expectedGlyph, icon.Glyph);
        }

        private static IEnumerable<string> CatalogRoutes()
        {
            yield return "home";
            yield return "WhatsNew";
            yield return "AllControls";

            foreach (var group in GalleryCatalog.Groups)
            {
                yield return "category/" + group.UniqueId;
            }

            foreach (var item in GalleryCatalog.Items)
            {
                yield return "item/" + item.UniqueId;
            }
        }
    }
}
