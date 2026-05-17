using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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

            Assert.AreEqual(0, checkBox.Template.Triggers.Count);
            Assert.IsInstanceOfType(root, typeof(GridEx));
            AssertStateSetters(root, "IndeterminateNormal", "DownLevelCheckGlyph.Data", "DownLevelCheckGlyph.Margin", "DownLevelCheckGlyph.Opacity");
            AssertStateSetters(root, "IndeterminatePointerOver", "DownLevelCheckGlyph.Data", "DownLevelCheckGlyph.Margin", "DownLevelCheckGlyph.Opacity");
            AssertStateSetters(root, "IndeterminatePressed", "DownLevelCheckGlyph.Data", "DownLevelCheckGlyph.Margin", "DownLevelCheckGlyph.Opacity");
            AssertStateSetters(root, "IndeterminateDisabled", "DownLevelCheckGlyph.Data", "DownLevelCheckGlyph.Margin", "DownLevelCheckGlyph.Opacity");
            AssertAnimatedIconStateSetter(root, "IndeterminateNormal", "NormalIndeterminate");
            AssertAnimatedIconStateSetter(root, "IndeterminatePointerOver", "PointerOverIndeterminate");
            AssertAnimatedIconStateSetter(root, "IndeterminatePressed", "PressedIndeterminate");
            AssertAnimatedIconStateSetter(root, "IndeterminateDisabled", "NormalIndeterminate");
            Assert.AreEqual("IndeterminateNormal", GetCurrentStateName(root));
            Assert.AreEqual("NormalIndeterminate", AnimatedIcon.GetState(glyph));
            Assert.AreEqual(new Thickness(0), glyph.Margin);
            Assert.AreEqual(1.0, glyph.Opacity);

            checkBox.IsChecked = false;
            host.UpdateLayout();

            Assert.AreEqual("UncheckedNormal", GetCurrentStateName(root));
            Assert.AreEqual("NormalOff", AnimatedIcon.GetState(glyph));
            Assert.AreEqual(new Thickness(4), glyph.Margin);
            Assert.AreEqual(0.0, glyph.Opacity);
        });
    }

    [TestMethod]
    public void CombinedStatesUseWinUISourceVisualStateSetters()
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

            AssertStateSetters(root, "UncheckedNormal", "DownLevelCheckGlyph.(local:AnimatedIcon.State)");
            AssertStateSetters(root, "UncheckedPointerOver", SourceColorTargets().Append("DownLevelCheckGlyph.(local:AnimatedIcon.State)").ToArray());
            AssertStateSetters(root, "UncheckedPressed", SourceColorTargets().Append("DownLevelCheckGlyph.(local:AnimatedIcon.State)").ToArray());
            AssertStateSetters(root, "UncheckedDisabled", SourceColorTargets().Append("DownLevelCheckGlyph.(local:AnimatedIcon.State)").ToArray());
            AssertStateSetters(root, "CheckedNormal", SourceColorTargets().Concat(new[] { "DownLevelCheckGlyph.(local:AnimatedIcon.State)", "DownLevelCheckGlyph.Opacity" }).ToArray());
            AssertStateSetters(root, "CheckedPointerOver", SourceColorTargets().Concat(new[] { "DownLevelCheckGlyph.(local:AnimatedIcon.State)", "DownLevelCheckGlyph.Opacity" }).ToArray());
            AssertStateSetters(root, "CheckedPressed", SourceColorTargets().Concat(new[] { "DownLevelCheckGlyph.(local:AnimatedIcon.State)", "DownLevelCheckGlyph.Opacity" }).ToArray());
            AssertStateSetters(root, "CheckedDisabled", SourceColorTargets().Concat(new[] { "DownLevelCheckGlyph.(local:AnimatedIcon.State)", "DownLevelCheckGlyph.Opacity" }).ToArray());
            AssertStateSetters(root, "IndeterminateNormal", SourceColorTargets().Concat(new[] { "DownLevelCheckGlyph.(local:AnimatedIcon.State)", "DownLevelCheckGlyph.Data", "DownLevelCheckGlyph.Margin", "DownLevelCheckGlyph.Opacity" }).ToArray());
            AssertStateSetters(root, "IndeterminatePointerOver", SourceColorTargets().Concat(new[] { "DownLevelCheckGlyph.(local:AnimatedIcon.State)", "DownLevelCheckGlyph.Data", "DownLevelCheckGlyph.Margin", "DownLevelCheckGlyph.Opacity" }).ToArray());
            AssertStateSetters(root, "IndeterminatePressed", SourceColorTargets().Concat(new[] { "DownLevelCheckGlyph.(local:AnimatedIcon.State)", "DownLevelCheckGlyph.Data", "DownLevelCheckGlyph.Margin", "DownLevelCheckGlyph.Opacity" }).ToArray());
            AssertStateSetters(root, "IndeterminateDisabled", SourceColorTargets().Concat(new[] { "DownLevelCheckGlyph.(local:AnimatedIcon.State)", "DownLevelCheckGlyph.Data", "DownLevelCheckGlyph.Margin", "DownLevelCheckGlyph.Opacity" }).ToArray());

            AssertAnimatedIconState(root, glyph, "UncheckedNormal", "NormalOff");
            AssertAnimatedIconState(root, glyph, "CheckedNormal", "NormalOn");
            AssertAnimatedIconState(root, glyph, "IndeterminateNormal", "NormalIndeterminate");
        });
    }

    [TestMethod]
    public void AddAndSubtractKeysFollowWinUICheckBoxSourceBehavior()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var checkBox = new CheckBox
            {
                IsChecked = false,
                Content = "Option"
            };

            using var host = new TestWindowHost(checkBox, width: 180, height: 80);
            host.UpdateLayout();

            var addArgs = RaiseKey(checkBox, Key.Add);
            Assert.IsTrue(addArgs.Handled);
            Assert.AreEqual(true, checkBox.IsChecked);

            var subtractArgs = RaiseKey(checkBox, Key.Subtract);
            Assert.IsTrue(subtractArgs.Handled);
            Assert.AreEqual(false, checkBox.IsChecked);

            checkBox.IsThreeState = true;
            var ignoredAddArgs = RaiseKey(checkBox, Key.Add);
            Assert.IsFalse(ignoredAddArgs.Handled);
            Assert.AreEqual(false, checkBox.IsChecked);
        });
    }

    private static void AssertStateSetters(
        FrameworkElement stateGroupsRoot,
        string stateName,
        params string[] setterTargets)
    {
        var group = GetCombinedStatesGroup(stateGroupsRoot);
        var state = group.States
            .Cast<VisualState>()
            .Single(item => item.Name == stateName);

        Assert.IsInstanceOfType(state, typeof(VisualStateEx));

        var stateEx = (VisualStateEx)state;
        foreach (var setterTarget in setterTargets)
        {
            Assert.IsTrue(
                stateEx.Setters.Any(setter => setter.Target == setterTarget),
                $"CombinedStates.{stateName} should set {setterTarget}.");
        }
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

    private static string[] SourceColorTargets()
    {
        return new[]
        {
            "ContentPresenter.Foreground",
            "RootGrid.Background",
            "RootGrid.BorderBrush",
            "NormalRectangle.Stroke",
            "NormalRectangle.Fill",
            "DownLevelCheckGlyph.Foreground"
        };
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

    private static KeyEventArgs RaiseKey(UIElement element, Key key)
    {
        var args = new KeyEventArgs(
            Keyboard.PrimaryDevice,
            PresentationSource.FromVisual(element),
            Environment.TickCount,
            key)
        {
            RoutedEvent = Keyboard.KeyDownEvent,
            Source = element
        };

        element.RaiseEvent(args);
        return args;
    }
}
