// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows.Automation.Peers;
using System.Windows.Controls;

namespace ModernWpf.Controls
{
    internal sealed class NumberBoxPlaceholderTextBlock : TextBlock
    {
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new PlaceholderAutomationPeer(this);
        }

        private sealed class PlaceholderAutomationPeer : TextBlockAutomationPeer
        {
            internal PlaceholderAutomationPeer(TextBlock owner)
                : base(owner)
            {
            }

            protected override bool IsControlElementCore()
            {
                return true;
            }

            protected override bool IsContentElementCore()
            {
                return false;
            }
        }
    }
}
