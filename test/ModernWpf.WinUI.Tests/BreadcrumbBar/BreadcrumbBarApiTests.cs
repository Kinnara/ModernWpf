using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf;
using ModernWpf.Controls;
using ModernWpf.Controls.Primitives;
using ModernWpf.Media.Animation;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.BreadcrumbBar;

[TestClass]
public class BreadcrumbBarApiTests
{
    [TestMethod]
    public void VerifyBreadcrumbDefaultAPIValues()
    {
        WpfTestHost.Run(() =>
        {
            var breadcrumb = new ModernWpf.Controls.BreadcrumbBar();

            Assert.IsNull(breadcrumb.ItemsSource);
            Assert.IsNull(breadcrumb.ItemTemplate);
        });
    }

    [TestMethod]
    public void VerifyDefaultBreadcrumb()
    {
        WpfTestHost.Run(() =>
        {
            var breadcrumb = new ModernWpf.Controls.BreadcrumbBar();

            using var host = new TestWindowHost(breadcrumb, width: 300, height: 80);

            Assert.AreEqual(0, breadcrumb.Containers.Count);
        });
    }

    [TestMethod]
    public void BreadcrumbBarItemAcceptsWinUIContentPresenterSurface()
    {
        WpfTestHost.Run(() =>
        {
            var transitions = new TransitionCollection();
            var item = new ModernWpf.Controls.BreadcrumbBarItem
            {
                ContentTransitions = transitions,
                CornerRadius = new CornerRadius(4)
            };

            Assert.AreSame(transitions, item.ContentTransitions);
            Assert.AreEqual(new CornerRadius(4), item.CornerRadius);
        });
    }

    [TestMethod]
    public void BreadcrumbBarItemTemplateUsesWinUIContentPresenter()
    {
        WpfTestHost.Run(() =>
        {
            var content = new Border { Width = 80, Height = 24 };
            var transitions = new TransitionCollection();
            var foreground = new SolidColorBrush(Colors.Blue);
            var item = new ModernWpf.Controls.BreadcrumbBarItem
            {
                Content = content,
                ContentTransitions = transitions,
                CornerRadius = new CornerRadius(5),
                Foreground = foreground,
                HorizontalContentAlignment = HorizontalAlignment.Right,
                VerticalContentAlignment = VerticalAlignment.Bottom
            };

            using var host = new TestWindowHost(item, width: 240, height: 80);

            var button = VisualTreeTestHelper
                .EnumerateDescendants(item)
                .OfType<Button>()
                .FirstOrDefault()
                ?? throw new AssertFailedException("Expected BreadcrumbBarItem template to contain an item button.");
            var presenter = FindTemplatePart<ContentPresenterEx>(item, "PART_ItemContentPresenter");

            Assert.AreSame(transitions, ControlHelper.GetContentTransitions(button));
            Assert.AreEqual(new CornerRadius(5), ((CornerRadius)button.GetValue(System.Windows.Controls.Border.CornerRadiusProperty)));
            Assert.AreSame(transitions, presenter.ContentTransitions);
            Assert.AreEqual(HorizontalAlignment.Right, presenter.HorizontalContentAlignment);
            Assert.AreEqual(VerticalAlignment.Bottom, presenter.VerticalContentAlignment);
        });
    }

