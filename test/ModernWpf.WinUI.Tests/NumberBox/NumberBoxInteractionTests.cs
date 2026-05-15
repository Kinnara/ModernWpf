using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Controls;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.NumberBox;

[TestClass]
public class NumberBoxInteractionTests
{
    [TestMethod]
    public void UpDownTest()
    {
        WpfTestHost.Run(() =>
        {
            var numberBox = CreateNumberBox();

            using var host = new TestWindowHost(numberBox, width: 320, height: 180);

            var upButton = FindTemplatePart<RepeatButton>(numberBox, "UpSpinButton");
            var downButton = FindTemplatePart<RepeatButton>(numberBox, "DownSpinButton");

            Click(upButton);
            Assert.AreEqual(1.0, numberBox.Value);

            Click(downButton);
            Assert.AreEqual(0.0, numberBox.Value);

            numberBox.SmallChange = 5;
            Click(upButton);
            Assert.AreEqual(5.0, numberBox.Value);

            numberBox.Value = 100;
            numberBox.IsWrapEnabled = true;
            host.UpdateLayout();

            Click(upButton);
            Assert.AreEqual(0.0, numberBox.Value);

            Click(downButton);
            Assert.AreEqual(100.0, numberBox.Value);

            var inputBox = FindTemplatePart<TextBox>(numberBox, "InputBox");
            inputBox.Text = "54";
            Click(upButton);
            Assert.AreEqual(59.0, numberBox.Value);
        });
    }

    [TestMethod]
    public void UpDownEnabledTest()
    {
        WpfTestHost.Run(() =>
        {
            var numberBox = CreateNumberBox();
            numberBox.Value = double.NaN;

            using var host = new TestWindowHost(numberBox, width: 320, height: 180);

            var upButton = FindTemplatePart<RepeatButton>(numberBox, "UpSpinButton");
            var downButton = FindTemplatePart<RepeatButton>(numberBox, "DownSpinButton");

            Assert.IsFalse(upButton.IsEnabled);
            Assert.IsFalse(downButton.IsEnabled);

            numberBox.Value = 0;
            host.UpdateLayout();
            Assert.IsTrue(upButton.IsEnabled);
            Assert.IsFalse(downButton.IsEnabled);

            numberBox.Value = 100;
            host.UpdateLayout();
            Assert.IsFalse(upButton.IsEnabled);
            Assert.IsTrue(downButton.IsEnabled);

            numberBox.IsWrapEnabled = true;
            host.UpdateLayout();
            Assert.IsTrue(upButton.IsEnabled);
            Assert.IsTrue(downButton.IsEnabled);

            numberBox.IsWrapEnabled = false;
            numberBox.Maximum = 200;
            host.UpdateLayout();
            Assert.IsTrue(upButton.IsEnabled);
            Assert.IsTrue(downButton.IsEnabled);

            numberBox.ValidationMode = NumberBoxValidationMode.Disabled;
            numberBox.Value = 0;
            host.UpdateLayout();
            Assert.IsTrue(upButton.IsEnabled);
            Assert.IsTrue(downButton.IsEnabled);

            numberBox.Value = double.NaN;
            host.UpdateLayout();
            Assert.IsFalse(upButton.IsEnabled);
            Assert.IsFalse(downButton.IsEnabled);
        });
    }

    [TestMethod]
    public void ValueTextTest()
    {
        WpfTestHost.Run(() =>
        {
            var numberBox = CreateNumberBox();

            using var host = new TestWindowHost(numberBox, width: 320, height: 180);

            var inputBox = FindTemplatePart<TextBox>(numberBox, "InputBox");
            var provider = GetRangeValueProvider(numberBox);

            provider.SetValue(10);
            host.UpdateLayout();
            Assert.AreEqual("10", inputBox.Text);

            numberBox.Text = "15";
            host.UpdateLayout();
            Assert.AreEqual(15.0, provider.Value);
            Assert.AreEqual("15", inputBox.Text);

            numberBox.Text = " 15 ";
            host.UpdateLayout();
            Assert.AreEqual("15", inputBox.Text);
        });
    }

