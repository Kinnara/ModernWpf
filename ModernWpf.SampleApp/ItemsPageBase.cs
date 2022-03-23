//*********************************************************
//
// Copyright (c) Microsoft. All rights reserved.
// THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
// IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
// PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//*********************************************************
using ModernWpf.Controls;
using ModernWpf.Media.Animation;
using ModernWpf.SampleApp.DataModel;
using SamplesCommon;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GridView = ModernWpf.Controls.GridView;
using Page = ModernWpf.Controls.Page;

namespace ModernWpf.SampleApp
{
    public abstract class ItemsPageBase : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string _itemId;
        private IEnumerable<ControlInfoDataItem> _items;

        public IEnumerable<ControlInfoDataItem> Items
        {
            get { return _items; }
            set { SetProperty(ref _items, value); }
        }

        /// <summary>
        /// Gets a value indicating whether the application's view is currently in "narrow" mode - i.e. on a mobile-ish device.
        /// </summary>
        protected virtual bool GetIsNarrowLayoutState()
        {
            throw new NotImplementedException();
        }

        protected void OnItemGridViewItemClick(object sender, ItemClickEventArgs e)
        {
            var gridView = (GridView)sender;
            var item = (ControlInfoDataItem)e.ClickedItem;

            _itemId = item.UniqueId;

            this.Frame.Navigate(typeof(ItemPage), _itemId, new DrillInNavigationTransitionInfo());
        }

        protected void OnItemGridViewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up)
            {
                //var nextElement = FocusManager.FindNextElement(FocusNavigationDirection.Up);
                //if (nextElement?.GetType() == typeof(NavigationViewItem))
                //{
                //    NavigationRootPage.Current.PageHeader.Focus();
                //}
                //else
                //{
                //    FocusManager.(FocusNavigationDirection.Up);
                //}
            }
        }

        protected async void OnItemGridViewLoaded(object sender, RoutedEventArgs e)
        {
            if (_itemId != null)
            {
                var gridView = (GridView)sender;
                var items = gridView.ItemsSource as IEnumerable<ControlInfoDataItem>;
                var item = items?.FirstOrDefault(s => s.UniqueId == _itemId);
                if (item != null)
                {
                    gridView.ScrollIntoView(item);

                    if (NavigationRootPage.Current.IsFocusSupported)
                    {
                        //((GridViewItem)gridView.ContainerFromItem(item))?.Focus(FocusState.Programmatic);
                    }
                }
            }
        }

        protected void OnItemGridViewSizeChanged(object sender, SizeChangedEventArgs e)
        {
            var gridView = (GridView)sender;

            //if (gridView.ItemsPanelRoot is ItemsWrapGrid wrapGrid)
            //{
            //    if (GetIsNarrowLayoutState())
            //    {
            //        double wrapGridPadding = 88;
            //        wrapGrid.HorizontalAlignment = HorizontalAlignment.Center;

            //        wrapGrid.ItemWidth = gridView.ActualWidth - gridView.Padding.Left - gridView.Padding.Right - wrapGridPadding;
            //    }
            //    else
            //    {
            //        wrapGrid.HorizontalAlignment = HorizontalAlignment.Left;
            //        wrapGrid.ItemWidth = double.NaN;
            //    }
            //}
        }

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(storage, value)) return false;

            storage = value;
            NotifyPropertyChanged(propertyName);
            return true;
        }

        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