    [TestMethod]
    public void BreadcrumbBarItemTemplateUsesVisualStateSettersForWinUIStateParity()
    {
        WpfTestHost.Run(() =>
        {
            var item = new ModernWpf.Controls.BreadcrumbBarItem
            {
                Content = "Node"
            };

            using var host = new TestWindowHost(item, width: 240, height: 80);
            host.UpdateLayout();

            var root = FindTemplatePart<FrameworkElement>(item, "PART_LayoutRoot");
            var itemButton = FindTemplatePart<Button>(item, "PART_ItemButton");
            var itemButtonRoot = VisualTreeTestHelper
                .EnumerateDescendants(itemButton)
                .OfType<FrameworkElement>()
                .FirstOrDefault(element => FindVisualStateGroup(element, "CommonStates") != null)
                ?? throw new AssertFailedException("Expected BreadcrumbBarItem button template to contain CommonStates.");
            AssertStateSetter(root, "ItemTypeStates", "EllipsisDropDown", "PART_ItemButton.Visibility");
            AssertStateSetter(root, "ItemTypeStates", "EllipsisDropDown", "PART_EllipsisDropDownItemContentPresenter.Visibility");
            AssertStateSetter(root, "ItemTypeStates", "EllipsisDropDown", "PART_LayoutRoot.(FocusVisualHelper.FocusVisualMargin)");
            AssertStateSetter(root, "ItemTypeStates", "EllipsisDropDown", "PART_ItemButton.(FocusVisualHelper.IsTemplateFocusTarget)");
            AssertStateSetter(root, "ItemTypeStates", "EllipsisDropDown", "PART_LayoutRoot.(FocusVisualHelper.IsTemplateFocusTarget)");
            AssertStateSetter(root, "InlineItemTypeStates", "Default", "PART_ChevronTextBlock.Text");
            AssertStateSetter(root, "InlineItemTypeStates", "DefaultRTL", "PART_ChevronTextBlock.Text");
            AssertStateSetter(root, "InlineItemTypeStates", "LastItem", "PART_ItemButton.Visibility");
            AssertStateSetter(root, "InlineItemTypeStates", "LastItem", "PART_ChevronTextBlock.Visibility");
            AssertStateSetter(root, "InlineItemTypeStates", "LastItem", "PART_LastItemContentPresenter.Visibility");
            AssertStateSetter(root, "InlineItemTypeStates", "LastItem", "PART_ItemButton.(FocusVisualHelper.IsTemplateFocusTarget)");
            AssertStateSetter(root, "InlineItemTypeStates", "LastItem", "PART_LastItemContentPresenter.(FocusVisualHelper.IsTemplateFocusTarget)");
            AssertStateSetter(root, "InlineItemTypeStates", "Ellipsis", "PART_EllipsisTextBlock.Visibility");
            AssertStateSetter(root, "InlineItemTypeStates", "EllipsisRTL", "PART_ChevronTextBlock.Text");

            AssertStateSetter(itemButtonRoot, "CommonStates", "CurrentNormal", "PART_ContentPresenter.Foreground");
            AssertStateSetter(itemButtonRoot, "CommonStates", "PointerOver", "PART_ContentPresenter.Foreground");
            AssertStateSetter(itemButtonRoot, "CommonStates", "PointerOver", "PART_ContentPresenter.Background");
            AssertStateSetter(itemButtonRoot, "CommonStates", "PointerOver", "PART_ContentPresenter.BorderBrush");
            AssertStateSetter(itemButtonRoot, "CommonStates", "Pressed", "PART_ContentPresenter.Foreground");
            AssertStateSetter(itemButtonRoot, "CommonStates", "Pressed", "PART_ContentPresenter.Background");
            AssertStateSetter(itemButtonRoot, "CommonStates", "Pressed", "PART_ContentPresenter.BorderBrush");
            AssertStateSetter(itemButtonRoot, "CommonStates", "Disabled", "PART_ContentPresenter.Foreground");
            AssertStateSetter(itemButtonRoot, "CommonStates", "Focus", "PART_ContentPresenter.Foreground");

            Assert.IsTrue(VisualStateManager.GoToState(itemButton, "Pressed", false));

            item.IsCurrentItem = true;
            host.UpdateLayout();

            Assert.AreEqual(Visibility.Collapsed, FindTemplatePart<Button>(item, "PART_ItemButton").Visibility);
            Assert.AreEqual(Visibility.Collapsed, FindTemplatePart<TextBlock>(item, "PART_ChevronTextBlock").Visibility);
            var lastItemPresenter = FindTemplatePart<ContentPresenterEx>(item, "PART_LastItemContentPresenter");
            Assert.AreEqual(Visibility.Visible, lastItemPresenter.Visibility);
            Assert.AreEqual(FontWeights.Normal, lastItemPresenter.FontWeight);
        });
    }