    [TestMethod]
    public void MinMaxTest()
    {
        WpfTestHost.Run(() =>
        {
            var numberBox = CreateNumberBox();

            using var host = new TestWindowHost(numberBox, width: 320, height: 180);

            var provider = GetRangeValueProvider(numberBox);
            Assert.AreEqual(0.0, provider.Minimum);
            Assert.AreEqual(100.0, provider.Maximum);

            provider.SetValue(10);
            provider.SetValue(-1);
            host.UpdateLayout();
            Assert.AreEqual(0.0, provider.Value);

            numberBox.Text = "123";
            host.UpdateLayout();
            Assert.AreEqual(100.0, provider.Value);

            numberBox.Maximum = 90;
            host.UpdateLayout();
            Assert.AreEqual(90.0, provider.Value);

            numberBox.Minimum = 200;
            host.UpdateLayout();
            Assert.AreEqual(200.0, provider.Minimum);
            Assert.AreEqual(200.0, provider.Maximum);
            Assert.AreEqual(200.0, provider.Value);

            numberBox.Maximum = 150;
            host.UpdateLayout();
            Assert.AreEqual(150.0, provider.Minimum);
            Assert.AreEqual(150.0, provider.Maximum);
            Assert.AreEqual(150.0, provider.Value);
        });
    }

    [TestMethod]
    public void ValidationDisabledTest()
    {
        WpfTestHost.Run(() =>
        {
            var numberBox = CreateNumberBox();
            numberBox.ValidationMode = NumberBoxValidationMode.Disabled;

            using var host = new TestWindowHost(numberBox, width: 320, height: 180);

            var provider = GetRangeValueProvider(numberBox);

            provider.SetValue(-10);
            host.UpdateLayout();
            Assert.AreEqual(-10.0, provider.Value);

            provider.SetValue(150);
            host.UpdateLayout();
            Assert.AreEqual(150.0, provider.Value);
        });
    }

    [TestMethod]
    public void ScrollTest()
    {
        WpfTestHost.Run(() =>
        {
            var numberBox = new ModernWpf.Controls.NumberBox
            {
                Width = 180,
                Value = 0,
                SmallChange = 1,
                ValidationMode = NumberBoxValidationMode.Disabled
            };
            var focusTarget = new Button { Content = "Focus target" };
            var root = new StackPanel();
            root.Children.Add(focusTarget);
            root.Children.Add(numberBox);

            using var host = new TestWindowHost(root, width: 320, height: 220);

            var inputBox = FindTemplatePart<TextBox>(numberBox, "InputBox");

            focusTarget.Focus();
            WpfTestHost.DoEvents();
            RaiseMouseWheel(inputBox, 120);
            Assert.AreEqual(0.0, numberBox.Value);

            inputBox.Focus();
            WpfTestHost.DoEvents();
            Assert.IsTrue(inputBox.IsFocused, "NumberBox input should accept focus before mouse wheel stepping.");

            RaiseMouseWheel(inputBox, 120);
            RaiseMouseWheel(inputBox, 120);
            Assert.AreEqual(2.0, numberBox.Value);

            RaiseMouseWheel(inputBox, -120);
            RaiseMouseWheel(inputBox, -120);
            RaiseMouseWheel(inputBox, -120);
            Assert.AreEqual(-1.0, numberBox.Value);
        });
    }

    [TestMethod]
    public void CustomFormatterTest()
    {
        WpfTestHost.Run(() =>
        {
            var numberBox = CreateNumberBox();
            numberBox.Value = 8;

            using var host = new TestWindowHost(numberBox, width: 320, height: 180);

            var inputBox = FindTemplatePart<TextBox>(numberBox, "InputBox");
            Assert.AreEqual("8", inputBox.Text);

            numberBox.NumberFormatter = new CommaDecimalFormatter();
            host.UpdateLayout();
            Assert.AreEqual("8,00", inputBox.Text);

            EnterText(inputBox, "7,45");
            Assert.AreEqual(7.45, numberBox.Value);
            Assert.AreEqual("7,45", inputBox.Text);
        });
    }

