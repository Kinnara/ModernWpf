using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Controls;
using ModernWpf.Controls.Primitives;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.SplitButton;

[TestClass]
public class SplitButtonInteractionTests
{
    [TestMethod]
    public void BasicInteractionTest()
    {
        WpfTestHost.Run(() =>
        {
            var splitButton = CreateSplitButton();
            var flyout = CreateCountingFlyout("TestFlyout");
            splitButton.Flyout = flyout.Flyout;

            var clickCount = 0;
            splitButton.Click += (sender, args) => clickCount++;

            using var host = new TestWindowHost(splitButton, width: 360, height: 180);

            InvokeButton(GetPrimaryButton(splitButton));
            Assert.AreEqual(1, clickCount);
            Assert.AreEqual(0, flyout.OpenedCount);
            Assert.IsFalse(flyout.Flyout.IsOpen);

            InvokeButton(GetSecondaryButton(splitButton));
            Assert.AreEqual(1, flyout.OpenedCount);
            Assert.AreEqual(0, flyout.ClosedCount);
            Assert.IsTrue(flyout.Flyout.IsOpen);
            Assert.AreEqual(FlyoutPlacementMode.BottomEdgeAlignedLeft, flyout.Flyout.GetEffectivePlacement());

            GetExpandCollapseProvider(splitButton).Collapse();
            WpfTestHost.DoEvents();
            Assert.AreEqual(1, flyout.ClosedCount);
            Assert.IsFalse(flyout.Flyout.IsOpen);
        });
    }

    [TestMethod]
    public void CommandTest()
    {
        WpfTestHost.Run(() =>
        {
            var command = new TestCommand();
            var splitButton = CreateSplitButton();
            splitButton.Command = command;

            using var host = new TestWindowHost(splitButton, width: 360, height: 180);

            var primaryButton = GetPrimaryButton(splitButton);
            Assert.IsTrue(primaryButton.IsEnabled);
            Assert.AreEqual(0, command.ExecuteCount);

            InvokeButton(primaryButton);
            Assert.AreEqual(1, command.ExecuteCount);

            command.CanExecuteValue = false;
            command.RaiseCanExecuteChanged();
            WpfTestHost.DoEvents();
            Assert.IsFalse(primaryButton.IsEnabled);

            InvokeButton(primaryButton);
            Assert.AreEqual(1, command.ExecuteCount);
        });
    }

    [TestMethod]
    public void AccessibilityTest()
    {
        WpfTestHost.Run(() =>
        {
            var splitButton = CreateSplitButton();
            var flyout = CreateCountingFlyout("TestFlyout");
            splitButton.Flyout = flyout.Flyout;

            var clickCount = 0;
            splitButton.Click += (sender, args) => clickCount++;

            using var host = new TestWindowHost(splitButton, width: 360, height: 180);

            var peer = FrameworkElementAutomationPeer.CreatePeerForElement(splitButton);
            Assert.IsNotNull(peer);
            Assert.AreEqual(AutomationControlType.SplitButton, peer!.GetAutomationControlType());
            Assert.AreEqual(nameof(ModernWpf.Controls.SplitButton), peer.GetClassName());
            Assert.AreEqual(0, peer.GetChildren()?.Count ?? 0);

            var invokeProvider = GetInvokeProvider(splitButton);
            invokeProvider.Invoke();
            WpfTestHost.DoEvents();
            Assert.AreEqual(1, clickCount);

            var expandCollapseProvider = GetExpandCollapseProvider(splitButton);
            Assert.AreEqual(ExpandCollapseState.Collapsed, expandCollapseProvider.ExpandCollapseState);

            expandCollapseProvider.Expand();
            WpfTestHost.DoEvents();
            Assert.AreEqual(1, flyout.OpenedCount);
            Assert.AreEqual(ExpandCollapseState.Expanded, expandCollapseProvider.ExpandCollapseState);

            expandCollapseProvider.Collapse();
            WpfTestHost.DoEvents();
            Assert.AreEqual(1, flyout.ClosedCount);
            Assert.AreEqual(ExpandCollapseState.Collapsed, expandCollapseProvider.ExpandCollapseState);
        });
    }