    [TestMethod]
    public void BreadcrumbBarItemFocusTargetsFollowWinUISourceStates()
    {
        WpfTestHost.Run(() =>
        {
            var item = new ModernWpf.Controls.BreadcrumbBarItem
            {
                Content = "Node"
            };

            using var host = new TestWindowHost(item, width: 240, height: 80);
            host.UpdateLayout();

            var root = FindTemplatePart<FrameworkElement>(item, "PART_LayoutRoot");
            var itemButton = FindTemplatePart<Button>(item, "PART_ItemButton");
            var lastItemPresenter = FindTemplatePart<ContentPresenterEx>(item, "PART_LastItemContentPresenter");

            Assert.IsTrue(item.Focusable);
            Assert.AreEqual(new Thickness(-3), FocusVisualHelper.GetFocusVisualMargin(itemButton));
            Assert.IsTrue(FocusVisualHelper.GetIsTemplateFocusTarget(itemButton));

            item.IsCurrentItem = true;
            host.UpdateLayout();

            Assert.IsFalse(FocusVisualHelper.GetIsTemplateFocusTarget(itemButton));
            Assert.IsTrue(FocusVisualHelper.GetIsTemplateFocusTarget(lastItemPresenter));
            Assert.AreEqual(new Thickness(-3), FocusVisualHelper.GetFocusVisualMargin(lastItemPresenter));

            item.IsCurrentItem = false;
            item.SetIsEllipsisDropDownItem(true);
            host.UpdateLayout();

            Assert.IsFalse(FocusVisualHelper.GetIsTemplateFocusTarget(itemButton));
            Assert.IsTrue(FocusVisualHelper.GetIsTemplateFocusTarget(root));
            Assert.AreEqual(new Thickness(-3), FocusVisualHelper.GetFocusVisualMargin(root));
        });
    }