    [TestMethod]
    public void ValueChangedTest()
    {
        WpfTestHost.Run(() =>
        {
            var numberBox = CreateNumberBox();
            numberBox.Value = double.NaN;

            var valueChanges = new List<(double OldValue, double NewValue)>();
            numberBox.ValueChanged += (_, args) => valueChanges.Add((args.OldValue, args.NewValue));

            using var host = new TestWindowHost(numberBox, width: 320, height: 180);

            var inputBox = FindTemplatePart<TextBox>(numberBox, "InputBox");
            var provider = GetRangeValueProvider(numberBox);

            EnterText(inputBox, "12");
            Assert.AreEqual("12", numberBox.Text);
            Assert.AreEqual(12.0, numberBox.Value);
            AssertValueChange(valueChanges, 0, double.NaN, 12.0);

            provider.SetValue(42);
            host.UpdateLayout();
            Assert.AreEqual("42", numberBox.Text);
            Assert.AreEqual(42.0, numberBox.Value);
            AssertValueChange(valueChanges, 1, 12.0, 42.0);

            EnterText(inputBox, "-5");
            Assert.AreEqual("0", numberBox.Text);
            Assert.AreEqual(0.0, numberBox.Value);
            AssertValueChange(valueChanges, 2, 42.0, 0.0);

            EnterText(inputBox, "150");
            Assert.AreEqual("100", numberBox.Text);
            Assert.AreEqual(100.0, numberBox.Value);
            AssertValueChange(valueChanges, 3, 0.0, 100.0);

            EnterText(inputBox, string.Empty);
            Assert.AreEqual(string.Empty, numberBox.Text);
            Assert.IsTrue(double.IsNaN(numberBox.Value));
            AssertValueChange(valueChanges, 4, 100.0, double.NaN);

            numberBox.Value = double.NaN;
            host.UpdateLayout();
            Assert.AreEqual(5, valueChanges.Count);
        });
    }

    [TestMethod]
    public void BasicKeyboardTest()
    {
        WpfTestHost.Run(() =>
        {
            var numberBox = CreateNumberBox();
            numberBox.LargeChange = 10;

            using var host = new TestWindowHost(numberBox, width: 320, height: 180);

            var inputBox = FindTemplatePart<TextBox>(numberBox, "InputBox");

            EnterText(inputBox, "  75 ");
            Assert.AreEqual(75.0, numberBox.Value);
            Assert.AreEqual("75", inputBox.Text);

            inputBox.Text = "3";
            RaiseKey(inputBox, Keyboard.KeyUpEvent, Key.Escape);
            Assert.AreEqual(75.0, numberBox.Value);
            Assert.AreEqual("75", inputBox.Text);

            RaiseKey(inputBox, Keyboard.PreviewKeyDownEvent, Key.Up);
            Assert.AreEqual(76.0, numberBox.Value);

            RaiseKey(inputBox, Keyboard.PreviewKeyDownEvent, Key.Down);
            Assert.AreEqual(75.0, numberBox.Value);

            RaiseKey(inputBox, Keyboard.PreviewKeyDownEvent, Key.PageUp);
            Assert.AreEqual(85.0, numberBox.Value);
        });
    }

