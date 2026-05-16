using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf;
using ModernWpf.Controls;
using ModernWpf.Controls.Primitives;
using ModernWpf.WinUI.TestApp;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.MenuFlyoutVisualStates;

[TestClass]
public class MenuFlyoutVisualStateTests
{
    [TestMethod]
    public void SubmenuItemTemplateUsesWinUIVisualStateSetters()
    {
        WpfTestHost.Run(() =>
        {
            var item = new MenuItem
            {
                Header = "Open",
                Icon = new TextBlock { Text = "I" },
                InputGestureText = "Ctrl+O",
                IsCheckable = true,
                IsChecked = true,
                Template = FindMenuItemTemplate("SubmenuItemTemplateKey")
            };
            MenuItemHelper.SetVisualStateSettersEnabled(item, true);

            using var host = new TestWindowHost(item, width: 260, height: 120);
            var root = FindTemplateChild<Border>(item, "LayoutRoot");

            AssertStateSetter(root, "CommonStates", "PointerOver",
                "LayoutRoot.Background",
                "ContentPresenter.Foreground",
                "CheckGlyph.Foreground",
                "IconContent.Foreground",
                "KeyboardAcceleratorTextBlock.Foreground");
            AssertStateSetter(root, "CommonStates", "Pressed",
                "LayoutRoot.Background",
                "ContentPresenter.Foreground",
                "CheckGlyph.Foreground",
                "IconContent.Foreground",
                "KeyboardAcceleratorTextBlock.Foreground");
            AssertStateSetter(root, "CommonStates", "Disabled",
                "LayoutRoot.Background",
                "ContentPresenter.Foreground",
                "CheckGlyph.Foreground",
                "IconContent.Foreground",
                "KeyboardAcceleratorTextBlock.Foreground");
            AssertStateSetter(root, "CheckStates", "Checked", "CheckGlyph.Opacity");
            AssertStateSetter(root, "KeyboardAcceleratorTextVisibility", "KeyboardAcceleratorTextVisible", "KeyboardAcceleratorTextBlock.Visibility");

            AssertCurrentState(root, "CommonStates", "Normal");
            AssertCurrentState(root, "CheckStates", "Checked");
            AssertCurrentState(root, "KeyboardAcceleratorTextVisibility", "KeyboardAcceleratorTextVisible");

            var checkGlyph = FindTemplateChild<FrameworkElement>(item, "CheckGlyph");
            var keyboardAcceleratorTextBlock = FindTemplateChild<FrameworkElement>(item, "KeyboardAcceleratorTextBlock");
            Assert.AreEqual(1.0, checkGlyph.Opacity);
            Assert.AreEqual(Visibility.Visible, keyboardAcceleratorTextBlock.Visibility);

            item.IsChecked = false;
            item.InputGestureText = string.Empty;
            item.IsEnabled = false;
            host.UpdateLayout();
            WpfTestHost.DoEvents();

            AssertCurrentState(root, "CommonStates", "Disabled");
            AssertCurrentState(root, "CheckStates", "Unchecked");
            AssertCurrentState(root, "KeyboardAcceleratorTextVisibility", "KeyboardAcceleratorTextCollapsed");
            Assert.AreSame(item.TryFindResource("MenuFlyoutItemBackgroundDisabled"), root.Background);
            Assert.AreSame(
                item.TryFindResource("MenuFlyoutItemForegroundDisabled"),
                FindTemplateChild<ContentPresenterEx>(item, "ContentPresenter").Foreground);
            Assert.AreEqual(0.0, checkGlyph.Opacity);
        });
    }

    private static ControlTemplate FindMenuItemTemplate(string resourceId)
    {
        var key = new ComponentResourceKey(typeof(MenuItem), resourceId);
        return Application.Current.TryFindResource(key) as ControlTemplate
            ?? throw new AssertFailedException($"Expected MenuItem template resource '{resourceId}'.");
    }

    private static void AssertStateSetter(FrameworkElement stateGroupsRoot, string groupName, string stateName, params string[] expectedTargets)
    {
        var group = FindVisualStateGroup(stateGroupsRoot, groupName);
        var state = group.States.OfType<VisualStateEx>().Single(item => item.Name == stateName);
        var actualTargets = state.Setters
            .Select(setter => string.IsNullOrEmpty(setter.Target) ? setter.Property : setter.Target)
            .ToArray();

        CollectionAssert.IsSubsetOf(expectedTargets, actualTargets);
    }

    private static void AssertCurrentState(FrameworkElement stateGroupsRoot, string groupName, string expectedStateName)
    {
        Assert.AreEqual(expectedStateName, FindVisualStateGroup(stateGroupsRoot, groupName).CurrentState?.Name);
    }

    private static VisualStateGroup FindVisualStateGroup(FrameworkElement stateGroupsRoot, string groupName)
    {
        return VisualStateManager.GetVisualStateGroups(stateGroupsRoot)
            .OfType<VisualStateGroup>()
            .Single(item => item.Name == groupName);
    }

    private static T FindTemplateChild<T>(Control control, string childName)
        where T : FrameworkElement
    {
        control.ApplyTemplate();
        return control.Template.FindName(childName, control) as T
            ?? throw new AssertFailedException($"Expected template child '{childName}'.");
    }
}
