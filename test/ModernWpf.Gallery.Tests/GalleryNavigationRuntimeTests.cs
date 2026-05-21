using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
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

        [TestMethod]
        public void WpfGalleryPageShellCardsMatchReferenceAutomationAndLayout()
        {
            WpfTestHost.Run(() =>
            {
                var homePage = new HomePage();
                RenderPage(homePage, () =>
                {
                    Assert.AreEqual(AutomationHeadingLevel.Level1, AutomationProperties.GetHeadingLevel((TextBlock)homePage.FindName("HeroVersionText")));
                    Assert.AreEqual(AutomationHeadingLevel.Level1, AutomationProperties.GetHeadingLevel((TextBlock)homePage.FindName("HeroTitleText")));
                    Assert.AreEqual(AutomationHeadingLevel.Level2, AutomationProperties.GetHeadingLevel((TextBlock)homePage.FindName("OverviewHeaderText")));
                    Assert.AreEqual(AutomationHeadingLevel.Level2, AutomationProperties.GetHeadingLevel((TextBlock)homePage.FindName("RecentlyAddedHeaderText")));
                    AssertNavigationItemsControl((ItemsControl)homePage.FindName("OverviewItemsControl"), "Items in group");
                    AssertNavigationItemsControl((ItemsControl)homePage.FindName("RecentlyAddedItemsControl"), "Recently Added and Updated Samples Section");

                    var firstGroup = GalleryCatalog.OverviewGroups.First();
                    AssertRenderedNavigationCard((ItemsControl)homePage.FindName("OverviewItemsControl"), firstGroup.Title);
                });

                var basicInputGroup = GalleryCatalog.FindGroup("BasicInput");
                var sectionPage = new SectionPage(basicInputGroup);
                RenderPage(sectionPage, () =>
                {
                    AssertPageHeaderLabel((Label)sectionPage.FindName("TitleLabel"), "Basic Input Page", AutomationHeadingLevel.Level1, 0);
                    AssertPageHeaderLabel((Label)sectionPage.FindName("DescriptionLabel"), string.Empty, AutomationHeadingLevel.Level2, 1);
                    AssertNavigationItemsControl((ItemsControl)sectionPage.FindName("GroupItemsControl"), "Items in group");
                    AssertRenderedNavigationCard((ItemsControl)sectionPage.FindName("GroupItemsControl"), basicInputGroup.Items.First().Title);
                });

                var allControlsPage = new AllControlsPage();
                RenderPage(allControlsPage, () =>
                {
                    AssertPageHeaderLabel((Label)allControlsPage.FindName("TitleLabel"), "All Controls Page", AutomationHeadingLevel.Level1, 0);
                    AssertPageHeaderLabel((Label)allControlsPage.FindName("DescriptionLabel"), string.Empty, AutomationHeadingLevel.Level2, 1);
                    AssertNavigationItemsControl((ItemsControl)allControlsPage.FindName("AllControlsItemsControl"), "Items in group");
                    AssertRenderedNavigationCard((ItemsControl)allControlsPage.FindName("AllControlsItemsControl"), GalleryCatalog.AllControlsItems.First().Title);
                });

                var itemPage = new ItemPage(GalleryCatalog.FindItem("Color"));
                RenderPage(itemPage, () =>
                {
                    AssertPageHeaderLabel((Label)itemPage.FindName("TitleLabel"), "Colors Page", AutomationHeadingLevel.Level1, 0);
                    AssertPageHeaderLabel((Label)itemPage.FindName("DescriptionLabel"), string.Empty, AutomationHeadingLevel.Level2, 1);
                });
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

        private static void AssertPageHeaderLabel(Label label, string automationName, AutomationHeadingLevel headingLevel, int tabIndex)
        {
            BindingOperations.GetBindingExpression(label, AutomationProperties.NameProperty)?.UpdateTarget();
            Assert.IsTrue(label.Focusable);
            Assert.AreEqual(KeyboardNavigationMode.Continue, KeyboardNavigation.GetTabNavigation(label));
            Assert.IsTrue(KeyboardNavigation.GetIsTabStop(label));
            Assert.AreEqual(tabIndex, KeyboardNavigation.GetTabIndex(label));
            Assert.AreEqual(automationName, AutomationProperties.GetName(label));
            Assert.AreEqual(headingLevel, AutomationProperties.GetHeadingLevel(label));
        }

        private static void AssertNavigationItemsControl(ItemsControl itemsControl, string automationName)
        {
            Assert.AreEqual(automationName, AutomationProperties.GetName(itemsControl));
            Assert.IsFalse(itemsControl.Focusable);
            var panel = (System.Windows.Controls.WrapPanel)itemsControl.ItemsPanel.LoadContent();
            Assert.AreEqual(new Thickness(10), panel.Margin);
        }

        private static void AssertRenderedNavigationCard(ItemsControl itemsControl, string title)
        {
            itemsControl.UpdateLayout();
            WpfTestHost.DoEvents();

            var button = FindVisualChildren<Button>(itemsControl).FirstOrDefault();
            Assert.IsNotNull(button);
            Assert.AreEqual(title + "Page", AutomationProperties.GetName(button));

            var titleText = FindVisualChildren<TextBlock>(button).FirstOrDefault(textBlock => string.Equals(textBlock.Text, title, StringComparison.Ordinal));
            Assert.IsNotNull(titleText);
            Assert.AreEqual(AutomationHeadingLevel.Level3, AutomationProperties.GetHeadingLevel(titleText));
        }

        private static void RenderPage(FrameworkElement page, Action assert)
        {
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
                assert();
            }
            finally
            {
                window.Content = null;
                window.Close();
                WpfTestHost.DoEvents();
            }
        }

        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject element)
            where T : DependencyObject
        {
            if (element == null)
            {
                yield break;
            }

            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
            {
                var child = VisualTreeHelper.GetChild(element, i);
                if (child is T match)
                {
                    yield return match;
                }

                foreach (var descendant in FindVisualChildren<T>(child))
                {
                    yield return descendant;
                }
            }
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
