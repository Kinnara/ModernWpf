using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Controls;
using ModernWpf.Controls.Primitives;
using ModernWpf.WinUI.TestApp;
using ModernWpf.WinUI.TestInfra;

using WpfComboBox = System.Windows.Controls.ComboBox;
using Rectangle = System.Windows.Shapes.Rectangle;

namespace ModernWpf.WinUI.Tests.ComboBox;

[TestClass]
public class ComboBoxApiTests
{
    [TestMethod]
    public void VerifyComboBoxDefaultStyleMatchesOfficialWpfFluent()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var comboBox = CreateComboBox();
            comboBox.SelectedIndex = 0;
            comboBox.SetValue(System.Windows.Controls.Border.CornerRadiusProperty, new CornerRadius(6));

            using var host = new TestWindowHost(comboBox);
            host.UpdateLayout();

            var implicitStyle = comboBox.Style;
            var defaultStyle = AssertStyle(comboBox, "DefaultComboBoxStyle");
            Assert.AreSame(defaultStyle, implicitStyle.BasedOn);
            Assert.AreEqual(HorizontalAlignment.Stretch, comboBox.HorizontalAlignment);
            Assert.AreEqual(VerticalAlignment.Center, comboBox.VerticalAlignment);
            Assert.AreEqual(comboBox.TryFindResource("TextControlThemeMinHeight"), comboBox.MinHeight);
            Assert.AreEqual(comboBox.TryFindResource("TextControlThemeMinWidth"), comboBox.MinWidth);
            Assert.AreEqual(comboBox.TryFindResource("ComboBoxPadding"), comboBox.Padding);
            Assert.AreSame(comboBox.TryFindResource("DefaultControlContextMenu"), comboBox.ContextMenu);
            Assert.IsNotNull(comboBox.FocusVisualStyle);

            var contentBorder = FindTemplateChild<Border>(comboBox, "ContentBorder");
            Assert.AreEqual(new CornerRadius(6), contentBorder.CornerRadius);

            var contentPresenter = FindTemplateChild<ContentPresenter>(comboBox, "PART_ContentPresenter");
            Assert.AreEqual(typeof(ContentPresenter), contentPresenter.GetType());
            Assert.AreEqual("Item 1", contentPresenter.Content);

            var toggleButton = FindTemplateChild<ToggleButton>(comboBox, "ToggleButton");
            Assert.IsFalse(toggleButton.Focusable);
            var chevronIcon = FindTemplateChild<TextBlock>(comboBox, "ChevronIcon");
            Assert.AreEqual(comboBox.TryFindResource("ComboBoxChevronDownGlyph"), chevronIcon.Text);
            Assert.AreEqual(comboBox.TryFindResource("ComboBoxChevronSize"), chevronIcon.FontSize);
            Assert.IsNotNull(FindTemplateChild<Popup>(comboBox, "PART_Popup"));
            Assert.IsTrue(comboBox.Template.Triggers.Count > 0);

