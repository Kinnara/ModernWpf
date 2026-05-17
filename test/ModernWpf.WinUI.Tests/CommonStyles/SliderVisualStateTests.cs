using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Shapes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf;
using ModernWpf.Controls.Primitives;
using ModernWpf.WinUI.TestApp;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.CommonStyles;

[TestClass]
public class SliderVisualStateTests
{
    [TestMethod]
    public void HorizontalSliderCommonStatesUseSourceVisualStateSetters()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var slider = CreateSlider(Orientation.Horizontal);
            using var host = new TestWindowHost(slider, width: 240, height: 100);
            host.UpdateLayout();

            var root = slider.GetTemplateRoot();
            var trackRect = FindTemplatePart<Rectangle>(slider, "HorizontalTrackRect");
            var decreaseRect = FindTemplatePart<Rectangle>(slider, "HorizontalDecreaseRect");
            var thumb = FindTemplatePart<Thumb>(slider, "HorizontalThumb");
            var container = FindTemplatePart<Border>(slider, "SliderContainer");

            Assert.AreEqual(0, slider.Template.Triggers.Count);
            Assert.IsTrue(SliderHelper.GetVisualStateSettersEnabled(slider));
            AssertSourceMetrics(slider);
            AssertStateSetters(
                root,
                "PointerOver",
                "HorizontalTrackRect.Fill",
                "HorizontalThumb.Background",
                "SliderContainer.Background",
                "HorizontalDecreaseRect.Fill");
            AssertStateSetters(
                root,
                "Pressed",
                "HorizontalTrackRect.Fill",
                "HorizontalThumb.Background",
                "SliderContainer.Background",
                "HorizontalDecreaseRect.Fill");
            AssertStateSetters(
                root,
                "Disabled",
                "HeaderContentPresenter.Foreground",
                "HorizontalDecreaseRect.Fill",
                "HorizontalTrackRect.Fill",
                "HorizontalThumb.Background",
                "TopTickBar.Fill",
                "BottomTickBar.Fill",
                "SliderContainer.Background");

            Assert.IsTrue(VisualStateManager.GoToState(slider, "PointerOver", false));
            Assert.AreSame(slider.TryFindResource("SliderTrackFillPointerOver"), trackRect.Fill);
            Assert.AreSame(slider.TryFindResource("SliderThumbBackgroundPointerOver"), thumb.Background);
            Assert.AreSame(slider.TryFindResource("SliderContainerBackgroundPointerOver"), container.Background);
            Assert.AreSame(slider.TryFindResource("SliderTrackValueFillPointerOver"), decreaseRect.Fill);