    [TestMethod]
    public void KeyboardTest()
    {
        WpfTestHost.Run(() =>
        {
            var splitButton = CreateSplitButton();
            var flyout = CreateCountingFlyout("TestFlyout");
            splitButton.Flyout = flyout.Flyout;
            var command = new TestCommand();
            splitButton.Command = command;

            var clickCount = 0;
            splitButton.Click += (sender, args) => clickCount++;

            using var host = new TestWindowHost(splitButton, width: 360, height: 180);

            splitButton.Focus();
            RaiseKey(splitButton, Keyboard.KeyDownEvent, Key.Space);
            RaiseKey(splitButton, Keyboard.KeyUpEvent, Key.Space);
            Assert.AreEqual(1, clickCount);
            Assert.AreEqual(1, command.ExecuteCount);

            RaiseKey(splitButton, Keyboard.KeyUpEvent, Key.F4);
            WpfTestHost.DoEvents();
            Assert.AreEqual(1, flyout.OpenedCount);
            Assert.IsTrue(flyout.Flyout.IsOpen);
            Assert.AreEqual(FlyoutPlacementMode.BottomEdgeAlignedLeft, flyout.Flyout.GetEffectivePlacement());

            GetExpandCollapseProvider(splitButton).Collapse();
            WpfTestHost.DoEvents();
            Assert.AreEqual(1, flyout.ClosedCount);
        });
    }

    [TestMethod]
    public void ToggleTest()
    {
        WpfTestHost.Run(() =>
        {
            var flyout = CreateCountingFlyout("ToggleFlyout");
            var toggleSplitButton = new ToggleSplitButton
            {
                Content = "ToggleSplitButton",
                Width = 220,
                Height = 40,
                Flyout = flyout.Flyout
            };

            var changedCount = 0;
            bool? checkedDuringClick = null;
            toggleSplitButton.IsCheckedChanged += (sender, args) => changedCount++;
            toggleSplitButton.Click += (sender, args) => checkedDuringClick = toggleSplitButton.IsChecked;

            using var host = new TestWindowHost(toggleSplitButton, width: 360, height: 180);

            Assert.IsFalse(toggleSplitButton.IsChecked);

            InvokeButton(GetPrimaryButton(toggleSplitButton));
            Assert.IsTrue(toggleSplitButton.IsChecked);
            Assert.AreEqual(1, changedCount);
            Assert.AreEqual(true, checkedDuringClick);

            InvokeButton(GetPrimaryButton(toggleSplitButton));
            Assert.IsFalse(toggleSplitButton.IsChecked);
            Assert.AreEqual(2, changedCount);
            Assert.AreEqual(false, checkedDuringClick);

            InvokeButton(GetSecondaryButton(toggleSplitButton));
            Assert.IsFalse(toggleSplitButton.IsChecked);
            Assert.AreEqual(2, changedCount);
            Assert.AreEqual(1, flyout.OpenedCount);
            Assert.IsTrue(flyout.Flyout.IsOpen);

            GetExpandCollapseProvider(toggleSplitButton).Collapse();
            WpfTestHost.DoEvents();
            Assert.AreEqual(1, flyout.ClosedCount);
        });
    }

    [TestMethod]
    public void ToggleAccessibilityTest()
    {
        WpfTestHost.Run(() =>
        {
            var toggleSplitButton = new ToggleSplitButton
            {
                Content = "ToggleSplitButton",
                Width = 220,
                Height = 40
            };

            using var host = new TestWindowHost(toggleSplitButton, width: 360, height: 180);

            var toggleProvider = GetToggleProvider(toggleSplitButton);
            Assert.AreEqual(ToggleState.Off, toggleProvider.ToggleState);
            Assert.IsFalse(toggleSplitButton.IsChecked);

            toggleProvider.Toggle();
            WpfTestHost.DoEvents();

            Assert.IsTrue(toggleSplitButton.IsChecked);
            Assert.AreEqual(ToggleState.On, toggleProvider.ToggleState);
        });
    }

    [TestMethod]
    public void ToggleExpandCollapseAutomationOpensFlyoutWithoutToggling()
    {
        WpfTestHost.Run(() =>
        {
            var flyout = CreateCountingFlyout("ToggleFlyout");
            var toggleSplitButton = new ToggleSplitButton
            {
                Content = "ToggleSplitButton",
                Width = 220,
                Height = 40,
                Flyout = flyout.Flyout
            };

            using var host = new TestWindowHost(toggleSplitButton, width: 360, height: 180);

            var expandCollapseProvider = GetExpandCollapseProvider(toggleSplitButton);
            Assert.AreEqual(ExpandCollapseState.Collapsed, expandCollapseProvider.ExpandCollapseState);
            Assert.IsFalse(toggleSplitButton.IsChecked);

            expandCollapseProvider.Expand();
            WpfTestHost.DoEvents();
            Assert.AreEqual(1, flyout.OpenedCount);
            Assert.IsTrue(flyout.Flyout.IsOpen);
            Assert.AreEqual(ExpandCollapseState.Expanded, expandCollapseProvider.ExpandCollapseState);
            Assert.IsFalse(toggleSplitButton.IsChecked);

            expandCollapseProvider.Collapse();
            WpfTestHost.DoEvents();
            Assert.AreEqual(1, flyout.ClosedCount);
            Assert.AreEqual(ExpandCollapseState.Collapsed, expandCollapseProvider.ExpandCollapseState);
            Assert.IsFalse(toggleSplitButton.IsChecked);
        });
    }

