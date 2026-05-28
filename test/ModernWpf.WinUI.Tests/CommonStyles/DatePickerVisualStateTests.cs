using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Effects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Controls;
using ModernWpf.Controls.Primitives;
using ModernWpf.WinUI.TestApp;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.CommonStyles;

[TestClass]
public class DatePickerVisualStateTests
{
    [TestMethod]
    public void DefaultDatePickerStyleUsesOfficialWpfFluentTriggerShape()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var defaultStyle = (Style)Application.Current.FindResource("DefaultDatePickerStyle");
            var textBoxStyle = (Style)Application.Current.FindResource("DefaultDatePickerTextBoxStyle");
            var implicitDatePickerStyle = (Style)Application.Current.FindResource(typeof(DatePicker));
            Assert.AreEqual(typeof(DatePicker), defaultStyle.TargetType);
            Assert.AreEqual(typeof(DatePickerTextBox), textBoxStyle.TargetType);
            Assert.AreEqual(typeof(DatePicker), implicitDatePickerStyle.TargetType);
            Assert.AreSame(defaultStyle, implicitDatePickerStyle.BasedOn);
            Assert.IsInstanceOfType(FindSetter(defaultStyle, DatePicker.CalendarStyleProperty).Value, typeof(Style));
            AssertDynamicResourceSetter(textBoxStyle, TextBoxBase.CaretBrushProperty, "DatePickerTextBoxCaretBrush");

            var datePicker = new DatePicker
            {
                Style = implicitDatePickerStyle
            };
            datePicker.SetValue(System.Windows.Controls.Border.CornerRadiusProperty, new CornerRadius(6));

            using var host = new TestWindowHost(datePicker, width: 240, height: 120);
            host.UpdateLayout();

            var root = FindTemplatePart<Grid>(datePicker, "PART_Root");
            var border = FindTemplatePart<Border>(datePicker, "BorderElement");
            var textBox = FindTemplatePart<DatePickerTextBox>(datePicker, "PART_TextBox");
            var button = FindTemplatePart<Button>(datePicker, "PART_Button");
            var buttonBorder = FindTemplatePart<Border>(button, "ButtonLayoutBorder");

            Assert.AreSame(datePicker.TryFindResource("DefaultControlContextMenu"), datePicker.ContextMenu);
            Assert.AreSame(datePicker.TryFindResource("DatePickerForeground"), datePicker.Foreground);
            Assert.AreSame(datePicker.TryFindResource("DatePickerBackground"), datePicker.Background);
            Assert.AreSame(datePicker.TryFindResource("TextControlElevationBorderBrush"), datePicker.BorderBrush);
            Assert.AreSame(datePicker.TryFindResource("DatePickerTextBoxCaretBrush"), textBox.CaretBrush);
            Assert.IsNotNull(datePicker.ContextMenu);
            Assert.IsNotNull(datePicker.CalendarStyle);
            Assert.AreEqual(new Thickness(1), datePicker.BorderThickness);
            Assert.AreEqual(datePicker.TryFindResource("TextControlThemeMinHeight"), datePicker.MinHeight);
            Assert.AreEqual(((CornerRadius)datePicker.GetValue(System.Windows.Controls.Border.CornerRadiusProperty)), border.CornerRadius);
            Assert.AreEqual(typeof(DatePickerTextBox), textBox.GetType());
            Assert.AreEqual((CornerRadius)datePicker.TryFindResource("ControlCornerRadius"), ((CornerRadius)button.GetValue(System.Windows.Controls.Border.CornerRadiusProperty)));
            Assert.AreEqual(((CornerRadius)button.GetValue(System.Windows.Controls.Border.CornerRadiusProperty)), buttonBorder.CornerRadius);
            Assert.AreEqual(0, VisualStateManager.GetVisualStateGroups(root).Count);
            Assert.IsNull(FindVisualChild<ContentPresenterEx>(datePicker));

            var replacementCaretBrush = Brushes.Magenta;
            datePicker.Resources["DatePickerTextBoxCaretBrush"] = replacementCaretBrush;
            host.UpdateLayout();
            Assert.AreSame(replacementCaretBrush, textBox.CaretBrush);
        });
    }

    [TestMethod]
    public void DatePickerCalendarStyleUsesOfficialPopupCalendarChrome()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var datePicker = new DatePicker
            {
                Style = (Style)Application.Current.FindResource(typeof(DatePicker))
            };
            var calendar = new Calendar();
            var panel = new StackPanel();
            panel.Children.Add(datePicker);
            panel.Children.Add(calendar);

            using var host = new TestWindowHost(panel, width: 360, height: 420);
            host.UpdateLayout();

            calendar.Style = datePicker.CalendarStyle
                ?? throw new AssertFailedException("Expected DatePicker CalendarStyle.");
            host.UpdateLayout();

            var root = FindVisualChild<Border>(calendar)
                ?? throw new AssertFailedException("Expected official WPF Fluent Calendar template to use Border chrome.");

            AssertDynamicResourceSetter(calendar.Style, Control.BackgroundProperty, "DatePickerPopupBackground");
            Assert.IsNotNull(calendar.Background);
            Assert.AreEqual(new Thickness(10), calendar.Margin);
            Assert.AreEqual((CornerRadius)calendar.TryFindResource("OverlayCornerRadius"), ((CornerRadius)calendar.GetValue(System.Windows.Controls.Border.CornerRadiusProperty)));
            Assert.AreEqual(((CornerRadius)calendar.GetValue(System.Windows.Controls.Border.CornerRadiusProperty)), root.CornerRadius);
            Assert.IsInstanceOfType(calendar.Effect, typeof(DropShadowEffect));
            Assert.IsNull(FindVisualChild<ContentPresenterEx>(calendar));
        });
    }

    private static T FindTemplatePart<T>(Control control, string name)
        where T : DependencyObject
    {
        var part = control.Template.FindName(name, control) as T;
        if (part == null)
        {
            throw new AssertFailedException($"Expected {control.GetType().Name} template part '{name}'.");
        }

        return part;
    }

    private static void AssertDynamicResourceSetter(Style style, DependencyProperty property, object resourceKey)
    {
        foreach (var setterBase in style.Setters)
        {
            if (setterBase is Setter setter && setter.Property == property)
            {
                if (setter.Value is not DynamicResourceExtension dynamicResource)
                {
                    throw new AssertFailedException($"Expected {property.Name} setter to use DynamicResourceExtension.");
                }

                Assert.AreEqual(resourceKey, dynamicResource.ResourceKey);
                return;
            }
        }

        throw new AssertFailedException($"Expected style setter for {property.Name}.");
    }

    private static Setter FindSetter(Style style, DependencyProperty property)
    {
        foreach (var setterBase in style.Setters)
        {
            if (setterBase is Setter setter && setter.Property == property)
            {
                return setter;
            }
        }

        throw new AssertFailedException($"Expected style setter for {property.Name}.");
    }

    private static T? FindVisualChild<T>(DependencyObject root)
        where T : DependencyObject
    {
        for (var i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(root); i++)
        {
            var child = System.Windows.Media.VisualTreeHelper.GetChild(root, i);
            if (child is T match)
            {
                return match;
            }

            var descendant = FindVisualChild<T>(child);
            if (descendant != null)
            {
                return descendant;
            }
        }

        return null;
    }
}
