using System.Linq;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf;
using ModernWpf.Controls;
using ModernWpf.Controls.Primitives;
using ModernWpf.WinUI.TestApp;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.NumberBox;

[TestClass]
public class NumberBoxApiTests
{
    [TestMethod]
    public void VerifyTextAlignmentPropogates()
    {
        WpfTestHost.Run(() =>
        {
            var numberBox = new ModernWpf.Controls.NumberBox();
            using var host = new TestWindowHost(numberBox);

            var textBox = FindTemplatePart<TextBox>(numberBox, "InputBox");
            Assert.AreEqual(TextAlignment.Left, textBox.TextAlignment);

            numberBox.TextAlignment = TextAlignment.Right;
            host.UpdateLayout();

            Assert.AreEqual(TextAlignment.Right, textBox.TextAlignment);
        });
    }

    [TestMethod]
    public void VerifyInputScopePropogates()
    {
        WpfTestHost.Run(() =>
        {
            var numberBox = new ModernWpf.Controls.NumberBox();
            using var host = new TestWindowHost(numberBox);

            var inputTextBox = FindTemplatePart<TextBox>(numberBox, "InputBox");
            Assert.AreEqual(1, inputTextBox.InputScope.Names.Count);
            Assert.AreEqual(InputScopeNameValue.Number, GetInputScopeName(inputTextBox).NameValue);

            var scopeName = new InputScopeName
            {
                NameValue = InputScopeNameValue.CurrencyAmountAndSymbol
            };
            var scope = new InputScope();
            scope.Names.Add(scopeName);

            numberBox.InputScope = scope;
            host.UpdateLayout();

            Assert.AreEqual(1, inputTextBox.InputScope.Names.Count);
            Assert.AreEqual(InputScopeNameValue.CurrencyAmountAndSymbol, GetInputScopeName(inputTextBox).NameValue);
        });
    }

    [TestMethod]
    public void VerifyIsEnabledChangeUpdatesVisualState()
    {
        WpfTestHost.Run(() =>
        {
            var numberBox = new ModernWpf.Controls.NumberBox();
            using var host = new TestWindowHost(numberBox);

            numberBox.IsEnabled = true;
            host.UpdateLayout();

            var layoutRoot = (FrameworkElement)VisualTreeHelper.GetChild(numberBox, 0);
            var header = FindControlTemplatePart<ContentPresenterEx>(numberBox, "HeaderContentPresenter");
            var normalHeaderForeground = header.Foreground;

            Assert.AreEqual("Normal", GetCurrentStateName(layoutRoot, "CommonStates"));

            numberBox.IsEnabled = false;
            host.UpdateLayout();
            Assert.AreEqual("Disabled", GetCurrentStateName(layoutRoot, "CommonStates"));
            Assert.AreNotSame(normalHeaderForeground, header.Foreground);

            numberBox.IsEnabled = true;
            host.UpdateLayout();
            Assert.AreEqual("Normal", GetCurrentStateName(layoutRoot, "CommonStates"));
            Assert.AreSame(normalHeaderForeground, header.Foreground);
        });
    }

    [TestMethod]
    public void VerifyUIANameBehavior()
    {
        WpfTestHost.Run(() =>
        {
            var numberBox = new ModernWpf.Controls.NumberBox();
            using var host = new TestWindowHost(numberBox);

            var textBox = FindTemplatePart<TextBox>(numberBox, "InputBox");
            numberBox.Header = "Some header";
            host.UpdateLayout();
            VerifyUIAName(textBox, "Some header");

            numberBox.Header = new Button();
            AutomationProperties.SetName(numberBox, "Some UIA name");
            host.UpdateLayout();
            VerifyUIAName(textBox, "Some UIA name");

            numberBox.Header = new Button();
            host.UpdateLayout();
            VerifyUIAName(textBox, "Some UIA name");

            numberBox.Minimum = 0;
            numberBox.Maximum = 10;
            host.UpdateLayout();
            VerifyUIAName(textBox, "Some UIA name Minimum0 Maximum10");

            numberBox.Minimum = 50;
            numberBox.Maximum = 100;
            host.UpdateLayout();
            VerifyUIAName(textBox, "Some UIA name Minimum50 Maximum100");
        });
    }

    [TestMethod]
    public void VerifyFinalWinUI2ResourcesAndTemplateMetrics()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var numberBox = new ModernWpf.Controls.NumberBox
            {
                SpinButtonPlacementMode = ModernWpf.Controls.NumberBoxSpinButtonPlacementMode.Inline
            };

            using var host = new TestWindowHost(numberBox, width: 320, height: 180);
            host.UpdateLayout();

            var layoutRoot = (FrameworkElement)VisualTreeHelper.GetChild(numberBox, 0);
            var layoutGrid = (Grid)layoutRoot;
            AssertStateSetter(layoutRoot, "SpinButtonStates", "SpinButtonsVisible",
                "DownSpinButton.Visibility",
                "UpSpinButton.Visibility",
                "InputBox.MinWidth",
                "SpinButtonsColumn.Width");
            Assert.AreEqual("SpinButtonsVisible", GetCurrentStateName(layoutRoot, "SpinButtonStates"));

