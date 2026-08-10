using System.Linq;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Input;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Automation.Peers;
using ModernWpf.Controls;
using ModernWpf.WinUI.TestApp;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.ItemContainer;

[TestClass]
public class ItemContainerApiTests
{
    [TestMethod]
    public void DefaultsAndXamlContentMatchCurrentWinUIShape()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var item = new ModernWpf.Controls.ItemContainer();

            Assert.IsNull(item.Child);
            Assert.IsFalse(item.IsSelected);
            Assert.AreEqual(new CornerRadius(), item.CornerRadius);
            Assert.IsNotNull(ModernWpf.Controls.ItemContainer.ChildProperty);
            Assert.IsNotNull(ModernWpf.Controls.ItemContainer.CornerRadiusProperty);
            Assert.IsNotNull(ModernWpf.Controls.ItemContainer.IsSelectedProperty);

            var parsed = (ModernWpf.Controls.ItemContainer)XamlReader.Parse(
                "<controls:ItemContainer " +
                "xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' " +
                "xmlns:controls='clr-namespace:ModernWpf.Controls;assembly=ModernWpf.Controls'>" +
                "<TextBlock Text='Preview 6 item' />" +
                "</controls:ItemContainer>");

            Assert.IsInstanceOfType<TextBlock>(parsed.Child);
            Assert.AreEqual("Preview 6 item", ((TextBlock)parsed.Child).Text);
        });
    }

    [TestMethod]
    public void TemplateHostsChildAndTracksSelection()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();
            var child = new TextBlock { Text = "Photo" };
            var item = new ModernWpf.Controls.ItemContainer
            {
                Child = child,
                CornerRadius = new CornerRadius(9)
            };

            using var host = new TestWindowHost(item, width: 240, height: 120);

            Assert.IsNotNull(item.Template);
            Assert.IsTrue(item.Focusable);
            Assert.AreSame(child, FindDescendant<ContentPresenter>(item, "PART_ContentPresenter").Content);
            Assert.AreEqual(
                new CornerRadius(9),
                FindDescendant<ModernWpf.Controls.GridEx>(item, "PART_ContainerRoot").CornerRadius);

            item.IsSelected = true;
            host.UpdateLayout();

            Assert.IsTrue(FindDescendant<CheckBox>(item, "PART_SelectionCheckbox").IsChecked == true);
        });
    }

    [TestMethod]
    public void AutomationPeerProvidesSelectionAndConditionalInvoke()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();
            var child = new TextBlock { Text = "Photo" };
            AutomationProperties.SetName(child, "Accessible photo");
            var item = new ModernWpf.Controls.ItemContainer { Child = child };

            using var host = new TestWindowHost(item, width: 240, height: 120);

            var peer = FrameworkElementAutomationPeer.CreatePeerForElement(item);
            Assert.IsInstanceOfType<ItemContainerAutomationPeer>(peer);
            Assert.AreEqual(nameof(ModernWpf.Controls.ItemContainer), peer.GetClassName());
            Assert.AreEqual(AutomationControlType.ListItem, peer.GetAutomationControlType());
            Assert.AreEqual("Accessible photo", peer.GetName());

            var selection = (ISelectionItemProvider)peer.GetPattern(PatternInterface.SelectionItem);
            Assert.IsNull(peer.GetPattern(PatternInterface.Invoke));
            Assert.IsFalse(selection.IsSelected);

            selection.Select();
            Assert.IsTrue(item.IsSelected);
            selection.RemoveFromSelection();
            Assert.IsFalse(item.IsSelected);

            item.CanUserInvokeInternal = ItemContainerUserInvokeMode.UserCanInvoke;
            var invoke = (IInvokeProvider)peer.GetPattern(PatternInterface.Invoke);
            var invokeCount = 0;
            ItemContainerInteractionTrigger trigger = default;
            item.ItemInvoked += (sender, args) =>
            {
                invokeCount++;
                trigger = args.InteractionTrigger;
                args.Handled = true;
            };

            invoke.Invoke();

            Assert.AreEqual(1, invokeCount);
            Assert.AreEqual(ItemContainerInteractionTrigger.AutomationInvoke, trigger);

            item.IsEnabled = false;
            Assert.ThrowsExactly<ElementNotEnabledException>(() => invoke.Invoke());
            Assert.ThrowsExactly<ElementNotEnabledException>(() => selection.Select());
            Assert.AreEqual(1, invokeCount);
        });
    }

    [TestMethod]
    public void PointerPressedDoubleClickAndKeyboardUseSourceShapedTriggers()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();
            var item = new TestItemContainer
            {
                Child = new TextBlock { Text = "Invokable item" },
                CanUserInvokeInternal = ItemContainerUserInvokeMode.UserCanInvoke
            };
            var triggers = new System.Collections.Generic.List<ItemContainerInteractionTrigger>();
            item.ItemInvoked += (sender, args) =>
            {
                triggers.Add(args.InteractionTrigger);
                args.Handled = true;
            };

            using var host = new TestWindowHost(item, width: 240, height: 120);

            item.RaiseMouseDown();
            item.RaiseDoubleClick();
            item.RaiseKeyDown(Key.Enter);
            item.RaiseKeyDown(Key.Space);

            CollectionAssert.AreEqual(
                new[]
                {
                    ItemContainerInteractionTrigger.PointerPressed,
                    ItemContainerInteractionTrigger.DoubleTap,
                    ItemContainerInteractionTrigger.EnterKey,
                    ItemContainerInteractionTrigger.SpaceKey
                },
                triggers);
            Mouse.Capture(null);
        });
    }

    private static T FindDescendant<T>(DependencyObject root, string name)
        where T : FrameworkElement
    {
        return VisualTreeTestHelper
            .EnumerateDescendants(root)
            .OfType<T>()
            .FirstOrDefault(element => element.Name == name)
            ?? throw new AssertFailedException($"Expected template part {name}.");
    }

    private sealed class TestItemContainer : ModernWpf.Controls.ItemContainer
    {
        public void RaiseMouseDown()
        {
            OnMouseLeftButtonDown(new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left)
            {
                RoutedEvent = MouseLeftButtonDownEvent
            });
        }

        public void RaiseDoubleClick()
        {
            OnMouseDoubleClick(new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left)
            {
                RoutedEvent = MouseDoubleClickEvent
            });
        }

        public void RaiseKeyDown(Key key)
        {
            OnKeyDown(new KeyEventArgs(
                Keyboard.PrimaryDevice,
                PresentationSource.FromVisual(this),
                System.Environment.TickCount,
                key)
            {
                RoutedEvent = KeyDownEvent
            });
        }
    }
}
