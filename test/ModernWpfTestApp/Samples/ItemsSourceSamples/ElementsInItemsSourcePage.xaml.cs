// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace MUXControlsTestApp.Samples
{
    public partial class ElementsInItemsSourcePage : Page
    {
        public ElementsInItemsSourcePage()
        {
            InitializeComponent();
            goBackButton.Click += delegate { NavigationService.GoBack(); };
        }
    }

    public class UICollection : ObservableCollection<UIElement>
    {
        public UICollection()
        {

        }
    }
}
