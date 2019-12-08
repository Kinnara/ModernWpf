// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using ModernWpf.Automation.Peers;

namespace ModernWpf.Controls
{
    public class ToggleSplitButton : SplitButton
    {
        public ToggleSplitButton()
        {
        }

        #region IsChecked

        public static readonly DependencyProperty IsCheckedProperty =
            DependencyProperty.Register(
                nameof(IsChecked),
                typeof(bool),
                typeof(ToggleSplitButton),
                new FrameworkPropertyMetadata(
                    false,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal,
                    OnIsCheckedPropertyChanged));

        public bool IsChecked
        {
            get => (bool)GetValue(IsCheckedProperty);
            set => SetValue(IsCheckedProperty, value);
        }

        private static void OnIsCheckedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ToggleSplitButton)d).OnIsCheckedChanged();
        }

        #endregion

        public event TypedEventHandler<ToggleSplitButton, ToggleSplitButtonIsCheckedChangedEventArgs> IsCheckedChanged;

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new ToggleSplitButtonAutomationPeer(this);
        }

        private void OnIsCheckedChanged()
        {
            if (m_hasLoaded)
            {
                IsCheckedChanged?.Invoke(this, new ToggleSplitButtonIsCheckedChangedEventArgs());

                var peer = FrameworkElementAutomationPeer.FromElement(this);
                if (peer != null)
                {
                    var newValue = IsChecked ? ToggleState.On : ToggleState.Off;
                    var oldValue = (newValue == ToggleState.On) ? ToggleState.Off : ToggleState.On;
                    peer.RaisePropertyChangedEvent(TogglePatternIdentifiers.ToggleStateProperty, oldValue, newValue);
                }
            }

            UpdateVisualStates();
        }

        internal override void OnClickPrimary(object sender, RoutedEventArgs e)
        {
            Toggle();

            base.OnClickPrimary(sender, e);
        }

        internal override bool InternalIsChecked => IsChecked;

        internal void Toggle()
        {
            SetCurrentValue(IsCheckedProperty, !IsChecked);
        }
    }
}