    [TestMethod]
    public void BreadcrumbBarItemStyleUsesWinUIResourceAliases()
    {
        WpfTestHost.Run(() =>
        {
            var resources = new ResourceDictionary
            {
                Source = new Uri("/ModernWpf.Controls;component/BreadcrumbBar/BreadcrumbBar.xaml", UriKind.Relative)
            };
            var itemStyle = (Style)resources["DefaultBreadcrumbBarItemStyle"];
            var item = new ModernWpf.Controls.BreadcrumbBarItem
            {
                Content = "Node",
                Style = itemStyle
            };
            item.Resources.MergedDictionaries.Add(resources);

            using var host = new TestWindowHost(item, width: 240, height: 80);
            host.UpdateLayout();

            Assert.AreEqual(typeof(ModernWpf.Controls.BreadcrumbBarItem), itemStyle.TargetType);
            AssertDynamicResourceSetter(itemStyle, Control.BackgroundProperty, "BreadcrumbBarBackgroundBrush");
            AssertDynamicResourceSetter(itemStyle, Control.BorderBrushProperty, "BreadcrumbBarBorderBrush");
            AssertSetterValue(itemStyle, ModernWpf.Controls.BreadcrumbBarItem.FocusVisualMarginProperty, new Thickness(1));
            AssertDynamicResourceSetter(itemStyle, Control.FontFamilyProperty, "ContentControlThemeFontFamily");
            AssertDynamicResourceSetter(itemStyle, Control.FontSizeProperty, "BreadcrumbBarItemThemeFontSize");
            AssertSetterValue(itemStyle, Control.FontWeightProperty, item.TryFindResource("BreadcrumbBarItemFontWeight"));
            AssertSetterValue(itemStyle, UIElement.FocusableProperty, true);
            AssertDynamicResourceSetter(itemStyle, Control.ForegroundProperty, "BreadcrumbBarForegroundBrush");
            AssertSetterValue(itemStyle, FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Stretch);
            AssertSetterValue(itemStyle, Control.HorizontalContentAlignmentProperty, HorizontalAlignment.Stretch);
            AssertSetterValue(itemStyle, Control.IsTabStopProperty, true);
            AssertDynamicResourceSetter(itemStyle, BreadcrumbBarItem.UseSystemFocusVisualsProperty, "UseSystemFocusVisuals");
            AssertSetterValue(itemStyle, FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);
            AssertSetterValue(itemStyle, Control.VerticalContentAlignmentProperty, VerticalAlignment.Center);
            AssertDynamicResourceSetter(itemStyle, BreadcrumbBarItem.CornerRadiusProperty, "ControlCornerRadius");
            Assert.IsInstanceOfType(GetSetterValue(itemStyle, Control.TemplateProperty), typeof(ControlTemplate));

            Assert.AreSame(item.TryFindResource("BreadcrumbBarBackgroundBrush"), item.Background);
            Assert.AreSame(item.TryFindResource("BreadcrumbBarBorderBrush"), item.BorderBrush);
            Assert.AreSame(item.TryFindResource("ContentControlThemeFontFamily"), item.FontFamily);
            Assert.AreEqual(item.TryFindResource("BreadcrumbBarItemThemeFontSize"), item.FontSize);
            Assert.AreEqual(item.TryFindResource("BreadcrumbBarItemFontWeight"), item.FontWeight);
            Assert.AreSame(item.TryFindResource("BreadcrumbBarForegroundBrush"), item.Foreground);
            Assert.AreEqual(item.TryFindResource("UseSystemFocusVisuals"), item.UseSystemFocusVisuals);
            Assert.AreEqual(item.TryFindResource("ControlCornerRadius"), item.CornerRadius);
            Assert.AreEqual(new Thickness(1), item.FocusVisualMargin);

            AssertResourceAlias(item, "BreadcrumbBarNormalForegroundBrush", "TextFillColorPrimaryBrush");
            AssertResourceAlias(item, "BreadcrumbBarHoverForegroundBrush", "TextFillColorSecondaryBrush");
            AssertResourceAlias(item, "BreadcrumbBarPressedForegroundBrush", "TextFillColorTertiaryBrush");
            AssertResourceAlias(item, "BreadcrumbBarDisabledForegroundBrush", "TextFillColorDisabledBrush");
            AssertResourceAlias(item, "BreadcrumbBarFocusForegroundBrush", "TextFillColorPrimaryBrush");
            AssertResourceAlias(item, "BreadcrumbBarCurrentNormalForegroundBrush", "TextFillColorPrimaryBrush");
            AssertResourceAlias(item, "BreadcrumbBarCurrentHoverForegroundBrush", "TextFillColorSecondaryBrush");
            AssertResourceAlias(item, "BreadcrumbBarCurrentPressedForegroundBrush", "TextFillColorTertiaryBrush");
            AssertResourceAlias(item, "BreadcrumbBarCurrentDisabledForegroundBrush", "TextFillColorDisabledBrush");
            AssertResourceAlias(item, "BreadcrumbBarCurrentFocusForegroundBrush", "TextFillColorPrimaryBrush");
            AssertResourceAlias(item, "BreadcrumbBarEllipsisDropDownItemBackground", "SubtleFillColorTransparentBrush");
            AssertResourceAlias(item, "BreadcrumbBarEllipsisDropDownItemBackgroundPointerOver", "SubtleFillColorSecondaryBrush");
            AssertResourceAlias(item, "BreadcrumbBarEllipsisDropDownItemBackgroundPressed", "SubtleFillColorTertiaryBrush");
            AssertResourceAlias(item, "BreadcrumbBarEllipsisDropDownItemBackgroundDisabled", "SubtleFillColorTransparentBrush");
            AssertResourceAlias(item, "BreadcrumbBarEllipsisDropDownItemForegroundPointerOver", "TextFillColorPrimaryBrush");
            AssertResourceAlias(item, "BreadcrumbBarEllipsisDropDownItemForegroundPressed", "TextFillColorPrimaryBrush");
            AssertResourceAlias(item, "BreadcrumbBarEllipsisDropDownItemForegroundDisabled", "TextFillColorDisabledBrush");
            AssertResourceAlias(item, "BreadcrumbBarForegroundBrush", "TextFillColorPrimaryBrush");
            AssertResourceAlias(item, "BreadcrumbBarEllipsisFlyoutPresenterBackground", "AcrylicBackgroundFillColorDefaultBrush");
            AssertResourceAlias(item, "BreadcrumbBarEllipsisFlyoutPresenterBorderBrush", "SurfaceStrokeColorFlyoutBrush");
            AssertResourceAlias(item, "BreadcrumbBarItemThemeFontSize", "ControlContentThemeFontSize");
            AssertSolidColorBrush(item.TryFindResource("BreadcrumbBarBackgroundBrush"), Colors.Transparent);
            AssertSolidColorBrush(item.TryFindResource("BreadcrumbBarBorderBrush"), Colors.Transparent);
            Assert.AreEqual(new Thickness(1), item.TryFindResource("BreadcrumbBarEllipsisFlyoutPresenterBorderThemeThickness"));
            Assert.AreEqual(12.0, item.TryFindResource("BreadcrumbBarChevronFontSize"));
            Assert.AreEqual(new Thickness(2, 0, 2, 0), item.TryFindResource("BreadcrumbBarChevronPadding"));
            Assert.AreEqual("\uE974", item.TryFindResource("BreadcrumbBarChevronLeftToRight"));
            Assert.AreEqual("\uE973", item.TryFindResource("BreadcrumbBarChevronRightToLeft"));

            var itemButton = FindTemplatePart<Button>(item, "PART_ItemButton");
            AssertDynamicResourceSetter(itemButton.Style!, Control.ForegroundProperty, "BreadcrumbBarNormalForegroundBrush");
            AssertDynamicResourceSetter(itemButton.Style!, Control.BackgroundProperty, "BreadcrumbBarBackgroundBrush");
            AssertDynamicResourceSetter(itemButton.Style!, Control.BorderBrushProperty, "BreadcrumbBarBorderBrush");
            AssertDynamicResourceSetter(itemButton.Style!, Control.FontFamilyProperty, "ContentControlThemeFontFamily");
            AssertDynamicResourceSetter(itemButton.Style!, Control.FontSizeProperty, "BreadcrumbBarItemThemeFontSize");

            var lastItemPresenter = FindTemplatePart<ContentPresenterEx>(item, "PART_LastItemContentPresenter");
            Assert.AreSame(item.TryFindResource("BreadcrumbBarCurrentNormalForegroundBrush"), lastItemPresenter.Foreground);

            var chevronTextBlock = FindTemplatePart<TextBlock>(item, "PART_ChevronTextBlock");
            Assert.AreEqual(item.TryFindResource("BreadcrumbBarChevronFontSize"), chevronTextBlock.FontSize);
            Assert.AreSame(item.TryFindResource("BreadcrumbBarNormalForegroundBrush"), chevronTextBlock.Foreground);
            Assert.AreEqual(item.TryFindResource("BreadcrumbBarChevronPadding"), chevronTextBlock.Padding);

            var layoutRoot = FindTemplatePart<FrameworkElement>(item, "PART_LayoutRoot");
            var flyout = layoutRoot.Resources["PART_EllipsisFlyout"] as Flyout
                ?? throw new AssertFailedException("Expected BreadcrumbBarItem template to expose the ellipsis flyout.");
            var flyoutPresenterStyle = flyout.FlyoutPresenterStyle;
            Assert.AreEqual(typeof(FlyoutPresenter), flyoutPresenterStyle.TargetType);
            AssertDynamicResourceSetter(flyoutPresenterStyle, Control.BackgroundProperty, "BreadcrumbBarEllipsisFlyoutPresenterBackground");
            AssertDynamicResourceSetter(flyoutPresenterStyle, Control.BorderBrushProperty, "BreadcrumbBarEllipsisFlyoutPresenterBorderBrush");
            AssertSetterValue(flyoutPresenterStyle, Control.BorderThicknessProperty, item.TryFindResource("BreadcrumbBarEllipsisFlyoutPresenterBorderThemeThickness"));
            AssertSetterValue(flyoutPresenterStyle, Control.PaddingProperty, new Thickness(0, 2, 0, 2));
            AssertDynamicResourceSetter(flyoutPresenterStyle, FrameworkElement.MaxWidthProperty, "FlyoutThemeMaxWidth");
            AssertSetterValue(flyoutPresenterStyle, FrameworkElement.MinHeightProperty, 40.0);
            AssertDynamicResourceSetter(flyoutPresenterStyle, System.Windows.Controls.Border.CornerRadiusProperty, "OverlayCornerRadius");
            Assert.IsInstanceOfType(GetSetterValue(flyoutPresenterStyle, Control.TemplateProperty), typeof(ControlTemplate));
        });
    }

