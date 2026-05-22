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
using ModernWpf.Gallery.Controls;
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

                var mediaItem = topLevelItems.Single(item => string.Equals(GetNavigationItemText(item), "Media Controls", StringComparison.Ordinal));
                AssertFontIconGlyph(mediaItem, "\uE8B9");
                CollectionAssert.AreEqual(
                    new[] { "Canvas", "Image" },
                    mediaItem.MenuItems.OfType<NavigationViewItem>().Select(GetNavigationItemText).ToArray());
                Assert.IsNull(mediaItem.MenuItems.OfType<NavigationViewItem>().First().Icon);
            });
        }

        [TestMethod]
        public void HomePageOverviewUsesWpfReferenceGroupFilter()
        {
            WpfTestHost.Run(() =>
            {
                var page = new HomePage();
                var expected = GalleryCatalog.OverviewGroups.Select(group => group.UniqueId).ToArray();
                var actual = ((IEnumerable<GalleryGroup>)page.NavigationCards).Select(group => group.UniqueId).ToArray();

                CollectionAssert.AreEqual(expected, actual);
                Assert.IsFalse(actual.Contains("DesignGuidance"));
                Assert.IsFalse(actual.Contains("Samples"));
                CollectionAssert.AreEqual(
                    GalleryCatalog.NewOrUpdatedItems.Select(item => item.UniqueId).ToArray(),
                    ((IEnumerable<GalleryItem>)page.RecentlyAddedOrUpdatedSamplesInfo).Select(item => item.UniqueId).ToArray());
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
                    AssertBindingPath((ItemsControl)homePage.FindName("OverviewItemsControl"), ItemsControl.ItemsSourceProperty, "ViewModel.NavigationCards");
                    AssertBindingPath((ItemsControl)homePage.FindName("RecentlyAddedItemsControl"), ItemsControl.ItemsSourceProperty, "ViewModel.RecentlyAddedOrUpdatedSamplesInfo");
                    Assert.AreEqual(new Thickness(0), ((Grid)homePage.FindName("HomeContentGrid")).Margin);
                    Assert.AreEqual(new Thickness(0), ((TextBlock)homePage.FindName("RecentlyAddedHeaderText")).Margin);

                    var firstGroup = GalleryCatalog.OverviewGroups.First();
                    AssertRenderedNavigationCard((ItemsControl)homePage.FindName("OverviewItemsControl"), firstGroup.Title, firstGroup.Description, homePage.ViewModel.NavigateCommand);
                });

                var basicInputGroup = GalleryCatalog.FindGroup("BasicInput");
                var sectionPage = new SectionPage(basicInputGroup);
                RenderPage(sectionPage, () =>
                {
                    AssertReferencePageHeader((PageHeader)sectionPage.FindName("PageHeader"), basicInputGroup.Title, basicInputGroup.PageDescription, true);
                    AssertNavigationItemsControl((ItemsControl)sectionPage.FindName("GroupItemsControl"), "Items in group");
                    AssertBindingPath((ItemsControl)sectionPage.FindName("GroupItemsControl"), ItemsControl.ItemsSourceProperty, "ViewModel.NavigationCards");
                    AssertReferenceCategoryPageRoot((Grid)sectionPage.FindName("ContentRootGrid"), false);
                    Assert.AreEqual(basicInputGroup.Title, sectionPage.PageTitle);
                    Assert.AreEqual(basicInputGroup.PageDescription, sectionPage.PageDescription);
                    AssertRenderedNavigationCard((ItemsControl)sectionPage.FindName("GroupItemsControl"), basicInputGroup.Items.First().Title, basicInputGroup.Items.First().Description, sectionPage.ViewModel.NavigateCommand);
                });

                var mediaGroup = GalleryCatalog.FindGroup("Media");
                var mediaPage = new SectionPage(mediaGroup);
                RenderPage(mediaPage, () =>
                {
                    AssertReferencePageHeader((PageHeader)mediaPage.FindName("PageHeader"), mediaGroup.Title, mediaGroup.PageDescription, true);
                    AssertReferenceCategoryPageRoot((Grid)mediaPage.FindName("ContentRootGrid"), false);
                    AssertRenderedNavigationCard((ItemsControl)mediaPage.FindName("GroupItemsControl"), "Canvas", GalleryCatalog.FindItem("Canvas").Description, mediaPage.ViewModel.NavigateCommand);
                });

                var allControlsPage = new AllControlsPage();
                RenderPage(allControlsPage, () =>
                {
                    AssertReferencePageHeader((PageHeader)allControlsPage.FindName("PageHeader"), "All Controls", string.Empty, true);
                    AssertNavigationItemsControl((ItemsControl)allControlsPage.FindName("AllControlsItemsControl"), "Items in group");
                    AssertBindingPath((ItemsControl)allControlsPage.FindName("AllControlsItemsControl"), ItemsControl.ItemsSourceProperty, "ViewModel.NavigationCards");
                    AssertReferenceCategoryPageRoot((Grid)allControlsPage.FindName("ContentRootGrid"), true);
                    Assert.AreEqual("All Controls", allControlsPage.PageTitle);
                    Assert.AreEqual(string.Empty, allControlsPage.PageDescription);
                    AssertRenderedNavigationCard((ItemsControl)allControlsPage.FindName("AllControlsItemsControl"), GalleryCatalog.AllControlsItems.First().Title, GalleryCatalog.AllControlsItems.First().Description, allControlsPage.ViewModel.NavigateCommand);
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

        private static void AssertReferencePageHeader(PageHeader pageHeader, string title, string description, bool assertOfficialBindings = false)
        {
            Assert.IsNotNull(pageHeader);
            Assert.AreEqual(new Thickness(0, 0, 0, 40), pageHeader.Margin);
            Assert.AreEqual(title, pageHeader.Title);
            Assert.AreEqual(description, pageHeader.Description);

            if (assertOfficialBindings)
            {
                AssertBindingPath(pageHeader, PageHeader.TitleProperty, "ViewModel.PageTitle");
                AssertBindingPath(pageHeader, PageHeader.DescriptionProperty, "ViewModel.PageDescription");
            }

            pageHeader.ApplyTemplate();

            var labels = FindVisualChildren<Label>(pageHeader).ToArray();
            Assert.AreEqual(2, labels.Length);

            var titleLabel = (Label)pageHeader.Template.FindName("TitleTextBlock", pageHeader);
            Assert.AreSame(labels[0], titleLabel);
            Assert.AreEqual(string.Empty, labels[1].Name);

            AssertPageHeaderLabel(
                titleLabel,
                title + " Page",
                AutomationHeadingLevel.Level1,
                0);

            AssertPageHeaderLabel(
                labels[1],
                string.Empty,
                AutomationHeadingLevel.Level2,
                1);
        }

        private static void AssertBindingPath(DependencyObject target, DependencyProperty property, string expectedPath)
        {
            var expression = BindingOperations.GetBindingExpression(target, property);
            Assert.IsNotNull(expression);
            Assert.AreEqual(expectedPath, expression.ParentBinding.Path.Path);
        }

        private static void AssertNavigationItemsControl(ItemsControl itemsControl, string automationName)
        {
            Assert.AreEqual(automationName, AutomationProperties.GetName(itemsControl));
            Assert.IsFalse(itemsControl.Focusable);
            var panel = (System.Windows.Controls.WrapPanel)itemsControl.ItemsPanel.LoadContent();
            Assert.AreEqual(new Thickness(10), panel.Margin);
        }

        private static void AssertReferenceCategoryPageRoot(Grid root, bool hasItemsScrollViewer)
        {
            Assert.AreEqual(new Thickness(0), root.Margin);
            Assert.AreEqual(2, root.RowDefinitions.Count);
            Assert.AreEqual(GridUnitType.Auto, root.RowDefinitions[0].Height.GridUnitType);
            Assert.AreEqual(GridUnitType.Star, root.RowDefinitions[1].Height.GridUnitType);

            var scrollViewers = root.Children.OfType<ScrollViewer>().ToArray();
            if (hasItemsScrollViewer)
            {
                Assert.AreEqual(1, scrollViewers.Length);
                Assert.AreEqual(1, Grid.GetRow(scrollViewers[0]));
                Assert.AreEqual(new Thickness(0), scrollViewers[0].Margin);
                Assert.AreEqual(ScrollBarVisibility.Auto, scrollViewers[0].VerticalScrollBarVisibility);
            }
            else
            {
                Assert.AreEqual(0, scrollViewers.Length);
            }
        }

        private static void AssertRenderedNavigationCard(ItemsControl itemsControl, string title, string description, ICommand expectedCommand)
        {
            itemsControl.UpdateLayout();
            WpfTestHost.DoEvents();

            var button = FindVisualChildren<Button>(itemsControl).FirstOrDefault();
            Assert.IsNotNull(button);
            Assert.AreEqual(360d, button.Width);
            Assert.AreEqual(90d, button.Height);
            Assert.AreEqual(new Thickness(7), button.Margin);
            Assert.AreEqual(new Thickness(20, 10, 20, 10), button.Padding);
            Assert.AreEqual(HorizontalAlignment.Left, button.HorizontalContentAlignment);
            Assert.AreSame(expectedCommand, button.Command);
            Assert.AreSame(itemsControl.Items[0], button.CommandParameter);
            Assert.AreEqual(title + "Page", AutomationProperties.GetName(button));

            var image = FindVisualChildren<Image>(button).FirstOrDefault();
            Assert.IsNotNull(image);
            Assert.AreEqual(50d, image.Width);
            Assert.AreEqual(50d, image.Height);
            Assert.AreEqual(new Thickness(0, 0, 8, 0), image.Margin);

            var titleText = FindVisualChildren<TextBlock>(button).FirstOrDefault(textBlock => string.Equals(textBlock.Text, title, StringComparison.Ordinal));
            Assert.IsNotNull(titleText);
            Assert.AreEqual(AutomationHeadingLevel.Level3, AutomationProperties.GetHeadingLevel(titleText));

            if (!string.IsNullOrEmpty(description))
            {
                var descriptionText = FindVisualChildren<TextBlock>(button).FirstOrDefault(textBlock => string.Equals(textBlock.Text, description, StringComparison.Ordinal));
                Assert.IsNotNull(descriptionText);
                Assert.AreEqual(240d, descriptionText.Width);
                Assert.AreEqual(0.7, descriptionText.Opacity, 0.001);
            }
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
