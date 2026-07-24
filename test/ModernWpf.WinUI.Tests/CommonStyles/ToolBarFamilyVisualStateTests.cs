using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;
using System.Windows.Media;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf;
using ModernWpf.Controls;
using ModernWpf.Controls.Primitives;
using ModernWpf.WinUI.TestApp;
using ModernWpf.WinUI.TestInfra;
using WpfComboBox = System.Windows.Controls.ComboBox;

namespace ModernWpf.WinUI.Tests.CommonStyles;

[TestClass]
public class ToolBarFamilyVisualStateTests
{
    [TestMethod]
    public void DefaultSeparatorStyleUsesOfficialWpfFluentTemplateShape()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var defaultStyle = (Style)Application.Current.FindResource("DefaultSeparatorStyle");
            var implicitStyle = (Style)Application.Current.FindResource(typeof(Separator));
            Assert.AreEqual(typeof(Separator), defaultStyle.TargetType);
            Assert.AreEqual(typeof(Separator), implicitStyle.TargetType);
            Assert.AreSame(defaultStyle, implicitStyle.BasedOn);

            var setters = defaultStyle.Setters.OfType<Setter>().ToArray();
            AssertDynamicResourceSetter(setters, Control.BorderBrushProperty, "SeparatorBorderBrush");
            AssertBrushSetter(setters, Control.BackgroundProperty, Colors.Transparent);
            AssertSetter(setters, FrameworkElement.MarginProperty, new Thickness(0));
            AssertSetter(setters, Control.BorderThicknessProperty, new Thickness(1, 1, 0, 0));
            AssertSetter(setters, UIElement.FocusableProperty, false);
            AssertSetter(setters, UIElement.SnapsToDevicePixelsProperty, true);
            AssertSetter(setters, Control.OverridesDefaultStyleProperty, true);

