// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using ModernWpf.Controls;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;

namespace ItemsRepeaterTestApp.Samples.Selection
{
    public class SelectionContainerAutomationPeer : FrameworkElementAutomationPeer, ISelectionProvider
    {
        private SelectionContainer _owner;

        public SelectionContainerAutomationPeer(FrameworkElement owner) :
            base(owner)
        {
            _owner = (SelectionContainer)owner;
        }

        public override object GetPattern(PatternInterface patternInterface)
        {
            if (patternInterface == PatternInterface.Selection)
            {
                return this;
            }

            return base.GetPattern(patternInterface);
        }

        public IRawElementProviderSimple[] GetSelection()
        {
            List<IRawElementProviderSimple> selected = new List<IRawElementProviderSimple>();
            // TODO: Need to get peers here or work with accesibility team to figure 
            // out what to give here. The selected elements might not be realized.
            return selected.ToArray();
        }

        public bool CanSelectMultiple
        {
            get
            {
                return !_owner.SelectionModel.SingleSelect;
            }
        }

        public bool IsSelectionRequired
        {
            get
            {
                return false;
            }
        }

        internal void SelectionChanged(SelectionModelSelectionChangedEventArgs args)
        {
            RaiseAutomationEvent(AutomationEvents.SelectionPatternOnInvalidated);
        }
    }
}