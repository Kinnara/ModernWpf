using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Controls;
using ModernWpf.WinUI.TestApp;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.CommonStyles;

[TestClass]
public class CheckBoxVisualStateTests
{
    [TestMethod]
    public void IndeterminateStatesUseVisualStateSettersForGlyphMargin()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var checkBox = new CheckBox
            {
                IsThreeState = true,
                IsChecked = null,
                Content = "Option"
            };

            using var host = new TestWindowHost(checkBox, width: 180, height: 80);
            host.UpdateLayout();

            var root = FindTemplatePart<FrameworkElement>(checkBox, "RootGrid");
            var glyph = FindTemplatePart<FontIconFallback>(checkBox, "DownLevelCheckGlyph");

            AssertStateSetter(root, "IndeterminateNormal", "DownLevelCheckGlyph.Margin");
            AssertStateSetter(root, "IndeterminatePointerOver", "DownLevelCheckGlyph.Margin");
            AssertStateSetter(root, "IndeterminatePressed", "DownLevelCheckGlyph.Margin");
            AssertStateSetter(root, "IndeterminateDisabled", "DownLevelCheckGlyph.Margin");
            AssertAnimatedIconStateSetter(root, "IndeterminateNormal", "NormalIndeterminate");
            AssertAnimatedIconStateSetter(root, "IndeterminatePointerOver", "PointerOverIndeterminate");
            AssertAnimatedIconStateSetter(root, "IndeterminatePressed", "PressedIndeterminate");
            AssertAnimatedIconStateSetter(root, "IndeterminateDisabled", "NormalIndeterminate");
            Assert.AreEqual("IndeterminateNormal", GetCurrentStateName(root));
            Assert.AreEqual("NormalIndeterminate", AnimatedIcon.GetState(glyph));
            Assert.AreEqual(new Thickness(0), glyph.Margin);

            checkBox.IsChecked = false;
            host.UpdateLayout();

            Assert.AreEqual("UncheckedNormal", GetCurrentStateName(root));
            Assert.AreEqual("NormalOff", AnimatedIcon.GetState(glyph));
            Assert.AreEqual(new Thickness(4), glyph.Margin);
        });
    }

    [TestMethod]
    public void CombinedStatesUseVisualStateSettersForAnimatedIconState()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var checkBox = new CheckBox
            {
                IsThreeState = true,
                Content = "Option"
            };

            using var host = new TestWindowHost(checkBox, width: 180, height: 80);
            host.UpdateLayout();

            var root = FindTemplatePart<FrameworkElement>(checkBox, "RootGrid");
            var glyph = FindTemplatePart<FontIconFallback>(checkBox, "DownLevelCheckGlyph");

            AssertAnimatedIconState(root, glyph, "UncheckedNormal", "NormalOff");
            AssertAnimatedIconState(root, glyph, "UncheckedPointerOver", "PointerOverOff");
            AssertAnimatedIconState(root, glyph, "UncheckedPressed", "PressedOff");
            AssertAnimatedIconState(root, glyph, "UncheckedDisabled", "NormalOff");
            AssertAnimatedIconState(root, glyph, "CheckedNormal", "NormalOn");
            AssertAnimatedIconState(root, glyph, "CheckedPointerOver", "PointerOverOn");
            AssertAnimatedIconState(root, glyph, "CheckedPressed", "PressedOn");
            AssertAnimatedIconState(root, glyph, "CheckedDisabled", "NormalOn");
            AssertAnimatedIconState(root, glyph, "IndeterminateNormal", "NormalIndeterminate");
            AssertAnimatedIconState(root, glyph, "IndeterminatePointerOver", "PointerOverIndeterminate");
            AssertAnimatedIconState(root, glyph, "IndeterminatePressed", "PressedIndeterminate");
            AssertAnimatedIconState(root, glyph, "IndeterminateDisabled", "NormalIndeterminate");
        });
    }

    private static void AssertStateSetter(
        FrameworkElement stateGroupsRoot,
        string stateName,
        string setterTarget)
    {
        var group = GetCombinedStatesGroup(stateGroupsRoot);
        var state = group.States
            .Cast<VisualState>()
            .Single(item => item.Name == stateName);

        Assert.IsInstanceOfType(state, typeof(VisualStateEx));

        var stateEx = (VisualStateEx)state;
        Assert.IsTrue(
            stateEx.Setters.Any(setter => setter.Target == setterTarget),
            $"CombinedStates.{stateName} should set {setterTarget}.");
    }

    private static void AssertAnimatedIconState(
        FrameworkElement stateGroupsRoot,
        DependencyObject glyph,
        string stateName,
        string expectedValue)
    {
        AssertAnimatedIconStateSetter(stateGroupsRoot, stateName, expectedValue);
        Assert.IsTrue(VisualStateManager.GoToElementState(stateGroupsRoot, stateName, false));
        Assert.AreEqual(expectedValue, AnimatedIcon.GetState(glyph));
    }

    private static void AssertAnimatedIconStateSetter(
        FrameworkElement stateGroupsRoot,
        string stateName,
        string expectedValue)
    {
        var group = GetCombinedStatesGroup(stateGroupsRoot);
        var state = group.States
            .Cast<VisualState>()
            .Single(item => item.Name == stateName);

        Assert.IsInstanceOfType(state, typeof(VisualStateEx));

        var stateEx = (VisualStateEx)state;
        var setter = stateEx.Setters.Single(item => item.Target == "DownLevelCheckGlyph.(local:AnimatedIcon.State)");

        Assert.AreEqual(expectedValue, setter.Value);
    }

    private static string GetCurrentStateName(FrameworkElement stateGroupsRoot)
    {
        var group = GetCombinedStatesGroup(stateGroupsRoot);
        Assert.IsNotNull(group.CurrentState);
        return group.CurrentState.Name;
    }

    private static VisualStateGroup GetCombinedStatesGroup(FrameworkElement stateGroupsRoot)
    {
        return VisualStateManager.GetVisualStateGroups(stateGroupsRoot)
            .OfType<VisualStateGroup>()
            .Single(item => item.Name == "CombinedStates");
    }

    private static T FindTemplatePart<T>(Control control, string name)
        where T : DependencyObject
    {
        var part = control.Template.FindName(name, control) as T;
        if (part == null)
        {
            throw new AssertFailedException($"Expected CheckBox template part '{name}'.");
        }

        return part;
    }
}
