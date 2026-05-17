using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Controls;
using ModernWpf.Controls.Primitives;
using ModernWpf.WinUI.TestApp;
using ModernWpf.WinUI.TestInfra;
using WpfDatePickerTextBox = System.Windows.Controls.Primitives.DatePickerTextBox;

namespace ModernWpf.WinUI.Tests.CommonStyles;

[TestClass]
public class DatePickerVisualStateTests
{
    [TestMethod]
    public void CalendarDatePickerCommonStatesUseSourceVisualStateSetters()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var datePicker = new DatePicker();

            using var host = new TestWindowHost(datePicker, width: 240, height: 120);
            host.UpdateLayout();

            var root = FindTemplatePart<FrameworkElement>(datePicker, "Root");
            var background = FindTemplatePart<Border>(datePicker, "Background");
            var textBox = FindTemplatePart<WpfDatePickerTextBox>(datePicker, "PART_TextBox");
            var button = FindTemplatePart<Button>(datePicker, "PART_Button");

            AssertStateSetter(root, "CommonStates", "PointerOver", "Background.Background");
            AssertStateSetter(root, "CommonStates", "PointerOver", "Background.BorderBrush");
            AssertStateSetter(root, "CommonStates", "PointerOver", "PART_TextBox.Foreground");
            AssertStateSetter(root, "CommonStates", "PointerOver", "PART_Button.Foreground");
            AssertStateSetter(root, "CommonStates", "Pressed", "Background.Background");
            AssertStateSetter(root, "CommonStates", "Disabled", "HeaderContentPresenter.Foreground");

            Assert.IsTrue(VisualStateManager.GoToState(datePicker, "PointerOver", false));
            Assert.AreEqual("PointerOver", GetCurrentStateName(root, "CommonStates"));
            Assert.AreSame(datePicker.TryFindResource("CalendarDatePickerBackgroundPointerOver"), background.Background);
            Assert.AreSame(datePicker.TryFindResource("CalendarDatePickerBorderBrushPointerOver"), background.BorderBrush);
            Assert.AreSame(datePicker.TryFindResource("CalendarDatePickerTextForegroundPointerOver"), textBox.Foreground);
            Assert.AreSame(datePicker.TryFindResource("CalendarDatePickerCalendarGlyphForegroundPointerOver"), button.Foreground);