    [TestMethod]
    public void BasicExpressionTest()
    {
        WpfTestHost.Run(() =>
        {
            var numberBox = new ModernWpf.Controls.NumberBox
            {
                Width = 180,
                Value = 0
            };

            using var host = new TestWindowHost(numberBox, width: 320, height: 180);

            var inputBox = FindTemplatePart<TextBox>(numberBox, "InputBox");

            EnterText(inputBox, "5 + 3");
            Assert.AreEqual(0.0, numberBox.Value);

            numberBox.AcceptsExpression = true;

            const double resetValue = 1234;
            var expressions = new Dictionary<string, double>
            {
                ["5"] = 5,
                ["-358"] = -358,
                ["12.34"] = 12.34,
                ["5 + 3"] = 8,
                ["12345 + 67 + 890"] = 13302,
                ["000 + 0011"] = 11,
                ["5 - 3 + 2"] = 4,
                ["3 + 2 - 5"] = 0,
                ["9 - 2 * 6 / 4"] = 6,
                ["9 - -7"] = 16,
                ["9-3*2"] = 3,
                [" 10  *   6  "] = 60,
                ["10 /( 2 + 3 )"] = 2,
                ["5 * -40"] = -200,
                ["(1 - 4) / (2 + 1)"] = -1,
                ["3 * ((4 + 8) / 2)"] = 18,
                ["23 * ((0 - 48) / 8)"] = -138,
                ["((74-71)*2)^3"] = 216,
                ["2 - 2 ^ 3"] = -6,
                ["2 ^ 2 ^ 2 / 2 + 9"] = 17,
                ["5 ^ -2"] = 0.04,
                ["5.09 + 14.333"] = 19.423,
                ["2.5 * 0.35"] = 0.875,
                ["-2 - 5"] = -7,
                ["(10)"] = 10,
                ["(-9)"] = -9,
                ["0^0"] = 1,
                ["5x + 3y"] = resetValue,
                ["5 + (3"] = resetValue,
                ["9 + (2 + 3))"] = resetValue,
                ["(2 + 3)(1 + 5)"] = resetValue,
                ["9 + + 7"] = resetValue,
                ["9 - * 7"] = resetValue,
                ["9 - - 7"] = resetValue,
                ["+9"] = resetValue,
                ["1 / 0"] = resetValue,
                ["-(3 + 5)"] = resetValue
            };

            foreach (var expression in expressions)
            {
                numberBox.Value = resetValue;
                host.UpdateLayout();

                EnterText(inputBox, expression.Key);

                Assert.AreEqual(
                    expression.Value,
                    numberBox.Value,
                    0.00001,
                    $"Expression '{expression.Key}' should evaluate to {expression.Value}.");
            }
        });
    }