            Assert.IsTrue(VisualStateManager.GoToState(slider, "Pressed", false));
            Assert.AreSame(slider.TryFindResource("SliderTrackFillPressed"), trackRect.Fill);
            Assert.AreSame(slider.TryFindResource("SliderThumbBackgroundPressed"), thumb.Background);
            Assert.AreSame(slider.TryFindResource("SliderContainerBackgroundPressed"), container.Background);
            Assert.AreSame(slider.TryFindResource("SliderTrackValueFillPressed"), decreaseRect.Fill);
        });
    }

    [TestMethod]
    public void VerticalSliderCommonStatesUseSourceVisualStateSetters()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var slider = CreateSlider(Orientation.Vertical);
            using var host = new TestWindowHost(slider, width: 100, height: 240);
            host.UpdateLayout();

            var root = slider.GetTemplateRoot();
            var trackRect = FindTemplatePart<Rectangle>(slider, "VerticalTrackRect");
            var decreaseRect = FindTemplatePart<Rectangle>(slider, "VerticalDecreaseRect");
            var thumb = FindTemplatePart<Thumb>(slider, "VerticalThumb");
            var container = FindTemplatePart<Border>(slider, "SliderContainer");

            Assert.AreEqual(0, slider.Template.Triggers.Count);
            Assert.IsTrue(SliderHelper.GetVisualStateSettersEnabled(slider));
            AssertSourceMetrics(slider);
            AssertStateSetters(
                root,
                "PointerOver",
                "VerticalTrackRect.Fill",
                "VerticalThumb.Background",
                "SliderContainer.Background",
                "VerticalDecreaseRect.Fill");
            AssertStateSetters(
                root,
                "Pressed",
                "VerticalTrackRect.Fill",
                "VerticalThumb.Background",
                "SliderContainer.Background",
                "VerticalDecreaseRect.Fill");
            AssertStateSetters(
                root,
                "Disabled",
                "HeaderContentPresenter.Foreground",
                "VerticalDecreaseRect.Fill",
                "VerticalTrackRect.Fill",
                "VerticalThumb.Background",
                "LeftTickBar.Fill",
                "RightTickBar.Fill",
                "SliderContainer.Background");

            Assert.IsTrue(VisualStateManager.GoToState(slider, "PointerOver", false));
            Assert.AreSame(slider.TryFindResource("SliderTrackFillPointerOver"), trackRect.Fill);
            Assert.AreSame(slider.TryFindResource("SliderThumbBackgroundPointerOver"), thumb.Background);
            Assert.AreSame(slider.TryFindResource("SliderContainerBackgroundPointerOver"), container.Background);
            Assert.AreSame(slider.TryFindResource("SliderTrackValueFillPointerOver"), decreaseRect.Fill);
        });
    }

    [TestMethod]
    public void SliderHelperDrivesDisabledStateAndTickPlacement()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var slider = CreateSlider(Orientation.Horizontal);
            using var host = new TestWindowHost(slider, width: 240, height: 100);
            host.UpdateLayout();

            var root = slider.GetTemplateRoot();
            var topTickBar = FindTemplatePart<TickBar>(slider, "TopTickBar");
            var bottomTickBar = FindTemplatePart<TickBar>(slider, "BottomTickBar");
            var trackRect = FindTemplatePart<Rectangle>(slider, "HorizontalTrackRect");
            var thumb = FindTemplatePart<Thumb>(slider, "HorizontalThumb");

            Assert.AreEqual(Visibility.Collapsed, topTickBar.Visibility);
            Assert.AreEqual(Visibility.Collapsed, bottomTickBar.Visibility);

            slider.TickPlacement = TickPlacement.TopLeft;
            host.UpdateLayout();
            Assert.AreEqual(Visibility.Visible, topTickBar.Visibility);
            Assert.AreEqual(Visibility.Collapsed, bottomTickBar.Visibility);

            slider.TickPlacement = TickPlacement.BottomRight;
            host.UpdateLayout();
            Assert.AreEqual(Visibility.Collapsed, topTickBar.Visibility);
            Assert.AreEqual(Visibility.Visible, bottomTickBar.Visibility);

            slider.TickPlacement = TickPlacement.Both;
            host.UpdateLayout();
            Assert.AreEqual(Visibility.Visible, topTickBar.Visibility);
            Assert.AreEqual(Visibility.Visible, bottomTickBar.Visibility);

            slider.IsEnabled = false;
            host.UpdateLayout();

            Assert.AreEqual("Disabled", GetCurrentStateName(root));
            Assert.AreSame(slider.TryFindResource("SliderTrackFillDisabled"), trackRect.Fill);
            Assert.AreSame(slider.TryFindResource("SliderThumbBackgroundDisabled"), thumb.Background);
            Assert.AreSame(slider.TryFindResource("SliderTickBarFillDisabled"), topTickBar.Fill);
            Assert.AreSame(slider.TryFindResource("SliderTickBarFillDisabled"), bottomTickBar.Fill);
        });
    }

    private static Slider CreateSlider(Orientation orientation)
    {
        return new Slider
        {
            Orientation = orientation,
            Minimum = 0,
            Maximum = 100,
            Value = 50
        };
    }

    private static void AssertSourceMetrics(Slider slider)
    {
        Assert.AreEqual(14.0, slider.TryFindResource("SliderPreContentMargin"));
        Assert.AreEqual(14.0, slider.TryFindResource("SliderPostContentMargin"));
        Assert.AreEqual(18.0, slider.TryFindResource("SliderHorizontalThumbWidth"));
        Assert.AreEqual(18.0, slider.TryFindResource("SliderHorizontalThumbHeight"));
        Assert.AreEqual(18.0, slider.TryFindResource("SliderVerticalThumbWidth"));
        Assert.AreEqual(18.0, slider.TryFindResource("SliderVerticalThumbHeight"));
    }

    private static void AssertStateSetters(FrameworkElement stateGroupsRoot, string stateName, params string[] expectedTargets)
    {
        var state = FindCommonState(stateGroupsRoot, stateName);
        var actualTargets = state.Setters
            .Select(setter => string.IsNullOrEmpty(setter.Target) ? setter.Property : setter.Target)
            .ToArray();

        CollectionAssert.IsSubsetOf(expectedTargets, actualTargets);
    }

    private static VisualStateEx FindCommonState(FrameworkElement stateGroupsRoot, string stateName)
    {
        var group = VisualStateManager.GetVisualStateGroups(stateGroupsRoot)
            .OfType<VisualStateGroup>()
            .Single(item => item.Name == "CommonStates");
        var state = group.States
            .Cast<VisualState>()
            .Single(item => item.Name == stateName);

        Assert.IsInstanceOfType(state, typeof(VisualStateEx));
        return (VisualStateEx)state;
    }

    private static string GetCurrentStateName(FrameworkElement stateGroupsRoot)
    {
        var group = VisualStateManager.GetVisualStateGroups(stateGroupsRoot)
            .OfType<VisualStateGroup>()
            .Single(item => item.Name == "CommonStates");
        Assert.IsNotNull(group.CurrentState);
        return group.CurrentState.Name;
    }

    private static T FindTemplatePart<T>(Slider slider, string name)
        where T : DependencyObject
    {
        var part = slider.Template.FindName(name, slider) as T;
        if (part == null)
        {
            throw new AssertFailedException($"Expected Slider template part '{name}'.");
        }

        return part;
    }
}