    [TestMethod]
    public void BreadcrumbBarTemplateUsesWinUIItemsRepeater()
    {
        WpfTestHost.Run(() =>
        {
            var resources = new ResourceDictionary
            {
                Source = new Uri("/ModernWpf.Controls;component/BreadcrumbBar/BreadcrumbBar.xaml", UriKind.Relative)
            };
            var breadcrumbStyle = (Style)resources[typeof(ModernWpf.Controls.BreadcrumbBar)];
            var breadcrumb = new ModernWpf.Controls.BreadcrumbBar
            {
                ItemsSource = new[] { "Root", "Node A", "Node B" },
                Style = breadcrumbStyle
            };
            breadcrumb.Resources.MergedDictionaries.Add(resources);

            using var host = new TestWindowHost(breadcrumb, width: 300, height: 80);

            Assert.AreEqual(typeof(ModernWpf.Controls.BreadcrumbBar), breadcrumbStyle.TargetType);
            AssertSetterValue(breadcrumbStyle, Control.IsTabStopProperty, false);
            Assert.IsInstanceOfType(GetSetterValue(breadcrumbStyle, Control.TemplateProperty), typeof(ControlTemplate));

            var repeater = FindTemplatePart<ItemsRepeater>(breadcrumb, "PART_ItemsRepeater");

            Assert.IsNotNull(repeater);
            Assert.IsInstanceOfType(repeater.Layout, typeof(NonVirtualizingLayout));
            Assert.IsNull(breadcrumb.Template?.FindName("PART_RootPanel", breadcrumb));
        });
    }

