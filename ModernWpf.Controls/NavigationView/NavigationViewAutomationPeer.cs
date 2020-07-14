// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using ModernWpf.Controls;

namespace ModernWpf.Automation.Peers
{
    internal class NavigationViewAutomationPeer : FrameworkElementAutomationPeer, ISelectionProvider
    {
        public NavigationViewAutomationPeer(NavigationView owner) :
            base(owner)
        {
        }

        public override object GetPattern(PatternInterface patternInterface)
        {
            if (patternInterface == PatternInterface.Selection)
            {
                return this;
            }

            return base.GetPattern(patternInterface);
        }

        public bool CanSelectMultiple => false;

        public bool IsSelectionRequired => false;

        public IRawElementProviderSimple[] GetSelection()
        {
            if (Owner is NavigationView nv)
            {
                if (nv.GetSelectedContainer() is { } nvi)
                {
                    if (FrameworkElementAutomationPeer.CreatePeerForElement(nvi) is { } peer)
                    {
                        return new[] { ProviderFromPeer(peer) };
                    }
                }
            }
            return new IRawElementProviderSimple[0];
        }

        internal void RaiseSelectionChangedEvent(object oldSelection, object newSelecttion)
        {
            if (AutomationPeer.ListenerExists(AutomationEvents.SelectionPatternOnInvalidated))
            {
                if (Owner is NavigationView nv)
                {
                    if (nv.GetSelectedContainer() is { } nvi)
                    {
                        if (FrameworkElementAutomationPeer.CreatePeerForElement(nvi) is { } peer)
                        {
                            peer.RaiseAutomationEvent(AutomationEvents.SelectionItemPatternOnElementSelected);
                        }
                    }
                }
            }
        }
    }
}