            var inputBox = FindTemplatePart<TextBox>(numberBox, "InputBox");
            Assert.AreEqual(120.0, inputBox.MinWidth);
            Assert.AreEqual(new GridLength(72), layoutGrid.ColumnDefinitions[2].Width);
            var inlineInputBoxStyle = inputBox.Style;

            var upButton = FindTemplatePart<RepeatButton>(numberBox, "UpSpinButton");
            var downButton = FindTemplatePart<RepeatButton>(numberBox, "DownSpinButton");
            Assert.AreEqual(Visibility.Visible, upButton.Visibility);
            Assert.AreEqual(Visibility.Visible, downButton.Visibility);
            AssertInlineSpinButtonMetrics(upButton, numberBox.FontSize);
            AssertInlineSpinButtonMetrics(downButton, numberBox.FontSize);

            numberBox.SpinButtonPlacementMode = ModernWpf.Controls.NumberBoxSpinButtonPlacementMode.Compact;
            host.UpdateLayout();

            Assert.AreEqual("SpinButtonsPopup", GetCurrentStateName(layoutRoot, "SpinButtonStates"));
            Assert.IsNotNull(inputBox.Style);
            Assert.AreNotSame(inlineInputBoxStyle, inputBox.Style);

            var inputBoxRoot = (FrameworkElement)VisualTreeHelper.GetChild(inputBox, 0);
            AssertStateSetter(inputBoxRoot, "SpinButtonStates", "SpinButtonsPopup",
                "PopupIndicator.Visibility");
            Assert.AreEqual("SpinButtonsPopup", GetCurrentStateName(inputBoxRoot, "SpinButtonStates"));

            var popupIndicator = FindTemplatePart<FontIconFallback>(inputBox, "PopupIndicator");
            Assert.AreEqual(Visibility.Visible, popupIndicator.Visibility);

            var popup = FindTemplatePart<Popup>(numberBox, "UpDownPopup");
            Assert.AreEqual(-21.0, popup.HorizontalOffset);
            Assert.AreEqual(-27.0, popup.VerticalOffset);

            var chrome = popup.Child as ThemeShadowChrome;
            Assert.IsNotNull(chrome);
            Assert.AreEqual(16.0, chrome!.Depth);

            var popupRoot = chrome.Child as Border;
            Assert.IsNotNull(popupRoot);
            Assert.AreEqual(new Thickness(6), popupRoot!.Padding);
            Assert.AreEqual(new Thickness(1), popupRoot.BorderThickness);

            var popupUpButton = FindTemplatePart<RepeatButton>(popupRoot, "PopupUpSpinButton");
            var popupDownButton = FindTemplatePart<RepeatButton>(popupRoot, "PopupDownSpinButton");
            AssertPopupSpinButtonMetrics(popupUpButton, new Thickness(0, 0, 0, 4));
            AssertPopupSpinButtonMetrics(popupDownButton, new Thickness(0));

            AssertGlobalResourceValue(numberBox, "NumberBoxSpinButtonBorderThickness", new Thickness(0, 1, 1, 1));
            AssertGlobalResourceValue(numberBox, "NumberBoxIconMargin", new Thickness(10, 0, 0, 0));
            AssertGlobalResourceValue(numberBox, "NumberBoxPopupHorizonalOffset", -21.0);
            AssertGlobalResourceValue(numberBox, "NumberBoxPopupVerticalOffset", -27.0);
            AssertGlobalResourceValue(numberBox, "NumberBoxPopupShadowDepth", 16.0);
            AssertGlobalResourceValue(numberBox, "NumberBoxMinWidth", 120.0);
            AssertGlobalResourceValue(numberBox, "NumberBoxPopupIndicatorMargin", new Thickness(0, 0, 8, 0));

            foreach (var themeName in new[] { "Light", "Dark" })
            {
                AssertThemeResourceReference(themeName, "NumberBoxPopupIndicatorForeground", "TextFillColorSecondaryBrush");
                AssertThemeResourceReference(themeName, "NumberBoxPopupBackground", "AcrylicBackgroundFillColorDefaultBrush");
                AssertThemeResourceReference(themeName, "NumberBoxPopupBorderBrush", "SurfaceStrokeColorFlyoutBrush");
                AssertThemeResourceReference(themeName, "NumberBoxPopupSpinButtonBackground", "SubtleFillColorTransparentBrush");
                AssertThemeResourceValue(themeName, "NumberBoxPopupBorderThickness", new Thickness(1));
                AssertThemeResourceValue(themeName, "NumberBoxPopupSpinButtonBorderThickness", new Thickness(0));
            }

