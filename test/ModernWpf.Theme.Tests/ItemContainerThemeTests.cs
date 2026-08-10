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
    [TestMethod]
    public void Preview6ItemContainerTemplateResolvesForLightAndDark()
    {
        WpfTestHost.Run(() =>
        {
            ThemeTestApplication.EnsureInitialized();
            var itemContainer = new ItemContainer
            {
                Child = new TextBlock { Text = "Preview 6 item" }
            };

            using var host = new TestWindowHost(itemContainer, width: 360, height: 220);
            Color? lightBorderColor = null;
            foreach (var theme in new[] { ElementTheme.Light, ElementTheme.Dark })
            {
                ThemeManager.SetRequestedTheme(itemContainer, theme);
                itemContainer.IsSelected = true;
                WpfTestHost.DoEvents();
                host.UpdateLayout();

                Assert.AreEqual(theme, ThemeManager.GetActualTheme(itemContainer));
                var containerRoot = (ModernWpf.Controls.GridEx?)itemContainer.Template.FindName(
                    "PART_ContainerRoot",
                    itemContainer);
                var commonVisual = (ModernWpf.Controls.GridEx?)itemContainer.Template.FindName(
                    "PART_CommonVisual",
                    itemContainer);
                var selectionVisual = (ModernWpf.Controls.GridEx?)itemContainer.Template.FindName(
                    "PART_SelectionVisual",
                    itemContainer);
                Assert.IsNotNull(containerRoot);
                Assert.IsNotNull(commonVisual);
                Assert.IsNotNull(selectionVisual);
                Assert.IsNotNull(commonVisual.Background);
                Assert.IsNotNull(commonVisual.BorderBrush);
                Assert.IsNotNull(selectionVisual.BorderBrush);

                if (commonVisual.BorderBrush is SolidColorBrush borderBrush)
                {
                    if (theme == ElementTheme.Light)
                    {
                        lightBorderColor = borderBrush.Color;
                    }
                    else if (lightBorderColor.HasValue)
                    {
                        Assert.AreNotEqual(lightBorderColor.Value, borderBrush.Color);
                    }
                }
            }
        });
    }

    [TestMethod]
    public void HighContrastDictionarySuppliesItemContainerSemanticBrushes()
    {
        WpfTestHost.Run(() =>
        {
            var dictionary = ThemeResources.Current.GetThemeDictionary(ThemeManager.HighContrastKey);
            foreach (var key in new[]
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
            })
            {
                Assert.IsTrue(dictionary.Contains(key), $"HighContrast is missing {key}.");
                Assert.IsInstanceOfType<Brush>(dictionary[key], $"HighContrast:{key}");
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
            Assert.AreEqual(true, checkBox.IsChecked);
            Assert.IsFalse(checkBox.IsHitTestVisible);
            Assert.AreEqual(0.0, checkBox.MinWidth);
            Assert.AreEqual(new Thickness(4, -2, 4, -2), checkBox.Margin);
            Assert.AreEqual(HorizontalAlignment.Right, checkBox.HorizontalAlignment);
            Assert.AreEqual(VerticalAlignment.Top, checkBox.VerticalAlignment);

            itemContainer.IsEnabled = false;
            WpfTestHost.DoEvents();
            var containerRoot = (ModernWpf.Controls.GridEx?)itemContainer.Template.FindName(
                "PART_ContainerRoot",
                itemContainer);
            Assert.IsNotNull(containerRoot);
            Assert.AreEqual(0.3, containerRoot.Opacity);
        });
    }
}
