using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Shapes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Controls;
using ModernWpf.Controls.Primitives;
using ModernWpf.WinUI.TestApp;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.CommonStyles;

[TestClass]
public class RadioButtonVisualStateTests
{
    [TestMethod]
    public void DefaultRadioButtonStyleUsesSourceVisualStatesWithoutTemplateTriggers()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var radioButton = CreateRadioButton();
            using var host = new TestWindowHost(radioButton, width: 180, height: 80);
            host.UpdateLayout();

            var root = FindTemplatePart<FrameworkElement>(radioButton, "RootGrid");

            Assert.AreEqual(0, radioButton.Template.Triggers.Count);
            Assert.IsTrue(ButtonHelper.GetVisualStateSettersEnabled(radioButton));
            Assert.IsFalse(GetCommonStates(root).States.Cast<VisualState>().Any(state => state.Name == "MouseOver"));
            AssertStateSetters(root, "PointerOver", SourceCommonStateTargets());
            AssertStateSetters(root, "Pressed", SourceCommonStateTargets());
            AssertStateSetters(root, "Disabled", SourceCommonStateTargets());
            AssertStateSetters(root, "Checked", "CheckGlyph.Stroke", "PressedCheckGlyph.Background");
        });
    }

    [TestMethod]
    public void CommonStatesApplySourceResourceTargets()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var radioButton = CreateRadioButton();
            using var host = new TestWindowHost(radioButton, width: 180, height: 80);
            host.UpdateLayout();

            var root = FindTemplatePart<Border>(radioButton, "RootGrid");
            var presenter = FindTemplatePart<ContentPresenterEx>(radioButton, "ContentPresenter");
            var outerEllipse = FindTemplatePart<Ellipse>(radioButton, "OuterEllipse");
            var checkOuterEllipse = FindTemplatePart<Ellipse>(radioButton, "CheckOuterEllipse");
            var checkGlyph = FindTemplatePart<Ellipse>(radioButton, "CheckGlyph");

            AssertVisualStateAppliesResources(
                radioButton,
                "PointerOver",
                presenter,
                root,
                outerEllipse,
                checkOuterEllipse,
                checkGlyph,
                "RadioButtonForegroundPointerOver",
                "RadioButtonBackgroundPointerOver",
                "RadioButtonBorderBrushPointerOver",
                "RadioButtonOuterEllipseStrokePointerOver",
                "RadioButtonOuterEllipseFillPointerOver",
                "RadioButtonOuterEllipseCheckedStrokePointerOver",
                "RadioButtonOuterEllipseCheckedFillPointerOver",
                "RadioButtonCheckGlyphFillPointerOver",
                "RadioButtonCheckGlyphStrokePointerOver");

            AssertVisualStateAppliesResources(
                radioButton,
                "Pressed",
                presenter,
                root,
                outerEllipse,
                checkOuterEllipse,
                checkGlyph,
                "RadioButtonForegroundPressed",
                "RadioButtonBackgroundPressed",
                "RadioButtonBorderBrushPressed",
                "RadioButtonOuterEllipseStrokePressed",
                "RadioButtonOuterEllipseFillPressed",
                "RadioButtonOuterEllipseCheckedStrokePressed",
                "RadioButtonOuterEllipseCheckedFillPressed",
                "RadioButtonCheckGlyphFillPressed",
                "RadioButtonCheckGlyphStrokePressed");

            AssertVisualStateAppliesResources(
                radioButton,
                "Disabled",
                presenter,
                root,
                outerEllipse,
                checkOuterEllipse,
                checkGlyph,
                "RadioButtonForegroundDisabled",
                "RadioButtonBackgroundDisabled",
                "RadioButtonBorderBrushDisabled",
                "RadioButtonOuterEllipseStrokeDisabled",
                "RadioButtonOuterEllipseFillDisabled",
                "RadioButtonOuterEllipseCheckedStrokeDisabled",
                "RadioButtonOuterEllipseCheckedFillDisabled",
                "RadioButtonCheckGlyphFillDisabled",
                "RadioButtonCheckGlyphStrokeDisabled");
        });
    }

    [TestMethod]
    public void CheckedStateAppliesSourceGlyphTargets()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var radioButton = CreateRadioButton();
            radioButton.IsChecked = true;
            using var host = new TestWindowHost(radioButton, width: 180, height: 80);
            host.UpdateLayout();

            var checkGlyph = FindTemplatePart<Ellipse>(radioButton, "CheckGlyph");
            var pressedCheckGlyph = FindTemplatePart<Border>(radioButton, "PressedCheckGlyph");

            Assert.IsTrue(VisualStateManager.GoToState(radioButton, "Checked", false));
            Assert.AreSame(checkGlyph.TryFindResource("RadioButtonCheckGlyphStrokeChecked"), checkGlyph.Stroke);
            Assert.AreSame(pressedCheckGlyph.TryFindResource("RadioButtonCheckGlyphFillPressed"), pressedCheckGlyph.Background);

            Assert.IsTrue(VisualStateManager.GoToState(radioButton, "PointerOver", false));
            Assert.AreSame(checkGlyph.TryFindResource("RadioButtonCheckGlyphStrokePointerOver"), checkGlyph.Stroke);
        });
    }

    private static RadioButton CreateRadioButton()
    {
        return new RadioButton
        {
            Width = 150,
            Height = 48,
            Content = "Option"
        };
    }

    private static void AssertStateSetters(
        FrameworkElement stateGroupsRoot,
        string stateName,
        params string[] setterTargets)
    {
        var groups = VisualStateManager.GetVisualStateGroups(stateGroupsRoot)
            .OfType<VisualStateGroup>();
        var state = groups
            .SelectMany(group => group.States.Cast<VisualState>())
            .Single(item => item.Name == stateName);

        Assert.IsInstanceOfType(state, typeof(VisualStateEx));

        var stateEx = (VisualStateEx)state;
        foreach (var setterTarget in setterTargets)
        {
            Assert.IsTrue(
                stateEx.Setters.Any(setter => setter.Target == setterTarget),
                $"{stateName} should set {setterTarget}.");
        }
    }

    private static void AssertVisualStateAppliesResources(
        RadioButton radioButton,
        string stateName,
        ContentPresenterEx presenter,
        Border root,
        Ellipse outerEllipse,
        Ellipse checkOuterEllipse,
        Ellipse checkGlyph,
        string foregroundKey,
        string backgroundKey,
        string borderBrushKey,
        string outerStrokeKey,
        string outerFillKey,
        string checkedStrokeKey,
        string checkedFillKey,
        string glyphFillKey,
        string glyphStrokeKey)
    {
        Assert.IsTrue(VisualStateManager.GoToState(radioButton, stateName, false));
        Assert.AreSame(presenter.TryFindResource(foregroundKey), presenter.Foreground);
        Assert.AreSame(root.TryFindResource(backgroundKey), root.Background);
        Assert.AreSame(root.TryFindResource(borderBrushKey), root.BorderBrush);
        Assert.AreSame(outerEllipse.TryFindResource(outerStrokeKey), outerEllipse.Stroke);
        Assert.AreSame(outerEllipse.TryFindResource(outerFillKey), outerEllipse.Fill);
        Assert.AreSame(checkOuterEllipse.TryFindResource(checkedStrokeKey), checkOuterEllipse.Stroke);
        Assert.AreSame(checkOuterEllipse.TryFindResource(checkedFillKey), checkOuterEllipse.Fill);
        Assert.AreSame(checkGlyph.TryFindResource(glyphFillKey), checkGlyph.Fill);
        Assert.AreSame(checkGlyph.TryFindResource(glyphStrokeKey), checkGlyph.Stroke);
    }

    private static VisualStateGroup GetCommonStates(FrameworkElement stateGroupsRoot)
    {
        return VisualStateManager.GetVisualStateGroups(stateGroupsRoot)
            .OfType<VisualStateGroup>()
            .Single(group => group.Name == "CommonStates");
    }

    private static string[] SourceCommonStateTargets()
    {
        return new[]
        {
            "ContentPresenter.Foreground",
            "RootGrid.Background",
            "RootGrid.BorderBrush",
            "OuterEllipse.Stroke",
            "OuterEllipse.Fill",
            "CheckOuterEllipse.Stroke",
            "CheckOuterEllipse.Fill",
            "CheckGlyph.Fill",
            "CheckGlyph.Stroke"
        };
    }

    private static T FindTemplatePart<T>(Control control, string name)
        where T : DependencyObject
    {
        control.ApplyTemplate();
        return control.Template.FindName(name, control) as T
            ?? throw new AssertFailedException($"Expected RadioButton template part '{name}'.");
    }
}
