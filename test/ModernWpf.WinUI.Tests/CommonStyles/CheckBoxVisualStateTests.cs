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
            Assert.AreEqual("IndeterminateNormal", GetCurrentStateName(root));
            Assert.AreEqual(new Thickness(0), glyph.Margin);

            checkBox.IsChecked = false;
            host.UpdateLayout();

            Assert.AreEqual("UncheckedNormal", GetCurrentStateName(root));
            Assert.AreEqual(new Thickness(4), glyph.Margin);
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
