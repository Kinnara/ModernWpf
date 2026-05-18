using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Controls;
using ModernWpf.Controls.Primitives;
using ModernWpf.WinUI.TestApp;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.CommonStyles;

[TestClass]
public class RichTextBoxVisualStateTests
{
    [TestMethod]
    public void DefaultRichTextBoxStyleUsesOfficialWpfFluentTriggerShape()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var defaultStyle = (Style)Application.Current.FindResource("DefaultRichTextBoxStyle");
            var implicitRichTextBoxStyle = (Style)Application.Current.FindResource(typeof(RichTextBox));
            Assert.AreEqual(typeof(RichTextBox), defaultStyle.TargetType);
            Assert.AreEqual(typeof(RichTextBox), implicitRichTextBoxStyle.TargetType);
            Assert.AreSame(defaultStyle, implicitRichTextBoxStyle.BasedOn);

            var richTextBox = CreateRichTextBox();
            using var host = new TestWindowHost(richTextBox, width: 260, height: 140);
            host.UpdateLayout();

            AssertOfficialStyleSetters(richTextBox);
            AssertTemplateUsesOfficialWpfPresenterShape(richTextBox);
            AssertOfficialTriggerShape(richTextBox.Template);
            AssertDisabledTriggerAppliesResources(richTextBox);
        });
    }

    private static RichTextBox CreateRichTextBox()
    {
        var richTextBox = new RichTextBox
        {
            Width = 220,
            Height = 96
        };

        richTextBox.Document.Blocks.Add(new Paragraph(new Run("Rich text")));
        return richTextBox;
    }

    private static void AssertOfficialStyleSetters(RichTextBox richTextBox)
    {
        Assert.IsNull(richTextBox.FocusVisualStyle);
        Assert.AreSame(richTextBox.TryFindResource("TextControlForeground"), richTextBox.Foreground);
        Assert.AreSame(richTextBox.TryFindResource("TextControlForeground"), richTextBox.CaretBrush);
        Assert.AreSame(richTextBox.TryFindResource("TextControlBackground"), richTextBox.Background);
        Assert.AreSame(richTextBox.TryFindResource("TextControlBorderBrush"), richTextBox.BorderBrush);
        Assert.AreEqual((Thickness)richTextBox.TryFindResource("TextControlBorderThemeThickness"), richTextBox.BorderThickness);
        Assert.AreEqual(ScrollBarVisibility.Auto, ScrollViewer.GetHorizontalScrollBarVisibility(richTextBox));
        Assert.AreEqual(ScrollBarVisibility.Auto, ScrollViewer.GetVerticalScrollBarVisibility(richTextBox));
        Assert.AreEqual(HorizontalAlignment.Left, richTextBox.HorizontalContentAlignment);
        Assert.AreEqual(VerticalAlignment.Top, richTextBox.VerticalContentAlignment);
        Assert.AreEqual(Cursors.IBeam, richTextBox.Cursor);
        Assert.AreEqual((double)richTextBox.TryFindResource("TextControlThemeMinHeight"), richTextBox.MinHeight);
        Assert.AreEqual((double)richTextBox.TryFindResource("TextControlThemeMinWidth"), richTextBox.MinWidth);
        Assert.AreEqual((Thickness)richTextBox.TryFindResource("TextControlThemePadding"), richTextBox.Padding);
        Assert.IsTrue(richTextBox.AllowDrop);
        Assert.AreEqual(PanningMode.VerticalFirst, ScrollViewer.GetPanningMode(richTextBox));
        Assert.IsFalse(Stylus.GetIsFlicksEnabled(richTextBox));
        Assert.IsTrue(richTextBox.OverridesDefaultStyle);
        Assert.AreSame(richTextBox.TryFindResource("TextControlSelectionHighlightColor"), richTextBox.SelectionBrush);
        Assert.IsTrue(TextContextMenu.GetUsingTextContextMenu(richTextBox));
        Assert.AreSame(richTextBox.TryFindResource("TextControlValidationErrorTemplate"), Validation.GetErrorTemplate(richTextBox));
        Assert.AreEqual(new Thickness(0, 0, 0, 8), (Thickness)richTextBox.TryFindResource("RichEditBoxTopHeaderMargin"));
    }

    private static void AssertTemplateUsesOfficialWpfPresenterShape(RichTextBox richTextBox)
    {
        richTextBox.ApplyTemplate();

        var contentBorder = GetTemplateChild<Border>(richTextBox, "ContentBorder");
        var contentHost = GetTemplateChild<ScrollViewer>(richTextBox, "PART_ContentHost");

        Assert.AreEqual(richTextBox.MinWidth, contentBorder.MinWidth);
        Assert.AreEqual(richTextBox.MinHeight, contentBorder.MinHeight);
        Assert.AreSame(richTextBox.Background, contentBorder.Background);
        Assert.AreSame(richTextBox.BorderBrush, contentBorder.BorderBrush);
        Assert.AreEqual(richTextBox.BorderThickness, contentBorder.BorderThickness);
        Assert.AreEqual(((CornerRadius)richTextBox.GetValue(System.Windows.Controls.Border.CornerRadiusProperty)), contentBorder.CornerRadius);
        Assert.IsTrue(ValidationHelper.GetIsTemplateValidationAdornerSite(contentBorder));

        Assert.AreEqual(richTextBox.BorderThickness, contentHost.Margin);
        Assert.AreEqual(richTextBox.Padding, contentHost.Padding);
        Assert.AreEqual(ScrollViewer.GetHorizontalScrollBarVisibility(richTextBox), contentHost.HorizontalScrollBarVisibility);
        Assert.AreEqual(ScrollViewer.GetVerticalScrollBarVisibility(richTextBox), contentHost.VerticalScrollBarVisibility);
        Assert.AreEqual(ScrollViewer.GetIsDeferredScrollingEnabled(richTextBox), contentHost.IsDeferredScrollingEnabled);
        Assert.AreEqual(richTextBox.IsTabStop, contentHost.IsTabStop);
        Assert.AreSame(richTextBox.Foreground, TextElement.GetForeground(contentHost));

        Assert.IsNull(richTextBox.Template.FindName("HeaderContentPresenter", richTextBox));
        Assert.IsNull(richTextBox.Template.FindName("DescriptionPresenter", richTextBox));
        Assert.IsNull(richTextBox.Template.FindName("PlaceholderTextContentPresenter", richTextBox));
        Assert.IsNull(VisualTreeTestHelper.FindDescendant<ContentPresenterEx>(richTextBox));
        Assert.AreEqual(0, VisualStateManager.GetVisualStateGroups(contentBorder).Count);
    }

    private static void AssertOfficialTriggerShape(ControlTemplate template)
    {
        var triggers = template.Triggers.OfType<Trigger>().ToArray();
        Assert.AreEqual(3, triggers.Length);

        AssertTrigger(triggers, "IsMouseOver", true,
            ("ContentBorder", "Background", "TextControlBackgroundPointerOver"),
            ("ContentBorder", "BorderBrush", "TextControlBorderBrushPointerOver"),
            ("PART_ContentHost", "Foreground", "TextControlForegroundPointerOver"));

        AssertTrigger(triggers, "IsFocused", true,
            ("ContentBorder", "BorderThickness", "TextControlBorderThemeThicknessFocused"),
            ("ContentBorder", "Background", "TextControlBackgroundFocused"),
            ("ContentBorder", "BorderBrush", "TextControlBorderBrushFocused"),
            ("PART_ContentHost", "Foreground", "TextControlForegroundFocused"));

        AssertTrigger(triggers, "IsEnabled", false,
            ("ContentBorder", "Background", "TextControlBackgroundDisabled"),
            ("ContentBorder", "BorderBrush", "TextControlBorderBrushDisabled"),
            ("PART_ContentHost", "Foreground", "TextControlForegroundDisabled"));
    }

    private static void AssertTrigger(
        Trigger[] triggers,
        string propertyName,
        object value,
        params (string TargetName, string PropertyName, string ResourceKey)[] expectedSetters)
    {
        var trigger = triggers.Single(item => item.Property.Name == propertyName && Equals(item.Value, value));
        var setters = trigger.Setters.OfType<Setter>().ToArray();

        Assert.AreEqual(expectedSetters.Length, setters.Length);

        foreach (var expectedSetter in expectedSetters)
        {
            AssertSetter(setters, expectedSetter.TargetName, expectedSetter.PropertyName, expectedSetter.ResourceKey);
        }
    }

    private static void AssertSetter(Setter[] setters, string targetName, string propertyName, string resourceKey)
    {
        var setter = setters.Single(item =>
            item.TargetName == targetName &&
            item.Property.Name == propertyName);

        Assert.IsInstanceOfType(setter.Value, typeof(DynamicResourceExtension));
        var resource = (DynamicResourceExtension)setter.Value;
        Assert.AreEqual(resourceKey, resource.ResourceKey);
    }

    private static void AssertDisabledTriggerAppliesResources(RichTextBox richTextBox)
    {
        var contentBorder = GetTemplateChild<Border>(richTextBox, "ContentBorder");
        var contentHost = GetTemplateChild<ScrollViewer>(richTextBox, "PART_ContentHost");

        richTextBox.IsEnabled = false;
        richTextBox.UpdateLayout();

        Assert.AreSame(contentBorder.TryFindResource("TextControlBackgroundDisabled"), contentBorder.Background);
        Assert.AreSame(contentBorder.TryFindResource("TextControlBorderBrushDisabled"), contentBorder.BorderBrush);
        Assert.AreSame(contentHost.TryFindResource("TextControlForegroundDisabled"), contentHost.Foreground);
    }

    private static T GetTemplateChild<T>(Control control, string name)
        where T : DependencyObject
    {
        return control.Template.FindName(name, control) as T
            ?? throw new AssertFailedException($"Expected {control.GetType().Name} template child '{name}' to be a {typeof(T).Name}.");
    }
}
