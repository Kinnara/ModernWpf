using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.RatingControl;

[TestClass]
public class RatingControlInteractionTests
{
    [TestMethod]
    public void BasicKeyboardTest()
    {
        WpfTestHost.Run(() =>
        {
            var ratingControl = new ModernWpf.Controls.RatingControl
            {
                PlaceholderValue = 2.5
            };

            using var host = new TestWindowHost(ratingControl, width: 360, height: 180);

            RaiseKey(ratingControl, Key.Right);
            Assert.AreEqual(1.0, ratingControl.Value);

            ratingControl.FlowDirection = FlowDirection.RightToLeft;
            RaiseKey(ratingControl, Key.Left);
            Assert.AreEqual(2.0, ratingControl.Value, "Left should increase the rating in RTL.");

            RaiseKey(ratingControl, Key.Home);
            Assert.AreEqual(-1.0, ratingControl.Value);

            RaiseKey(ratingControl, Key.End);
            Assert.AreEqual(5.0, ratingControl.Value);

            RaiseKey(ratingControl, Key.Down);
            Assert.AreEqual(4.0, ratingControl.Value);

            RaiseKey(ratingControl, Key.Up);
            Assert.AreEqual(5.0, ratingControl.Value);

            ratingControl.FlowDirection = FlowDirection.LeftToRight;
            RaiseKey(ratingControl, Key.Home);
            Assert.AreEqual(-1.0, ratingControl.Value);

            RaiseKey(ratingControl, Key.End);
            Assert.AreEqual(5.0, ratingControl.Value);

            RaiseKey(ratingControl, Key.Down);
            Assert.AreEqual(4.0, ratingControl.Value);

            RaiseKey(ratingControl, Key.Up);
            Assert.AreEqual(5.0, ratingControl.Value);
        });
    }

    [TestMethod]
    public void VerifyDependencyPropertyBinding()
    {
        WpfTestHost.Run(() =>
        {
            var source = new RatingBindingSource { RatingValue = 2.0 };
            var ratingControl = new ModernWpf.Controls.RatingControl();
            BindingOperations.SetBinding(
                ratingControl,
                ModernWpf.Controls.RatingControl.ValueProperty,
                new Binding(nameof(RatingBindingSource.RatingValue))
                {
                    Source = source,
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                });

            using var host = new TestWindowHost(ratingControl, width: 360, height: 180);

            Assert.AreEqual(2.0, ratingControl.Value);

            ratingControl.Value = 4.0;
            WpfTestHost.DoEvents();
            Assert.AreEqual(4.0, source.RatingValue);

            source.RatingValue = 3.0;
            WpfTestHost.DoEvents();
            Assert.AreEqual(3.0, ratingControl.Value);
        });
    }

    [TestMethod]
    public void MaxRatingAutomationValueUsesConfiguredMaximum()
    {
        WpfTestHost.Run(() =>
        {
            var ratingControl = new ModernWpf.Controls.RatingControl
            {
                MaxRating = 9
            };

            using var host = new TestWindowHost(ratingControl, width: 420, height: 180);

            var valueProvider = GetValueProvider(ratingControl);
            var rangeProvider = GetRangeValueProvider(ratingControl);

            Assert.AreEqual(9.0, rangeProvider.Maximum);

            rangeProvider.SetValue(9);
            host.UpdateLayout();
            Assert.AreEqual(9.0, ratingControl.Value);
            Assert.AreEqual("Rating, 9 of 9", valueProvider.Value);
        });
    }

    [TestMethod]
    public void VerifyReadOnlyIsntInteractive()
    {
        WpfTestHost.Run(() =>
        {
            var ratingControl = new ModernWpf.Controls.RatingControl
            {
                IsReadOnly = true,
                Value = 2.2
            };

            using var host = new TestWindowHost(ratingControl, width: 360, height: 180);

            for (int i = 0; i < 6; i++)
            {
                RaiseKey(ratingControl, Key.Right);
            }

            Assert.AreEqual(2.2, ratingControl.Value);
        });
    }