    private static ModernWpf.Controls.SplitButton CreateSplitButton()
    {
        return new ModernWpf.Controls.SplitButton
        {
            Content = "TestSplitButton",
            Width = 220,
            Height = 40
        };
    }

    private static IInvokeProvider GetInvokeProvider(ModernWpf.Controls.SplitButton splitButton)
    {
        var peer = FrameworkElementAutomationPeer.CreatePeerForElement(splitButton);
        Assert.IsNotNull(peer);

        if (peer!.GetPattern(PatternInterface.Invoke) is IInvokeProvider provider)
        {
            return provider;
        }

        Assert.Fail("SplitButton should expose IInvokeProvider.");
        throw new InvalidOperationException();
    }

    private static IExpandCollapseProvider GetExpandCollapseProvider(ModernWpf.Controls.SplitButton splitButton)
    {
        var peer = FrameworkElementAutomationPeer.FromElement(splitButton)
            ?? FrameworkElementAutomationPeer.CreatePeerForElement(splitButton);
        Assert.IsNotNull(peer);

        if (peer!.GetPattern(PatternInterface.ExpandCollapse) is IExpandCollapseProvider provider)
        {
            return provider;
        }

        Assert.Fail("SplitButton should expose IExpandCollapseProvider.");
        throw new InvalidOperationException();
    }

    private static IToggleProvider GetToggleProvider(ToggleSplitButton toggleSplitButton)
    {
        var peer = FrameworkElementAutomationPeer.CreatePeerForElement(toggleSplitButton);
        Assert.IsNotNull(peer);

        if (peer!.GetPattern(PatternInterface.Toggle) is IToggleProvider provider)
        {
            return provider;
        }

        Assert.Fail("ToggleSplitButton should expose IToggleProvider.");
        throw new InvalidOperationException();
    }

    private static Button GetPrimaryButton(ModernWpf.Controls.SplitButton splitButton)
    {
        return FindTemplateButton(splitButton, "PrimaryButton");
    }

    private static Button GetSecondaryButton(ModernWpf.Controls.SplitButton splitButton)
    {
        return FindTemplateButton(splitButton, "SecondaryButton");
    }

    private static Button FindTemplateButton(ModernWpf.Controls.SplitButton splitButton, string name)
    {
        splitButton.ApplyTemplate();
        WpfTestHost.DoEvents();

        foreach (var button in FindVisualChildren<Button>(splitButton))
        {
            if (button.Name == name)
            {
                return button;
            }
        }

        Assert.Fail($"Could not find SplitButton template part '{name}'.");
        throw new InvalidOperationException();
    }

    private static void InvokeButton(Button button)
    {
        if (!button.IsEnabled)
        {
            WpfTestHost.DoEvents();
            return;
        }

        var peer = FrameworkElementAutomationPeer.CreatePeerForElement(button) as ButtonAutomationPeer;
        Assert.IsNotNull(peer);

        if (peer!.GetPattern(PatternInterface.Invoke) is IInvokeProvider provider)
        {
            provider.Invoke();
            WpfTestHost.DoEvents();
            return;
        }

        Assert.Fail("Button should expose IInvokeProvider.");
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

    private static IEnumerable<T> FindVisualChildren<T>(DependencyObject root)
        where T : DependencyObject
    {
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
        {
            var child = VisualTreeHelper.GetChild(root, i);
            if (child is T match)
            {
                yield return match;
            }

            foreach (var descendant in FindVisualChildren<T>(child))
            {
                yield return descendant;
            }
        }
    }

    private static CountingFlyout CreateCountingFlyout(string name)
    {
        var flyout = new Flyout
        {
            Content = new TextBlock
            {
                Name = name,
                Text = name,
                MinWidth = 120,
                MinHeight = 32
            }
        };
        var countingFlyout = new CountingFlyout(flyout);
        flyout.Opened += (sender, args) => countingFlyout.OpenedCount++;
        flyout.Closed += (sender, args) => countingFlyout.ClosedCount++;
        return countingFlyout;
    }

    private sealed class CountingFlyout
    {
        public CountingFlyout(Flyout flyout)
        {
            Flyout = flyout;
        }

        public Flyout Flyout { get; }

        public int OpenedCount { get; set; }

        public int ClosedCount { get; set; }
    }

#pragma warning disable CS0067
    private sealed class TestCommand : ICommand
    {
        public event EventHandler? CanExecuteChanged;

        public bool CanExecuteValue { get; set; } = true;

        public int ExecuteCount { get; private set; }

        public bool CanExecute(object? parameter)
        {
            return CanExecuteValue;
        }

        public void Execute(object? parameter)
        {
            ExecuteCount++;
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
#pragma warning restore CS0067
}
