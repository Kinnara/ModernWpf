// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows.Automation.Peers;
using ModernWpf.Controls;

namespace ModernWpf.Automation.Peers
{
    public class RadioButtonsAutomationPeer : FrameworkElementAutomationPeer
    {
        public RadioButtonsAutomationPeer(RadioButtons owner)
            : base(owner)
        {
        }

        protected override string GetClassNameCore()
        {
            return nameof(RadioButtons);
        }

        protected override string GetNameCore()
        {
            var name = base.GetNameCore();
            if (string.IsNullOrEmpty(name) && Owner is RadioButtons radioButtons)
            {
                name = SharedHelpers.TryGetStringRepresentationFromObject(radioButtons.Header);
            }

            return name;
        }

        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.Group;
        }
    }
}
