using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Controls;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.Theme.Tests;

[TestClass]
public class ItemContainerThemeTests
{
    private static readonly string[] ThemeBrushKeys =
    {
        "ItemContainerBackground",
        "ItemContainerPointerOverBackground",
        "ItemContainerPressedBackground",
        "ItemContainerBorderBrush",
        "ItemContainerPointerOverBorderBrush",
        "ItemContainerPressedBorderBrush",
        "ItemContainerSelectedBackground",
        "ItemContainerSelectedPointerOverBackground",
        "ItemContainerSelectedPressedBackground",
        "ItemContainerSelectionVisualBackground",
        "ItemContainerSelectionVisualPointerOverBackground",
        "ItemContainerSelectionVisualPressedBackground",
        "ItemContainerSelectedInnerBorderBrush",
        "ItemContainerCheckboxBackgroundUnchecked"
    };

    [TestMethod]
    public void Preview6ItemContainerThemeKeysExistInLightDarkAndHighContrast()
    {
        WpfTestHost.Run(() =>
        {
            foreach (string themeName in new[] { "Light", "Dark", "HighContrast" })
            {
                ResourceDictionary dictionary = ThemeResources.Current.GetThemeDictionary(themeName);
                foreach (string key in ThemeBrushKeys)
                {
                    Assert.IsTrue(dictionary.Contains(key), $"{themeName} is missing {key}.");
                    Assert.IsInstanceOfType<Brush>(dictionary[key], $"{themeName}:{key}");
                }
            }
        });
    }

    [TestMethod]
    public void CompactResourceEntryKeepsItemContainerTemplateAndSharedKeys()
    {
        WpfTestHost.Run(() =>
        {
            ThemeTestApplication.EnsureInitialized();
            var root = new Grid();
            root.Resources.MergedDictionaries.Add(new ThemeResources());
            root.Resources.MergedDictionaries.Add(new FluentControlsResources { UseCompactResources = true });

            var itemContainer = new ItemContainer
            {
                IsSelected = true,
                Child = new TextBlock { Text = "Preview 6 item" }
            };
            root.Children.Add(itemContainer);

            using var host = new TestWindowHost(root, width: 360, height: 220);
            host.UpdateLayout();

            var checkBox = (CheckBox?)itemContainer.Template.FindName(
                "PART_SelectionCheckbox",
                itemContainer);
            Assert.IsNotNull(checkBox);
            Assert.AreEqual(Visibility.Collapsed, checkBox.Visibility);
            Assert.AreEqual(true, checkBox.IsChecked);
            Assert.AreEqual(0.3, itemContainer.FindResource("ItemContainerDisabledOpacity"));
            Assert.AreEqual(0.0, itemContainer.FindResource("ItemContainerCheckboxMinWidth"));
            Assert.AreEqual(new Thickness(4, -2, 4, -2), itemContainer.FindResource("ItemContainerCheckboxMargin"));
            Assert.AreEqual(new Thickness(2), itemContainer.FindResource("ItemContainerSelectedInnerMargin"));
            Assert.AreEqual(new Thickness(1), itemContainer.FindResource("ItemContainerSelectedInnerThickness"));
            Assert.AreEqual(
                HorizontalAlignment.Right,
                itemContainer.FindResource("ItemContainerCheckboxHorizontalAlignment"));
            Assert.AreEqual(
                VerticalAlignment.Top,
                itemContainer.FindResource("ItemContainerCheckboxVerticalAlignment"));
        });
    }
}