            Assert.IsNull(VisualTreeTestHelper.FindDescendant<ContentPresenterEx>(comboBox));
            Assert.IsNull(VisualTreeTestHelper.FindDescendant<FontIconFallback>(comboBox));
        });
    }

    [TestMethod]
    public void VerifyComboBoxItemTemplateUsesOfficialWpfPresenter()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var item = new ComboBoxItem
            {
                Content = "Item content"
            };

            using var host = new TestWindowHost(item);
            host.UpdateLayout();

            var implicitStyle = item.Style;
            var defaultStyle = AssertStyle(item, "DefaultComboBoxItemStyle");
            Assert.AreSame(defaultStyle, implicitStyle.BasedOn);

            var presenter = FindTemplateChild<ContentPresenter>(item, "PART_ContentPresenter");
            Assert.AreEqual(typeof(ContentPresenter), presenter.GetType());
            Assert.AreEqual("Item content", presenter.Content);

            var activeRectangle = FindTemplateChild<Rectangle>(item, "ActiveRectangle");
            Assert.AreEqual(Visibility.Collapsed, activeRectangle.Visibility);

            item.IsSelected = true;
            host.UpdateLayout();
            Assert.AreEqual(Visibility.Visible, activeRectangle.Visibility);

            Assert.IsNull(VisualTreeTestHelper.FindDescendant<ContentPresenterEx>(item));
        });
    }

    [TestMethod]
    public void VerifyEditableComboBoxUsesOfficialTextBoxTemplate()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var comboBox = CreateComboBox();
            comboBox.IsEditable = true;
            comboBox.SetValue(System.Windows.Controls.Border.CornerRadiusProperty, new CornerRadius(4));

            using var host = new TestWindowHost(comboBox);
            host.UpdateLayout();

            var editableTextBox = FindTemplateChild<TextBox>(comboBox, "PART_EditableTextBox");
            Assert.AreSame(comboBox.TryFindResource("DefaultComboBoxTextBoxStyle"), editableTextBox.Style);
            Assert.AreEqual(new Thickness(11, 5, 38, 6), editableTextBox.Padding);
            Assert.AreEqual(new CornerRadius(4), ((CornerRadius)editableTextBox.GetValue(System.Windows.Controls.Border.CornerRadiusProperty)));

            editableTextBox.ApplyTemplate();
            var editableContentBorder = FindTemplateChild<Border>(editableTextBox, "ContentBorder");
            Assert.AreEqual(new CornerRadius(4), editableContentBorder.CornerRadius);
            Assert.IsNotNull(FindTemplateChild<ScrollViewer>(editableTextBox, "PART_ContentHost"));

            Assert.IsNotNull(FindTemplateChild<Border>(comboBox, "DropDownOverlay"));
            Assert.IsNotNull(FindTemplateChild<ToggleButton>(comboBox, "ToggleButton"));
            Assert.IsNotNull(FindTemplateChild<TextBlock>(comboBox, "ChevronIcon"));
            Assert.IsNotNull(FindTemplateChild<Popup>(comboBox, "PART_Popup"));
            Assert.IsTrue(comboBox.Template.Triggers.Count > 0);

            Assert.IsNull(VisualTreeTestHelper.FindDescendant<ContentPresenterEx>(comboBox));
            Assert.IsNull(VisualTreeTestHelper.FindDescendant<FontIconFallback>(comboBox));
        });
    }

    [TestMethod]
    public void OfficialComboBoxStyleDeletesWinUILayer()
    {
        var repoRoot = FindRepoRoot();
        var text = File.ReadAllText(Path.Combine(repoRoot, "ModernWpf", "Styles", "ComboBox.xaml"));

        Assert.IsFalse(text.Contains("ComboBoxHelper", StringComparison.Ordinal));
        Assert.IsFalse(text.Contains("VisualStateEx", StringComparison.Ordinal));
        Assert.IsFalse(text.Contains("ContentPresenterEx", StringComparison.Ordinal));
        Assert.IsFalse(text.Contains("FontIconFallback", StringComparison.Ordinal));
        Assert.IsFalse(text.Contains("ControlHelper.CornerRadius", StringComparison.Ordinal));
        Assert.IsFalse(text.Contains("System.Runtime", StringComparison.Ordinal));
        StringAssert.Contains(text, "DefaultComboBoxTextBoxStyle");
        StringAssert.Contains(text, "DefaultComboBoxToggleButtonStyle");
    }

    [TestMethod]
    public void ComboBoxThemeResourcesRemainMapped()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            AssertThemeResourceReference("Light", "ComboBoxDropDownBackground", "AcrylicInAppFillColorDefaultBrush");
            AssertThemeResourceReference("Dark", "ComboBoxDropDownBackground", "AcrylicInAppFillColorDefaultBrush");
            AssertThemeResourceReference("Light", "ComboBoxDropDownBorderBrush", "SurfaceStrokeColorFlyoutBrush");
            AssertThemeResourceReference("Dark", "ComboBoxDropDownBorderBrush", "SurfaceStrokeColorFlyoutBrush");
            AssertThemeResourceReference("HighContrast", "ComboBoxDropDownBackground", "SystemControlBackgroundChromeMediumLowBrush");
            AssertThemeResourceReference("HighContrast", "ComboBoxDropDownBorderBrush", "SystemControlForegroundChromeHighBrush");
        });
    }

    [TestMethod]
    public void DataGridComboBoxAdapterStylesRemainResolvable()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var comboBox = new WpfComboBox();
            var defaultStyle = AssertStyle(comboBox, "DefaultComboBoxStyle");
            var editingStyle = AssertStyle(comboBox, "DataGridComboBoxStyle");
            var elementStyle = AssertStyle(comboBox, "DataGridTextBlockComboBoxStyle");

            Assert.AreSame(defaultStyle, editingStyle.BasedOn);
            Assert.AreSame(defaultStyle, elementStyle.BasedOn);

            var cornerRadiusSetter = editingStyle.Setters
                .OfType<Setter>()
                .Single(setter => setter.Property == System.Windows.Controls.Border.CornerRadiusProperty);
            Assert.AreEqual(new CornerRadius(), cornerRadiusSetter.Value);

            var templateSetter = elementStyle.Setters
                .OfType<Setter>()
                .Single(setter => setter.Property == Control.TemplateProperty);
            Assert.IsInstanceOfType(templateSetter.Value, typeof(ControlTemplate));
        });
    }

    private static WpfComboBox CreateComboBox()
    {
        var comboBox = new WpfComboBox();
        comboBox.Items.Add("Item 1");
        comboBox.Items.Add("Item 2");
        comboBox.Items.Add("Item 3");
        return comboBox;
    }

    private static T FindTemplateChild<T>(Control control, string name)
        where T : FrameworkElement
    {
        control.ApplyTemplate();
        return control.Template?.FindName(name, control) as T
            ?? throw new AssertFailedException($"Expected template child '{name}' on {control.GetType().Name}.");
    }

    private static Style AssertStyle(FrameworkElement element, string resourceKey)
    {
        return element.TryFindResource(resourceKey) as Style
            ?? throw new AssertFailedException($"Expected style resource '{resourceKey}'.");
    }

    private static void AssertThemeResourceReference(string themeName, string resourceKey, string expectedResourceKey)
    {
        var themeDictionary = ThemeResources.Current.GetThemeDictionary(themeName);
        Assert.IsTrue(themeDictionary.Contains(resourceKey), $"{themeName} is missing {resourceKey}.");
        Assert.IsTrue(themeDictionary.Contains(expectedResourceKey), $"{themeName} is missing {expectedResourceKey}.");
        Assert.AreSame(themeDictionary[expectedResourceKey], themeDictionary[resourceKey], $"{themeName}:{resourceKey}");
    }

    private static string FindRepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory != null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "ModernWpf.sln")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        Assert.Fail("Could not locate ModernWpf.sln from the test output directory.");
        return string.Empty;
    }
}
