using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
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
    public void HasDateStatesUseVisualStateSetters()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var datePicker = new DatePicker();

            using var host = new TestWindowHost(datePicker, width: 240, height: 120);
            host.UpdateLayout();

            var root = FindTemplatePart<FrameworkElement>(datePicker, "Root");
            var button = FindTemplatePart<Button>(datePicker, "PART_Button");

            AssertStateSetter(root, "HasDateStates", "HasNoDate", "PART_Button.Foreground");
            Assert.AreEqual("HasNoDate", GetCurrentStateName(root, "HasDateStates"));
            Assert.AreSame(datePicker.TryFindResource("CalendarDatePickerCalendarGlyphForeground"), button.Foreground);

            datePicker.SelectedDate = new DateTime(2026, 5, 16);
            host.UpdateLayout();

            Assert.AreEqual("HasDate", GetCurrentStateName(root, "HasDateStates"));
            Assert.AreSame(datePicker.Foreground, button.Foreground);
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
