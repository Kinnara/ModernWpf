// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using ModernWpf.Controls;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace MUXControlsTestApp.Samples
{
    public partial class BasicDemo : Page
    {
        public BasicDemo()
        {
            InitializeComponent();
            goBackButton.Click += delegate { NavigationService.GoBack(); };
            repeater.ItemTemplate = Resources["elementFactory"];
            var stack = repeater.Layout as StackLayout;
            int numItems = (stack != null && stack.DisableVirtualization) ? 10 : 10000;
            repeater.ItemsSource = Enumerable.Range(0, numItems).Select(x => x.ToString());
        }

        private void OnSelectTemplateKey(RecyclingElementFactory sender, SelectTemplateEventArgs args)
        {
            args.TemplateKey = (int.Parse(args.DataContext.ToString()) % 2 == 0) ? "even" : "odd";
        }
    }
}