            AssertThemeResourceReference("HighContrast", "NumberBoxPopupIndicatorForeground", "SystemControlForegroundBaseMediumBrush");
            AssertThemeResourceReference("HighContrast", "NumberBoxPopupBackground", "SystemControlBackgroundBaseHighBrush");
            AssertThemeResourceReference("HighContrast", "NumberBoxPopupBorderBrush", "SystemControlTransientBorderBrush");
            AssertThemeResourceReference("HighContrast", "NumberBoxPopupSpinButtonBackground", "SystemControlTransparentBrush");
            AssertThemeResourceValue("HighContrast", "NumberBoxPopupBorderThickness", new Thickness(1));
            AssertThemeResourceValue("HighContrast", "NumberBoxPopupSpinButtonBorderThickness", new Thickness(2));
        });
    }

    private static T FindTemplatePart<T>(DependencyObject root, string name)
        where T : FrameworkElement
    {
        return VisualTreeTestHelper.EnumerateDescendants(root)
            .OfType<T>()
            .Single(element => element.Name == name);
    }

    private static T FindControlTemplatePart<T>(Control control, string name)
        where T : FrameworkElement
    {
        control.ApplyTemplate();
        return control.Template.FindName(name, control) as T
            ?? throw new AssertFailedException($"Expected control template part '{name}'.");
    }

    private static InputScopeName GetInputScopeName(TextBox textBox)
    {
        return textBox.InputScope.Names[0] as InputScopeName
            ?? throw new AssertFailedException("Expected an InputScopeName entry.");
    }

    private static void AssertInlineSpinButtonMetrics(RepeatButton button, double expectedFontSize)
    {
        Assert.AreEqual(32.0, button.MinWidth);
        Assert.AreEqual(new Thickness(0), button.Padding);
        Assert.AreEqual(expectedFontSize, button.FontSize);
        Assert.AreEqual(new Thickness(0, 1, 1, 1), button.BorderThickness);
    }

    private static void AssertPopupSpinButtonMetrics(RepeatButton button, Thickness expectedMargin)
    {
        Assert.IsFalse(button.Focusable);
        Assert.AreEqual(36.0, button.Width);
        Assert.AreEqual(36.0, button.Height);
        Assert.AreEqual(new Thickness(0), button.Padding);
        Assert.AreEqual(expectedMargin, button.Margin);
        Assert.AreEqual(16.0, button.FontSize);
    }

    private static void AssertGlobalResourceValue<T>(FrameworkElement element, object resourceKey, T expectedValue)
    {
        Assert.AreEqual(expectedValue, element.TryFindResource(resourceKey), resourceKey.ToString());
    }

    private static void AssertThemeResourceReference(string themeName, string resourceKey, object expectedResourceKey)
    {
        var themeDictionary = ThemeResources.Current.GetThemeDictionary(themeName);
        Assert.IsTrue(themeDictionary.Contains(resourceKey), $"{themeName} is missing {resourceKey}.");
        Assert.IsTrue(themeDictionary.Contains(expectedResourceKey), $"{themeName} is missing {expectedResourceKey}.");
        Assert.AreSame(themeDictionary[expectedResourceKey], themeDictionary[resourceKey], $"{themeName}:{resourceKey}");
    }

    private static void AssertThemeResourceValue<T>(string themeName, string resourceKey, T expectedValue)
    {
        var themeDictionary = ThemeResources.Current.GetThemeDictionary(themeName);
        Assert.IsTrue(themeDictionary.Contains(resourceKey), $"{themeName} is missing {resourceKey}.");
        Assert.AreEqual(expectedValue, themeDictionary[resourceKey]);
    }

    private static void VerifyUIAName(FrameworkElement element, string expectedName)
    {
        var peer = FrameworkElementAutomationPeer.FromElement(element)
            ?? FrameworkElementAutomationPeer.CreatePeerForElement(element);

        Assert.AreEqual(expectedName, peer.GetName());
    }

    private static string GetCurrentStateName(FrameworkElement stateGroupsRoot, string groupName)
    {
        var group = VisualStateManager.GetVisualStateGroups(stateGroupsRoot)
            .OfType<VisualStateGroup>()
            .Single(item => item.Name == groupName);
        Assert.IsNotNull(group.CurrentState);
        return group.CurrentState.Name;
    }

    private static void AssertStateSetter(FrameworkElement stateGroupsRoot, string groupName, string stateName, params string[] expectedTargets)
    {
        var group = VisualStateManager.GetVisualStateGroups(stateGroupsRoot)
            .OfType<VisualStateGroup>()
            .Single(item => item.Name == groupName);
        var state = group.States.OfType<VisualStateEx>().Single(item => item.Name == stateName);
        var actualTargets = state.Setters
            .Select(setter => string.IsNullOrEmpty(setter.Target) ? setter.Property : setter.Target)
            .ToArray();

        CollectionAssert.IsSubsetOf(expectedTargets, actualTargets);
    }
}
