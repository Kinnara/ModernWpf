using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Controls;
using ModernWpf.WinUI.TestApp;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.ToggleSwitchControl;

[TestClass]
public class ToggleSwitchApiTests
{
    [TestMethod]
    public void DraggingStateUsesVisualStateSetters()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var toggleSwitch = new ModernWpf.Controls.ToggleSwitch();
            using var host = new TestWindowHost(toggleSwitch, width: 260, height: 120);
            host.UpdateLayout();

            var switchKnobOn = FindNamedDescendant<Border>(toggleSwitch, "SwitchKnobOn");
            var switchKnobOff = FindNamedDescendant<Rectangle>(toggleSwitch, "SwitchKnobOff");
            var switchKnobBounds = FindNamedDescendant<Rectangle>(toggleSwitch, "SwitchKnobBounds");

            Assert.IsTrue(System.Windows.VisualStateManager.GoToState(toggleSwitch, "Dragging", false));
            host.UpdateLayout();

            Assert.AreEqual(HorizontalAlignment.Right, switchKnobOn.HorizontalAlignment);
            Assert.AreEqual(new Thickness(0, 0, 3, 0), switchKnobOn.Margin);
            Assert.AreEqual(HorizontalAlignment.Left, switchKnobOff.HorizontalAlignment);
            Assert.AreEqual(new Thickness(3, 0, 0, 0), switchKnobOff.Margin);
            AssertBrushEquals((Brush)switchKnobBounds.TryFindResource("ToggleSwitchFillOnPressed"), switchKnobBounds.Fill);
            AssertBrushEquals((Brush)switchKnobBounds.TryFindResource("ToggleSwitchStrokeOnPressed"), switchKnobBounds.Stroke);
        });
    }

    [TestMethod]
    public void VerifyContentPresentersMatchWinUITemplate()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var headerTemplate = CreateTextTemplate();
            var offContentTemplate = CreateTextTemplate();
            var onContentTemplate = CreateTextTemplate();
            var toggleSwitch = new ModernWpf.Controls.ToggleSwitch
            {
                Header = "Header text",
                HeaderTemplate = headerTemplate,
                OffContent = "Off text",
                OffContentTemplate = offContentTemplate,
                OnContent = "On text",
                OnContentTemplate = onContentTemplate
            };

            using var host = new TestWindowHost(toggleSwitch, width: 260, height: 120);
            host.UpdateLayout();

            var headerPresenter = FindNamedDescendant<ContentPresenterEx>(toggleSwitch, "HeaderContentPresenter");
            var offPresenter = FindNamedDescendant<ContentPresenterEx>(toggleSwitch, "OffContentPresenter");
            var onPresenter = FindNamedDescendant<ContentPresenterEx>(toggleSwitch, "OnContentPresenter");

            Assert.AreEqual("Header text", headerPresenter.Content);
            Assert.AreSame(headerTemplate, headerPresenter.ContentTemplate);
            AssertBrushEquals((Brush)headerPresenter.TryFindResource("ToggleSwitchHeaderForeground"), headerPresenter.Foreground);

            Assert.AreEqual("Off text", offPresenter.Content);
            Assert.AreSame(offContentTemplate, offPresenter.ContentTemplate);
            AssertBrushEquals(toggleSwitch.Foreground, offPresenter.Foreground);

            Assert.AreEqual("On text", onPresenter.Content);
            Assert.AreSame(onContentTemplate, onPresenter.ContentTemplate);
            AssertBrushEquals(toggleSwitch.Foreground, onPresenter.Foreground);

            toggleSwitch.IsEnabled = false;
            host.UpdateLayout();

            AssertBrushEquals((Brush)headerPresenter.TryFindResource("ToggleSwitchHeaderForegroundDisabled"), headerPresenter.Foreground);
            AssertBrushEquals((Brush)offPresenter.TryFindResource("ToggleSwitchContentForegroundDisabled"), offPresenter.Foreground);
            AssertBrushEquals((Brush)onPresenter.TryFindResource("ToggleSwitchContentForegroundDisabled"), onPresenter.Foreground);
        });
    }

    private static DataTemplate CreateTextTemplate()
    {
        return (DataTemplate)XamlReader.Parse(
            @"<DataTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'>
                <TextBlock Text='{Binding}'/>
            </DataTemplate>");
    }

    private static void AssertBrushEquals(Brush expected, Brush actual)
    {
        Assert.IsNotNull(expected);
        Assert.IsNotNull(actual);

        if (expected is SolidColorBrush expectedSolid && actual is SolidColorBrush actualSolid)
        {
            Assert.AreEqual(expectedSolid.Color, actualSolid.Color);
            Assert.AreEqual(expectedSolid.Opacity, actualSolid.Opacity);
            return;
        }

        Assert.AreEqual(expected.ToString(), actual.ToString());
    }

    private static T FindNamedDescendant<T>(DependencyObject root, string name)
        where T : FrameworkElement
    {
        foreach (var descendant in VisualTreeTestHelper.EnumerateDescendants(root))
        {
            if (descendant is T element && element.Name == name)
            {
                return element;
            }
        }

        throw new InvalidOperationException($"Could not find descendant named '{name}'.");
    }
}