    [TestMethod]
    public void VerifyNumberBoxHeaderBehavior()
    {
        WpfTestHost.Run(() =>
        {
            var header = new TextBlock { Text = "Header before template" };
            var numberBox = new ModernWpf.Controls.NumberBox
            {
                Width = 180,
                Header = header,
                Description = "Description text"
            };

            using var host = new TestWindowHost(numberBox, width: 320, height: 180);

            var headerPresenter = FindControlTemplatePart<ContentPresenterEx>(numberBox, "HeaderContentPresenter");
            Assert.AreEqual(Visibility.Visible, headerPresenter.Visibility);
            Assert.AreSame(header, headerPresenter.Content);

            var descriptionPresenter = FindControlTemplatePart<ContentPresenterEx>(numberBox, "DescriptionPresenter");
            Assert.AreEqual(Visibility.Visible, descriptionPresenter.Visibility);
            Assert.AreEqual("Description text", descriptionPresenter.Content);
            Assert.AreEqual(3, Grid.GetColumnSpan(descriptionPresenter));
            AssertBrushEquals(
                (Brush)descriptionPresenter.TryFindResource("SystemControlDescriptionTextForegroundBrush"),
                descriptionPresenter.Foreground);

            numberBox.Header = null;
            host.UpdateLayout();
            Assert.AreEqual(Visibility.Collapsed, headerPresenter.Visibility);

            var headerTemplate = (DataTemplate)XamlReader.Parse(
                @"<DataTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
                    xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
                    <TextBlock x:Name='HeaderTemplateTestingBlock' Text='{Binding}' />
                </DataTemplate>");
            numberBox.Header = "Templated header";
            numberBox.HeaderTemplate = headerTemplate;
            host.UpdateLayout();

            Assert.AreEqual(Visibility.Visible, headerPresenter.Visibility);
            Assert.AreEqual(headerTemplate, headerPresenter.ContentTemplate);
        });
    }

    private static ModernWpf.Controls.NumberBox CreateNumberBox()
    {
        return new ModernWpf.Controls.NumberBox
        {
            Width = 180,
            Minimum = 0,
            Maximum = 100,
            Value = 0,
            SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Inline
        };
    }

    private static IRangeValueProvider GetRangeValueProvider(ModernWpf.Controls.NumberBox numberBox)
    {
        var peer = FrameworkElementAutomationPeer.CreatePeerForElement(numberBox);
        Assert.IsNotNull(peer);

        if (peer!.GetPattern(PatternInterface.RangeValue) is IRangeValueProvider provider)
        {
            return provider;
        }

        Assert.Fail("NumberBox should expose IRangeValueProvider.");
        throw new InvalidOperationException();
    }

    private static void EnterText(TextBox inputBox, string text)
    {
        inputBox.Text = text;
        RaiseKey(inputBox, Keyboard.KeyUpEvent, Key.Enter);
    }

    private static void Click(RepeatButton button)
    {
        button.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent, button));
        WpfTestHost.DoEvents();
    }

    private static void RaiseMouseWheel(UIElement element, int delta)
    {
        var args = new MouseWheelEventArgs(
            Mouse.PrimaryDevice,
            Environment.TickCount,
            delta)
        {
            RoutedEvent = UIElement.MouseWheelEvent
        };

        element.RaiseEvent(args);
        WpfTestHost.DoEvents();
    }

    private static void RaiseKey(UIElement element, RoutedEvent routedEvent, Key key)
    {
        var args = new KeyEventArgs(
            Keyboard.PrimaryDevice,
            PresentationSource.FromVisual(element),
            Environment.TickCount,
            key)
        {
            RoutedEvent = routedEvent
        };

        element.RaiseEvent(args);
        WpfTestHost.DoEvents();
    }

    private static void AssertValueChange(
        IReadOnlyList<(double OldValue, double NewValue)> valueChanges,
        int index,
        double expectedOldValue,
        double expectedNewValue)
    {
        Assert.IsTrue(index < valueChanges.Count, $"Expected value change at index {index}.");
        AssertDoublesEqual(expectedOldValue, valueChanges[index].OldValue);
        AssertDoublesEqual(expectedNewValue, valueChanges[index].NewValue);
    }

    private static void AssertDoublesEqual(double expected, double actual)
    {
        if (double.IsNaN(expected))
        {
            Assert.IsTrue(double.IsNaN(actual), $"Expected NaN but got {actual}.");
        }
        else
        {
            Assert.AreEqual(expected, actual);
        }
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

    private static T FindTemplatePart<T>(DependencyObject root, string name)
        where T : FrameworkElement
    {
        return VisualTreeTestHelper.EnumerateDescendants(root)
            .OfType<T>()
            .Single(element => element.Name == name);
    }

    private static T FindControlTemplatePart<T>(FrameworkElement control, string name)
        where T : FrameworkElement
    {
        return VisualTreeTestHelper.EnumerateDescendants(control)
            .OfType<T>()
            .Single(element => element.Name == name && ReferenceEquals(element.TemplatedParent, control));
    }

    private sealed class CommaDecimalFormatter : INumberBoxNumberFormatter
    {
        public string FormatDouble(double value)
        {
            return value.ToString("0.00", CultureInfo.InvariantCulture).Replace('.', ',');
        }

        public double? ParseDouble(string text)
        {
            return double.TryParse(
                text.Replace(',', '.'),
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out var value)
                ? value
                : null;
        }
    }
}
