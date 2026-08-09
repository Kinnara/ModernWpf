using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Automation.Peers;
using ModernWpf.Controls;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.ItemContainer;

[TestClass]
public class ItemContainerTests
{
    [TestMethod]
    public void MatchesCurrentWinUiApiDefaultsAndContentShape()
    {
        WpfTestHost.Run(() =>
        {
            var itemContainer = new Controls.ItemContainer();

            Assert.IsFalse(typeof(Controls.ItemContainer).IsSealed);
            Assert.IsNull(itemContainer.Child);
            Assert.IsFalse(itemContainer.IsSelected);
            Assert.AreEqual(new CornerRadius(4), itemContainer.CornerRadius);
            Assert.IsTrue(itemContainer.Focusable);
            Assert.IsNotNull(Controls.ItemContainer.ChildProperty);
            Assert.IsNotNull(Controls.ItemContainer.IsSelectedProperty);
            Assert.IsNotNull(Controls.ItemContainer.CornerRadiusProperty);

            var contentProperty = typeof(Controls.ItemContainer)
                .GetCustomAttributes<ContentPropertyAttribute>()
                .Single();
            Assert.AreEqual(nameof(Controls.ItemContainer.Child), contentProperty.Name);

            var parsed = (Controls.ItemContainer)XamlReader.Parse(
                "<ui:ItemContainer " +
                "xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' " +
                "xmlns:ui='clr-namespace:ModernWpf.Controls;assembly=ModernWpf.Controls'>" +
                "<TextBlock Text='Child content' />" +
                "</ui:ItemContainer>");

            Assert.IsInstanceOfType<TextBlock>(parsed.Child);
            Assert.AreEqual("Child content", ((TextBlock)parsed.Child).Text);
        });
    }

    [TestMethod]
    public void TemplateReflectsSelectionAndMultipleSelectionState()
    {
        WpfTestHost.Run(() =>
        {
            var itemContainer = new Controls.ItemContainer
            {
                Child = new TextBlock { Text = "Sample item" },
                IsSelected = true,
                MultiSelectMode = ItemContainerMultiSelectMode.Multiple
            };

            using var host = new TestWindowHost(itemContainer, width: 320, height: 180);
            host.UpdateLayout();

            var selectionCheckBox = (CheckBox)itemContainer.Template.FindName(
                Controls.ItemContainer.SelectionCheckBoxPartName,
                itemContainer);
            var selectionVisual = (Border)itemContainer.Template.FindName(
                "PART_SelectionVisual",
                itemContainer);

            Assert.AreEqual(Visibility.Visible, selectionCheckBox.Visibility);
            Assert.AreEqual(true, selectionCheckBox.IsChecked);
            Assert.AreNotEqual(Brushes.Transparent, selectionVisual.BorderBrush);

            itemContainer.IsSelected = false;
            itemContainer.MultiSelectMode = ItemContainerMultiSelectMode.Single;

            Assert.AreEqual(Visibility.Collapsed, selectionCheckBox.Visibility);
            Assert.AreEqual(false, selectionCheckBox.IsChecked);
        });
    }

    [TestMethod]
    public void AutomationPeerExposesSelectionAndOptInInvokePatterns()
    {
        WpfTestHost.Run(() =>
        {
            var itemContainer = new Controls.ItemContainer
            {
                Child = new TextBlock { Text = "Sample item" }
            };
            var peer = new ItemContainerAutomationPeer(itemContainer);

            Assert.AreEqual(AutomationControlType.ListItem, peer.GetAutomationControlType());
            Assert.AreEqual(nameof(Controls.ItemContainer), peer.GetClassName());
            Assert.AreEqual("Sample item", peer.GetName());
            Assert.AreEqual("ItemContainer", peer.GetLocalizedControlType());
            Assert.AreSame(peer, peer.GetPattern(PatternInterface.SelectionItem));
            Assert.IsNull(peer.GetPattern(PatternInterface.Invoke));

            ((ISelectionItemProvider)peer).Select();
            Assert.IsTrue(itemContainer.IsSelected);
            ((ISelectionItemProvider)peer).RemoveFromSelection();
            Assert.IsFalse(itemContainer.IsSelected);

            itemContainer.CanUserSelect = ItemContainerUserSelectMode.UserCannotSelect;
            Assert.IsNull(peer.GetPattern(PatternInterface.SelectionItem));

            ItemContainerInvokedEventArgs? invokedArgs = null;
            itemContainer.CanUserInvoke = ItemContainerUserInvokeMode.UserCanInvoke;
            itemContainer.ItemInvoked += (_, args) => invokedArgs = args;

            Assert.AreSame(peer, peer.GetPattern(PatternInterface.Invoke));
            ((IInvokeProvider)peer).Invoke();
            Assert.IsNotNull(invokedArgs);
            Assert.AreEqual(ItemContainerInteractionTrigger.AutomationInvoke, invokedArgs.InteractionTrigger);

            var emptyPeer = new ItemContainerAutomationPeer(new Controls.ItemContainer());
            Assert.AreEqual("ItemContainer", emptyPeer.GetName());
        });
    }
}
