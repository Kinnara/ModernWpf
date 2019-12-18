// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using ItemsRepeaterTestApp.Samples;
using ItemsRepeaterTestApp.Samples.Selection;
using System;
using System.Windows.Controls;

namespace ItemsRepeaterTestApp
{
    public partial class RepeaterTestUIPage : Page
    {
        public RepeaterTestUIPage()
        {
            InitializeComponent();

            //defaultDemo.Click += delegate
            //{
            //    Navigate(typeof(Defaults));
            //};

            basicDemo.Click += delegate
            {
                Navigate(typeof(BasicDemo));
            };

            itemsSourceDemo.Click += delegate
            {
                Navigate(typeof(ElementsInItemsSourcePage));
            };

            itemTemplateDemo.Click += delegate
            {
                Navigate(typeof(ItemTemplateDemo));
            };

            collectionChangeDemo.Click += delegate
            {
                Navigate(typeof(CollectionChangeDemo));
            };

            circleLayoutDemo.Click += delegate
            {
                Navigate(typeof(CircleLayoutSamplePage));
            };

            pinterestLayoutDemo.Click += delegate
            {
                Navigate(typeof(PinterestLayoutSamplePage));
            };

            flowLayoutDemo.Click += delegate
            {
                Navigate(typeof(FlowLayoutDemoPage));
            };

            activityFeedLayoutDemo.Click += delegate
            {
                Navigate(typeof(ActivityFeedSamplePage));
            };

            storeDemo.Click += delegate
            {
                Navigate(typeof(StoreDemoPage));
            };

            flatSelectionDemo.Click += delegate
            {
                Navigate(typeof(FlatSample));
            };

            groupedSelectionDemo.Click += delegate
            {
                Navigate(typeof(GroupedSample));
            };

            treeSelectionDemo.Click += delegate
            {
                Navigate(typeof(TreeViewSample));
            };
        }

        private void Navigate(Type type, object parameter = null)
        {
            NavigationService.Navigate(Activator.CreateInstance(type), parameter);
        }
    }
}