            var template = (ControlTemplate)setters.Single(item => item.Property == Control.TemplateProperty).Value;
            Assert.AreEqual(typeof(Separator), template.TargetType);
            Assert.IsInstanceOfType(template.LoadContent(), typeof(Border));
        });
    }

    [TestMethod]
    public void DefaultThumbStyleUsesOfficialWpfFluentTemplateShape()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var defaultStyle = (Style)Application.Current.FindResource("DefaultThumbStyle");
            var implicitStyle = (Style)Application.Current.FindResource(typeof(Thumb));
            Assert.AreEqual(typeof(Thumb), defaultStyle.TargetType);
            Assert.AreEqual(typeof(Thumb), implicitStyle.TargetType);
            Assert.AreSame(defaultStyle, implicitStyle.BasedOn);

            var setters = defaultStyle.Setters.OfType<Setter>().ToArray();
            AssertDynamicResourceSetter(setters, Control.BackgroundProperty, "ThumbBackground");
            AssertDynamicResourceSetter(setters, System.Windows.Controls.Border.CornerRadiusProperty, "ControlCornerRadius");
            AssertSetter(setters, UIElement.SnapsToDevicePixelsProperty, true);
            AssertSetter(setters, Control.OverridesDefaultStyleProperty, true);
            AssertSetter(setters, Control.IsTabStopProperty, false);
            AssertSetter(setters, UIElement.FocusableProperty, false);
            Assert.IsTrue(setters.Any(item => item.Property == Control.TemplateProperty));

            var thumb = new Thumb();
            using var host = new TestWindowHost(thumb, width: 80, height: 40);
            host.UpdateLayout();

            var border = GetTemplateChild<Border>(thumb, "Border");
            Assert.AreSame(thumb.Background, border.Background);
            Assert.AreEqual(((CornerRadius)thumb.GetValue(System.Windows.Controls.Border.CornerRadiusProperty)), border.CornerRadius);

            thumb.IsEnabled = false;
            host.UpdateLayout();

            Assert.AreSame(thumb.TryFindResource("ThumbBackgroundDisabled"), border.Background);
            Assert.IsNull(VisualTreeTestHelper.FindDescendant<VisualStateEx>(thumb));
        });
    }

    [TestMethod]
    public void ToolBarUsesOfficialWpfFluentTemplateShape()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var toolBarStyle = (Style)Application.Current.FindResource(typeof(ToolBar));
            Assert.AreEqual(typeof(ToolBar), toolBarStyle.TargetType);

            var setters = toolBarStyle.Setters.OfType<Setter>().ToArray();
            AssertBrushSetter(setters, Control.BackgroundProperty, Colors.Transparent);
            AssertBrushSetter(setters, Control.BorderBrushProperty, Colors.Transparent);
            AssertSetter(setters, Control.BorderThicknessProperty, new Thickness(0));
            AssertSetter(setters, UIElement.SnapsToDevicePixelsProperty, true);
            AssertSetter(setters, Control.OverridesDefaultStyleProperty, true);
            Assert.IsTrue(setters.Any(item => item.Property == Control.TemplateProperty));

            var toolBar = new ToolBar
            {
                Items =
                {
                    new Button { Content = "One" },
                    new Separator(),
                    new ToggleButton { Content = "Two" }
                }
            };

            using var host = new TestWindowHost(toolBar, width: 320, height: 80);
            host.UpdateLayout();

            Assert.IsNotNull(VisualTreeTestHelper.FindDescendant<ToggleButton>(toolBar));
            Assert.IsNotNull(GetTemplateChild<Thumb>(toolBar, "ToolBarThumb"));
            Assert.IsNotNull(GetTemplateChild<ToolBarPanel>(toolBar, "PART_ToolBarPanel"));
            Assert.IsNotNull(GetTemplateChild<ToolBarOverflowPanel>(toolBar, "PART_ToolBarOverflowPanel", required: false));
        });
    }

    [TestMethod]
    public void ToolBarItemStylesUseOfficialWpfFluentResourceKeys()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            AssertBasedOn("ToolBar button style", ToolBar.ButtonStyleKey, "ToolBarButtonBaseStyle", typeof(Button));
            AssertBasedOn("ToolBar toggle style", ToolBar.ToggleButtonStyleKey, "DefaultToggleButtonStyle", typeof(ToggleButton));
            AssertBasedOn("ToolBar checkbox style", ToolBar.CheckBoxStyleKey, "DefaultCheckBoxStyle", typeof(CheckBox));
            AssertBasedOn("ToolBar radio style", ToolBar.RadioButtonStyleKey, "DefaultRadioButtonStyle", typeof(RadioButton));
            AssertBasedOn("ToolBar combobox style", ToolBar.ComboBoxStyleKey, "DefaultComboBoxStyle", typeof(WpfComboBox));
            AssertBasedOn("ToolBar menu style", ToolBar.MenuStyleKey, "DefaultMenuStyle", typeof(Menu));
            AssertBasedOn("ToolBar separator style", ToolBar.SeparatorStyleKey, "DefaultSeparatorStyle", typeof(Separator));

            var textBoxStyle = (Style)Application.Current.FindResource(ToolBar.TextBoxStyleKey);
            Assert.AreEqual(typeof(TextBox), textBoxStyle.TargetType);
            Assert.IsTrue(textBoxStyle.Setters.OfType<Setter>()
                .Any(item => item.Property == Control.TemplateProperty));

            var trayStyle = (Style)Application.Current.FindResource(typeof(ToolBarTray));
            Assert.AreEqual(typeof(ToolBarTray), trayStyle.TargetType);
            Assert.IsTrue(trayStyle.Setters.OfType<Setter>()
                .Any(item => item.Property == FrameworkElement.MarginProperty && Equals(item.Value, new Thickness(0))));
        });
    }

    [TestMethod]
    public void ToolBarFamilyDeletesModernWpfSpecificTemplateGuesses()
    {
        var repoRoot = FindRepoRoot();
        var text = string.Join(
            "\n",
            new[] { "Separator.xaml", "Thumb.xaml", "ToolBar.xaml" }
                .Select(file => System.IO.File.ReadAllText(System.IO.Path.Combine(repoRoot, "ModernWpf", "Styles", file))));

        Assert.IsFalse(text.Contains("VisualStateEx", System.StringComparison.Ordinal));
        Assert.IsFalse(text.Contains("ContentPresenterEx", System.StringComparison.Ordinal));
        Assert.IsFalse(text.Contains("BorderEx", System.StringComparison.Ordinal));
        Assert.IsFalse(text.Contains("ThemeShadowChrome", System.StringComparison.Ordinal));
        Assert.IsFalse(text.Contains("ToolBar.OverflowMode", System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ThemeDictionariesExposeOfficialToolBarFamilyAliases()
    {
        WpfTestHost.Run(() =>
        {
            foreach (var themeName in new[] { "Light", "Dark" })
            {
                AssertThemeResourceReference(themeName, "SeparatorBorderBrush", "DividerStrokeColorDefaultBrush");
                AssertThemeResourceReference(themeName, "ThumbBackground", "ControlStrongFillColorDefaultBrush");
                AssertThemeResourceReference(themeName, "ThumbBackgroundDisabled", "ControlStrongFillColorDisabledBrush");
                AssertThemeResourceReference(themeName, "MenuBorderColorDefaultBrush", "SurfaceStrokeColorFlyoutBrush");
            }

            AssertThemeResourceReference("HighContrast", "SeparatorBorderBrush", "SystemColorWindowTextColorBrush");
            AssertThemeResourceReference("HighContrast", "ThumbBackground", "SystemColorButtonTextColorBrush");
            AssertThemeResourceReference("HighContrast", "ThumbBackgroundDisabled", "SystemColorButtonFaceColorBrush");
            AssertThemeResourceReference("HighContrast", "MenuBorderColorDefaultBrush", "SystemColorWindowTextColorBrush");
        });
    }

    private static void AssertBasedOn(string description, object key, object basedOnKey, System.Type targetType)
    {
        var style = (Style)Application.Current.FindResource(key);
        var expectedBasedOn = (Style)Application.Current.FindResource(basedOnKey);
        Assert.AreEqual(targetType, style.TargetType, description);
        Assert.IsNotNull(style.BasedOn, description);
        Assert.AreEqual(expectedBasedOn.TargetType, style.BasedOn.TargetType, description);
    }

    private static void AssertSetter(Setter[] setters, DependencyProperty property, object value)
    {
        var setter = setters.Single(item => item.Property == property);
        Assert.AreEqual(value, setter.Value);
    }

    private static void AssertBrushSetter(Setter[] setters, DependencyProperty property, Color color)
    {
        var setter = setters.Single(item => item.Property == property);
        Assert.IsInstanceOfType(setter.Value, typeof(SolidColorBrush));
        Assert.AreEqual(color, ((SolidColorBrush)setter.Value).Color);
    }

    private static void AssertDynamicResourceSetter(Setter[] setters, DependencyProperty property, object resourceKey)
    {
        var setter = setters.Single(item => item.Property == property);
        Assert.IsInstanceOfType(setter.Value, typeof(DynamicResourceExtension));
        var dynamicResource = (DynamicResourceExtension)setter.Value;
        Assert.AreEqual(resourceKey, dynamicResource.ResourceKey);
    }

    private static T GetTemplateChild<T>(Control control, string name, bool required = true)
        where T : DependencyObject
    {
        var child = control.Template.FindName(name, control) as T;
        if (child == null && required)
        {
            throw new AssertFailedException($"Expected {control.GetType().Name} template child '{name}' to be a {typeof(T).Name}.");
        }

        return child!;
    }

    private static void AssertThemeResourceReference(string themeName, string key, object expectedResourceKey)
    {
        var theme = ThemeResources.Current.GetThemeDictionary(themeName);
        Assert.IsTrue(theme.Contains(expectedResourceKey), $"Theme is missing {expectedResourceKey}.");
        Assert.AreSame(theme[expectedResourceKey], theme[key], key);
    }

    private static string FindRepoRoot()
    {
        var directory = new System.IO.DirectoryInfo(System.AppContext.BaseDirectory);

        while (directory != null)
        {
            if (System.IO.File.Exists(System.IO.Path.Combine(directory.FullName, "ModernWpf.sln")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        Assert.Fail("Could not locate ModernWpf.sln from the test output directory.");
        return string.Empty;
    }
}