    [TestMethod]
    public void VerifyItemsSourceCreatesBreadcrumbBarItems()
    {
        WpfTestHost.Run(() =>
        {
            var breadcrumb = new ModernWpf.Controls.BreadcrumbBar
            {
                ItemsSource = new[] { "Root", "Node A", "Node B" }
            };

            using var host = new TestWindowHost(breadcrumb, width: 300, height: 80);

            Assert.AreEqual(3, breadcrumb.Containers.Count);
            Assert.AreEqual("Root", breadcrumb.ContainerFromIndex(0).Content);
            Assert.IsFalse(breadcrumb.ContainerFromIndex(0).IsCurrentItem);
            Assert.IsTrue(breadcrumb.ContainerFromIndex(2).IsCurrentItem);
            Assert.AreEqual(1, breadcrumb.ContainerFromIndex(0).GetValue(AutomationProperties.PositionInSetProperty));
            Assert.AreEqual(3, breadcrumb.ContainerFromIndex(0).GetValue(AutomationProperties.SizeOfSetProperty));
        });
    }

    [TestMethod]
    public void BreadcrumbBarRendersVisiblePixels()
    {
        WpfTestHost.Run(() =>
        {
            var breadcrumb = new ModernWpf.Controls.BreadcrumbBar
            {
                ItemsSource = new[] { "Root", "Node A", "Node B" }
            };

            using var host = new TestWindowHost(breadcrumb, width: 300, height: 80);
            host.UpdateLayout();

            Assert.IsTrue(breadcrumb.ActualWidth > 0, "Expected BreadcrumbBar to have a rendered width.");
            Assert.IsTrue(breadcrumb.ActualHeight > 0, "Expected BreadcrumbBar to have a rendered height.");
            var renderedPixels = CountRenderedPixels(breadcrumb);
            Assert.IsTrue(
                renderedPixels > 100,
                $"Expected BreadcrumbBar text and chevrons to render visible pixels, but only found {renderedPixels} rendered pixels.");
        });
    }

    [TestMethod]
    public void VerifyConstrainedWidthUsesWinUIEllipsisElement()
    {
        WpfTestHost.Run(() =>
        {
            var breadcrumb = new ModernWpf.Controls.BreadcrumbBar
            {
                ItemsSource = new[]
                {
                    "Very long root node",
                    "Very long child node",
                    "Current node"
                }
            };

            using var host = new TestWindowHost(breadcrumb, width: 110, height: 80);
            host.UpdateLayout();

            var repeater = FindTemplatePart<ItemsRepeater>(breadcrumb, "PART_ItemsRepeater");
            var ellipsis = repeater.TryGetElement(0) as BreadcrumbBarItem;
            var hiddenElements = breadcrumb.HiddenElements();

            Assert.IsNotNull(ellipsis);
            Assert.IsTrue(hiddenElements.Count > 0);
            Assert.AreEqual("Very long root node", hiddenElements[0]);
            Assert.AreEqual(3, breadcrumb.Containers.Count);
        });
    }

