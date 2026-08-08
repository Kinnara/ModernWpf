using System.Linq;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
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
    public void VerifyWinUI3ResourcesAndTemplateMetrics()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var resources = new ResourceDictionary
            {
                Source = new System.Uri("/ModernWpf.Controls;component/NumberBox/NumberBox.xaml", System.UriKind.Relative)
            };

            AssertResource(resources, "NumberBoxPopupIndicatorMargin", new Thickness(0, 0, 8, 0));

            var numberBoxTextBoxStyle = (Style)resources["NumberBoxTextBoxStyle"];
            Assert.AreEqual(typeof(TextBox), numberBoxTextBoxStyle.TargetType);
            Assert.IsNotNull(numberBoxTextBoxStyle.BasedOn);
            Assert.AreEqual(typeof(TextBox), numberBoxTextBoxStyle.BasedOn!.TargetType);
            Assert.IsInstanceOfType(GetStyleSetter(numberBoxTextBoxStyle, Control.TemplateProperty).Value, typeof(ControlTemplate));

            var style = (Style)resources[typeof(ModernWpf.Controls.NumberBox)];
            AssertStyleSetter(style, Control.IsTabStopProperty, false);
            AssertDynamicResourceSetter(style, ModernWpf.Controls.NumberBox.SelectionBrushProperty, "TextControlSelectionHighlightColor");
            AssertDynamicResourceSetter(style, Control.FontFamilyProperty, "ContentControlThemeFontFamily");
            AssertDynamicResourceSetter(style, Control.FontSizeProperty, "ControlContentThemeFontSize");
            AssertDynamicResourceSetter(style, ModernWpf.Controls.NumberBox.CornerRadiusProperty, "ControlCornerRadius");
            AssertDynamicResourceSetter(style, Control.BackgroundProperty, "TextControlBackground");
            AssertDynamicResourceSetter(style, Control.BorderThicknessProperty, "TextControlBorderThemeThickness");
            AssertDynamicResourceSetter(style, Control.BorderBrushProperty, "TextControlBorderBrush");
            AssertDynamicResourceSetter(style, Control.ForegroundProperty, "TextControlForeground");
            AssertDynamicResourceSetter(style, Control.PaddingProperty, "TextControlThemePadding");
            Assert.IsNull(GetStyleSetter(style, Control.FocusVisualStyleProperty).Value, Control.FocusVisualStyleProperty.Name);
            Assert.IsInstanceOfType(GetStyleSetter(style, Control.TemplateProperty).Value, typeof(ControlTemplate));

            var numberBox = new ModernWpf.Controls.NumberBox
            {
                SpinButtonPlacementMode = ModernWpf.Controls.NumberBoxSpinButtonPlacementMode.Inline,
                Style = style
            };
            numberBox.Resources.MergedDictionaries.Add(resources);

            using var host = new TestWindowHost(numberBox, width: 320, height: 180);
            host.UpdateLayout();

            Assert.AreSame(style, numberBox.Style);
            Assert.IsFalse(numberBox.IsTabStop);
            AssertBrushEquals((Brush)numberBox.TryFindResource("TextControlSelectionHighlightColor"), numberBox.SelectionBrush);
            Assert.AreEqual(((FontFamily)numberBox.TryFindResource("ContentControlThemeFontFamily")).Source, numberBox.FontFamily.Source);
            Assert.AreEqual(numberBox.TryFindResource("ControlContentThemeFontSize"), numberBox.FontSize);
            Assert.AreEqual(numberBox.TryFindResource("ControlCornerRadius"), numberBox.CornerRadius);
            AssertBrushEquals((Brush)numberBox.TryFindResource("TextControlBackground"), numberBox.Background);
            Assert.AreEqual(numberBox.TryFindResource("TextControlBorderThemeThickness"), numberBox.BorderThickness);
            AssertBrushEquals((Brush)numberBox.TryFindResource("TextControlBorderBrush"), numberBox.BorderBrush);
            AssertBrushEquals((Brush)numberBox.TryFindResource("TextControlForeground"), numberBox.Foreground);
            Assert.AreEqual(numberBox.TryFindResource("TextControlThemePadding"), numberBox.Padding);
            Assert.IsNull(numberBox.FocusVisualStyle);

            var layoutRoot = (FrameworkElement)VisualTreeHelper.GetChild(numberBox, 0);
            AssertStateSetter(layoutRoot, "SpinButtonStates", "SpinButtonsVisible",
                "DownSpinButton.Visibility",
                "UpSpinButton.Visibility",
                "InputEater.Visibility",
                "InputBox.MinWidth");
            Assert.AreEqual("SpinButtonsVisible", GetCurrentStateName(layoutRoot, "SpinButtonStates"));

            var inputBox = FindTemplatePart<TextBox>(numberBox, "InputBox");
            Assert.AreEqual(3, Grid.GetColumnSpan(inputBox));
            Assert.AreEqual(120.0, inputBox.MinWidth);
            Assert.AreSame(numberBoxTextBoxStyle, inputBox.Style);
            AssertBrushEquals(numberBox.SelectionBrush, inputBox.SelectionBrush);
            Assert.AreEqual(numberBox.FontSize, inputBox.FontSize);
            Assert.AreEqual(numberBox.FontFamily.Source, inputBox.FontFamily.Source);
            AssertBrushEquals(numberBox.Background, inputBox.Background);
            Assert.AreEqual(numberBox.BorderThickness, inputBox.BorderThickness);
            AssertBrushEquals(numberBox.BorderBrush, inputBox.BorderBrush);
            Assert.AreEqual(numberBox.Padding, inputBox.Padding);
            AssertBrushEquals(numberBox.Foreground, inputBox.Foreground);
            Assert.AreEqual(numberBox.TextAlignment, inputBox.TextAlignment);
            var inlineInputBoxStyle = inputBox.Style;
            var inputBoxRoot = (FrameworkElement)VisualTreeHelper.GetChild(inputBox, 0);
            var inputBoxGrid = (Grid)inputBoxRoot;
            Assert.AreEqual(new GridLength(72), inputBoxGrid.ColumnDefinitions[2].Width);

            var headerPresenter = FindControlTemplatePart<ContentPresenterEx>(numberBox, "HeaderContentPresenter");
            if (numberBox.TryFindResource("TextControlHeaderForeground") is Brush headerForeground)
            {
                AssertBrushEquals(headerForeground, headerPresenter.Foreground);
            }
            else
            {
                Assert.IsNotNull(headerPresenter.Foreground);
            }
            Assert.AreEqual(numberBox.TryFindResource("TextBoxTopHeaderMargin"), headerPresenter.Margin);
            Assert.AreEqual(19.0, headerPresenter.MinHeight);
            Assert.AreEqual(numberBox.FontSize, headerPresenter.FontSize);
            Assert.AreEqual(numberBox.FontFamily.Source, headerPresenter.FontFamily.Source);
            Assert.AreEqual(Visibility.Collapsed, headerPresenter.Visibility);

            var descriptionPresenter = FindControlTemplatePart<ContentPresenterEx>(numberBox, "DescriptionPresenter");
            AssertBrushEquals((Brush)numberBox.TryFindResource("SystemControlDescriptionTextForegroundBrush"), descriptionPresenter.Foreground);

            var inputEater = FindTemplatePart<Button>(numberBox, "InputEater");
            Assert.AreEqual(Visibility.Visible, inputEater.Visibility);
            Assert.AreEqual(new Thickness(4, 0, 0, 0), inputEater.Margin);
            Assert.IsFalse(inputEater.IsTabStop);

            var upButton = FindTemplatePart<RepeatButton>(numberBox, "UpSpinButton");
            var downButton = FindTemplatePart<RepeatButton>(numberBox, "DownSpinButton");
            Assert.AreEqual(Visibility.Visible, upButton.Visibility);
            Assert.AreEqual(Visibility.Visible, downButton.Visibility);
            AssertInlineSpinButtonMetrics(upButton, numberBox.FontSize, new Thickness(4), "\uE70E");
            AssertInlineSpinButtonMetrics(downButton, numberBox.FontSize, new Thickness(0, 4, 4, 4), "\uE70D");

            numberBox.SpinButtonPlacementMode = ModernWpf.Controls.NumberBoxSpinButtonPlacementMode.Compact;
            host.UpdateLayout();

            Assert.AreEqual("SpinButtonsPopup", GetCurrentStateName(layoutRoot, "SpinButtonStates"));
            Assert.IsNotNull(inputBox.Style);
            Assert.AreSame(inlineInputBoxStyle, inputBox.Style);

            AssertStateSetter(inputBoxRoot, "SpinButtonStates", "SpinButtonsPopup",
                "PopupIndicator.Visibility");
            Assert.AreEqual("SpinButtonsPopup", GetCurrentStateName(inputBoxRoot, "SpinButtonStates"));

            var popupIndicator = FindTemplatePart<TextBlock>(inputBox, "PopupIndicator");
            Assert.AreEqual(Visibility.Visible, popupIndicator.Visibility);
            Assert.AreEqual("\uEC8F", popupIndicator.Text);

            var popup = FindTemplatePart<Popup>(numberBox, "UpDownPopup");
            Assert.IsTrue(popup.AllowsTransparency);
            Assert.AreEqual(PlacementMode.Right, popup.Placement);
            Assert.AreEqual(-21.0, popup.HorizontalOffset);
            Assert.AreEqual(-27.0, popup.VerticalOffset);

            var chrome = popup.Child as ThemeShadowChrome;
            Assert.IsNotNull(chrome);
            Assert.AreEqual(16.0, chrome!.Depth);
            Assert.IsTrue(chrome.ReservesShadowSpace);
            Assert.AreEqual(new Thickness(8, 4, 8, 12), chrome.ShadowPadding);
            Assert.AreEqual(numberBox.TryFindResource("OverlayCornerRadius"), chrome.CornerRadius);
            Assert.IsFalse(VisualTreeTestHelper.EnumerateDescendants(chrome).OfType<Border>().Any(border => border.Effect is System.Windows.Media.Effects.BlurEffect));

            var popupRoot = chrome.Child as Border;
            Assert.IsNotNull(popupRoot);
            Assert.AreEqual(new Thickness(6), popupRoot!.Padding);
            AssertBrushEquals((Brush)numberBox.TryFindResource("NumberBoxPopupBackground"), popupRoot.Background);
            AssertBrushEquals((Brush)numberBox.TryFindResource("NumberBoxPopupBorderBrush"), popupRoot.BorderBrush);
            Assert.AreEqual(new Thickness(1), popupRoot.BorderThickness);
            Assert.AreEqual(numberBox.TryFindResource("OverlayCornerRadius"), popupRoot.CornerRadius);

            var popupUpButton = FindTemplatePart<RepeatButton>(popupRoot, "PopupUpSpinButton");
            var popupDownButton = FindTemplatePart<RepeatButton>(popupRoot, "PopupDownSpinButton");
            AssertPopupSpinButtonMetrics(popupUpButton, new Thickness(0, 0, 0, 4), "\uE70E");
            AssertPopupSpinButtonMetrics(popupDownButton, new Thickness(0), "\uE70D");

            AssertGlobalResourceValue(numberBox, "NumberBoxSpinButtonBorderThickness", new Thickness(0, 1, 1, 1));
            AssertGlobalResourceValue(numberBox, "NumberBoxIconMargin", new Thickness(10, 0, 0, 0));
            AssertGlobalResourceValue(numberBox, "NumberBoxPopupHorizonalOffset", -21.0);
            AssertGlobalResourceValue(numberBox, "NumberBoxPopupVerticalOffset", -27.0);
            AssertGlobalResourceValue(numberBox, "NumberBoxPopupShadowDepth", 16.0);
            AssertGlobalResourceValue(numberBox, "NumberBoxMinWidth", 120.0);
            AssertGlobalResourceValue(numberBox, "NumberBoxPopupIndicatorMargin", new Thickness(0, 0, 8, 0));

            var spinButtonStyle = AssertControlStyle(upButton);
            Assert.AreSame(spinButtonStyle, downButton.Style);
            Assert.AreEqual(typeof(RepeatButton), spinButtonStyle.TargetType);
            Assert.IsNotNull(spinButtonStyle.BasedOn);
            Assert.AreEqual(typeof(RepeatButton), spinButtonStyle.BasedOn!.TargetType);
            AssertStyleSetter(spinButtonStyle, Control.IsTabStopProperty, false);
            AssertStyleSetter(spinButtonStyle, FrameworkElement.MinWidthProperty, 32.0);
            AssertStyleSetter(spinButtonStyle, Control.PaddingProperty, new Thickness(0));
            AssertStyleSetter(spinButtonStyle, FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Stretch);
            AssertDynamicResourceSetter(spinButtonStyle, Control.BorderThicknessProperty, "NumberBoxSpinButtonBorderThickness");
            AssertDynamicResourceSetter(spinButtonStyle, Control.ForegroundProperty, "TextControlButtonForeground");
            AssertStyleSetter(spinButtonStyle, Control.FontSizeProperty, 12.0);
            AssertDynamicResourceSetter(spinButtonStyle, Control.FontFamilyProperty, "SymbolThemeFontFamily");

            var popupSpinButtonStyle = AssertControlStyle(popupUpButton);
            Assert.AreSame(popupSpinButtonStyle, popupDownButton.Style);
            Assert.AreEqual(typeof(RepeatButton), popupSpinButtonStyle.TargetType);
            Assert.IsNotNull(popupSpinButtonStyle.BasedOn);
            Assert.AreEqual(typeof(RepeatButton), popupSpinButtonStyle.BasedOn!.TargetType);
            AssertStyleSetter(popupSpinButtonStyle, UIElement.FocusableProperty, false);
            AssertStyleSetter(popupSpinButtonStyle, Control.IsTabStopProperty, false);
            AssertStyleSetter(popupSpinButtonStyle, FrameworkElement.WidthProperty, 36.0);
            AssertStyleSetter(popupSpinButtonStyle, FrameworkElement.HeightProperty, 36.0);
            AssertStyleSetter(popupSpinButtonStyle, Control.PaddingProperty, new Thickness(0));
            AssertDynamicResourceSetter(popupSpinButtonStyle, Control.BackgroundProperty, "NumberBoxPopupSpinButtonBackground");
            AssertDynamicResourceSetter(popupSpinButtonStyle, Control.BorderThicknessProperty, "NumberBoxPopupSpinButtonBorderThickness");
            AssertDynamicResourceSetter(popupSpinButtonStyle, Control.ForegroundProperty, "TextControlButtonForeground");
            AssertStyleSetter(popupSpinButtonStyle, Control.FontSizeProperty, 16.0);
            AssertDynamicResourceSetter(popupSpinButtonStyle, Control.FontFamilyProperty, "SymbolThemeFontFamily");
            AssertDynamicResourceSetter(popupSpinButtonStyle, System.Windows.Controls.Border.CornerRadiusProperty, "ControlCornerRadius");
            AssertBrushEquals((Brush)popupUpButton.TryFindResource("NumberBoxPopupSpinButtonBackground"), popupUpButton.Background);
            AssertBrushEquals((Brush)popupDownButton.TryFindResource("NumberBoxPopupSpinButtonBackground"), popupDownButton.Background);

            numberBox.Foreground = Brushes.White;
            host.UpdateLayout();

            var darkSecondaryForeground = new SolidColorBrush(Color.FromArgb(0xC5, 0xFF, 0xFF, 0xFF));
            AssertBrushEquals(darkSecondaryForeground, upButton.Foreground);
            AssertBrushEquals(darkSecondaryForeground, downButton.Foreground);
            AssertBrushEquals(Brushes.White, TextElement.GetForeground(popupRoot));
            AssertBrushEquals(darkSecondaryForeground, popupUpButton.Foreground);
            AssertBrushEquals(darkSecondaryForeground, popupDownButton.Foreground);

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

    [TestMethod]
    public void VerifyUIALabeledByForwarding()
    {
        WpfTestHost.Run(() =>
        {
            var label = new TextBlock { Text = "Amount" };
            var numberBox = new ModernWpf.Controls.NumberBox();
            AutomationProperties.SetLabeledBy(numberBox, label);

            using var host = new TestWindowHost(new StackPanel
            {
                Children =
                {
                    label,
                    numberBox
                }
            });

            var textBox = FindTemplatePart<TextBox>(numberBox, "InputBox");
            Assert.AreSame(label, AutomationProperties.GetLabeledBy(textBox));

            var nextLabel = new TextBlock { Text = "Updated amount" };
            ((StackPanel)host.Window.Content).Children.Insert(0, nextLabel);
            AutomationProperties.SetLabeledBy(numberBox, nextLabel);
            host.UpdateLayout();

            Assert.AreSame(nextLabel, AutomationProperties.GetLabeledBy(textBox));
        });
    }

    [TestMethod]
    public void VisiblePlaceholderParticipatesInControlAccessibilityView()
    {
        WpfTestHost.Run(() =>
        {
            var numberBox = new ModernWpf.Controls.NumberBox
            {
                PlaceholderText = "Enter a number"
            };

            using var host = new TestWindowHost(numberBox);
            host.UpdateLayout();

            var inputBox = FindTemplatePart<TextBox>(numberBox, "InputBox");
            var placeholder = FindTemplatePart<TextBlock>(inputBox, "PlaceholderTextContentPresenter");
            var peer = FrameworkElementAutomationPeer.CreatePeerForElement(placeholder);

            Assert.AreEqual(Visibility.Visible, placeholder.Visibility);
            Assert.IsNotNull(peer);
            Assert.IsTrue(peer!.IsControlElement());
            Assert.IsFalse(peer.IsContentElement());
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

    private static void AssertInlineSpinButtonMetrics(RepeatButton button, double expectedFontSize, Thickness expectedMargin, string expectedContent)
    {
        Assert.AreEqual(32.0, button.MinWidth);
        Assert.AreEqual(new Thickness(0), button.Padding);
        Assert.AreEqual(expectedFontSize, button.FontSize);
        Assert.AreEqual(expectedContent, button.Content);
        Assert.AreEqual(expectedMargin, button.Margin);
        Assert.AreEqual(new Thickness(0, 1, 1, 1), button.BorderThickness);
        Assert.AreEqual(button.TryFindResource("TextControlButtonBackground"), button.TryFindResource("RepeatButtonBackground"));
        Assert.AreEqual(button.TryFindResource("TextControlButtonForeground"), button.TryFindResource("RepeatButtonForeground"));
        Assert.AreEqual(button.TryFindResource("TextControlButtonBorderBrush"), button.TryFindResource("RepeatButtonBorderBrush"));
    }

    private static void AssertPopupSpinButtonMetrics(RepeatButton button, Thickness expectedMargin, string expectedContent)
    {
        Assert.IsFalse(button.Focusable);
        Assert.IsFalse(button.IsTabStop);
        Assert.AreEqual(36.0, button.Width);
        Assert.AreEqual(36.0, button.Height);
        Assert.AreEqual(new Thickness(0), button.Padding);
        Assert.AreEqual(expectedMargin, button.Margin);
        Assert.AreEqual(expectedContent, button.Content);
        Assert.AreEqual(16.0, button.FontSize);
    }

    private static void AssertResource(ResourceDictionary resources, string key, object expectedValue)
    {
        Assert.IsTrue(resources.Contains(key), $"Expected resource '{key}' to exist.");
        Assert.AreEqual(expectedValue, resources[key], $"Unexpected value for '{key}'.");
    }

    private static void AssertGlobalResourceValue<T>(FrameworkElement element, object resourceKey, T expectedValue)
    {
        Assert.AreEqual(expectedValue, element.TryFindResource(resourceKey), resourceKey.ToString());
    }

    private static Style AssertControlStyle(Control control)
    {
        Assert.IsNotNull(control.Style, $"{control.Name} should have an explicit style.");
        return control.Style!;
    }

    private static void AssertStyleSetter(Style style, DependencyProperty property, object expectedValue)
    {
        var setter = GetStyleSetter(style, property);
        Assert.AreEqual(expectedValue, setter.Value, property.Name);
    }

    private static void AssertDynamicResourceSetter(Style style, DependencyProperty property, object expectedResourceKey)
    {
        var setter = GetStyleSetter(style, property);
        var dynamicResource = setter.Value as DynamicResourceExtension;
        Assert.IsNotNull(dynamicResource, $"Expected {property.Name} to use DynamicResource.");
        Assert.AreEqual(expectedResourceKey, dynamicResource!.ResourceKey, property.Name);
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

    private static Setter GetStyleSetter(Style style, DependencyProperty property)
    {
        return style.Setters.OfType<Setter>().SingleOrDefault(setter => setter.Property == property)
            ?? throw new AssertFailedException($"Expected {style.TargetType.Name} style to set {property.Name}.");
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
