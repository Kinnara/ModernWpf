// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using ModernWpf.Controls;
using MUXControlsTestApp.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Automation;
using System.Windows.Controls;

namespace MUXControlsTestApp
{
    [TopLevelTestPage(Name = "RadioMenuFlyoutItem")]
    public sealed partial class RadioMenuFlyoutItemPage : TestPage
    {
        Dictionary<string, TextBlock> itemStates;

        public RadioMenuFlyoutItemPage()
        {
            this.InitializeComponent();

            itemStates = new Dictionary<string, TextBlock>();

            //if (ApiInformation.IsPropertyPresent("Windows.UI.Xaml.Controls.MenuFlyoutItem", "Icon"))
            {
                IconMenuFlyoutItem.Icon = new SymbolIcon(Symbol.Calendar);
                IconRadioMenuFlyoutItem.Icon = new SymbolIcon(Symbol.Calculator);
            }

            /*if (ApiInformation.IsPropertyPresent("Windows.UI.Xaml.Controls.Grid", "ColumnSpacing"))
            {
                ItemNames.Spacing = 4;
                ItemStates.Spacing = 4;
            }*/

            // register all RadioMenuFlyoutItems
            foreach (object item in ButtonMenuFlyout.Items)
            {
                if (item is RadioMenuItem)
                {
                    RadioMenuItem radioItem = item as RadioMenuItem;

                    radioItem.RegisterPropertyChangedCallback(RadioMenuItem.IsCheckedProperty, IsCheckedChanged);

                    TextBlock nameText = new TextBlock();
                    nameText.Text = (string)radioItem.Header;
                    ItemNames.Children.Add(nameText);

                    TextBlock stateText = new TextBlock();
                    AutomationProperties.SetName(stateText, (string)radioItem.Header + "State");
                    UpdateTextState(radioItem, stateText);
                    ItemStates.Children.Add(stateText);

                    itemStates.Add((string)radioItem.Header, stateText);
                }
            }
        }

        private void IsCheckedChanged(object o, EventArgs e)
        {
            RadioMenuItem radioItem = o as RadioMenuItem;
            TextBlock stateText;
            if (itemStates.TryGetValue((string)radioItem.Header, out stateText))
            {
                UpdateTextState(radioItem, stateText);
            }
        }

        private void UpdateTextState(RadioMenuItem item, TextBlock textBlock)
        {
            textBlock.Text = item.IsChecked ? "Checked" : "Unchecked";
        }
    }
}