            Assert.IsTrue(VisualStateManager.GoToState(datePicker, "Pressed", false));
            Assert.AreEqual("Pressed", GetCurrentStateName(root, "CommonStates"));
            Assert.AreSame(datePicker.TryFindResource("CalendarDatePickerBackgroundPressed"), background.Background);
            Assert.AreSame(datePicker.TryFindResource("CalendarDatePickerBorderBrushPressed"), background.BorderBrush);
            Assert.AreSame(datePicker.TryFindResource("CalendarDatePickerTextForegroundPressed"), textBox.Foreground);
            Assert.AreSame(datePicker.TryFindResource("CalendarDatePickerCalendarGlyphForegroundPressed"), button.Foreground);
        });
    }

    [TestMethod]
    public void CalendarDatePickerSelectionStatesUseSourceVisualStateSetters()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var datePicker = new DatePicker();

            using var host = new TestWindowHost(datePicker, width: 240, height: 120);
            host.UpdateLayout();

            var root = FindTemplatePart<FrameworkElement>(datePicker, "Root");
            var textBox = FindTemplatePart<WpfDatePickerTextBox>(datePicker, "PART_TextBox");
            var button = FindTemplatePart<Button>(datePicker, "PART_Button");

            AssertStateSetter(root, "SelectionStates", "Selected", "PART_TextBox.Foreground");
            AssertStateSetter(root, "SelectionStates", "Selected", "PART_Button.Foreground");
            Assert.AreEqual("Unselected", GetCurrentStateName(root, "SelectionStates"));
            Assert.AreSame(datePicker.TryFindResource("CalendarDatePickerCalendarGlyphForeground"), button.Foreground);

            datePicker.SelectedDate = new DateTime(2026, 5, 16);
            host.UpdateLayout();

            Assert.AreEqual("Selected", GetCurrentStateName(root, "SelectionStates"));
            Assert.AreSame(datePicker.TryFindResource("CalendarDatePickerTextForegroundSelected"), textBox.Foreground);
            Assert.AreSame(datePicker.TryFindResource("CalendarDatePickerTextForegroundSelected"), button.Foreground);
        });
    }

    [TestMethod]
    public void HeaderStatesUseVisualStateSetters()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var datePicker = new DatePicker();
            ControlHelper.SetHeader(datePicker, "Date");
            DatePickerHelper.SetHeaderPlacement(datePicker, DatePickerHeaderPlacement.Left);

            using var host = new TestWindowHost(datePicker, width: 300, height: 120);
            host.UpdateLayout();

            var root = FindTemplatePart<FrameworkElement>(datePicker, "Root");
            var header = FindTemplatePart<ContentPresenterEx>(datePicker, "HeaderContentPresenter");

            AssertStateSetter(root, "HeaderStates", "LeftHeader", "HeaderContentPresenter.(Grid.Row)");
            AssertStateSetter(root, "HeaderStates", "LeftHeader", "HeaderContentPresenter.(Grid.Column)");
            AssertStateSetter(root, "HeaderStates", "LeftHeader", "HeaderContentPresenter.(Grid.ColumnSpan)");
            AssertStateSetter(root, "HeaderStates", "LeftHeader", "HeaderContentPresenter.Margin");
            AssertStateSetter(root, "HeaderStates", "LeftHeader", "HeaderContentPresenter.MaxWidth");
            Assert.AreEqual("LeftHeader", GetCurrentStateName(root, "HeaderStates"));
            Assert.AreEqual(1, Grid.GetRow(header));
            Assert.AreEqual(0, Grid.GetColumn(header));
            Assert.AreEqual(1, Grid.GetColumnSpan(header));
            Assert.AreEqual(new Thickness(0, 0, 8, 0), header.Margin);
            Assert.AreEqual(100, header.MaxWidth);

            DatePickerHelper.SetHeaderPlacement(datePicker, DatePickerHeaderPlacement.Top);
            host.UpdateLayout();

            Assert.AreEqual("TopHeader", GetCurrentStateName(root, "HeaderStates"));
            Assert.AreEqual(0, Grid.GetRow(header));
            Assert.AreEqual(1, Grid.GetColumn(header));
            Assert.AreEqual(2, Grid.GetColumnSpan(header));
            Assert.AreEqual(new Thickness(0, 0, 0, 8), header.Margin);
            Assert.AreEqual(double.PositiveInfinity, header.MaxWidth);
        });
    }

    private static void AssertStateSetter(
        FrameworkElement stateGroupsRoot,
        string groupName,
        string stateName,
        string setterTarget)
    {
        var group = VisualStateManager.GetVisualStateGroups(stateGroupsRoot)
            .OfType<VisualStateGroup>()
            .Single(item => item.Name == groupName);
        var state = group.States
            .Cast<VisualState>()
            .Single(item => item.Name == stateName);

        Assert.IsInstanceOfType(state, typeof(VisualStateEx));

        var stateEx = (VisualStateEx)state;
        Assert.IsTrue(
            stateEx.Setters.Any(setter => setter.Target == setterTarget),
            $"{groupName}.{stateName} should set {setterTarget}.");
    }

    private static string GetCurrentStateName(FrameworkElement stateGroupsRoot, string groupName)
    {
        var group = VisualStateManager.GetVisualStateGroups(stateGroupsRoot)
            .OfType<VisualStateGroup>()
            .Single(item => item.Name == groupName);
        Assert.IsNotNull(group.CurrentState);
        return group.CurrentState.Name;
    }

    private static T FindTemplatePart<T>(Control control, string name)
        where T : DependencyObject
    {
        var part = control.Template.FindName(name, control) as T;
        if (part == null)
        {
            throw new AssertFailedException($"Expected DatePicker template part '{name}'.");
        }

        return part;
    }
}
