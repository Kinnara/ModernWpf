using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Shapes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Controls.Primitives;
using ModernWpf.WinUI.TestApp;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.CommonStyles;

[TestClass]
public class ScrollBarVisualStateTests
{
    [TestMethod]
    public void VerticalScrollBarConsciousStatesUseVisualStateSetters()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var scrollBar = CreateScrollBar(Orientation.Vertical);
            ScrollBarHelper.SetAutoHide(scrollBar, false);

            using var host = new TestWindowHost(scrollBar, width: 120, height: 180);

            var root = FindTemplatePart<System.Windows.Controls.Border>(scrollBar, "Root");
            var trackRect = FindTemplatePart<Rectangle>(scrollBar, "VerticalTrackRect");
            var thumb = FindTemplatePart<Thumb>(scrollBar, "VerticalThumb");

            AssertExpandedState(root);
            Assert.AreSame(scrollBar.TryFindResource("ScrollBarBackgroundPointerOver"), root.Background);
            Assert.AreSame(scrollBar.TryFindResource("ScrollBarBorderBrushPointerOver"), root.BorderBrush);
            Assert.AreSame(scrollBar.TryFindResource("ScrollBarTrackStrokePointerOver"), trackRect.Stroke);
            Assert.AreSame(scrollBar.TryFindResource("ScrollBarTrackFillPointerOver"), trackRect.Fill);
            Assert.AreSame(scrollBar.TryFindResource("ScrollBarThumbBackground"), thumb.Background);

            ScrollBarHelper.SetAutoHide(scrollBar, true);
            host.UpdateLayout();

            AssertCollapsedState(root);
            Assert.AreSame(scrollBar.TryFindResource("ScrollBarPanningThumbBackground"), thumb.Background);
        });
    }

    [TestMethod]
    public void HorizontalScrollBarConsciousStatesUseVisualStateSetters()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var scrollBar = CreateScrollBar(Orientation.Horizontal);
            ScrollBarHelper.SetAutoHide(scrollBar, false);

            using var host = new TestWindowHost(scrollBar, width: 180, height: 120);

            var root = FindTemplatePart<System.Windows.Controls.Border>(scrollBar, "Root");
            var trackRect = FindTemplatePart<Rectangle>(scrollBar, "HorizontalTrackRect");
            var thumb = FindTemplatePart<Thumb>(scrollBar, "HorizontalThumb");

            AssertExpandedState(root);
            Assert.AreSame(scrollBar.TryFindResource("ScrollBarBackgroundPointerOver"), root.Background);
            Assert.AreSame(scrollBar.TryFindResource("ScrollBarBorderBrushPointerOver"), root.BorderBrush);
            Assert.AreSame(scrollBar.TryFindResource("ScrollBarTrackStrokePointerOver"), trackRect.Stroke);
            Assert.AreSame(scrollBar.TryFindResource("ScrollBarTrackFillPointerOver"), trackRect.Fill);
            Assert.AreSame(scrollBar.TryFindResource("ScrollBarThumbBackground"), thumb.Background);

            ScrollBarHelper.SetAutoHide(scrollBar, true);
            host.UpdateLayout();

            AssertCollapsedState(root);
            Assert.AreSame(scrollBar.TryFindResource("ScrollBarPanningThumbBackground"), thumb.Background);
        });
    }

    private static ScrollBar CreateScrollBar(Orientation orientation)
    {
        return new ScrollBar
        {
            Orientation = orientation,
            Minimum = 0,
            Maximum = 100,
            ViewportSize = 20
        };
    }

    private static void AssertExpandedState(FrameworkElement stateGroupsRoot)
    {
        string stateName = GetConsciousStateName(stateGroupsRoot);
        Assert.IsTrue(
            stateName == "Expanded" || stateName == "ExpandedWithoutAnimation",
            $"Expected expanded ScrollBar conscious state, got '{stateName}'.");
    }

    private static void AssertCollapsedState(FrameworkElement stateGroupsRoot)
    {
        string stateName = GetConsciousStateName(stateGroupsRoot);
        Assert.IsTrue(
            stateName == "Collapsed" || stateName == "CollapsedWithoutAnimation",
            $"Expected collapsed ScrollBar conscious state, got '{stateName}'.");
    }

    private static string GetConsciousStateName(FrameworkElement stateGroupsRoot)
    {
        var consciousStates = VisualStateManager.GetVisualStateGroups(stateGroupsRoot)
            .OfType<VisualStateGroup>()
            .Single(group => group.Name == "ConsciousStates");
        Assert.IsNotNull(consciousStates.CurrentState);
        return consciousStates.CurrentState.Name;
    }

    private static T FindTemplatePart<T>(ScrollBar scrollBar, string name)
        where T : DependencyObject
    {
        var part = scrollBar.Template.FindName(name, scrollBar) as T;
        if (part == null)
        {
            throw new AssertFailedException($"Expected ScrollBar template part '{name}'.");
        }

        return part;
    }
}