    [TestMethod]
    public void VerifyCustomItemTemplate()
    {
        WpfTestHost.Run(() =>
        {
            var breadcrumb = new ModernWpf.Controls.BreadcrumbBar
            {
                ItemsSource = new[]
                {
                    new MockNode { Name = "Root" },
                    new MockNode { Name = "Node A" }
                },
                ItemTemplate = CreateNameTemplate()
            };

            using var host = new TestWindowHost(breadcrumb, width: 300, height: 80);

            var textBlocks = VisualTreeTestHelper
                .EnumerateDescendants(breadcrumb)
                .OfType<TextBlock>()
                .Where(textBlock => textBlock.Text == "Root" || textBlock.Text == "Node A")
                .ToList();

            Assert.AreEqual(2, textBlocks.Count);
        });
    }

    [TestMethod]
    public void VerifyItemClickedEventArgs()
    {
        WpfTestHost.Run(() =>
        {
            var breadcrumb = new ModernWpf.Controls.BreadcrumbBar
            {
                ItemsSource = new[] { "Root", "Node A", "Node B" }
            };

            object? clickedItem = null;
            var clickedIndex = -1;
            breadcrumb.ItemClicked += (sender, args) =>
            {
                clickedItem = args.Item;
                clickedIndex = args.Index;
            };

            using var host = new TestWindowHost(breadcrumb, width: 300, height: 80);

            RaiseItemButtonClick(breadcrumb.ContainerFromIndex(1));

            Assert.AreEqual("Node A", clickedItem);
            Assert.AreEqual(1, clickedIndex);

            clickedItem = null;
            clickedIndex = -1;

            RaiseItemButtonClick(breadcrumb.ContainerFromIndex(2));

            Assert.IsNull(clickedItem);
            Assert.AreEqual(-1, clickedIndex);
        });
    }

    [TestMethod]
    public void VerifyAutomationInvokePattern()
    {
        WpfTestHost.Run(() =>
        {
            var breadcrumb = new ModernWpf.Controls.BreadcrumbBar
            {
                ItemsSource = new[] { "Root", "Node A" }
            };

            object? clickedItem = null;
            breadcrumb.ItemClicked += (sender, args) => clickedItem = args.Item;

            using var host = new TestWindowHost(breadcrumb, width: 300, height: 80);

            var peer = FrameworkElementAutomationPeer.CreatePeerForElement(breadcrumb.ContainerFromIndex(0));
            var invokeProvider = (IInvokeProvider)peer.GetPattern(PatternInterface.Invoke);
            invokeProvider.Invoke();

            Assert.AreEqual("Root", clickedItem);
        });
    }

    [TestMethod]
    public void VerifyCollectionChangeGetsRespected()
    {
        WpfTestHost.Run(() =>
        {
            var items = new ObservableCollection<string> { "Root", "Node A" };
            var breadcrumb = new ModernWpf.Controls.BreadcrumbBar
            {
                ItemsSource = items
            };

            using var host = new TestWindowHost(breadcrumb, width: 300, height: 80);

            Assert.AreEqual(2, breadcrumb.Containers.Count);

            items.Add("Node B");
            host.UpdateLayout();

            Assert.AreEqual(3, breadcrumb.Containers.Count);
            Assert.AreEqual("Node B", breadcrumb.ContainerFromIndex(2).Content);
        });
    }

    private static void RaiseItemButtonClick(BreadcrumbBarItem item)
    {
        var button = VisualTreeTestHelper
            .EnumerateDescendants(item)
            .OfType<Button>()
            .FirstOrDefault();

        if (button == null)
        {
            Assert.Fail("Could not find breadcrumb item button.");
            throw new AssertFailedException();
        }

        button.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
    }

