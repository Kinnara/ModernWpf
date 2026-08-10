using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Controls;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.Theme.Tests;

[TestClass]
public class ItemsViewThemeTests
{
    [TestMethod]
    public void Preview7ItemsViewUsesSharedThemeResourcesInLightDarkAndHighContrast()
    {
        WpfTestHost.Run(() =>
        {
            foreach (string themeName in new[] { "Light", "Dark", "HighContrast" })
            {
                ResourceDictionary dictionary = ThemeResources.Current.GetThemeDictionary(themeName);
                foreach (string key in new[]
                {
                    "ItemContainerBackground",
                    "ItemContainerSelectedBackground",
                    "ItemContainerSelectionVisualBackground",
                    "ItemContainerCheckboxBackgroundUnchecked"
                })
                {
                    Assert.IsTrue(dictionary.Contains(key), $"{themeName} is missing {key}.");
                    Assert.IsInstanceOfType<Brush>(dictionary[key], $"{themeName}:{key}");
                }
            }
        });
    }

    [TestMethod]
    public void StandardAndCompactEntriesKeepItemsViewTemplateSelectionAndScrolling()
    {
        WpfTestHost.Run(() =>
        {
            ThemeTestApplication.EnsureInitialized();

            foreach (bool useCompactResources in new[] { false, true })
            {
                var root = new Grid();
                root.Resources.MergedDictionaries.Add(new ThemeResources());
                root.Resources.MergedDictionaries.Add(new FluentControlsResources
                {
                    UseCompactResources = useCompactResources
                });

                var itemsView = new ItemsView
                {
                    Width = 320,
                    Height = 180,
                    ItemsSource = new[] { "Alpha", "Beta", "Gamma", "Delta" },
                    SelectionMode = ItemsViewSelectionMode.Multiple
                };
                root.Children.Add(itemsView);

                using var host = new TestWindowHost(root, width: 380, height: 240);
                host.UpdateLayout();

                var repeater = (ItemsRepeater?)itemsView.Template.FindName("PART_ItemsRepeater", itemsView);
                Assert.IsNotNull(repeater);
                var itemContainer = (ItemContainer)repeater.GetOrCreateElement(1);
                itemsView.Select(1);

                Assert.IsNotNull(itemsView.ScrollView);
                Assert.IsInstanceOfType<StackLayout>(itemsView.Layout);
                Assert.IsTrue(itemContainer.IsSelected);
                Assert.IsInstanceOfType<Brush>(itemContainer.FindResource("ItemContainerSelectedBackground"));

                ThemeManager.SetRequestedTheme(root, ElementTheme.Dark);
                WpfTestHost.DoEvents();
                host.UpdateLayout();
                Assert.IsInstanceOfType<Brush>(itemContainer.FindResource("ItemContainerSelectedBackground"));

                ThemeManager.SetRequestedTheme(root, ElementTheme.Light);
                WpfTestHost.DoEvents();
                host.UpdateLayout();
                Assert.IsInstanceOfType<Brush>(itemContainer.FindResource("ItemContainerSelectionVisualBackground"));
            }
        });
    }
}
