using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Controls;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.Theme.Tests;

[TestClass]
public class TabViewThemeTests
{
    private static readonly string[] NewThemeKeys =
    {
        "TabViewButtonBackgroundActiveTab",
        "TabViewButtonForegroundActiveTab",
        "TabViewItemBorderBrush",
        "TabViewItemHeaderDragBackground",
        "TabViewButtonBorderThickness",
        "TabViewItemHeaderCloseButtonBorderThickness"
    };

    [TestMethod]
    public void Preview5TabViewThemeKeysExistInLightDarkAndHighContrast()
    {
        WpfTestHost.Run(() =>
        {
            foreach (var themeName in new[] { "Light", "Dark", "HighContrast" })
            {
                var dictionary = ThemeResources.Current.GetThemeDictionary(themeName);
                foreach (var key in NewThemeKeys)
                {
                    Assert.IsTrue(dictionary.Contains(key), $"{themeName} is missing {key}.");
                    Assert.IsNotNull(dictionary[key], $"{themeName}:{key} resolved to null.");
                }

                Assert.IsInstanceOfType(dictionary["TabViewButtonBackgroundActiveTab"], typeof(Brush));
                Assert.IsInstanceOfType(dictionary["TabViewButtonForegroundActiveTab"], typeof(Brush));
                Assert.IsInstanceOfType(dictionary["TabViewItemBorderBrush"], typeof(Brush));
                Assert.IsInstanceOfType(dictionary["TabViewItemHeaderDragBackground"], typeof(Brush));
                Assert.AreEqual(
                    themeName == "HighContrast" ? new Thickness(1) : new Thickness(0),
                    dictionary["TabViewButtonBorderThickness"]);
                Assert.AreEqual(
                    themeName == "HighContrast" ? new Thickness(1) : new Thickness(0),
                    dictionary["TabViewItemHeaderCloseButtonBorderThickness"]);
            }
        });
    }

    [TestMethod]
    public void CompactResourceEntryKeepsTabViewTemplateInteractive()
    {
        WpfTestHost.Run(() =>
        {
            ThemeTestApplication.EnsureInitialized();
            var resources = new FluentControlsResources { UseCompactResources = true };
            var root = new Grid();
            root.Resources.MergedDictionaries.Add(new ThemeResources());
            root.Resources.MergedDictionaries.Add(resources);

            var tabView = new ModernWpf.Controls.TabView();
            tabView.TabItems.Add(new TabViewItem { Header = "First", Content = "First content" });
            tabView.TabItems.Add(new TabViewItem { Header = "Second", Content = "Second content" });
            root.Children.Add(tabView);
            var addRequests = 0;
            tabView.AddTabButtonClick += (_, _) => addRequests++;

            using var host = new TestWindowHost(root, width: 520, height: 260);
            host.UpdateLayout();

            var addButton = (ButtonBase?)tabView.Template.FindName("PART_AddButton", tabView);
            var scrollViewer = (ScrollViewer?)tabView.Template.FindName("PART_ScrollViewer", tabView);
            Assert.IsNotNull(addButton);
            Assert.IsNotNull(scrollViewer);
            Assert.IsTrue(addButton.IsEnabled);
            Assert.IsTrue(tabView.ActualWidth > 0.0);
            Assert.IsTrue(tabView.ActualHeight > 0.0);
            Assert.AreEqual(32.0, (double)tabView.FindResource("TabViewItemMinHeight"));
            Assert.AreEqual(new Thickness(8, 3, 4, 3), tabView.FindResource("TabViewItemHeaderPadding"));

            addButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            Assert.AreEqual(1, addRequests);
        });
    }
}
