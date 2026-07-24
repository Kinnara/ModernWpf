using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Automation.Peers;
using ModernWpf.Controls;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.Repeater;

[TestClass]
public class RepeaterAutomationPeerTests
{
    [TestMethod]
    public void AutomationPeerReportsOnlyRealizedChildrenInItemIndexOrderLikeWinUI()
    {
        WpfTestHost.Run(() =>
        {
            var group = new StackPanel();
            group.Children.Add(new System.Windows.Controls.ListViewItem { Content = "Nested item 0" });
            group.Children.Add(new System.Windows.Controls.ListViewItem { Content = "Nested item 1" });

            var mapping = new Dictionary<int, UIElement>
            {
                [0] = new System.Windows.Controls.ListViewItem { Content = "Item 0" },
                [1] = new System.Windows.Controls.ListViewItem { Content = "Unrealized item 1" },
                [2] = group
            };

            var repeater = new ItemsRepeater
            {
                ItemsSource = Enumerable.Range(0, mapping.Count),
                ItemTemplate = new MappingElementFactory(mapping),
                Layout = new OutOfOrderAccessibilityLayout()
            };

            using var host = new TestWindowHost(repeater, width: 320, height: 180);

            var peer = new RepeaterAutomationPeer(repeater);
            Assert.AreEqual(AutomationControlType.Group, peer.GetAutomationControlType());

            var children = peer.GetChildren();
            Assert.IsNotNull(children);

            var owners = children!
                .Cast<FrameworkElementAutomationPeer>()
                .Select(childPeer => childPeer.Owner)
                .ToList();

            Assert.AreEqual(3, owners.Count);
            Assert.AreSame(mapping[0], owners[0]);
            Assert.AreSame(group.Children[0], owners[1]);
            Assert.AreSame(group.Children[1], owners[2]);
            Assert.IsFalse(owners.Contains(mapping[1]));
        });
    }

    private sealed class MappingElementFactory : ElementFactory
    {
        public MappingElementFactory(IReadOnlyDictionary<int, UIElement> mapping)
        {
            m_mapping = mapping;
        }

        protected override UIElement GetElementCore(ElementFactoryGetArgs args)
        {
            return m_mapping[(int)args.Data];
        }

        protected override void RecycleElementCore(ElementFactoryRecycleArgs args)
        {
        }

        private readonly IReadOnlyDictionary<int, UIElement> m_mapping;
    }

    private sealed class OutOfOrderAccessibilityLayout : VirtualizingLayout
    {
        protected override Size MeasureOverride(VirtualizingLayoutContext context, Size availableSize)
        {
            m_element2 = context.GetOrCreateElementAt(2);
            m_element0 = context.GetOrCreateElementAt(0);
            var element1 = context.GetOrCreateElementAt(1);

            m_element2.Measure(availableSize);
            m_element0.Measure(availableSize);
            context.RecycleElement(element1);

            return new Size(200, 100);
        }

        protected override Size ArrangeOverride(VirtualizingLayoutContext context, Size finalSize)
        {
            m_element0?.Arrange(new Rect(0, 0, finalSize.Width, 40));
            m_element2?.Arrange(new Rect(0, 40, finalSize.Width, 60));
            return finalSize;
        }

        private UIElement? m_element0;
        private UIElement? m_element2;
    }
}
