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
                DependencyObject parent = VisualTreeHelper.GetParent(OwnerItem);
                while (parent != null)
                {
                    if (parent is FrameworkElement element)
                    {
                        var peer = FrameworkElementAutomationPeer.FromElement(element)
                            ?? FrameworkElementAutomationPeer.CreatePeerForElement(element);
                        if (peer?.GetPattern(PatternInterface.Selection) != null)
                        {
                            return ProviderFromPeer(peer);
                        }
                    }

                    parent = VisualTreeHelper.GetParent(parent);
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
            if (!OwnerItem.IsEnabled)
            {
                throw new ElementNotEnabledException();
            }

            if (OwnerItem.CanUserInvoke)
            {
                OwnerItem.RaiseItemInvoked(ItemContainerInteractionTrigger.AutomationInvoke, null);
            }
        }

        public override object GetPattern(PatternInterface patternInterface)
        {
            if (patternInterface == PatternInterface.SelectionItem && OwnerItem.CanUserSelect)
            {
                return this;
            }

            if (patternInterface == PatternInterface.Invoke && OwnerItem.CanUserInvoke)
            {
                return this;
            }

            return base.GetPattern(patternInterface);
        }

        protected override string GetClassNameCore()
        {
            return nameof(ItemContainer);
        }

        protected override string GetLocalizedControlTypeCore()
        {
            return GetDefaultControlName();
        }

        protected override string GetNameCore()
        {
            var name = base.GetNameCore();
            if (string.IsNullOrEmpty(name) && OwnerItem.Child is FrameworkElement child)
            {
                name = AutomationProperties.GetName(child);
            }

            if (string.IsNullOrEmpty(name) && OwnerItem.Child is TextBlock textBlock)
            {
                name = textBlock.Text;
            }

            if (string.IsNullOrEmpty(name) && OwnerItem.Child is ContentControl contentControl)
            {
                name = contentControl.Content?.ToString();
            }

            if (string.IsNullOrEmpty(name) && OwnerItem.Child != null)
            {
                name = OwnerItem.Child.ToString();
            }

            return string.IsNullOrEmpty(name) ? GetDefaultControlName() : name;
        }

        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.ListItem;
        }

        private void UpdateSelection(bool isSelected)
        {
            if (!OwnerItem.IsEnabled)
            {
                throw new ElementNotEnabledException();
            }

            if (!isSelected || OwnerItem.CanUserSelect)
            {
                OwnerItem.IsSelected = isSelected;
            }
        }

        private static string GetDefaultControlName()
        {
            return ResourceAccessor.GetLocalizedStringResource(SR_ItemContainerDefaultControlName);
        }

        private ItemContainer OwnerItem => (ItemContainer)Owner;
    }
}