    [TestMethod]
    public void UIAValuePatternTest()
    {
        WpfTestHost.Run(() =>
        {
            var ratingControl = new ModernWpf.Controls.RatingControl
            {
                PlaceholderValue = 2.5
            };

            using var host = new TestWindowHost(ratingControl, width: 360, height: 180);

            var valueProvider = GetValueProvider(ratingControl);
            var rangeProvider = GetRangeValueProvider(ratingControl);

            Assert.AreEqual("Community Rating, 2.5 of 5", valueProvider.Value);

            valueProvider.SetValue("3");
            host.UpdateLayout();
            Assert.AreEqual(3.0, ratingControl.Value);
            Assert.AreEqual("Rating, 3 of 5", valueProvider.Value);
            Assert.AreEqual(3.0, rangeProvider.Value);

            rangeProvider.SetValue(2);
            host.UpdateLayout();
            Assert.AreEqual(2.0, ratingControl.Value);
            Assert.AreEqual("Rating, 2 of 5", valueProvider.Value);

            ratingControl.PlaceholderValue = -1;
            ratingControl.Value = -1;
            host.UpdateLayout();
            Assert.AreEqual("Rating Unset", valueProvider.Value);
            Assert.AreEqual(0.0, rangeProvider.Value);

            ratingControl.Value = 1.5;
            host.UpdateLayout();
            Assert.AreEqual("Rating, 1.5 of 5", valueProvider.Value);

            ratingControl.Value = 1.55;
            host.UpdateLayout();
            Assert.AreEqual("Rating, 1.55 of 5", valueProvider.Value);

            ratingControl.Value = 1.549;
            host.UpdateLayout();
            Assert.AreEqual("Rating, 1.55 of 5", valueProvider.Value);
        });
    }

    [TestMethod]
    public void VerifyUIAProperties()
    {
        WpfTestHost.Run(() =>
        {
            var readOnlyRating = new ModernWpf.Controls.RatingControl
            {
                IsReadOnly = true
            };
            var editableRating = new ModernWpf.Controls.RatingControl();
            var root = new StackPanel();
            root.Children.Add(readOnlyRating);
            root.Children.Add(editableRating);

            using var host = new TestWindowHost(root, width: 360, height: 240);

            var readOnlyPeer = FrameworkElementAutomationPeer.CreatePeerForElement(readOnlyRating);
            Assert.IsNotNull(readOnlyPeer);
            var readOnlyValueProvider = readOnlyPeer!.GetPattern(PatternInterface.Value) as IValueProvider;
            Assert.IsNotNull(readOnlyValueProvider);
            Assert.IsTrue(readOnlyValueProvider!.IsReadOnly);

            var editablePeer = FrameworkElementAutomationPeer.CreatePeerForElement(editableRating);
            Assert.IsNotNull(editablePeer);

            var editableValueProvider = editablePeer!.GetPattern(PatternInterface.Value) as IValueProvider;
            Assert.IsNotNull(editableValueProvider);
            Assert.IsFalse(editableValueProvider!.IsReadOnly);

            var rangeProvider = editablePeer.GetPattern(PatternInterface.RangeValue) as IRangeValueProvider;
            Assert.IsNotNull(rangeProvider);
            Assert.IsNull(editablePeer.GetPattern(PatternInterface.ExpandCollapse));
            Assert.AreEqual(AutomationControlType.Slider, editablePeer.GetAutomationControlType());
            Assert.AreEqual("Rating Slider", editablePeer.GetLocalizedControlType());
            Assert.AreEqual(5.0, rangeProvider!.Maximum);
        });
    }

    private static IValueProvider GetValueProvider(ModernWpf.Controls.RatingControl ratingControl)
    {
        var peer = FrameworkElementAutomationPeer.CreatePeerForElement(ratingControl);
        Assert.IsNotNull(peer);

        if (peer!.GetPattern(PatternInterface.Value) is IValueProvider provider)
        {
            return provider;
        }

        Assert.Fail("RatingControl should expose IValueProvider.");
        throw new InvalidOperationException();
    }

    private static IRangeValueProvider GetRangeValueProvider(ModernWpf.Controls.RatingControl ratingControl)
    {
        var peer = FrameworkElementAutomationPeer.FromElement(ratingControl)
            ?? FrameworkElementAutomationPeer.CreatePeerForElement(ratingControl);
        Assert.IsNotNull(peer);

        if (peer!.GetPattern(PatternInterface.RangeValue) is IRangeValueProvider provider)
        {
            return provider;
        }

        Assert.Fail("RatingControl should expose IRangeValueProvider.");
        throw new InvalidOperationException();
    }

    private static void RaiseKey(UIElement element, Key key)
    {
        var args = new KeyEventArgs(
            Keyboard.PrimaryDevice,
            PresentationSource.FromVisual(element),
            Environment.TickCount,
            key)
        {
            RoutedEvent = Keyboard.KeyDownEvent
        };

        element.RaiseEvent(args);
        WpfTestHost.DoEvents();
    }

    private sealed class RatingBindingSource : INotifyPropertyChanged
    {
        private double ratingValue;

        public event PropertyChangedEventHandler? PropertyChanged;

        public double RatingValue
        {
            get => ratingValue;
            set
            {
                if (ratingValue != value)
                {
                    ratingValue = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RatingValue)));
                }
            }
        }
    }
}
