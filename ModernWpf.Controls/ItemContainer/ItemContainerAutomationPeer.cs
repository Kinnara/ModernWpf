// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Media;
using ModernWpf.Controls;
using static ModernWpf.ResourceAccessor;

namespace ModernWpf.Automation.Peers
{
    public class ItemContainerAutomationPeer : FrameworkElementAutomationPeer, ISelectionItemProvider, IInvokeProvider
    {
        private static readonly ResourceAccessor ResourceAccessor = new ResourceAccessor(typeof(ItemContainer));

        public ItemContainerAutomationPeer(ItemContainer owner)
            : base(owner)
        {
        }

        public bool IsSelected => OwnerItem.IsSelected;

        public IRawElementProviderSimple SelectionContainer
        {
            get
            {
                DependencyObject current = VisualTreeHelper.GetParent(OwnerItem);
                while (current != null)
                {
                    if (current is UIElement element)
                    {
                        var peer = CreatePeerForElement(element);
                        if (peer?.GetPattern(PatternInterface.Selection) is ISelectionProvider)
                        {
                            return ProviderFromPeer(peer);
                        }
                    }

                    current = VisualTreeHelper.GetParent(current);
                }

                return null;
            }
        }

        public void AddToSelection()
        {
            UpdateSelection(true);
        }

        public void RemoveFromSelection()
        {
            UpdateSelection(false);
        }

        public void Select()
        {
            UpdateSelection(true);
        }

        public void Invoke()
        {
            if (OwnerItem.GetEffectiveCanUserInvoke())
            {
                OwnerItem.RaiseItemInvoked(ItemContainerInteractionTrigger.AutomationInvoke, OwnerItem);
            }
        }

        public override object GetPattern(PatternInterface patternInterface)
        {
            if (patternInterface == PatternInterface.SelectionItem && OwnerItem.GetEffectiveCanUserSelect())
            {
                return this;
            }

            if (patternInterface == PatternInterface.Invoke && OwnerItem.GetEffectiveCanUserInvoke())
            {
                return this;
            }

            return base.GetPattern(patternInterface);
        }

        internal void RaiseIsSelectedChanged(bool oldValue, bool newValue)
        {
            RaisePropertyChangedEvent(SelectionItemPatternIdentifiers.IsSelectedProperty, oldValue, newValue);
            if (newValue)
            {
                RaiseAutomationEvent(AutomationEvents.SelectionItemPatternOnElementSelected);
            }
            else
            {
                RaiseAutomationEvent(AutomationEvents.SelectionItemPatternOnElementRemovedFromSelection);
            }
        }

        protected override string GetClassNameCore()
        {
            return nameof(ItemContainer);
        }

        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.ListItem;
        }

        protected override string GetLocalizedControlTypeCore()
        {
            return GetDefaultControlName();
        }

        protected override string GetNameCore()
        {
            string name = AutomationProperties.GetName(OwnerItem);
            if (string.IsNullOrEmpty(name))
            {
                name = GetChildStringRepresentation(OwnerItem.Child);
            }

            return string.IsNullOrEmpty(name) ? GetDefaultControlName() : name;
        }

        private static string GetChildStringRepresentation(UIElement child)
        {
            if (child is TextBlock textBlock)
            {
                return textBlock.Text;
            }

            if (child is ContentControl contentControl)
            {
                return contentControl.Content?.ToString();
            }

            return child?.ToString();
        }

        private static string GetDefaultControlName()
        {
            return ResourceAccessor.GetLocalizedStringResource(SR_ItemContainerDefaultControlName);
        }

        private void UpdateSelection(bool isSelected)
        {
            if (!isSelected || OwnerItem.GetEffectiveCanUserSelect())
            {
                OwnerItem.IsSelected = isSelected;
            }
        }

        private ItemContainer OwnerItem => (ItemContainer)Owner;
    }
}
