// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Automation.Peers;
using ModernWpf.Controls;

namespace ModernWpf.Automation.Peers
{
    public class RepeaterAutomationPeer : FrameworkElementAutomationPeer
    {
        public RepeaterAutomationPeer(FrameworkElement owner) :
            base(owner)
        {
        }

        protected override List<AutomationPeer> GetChildrenCore()
        {
            var repeater = (ItemsRepeater)Owner;
            var childrenPeers = base.GetChildrenCore();
            if (childrenPeers == null) return null;
            int peerCount = childrenPeers.Count;

            List<Tuple<int /* index */, AutomationPeer>> realizedPeers = new List<Tuple<int, AutomationPeer>>();
            realizedPeers.Capacity = peerCount;

            // Filter out unrealized peers.
            {
                for (int i = 0; i < peerCount; ++i)
                {
                    var childPeer = childrenPeers[i];
                    var childElement = GetElement(childPeer, repeater);
                    if (childElement != null)
                    {
                        var virtInfo = ItemsRepeater.GetVirtualizationInfo(childElement);
                        if (virtInfo.IsRealized)
                        {
                            realizedPeers.Add(Tuple.Create(virtInfo.Index, childPeer));
                        }
                    }
                }
            }

            // Select peers.
            {
                var peers = new List<AutomationPeer>(realizedPeers.Count /* capacity */);
                foreach (var entry in realizedPeers.OrderBy(x => x.Item1)) // Sort peers by index.
                {
                    peers.Add(entry.Item2);
                }
                return peers;
            }
        }

        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.Group;
        }

        private UIElement GetElement(AutomationPeer childPeer, ItemsRepeater repeater)
        {
            var childElement = (DependencyObject)((FrameworkElementAutomationPeer)childPeer).Owner;

            var parent = CachedVisualTreeHelpers.GetParent(childElement);
            // Child peer could have given a descendant of the repeater's child. We
            // want to get to the immediate child.
            while (parent != null && parent as ItemsRepeater != repeater)
            {
                childElement = parent;
                parent = CachedVisualTreeHelpers.GetParent(childElement);
            }

            return (UIElement)childElement;
        }
    }
}