    private static T FindTemplatePart<T>(Control control, string name)
        where T : class
    {
        control.ApplyTemplate();

        return control.Template?.FindName(name, control) as T
            ?? throw new AssertFailedException($"Could not find template part '{name}'.");
    }

    private static void AssertStateSetter(
        FrameworkElement stateGroupsRoot,
        string groupName,
        string stateName,
        string expectedTarget)
    {
        var group = FindVisualStateGroup(stateGroupsRoot, groupName);
        Assert.IsNotNull(group, $"Expected visual state group '{groupName}'.");

        var state = FindVisualState(group!, stateName);
        Assert.IsNotNull(state, $"Expected visual state '{groupName}.{stateName}'.");
        Assert.IsInstanceOfType(state, typeof(VisualStateEx));

        var stateEx = (VisualStateEx)state!;
        foreach (VisualStateSetter setter in stateEx.Setters)
        {
            if (setter.Target == expectedTarget)
            {
                return;
            }
        }

        Assert.Fail($"Expected visual state '{groupName}.{stateName}' to contain setter '{expectedTarget}'.");
    }

    private static void AssertDynamicResourceSetter(Style style, DependencyProperty property, object expectedResourceKey)
    {
        Assert.IsNotNull(style, $"Expected style for {property.Name}.");
        var setterValue = GetSetterValue(style, property);

        var dynamicResource = setterValue as DynamicResourceExtension;
        Assert.IsNotNull(dynamicResource, $"Expected {property.Name} to use a dynamic resource.");
        Assert.AreEqual(expectedResourceKey, dynamicResource!.ResourceKey);
    }

    private static void AssertSetterValue(Style style, DependencyProperty property, object? expectedValue)
    {
        Assert.AreEqual(expectedValue, GetSetterValue(style, property));
    }

    private static object? GetSetterValue(Style style, DependencyProperty property)
    {
        var setter = style.Setters.OfType<Setter>().SingleOrDefault(setter => setter.Property == property);
        Assert.IsNotNull(setter, $"Expected local setter for {property.Name}.");
        return setter!.Value;
    }

    private static void AssertResourceAlias(FrameworkElement element, object resourceKey, object expectedResourceKey)
    {
        Assert.AreSame(
            element.TryFindResource(expectedResourceKey),
            element.TryFindResource(resourceKey),
            $"Unexpected resource alias for {resourceKey}.");
    }

    private static void AssertSolidColorBrush(object value, Color expectedColor)
    {
        Assert.IsInstanceOfType(value, typeof(SolidColorBrush));
        Assert.AreEqual(expectedColor, ((SolidColorBrush)value).Color);
    }

    private static int CountRenderedPixels(FrameworkElement element)
    {
        var width = Math.Max(1, (int)Math.Ceiling(element.ActualWidth));
        var height = Math.Max(1, (int)Math.Ceiling(element.ActualHeight));
        var bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(element);

        var stride = width * 4;
        var pixels = new byte[stride * height];
        bitmap.CopyPixels(pixels, stride, 0);

        var count = 0;
        for (var i = 0; i < pixels.Length; i += 4)
        {
            if (pixels[i] != 0 || pixels[i + 1] != 0 || pixels[i + 2] != 0 || pixels[i + 3] != 0)
            {
                count++;
            }
        }

        return count;
    }

    private static VisualStateGroup? FindVisualStateGroup(FrameworkElement stateGroupsRoot, string groupName)
    {
        foreach (VisualStateGroup group in VisualStateManager.GetVisualStateGroups(stateGroupsRoot))
        {
            if (group.Name == groupName)
            {
                return group;
            }
        }

        return null;
    }

    private static VisualState? FindVisualState(VisualStateGroup group, string stateName)
    {
        foreach (VisualState state in group.States)
        {
            if (state.Name == stateName)
            {
                return state;
            }
        }

        return null;
    }

    private static DataTemplate CreateNameTemplate()
    {
        var template = new DataTemplate(typeof(MockNode));
        var textBlock = new FrameworkElementFactory(typeof(TextBlock));
        textBlock.SetBinding(TextBlock.TextProperty, new Binding(nameof(MockNode.Name)));
        template.VisualTree = textBlock;
        return template;
    }

    private sealed class MockNode
    {
        public string Name { get; set; } = string.Empty;
    }
}
