// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using SplitDataSourceT = ModernWpf.Controls.SplitDataSourceBase<object, ModernWpf.Controls.NavigationViewSplitVectorID, double>;
using SplitVectorT = ModernWpf.Controls.SplitVector<object, ModernWpf.Controls.NavigationViewSplitVectorID>;

namespace ModernWpf.Controls
{
    enum NavigationViewSplitVectorID
    {
        NotInitialized = 0,
        PrimaryList = 1,
        OverflowList = 2,
        SkippedList = 3,
        Size = 4
    }

    class TopNavigationViewDataProvider : SplitDataSourceT
    {
        public TopNavigationViewDataProvider()
        {
            Func<object, int> lambda = (object value) =>
            {
                return IndexOf(value);
            };

            var primaryVector = new SplitVectorT(NavigationViewSplitVectorID.PrimaryList, lambda);
            var overflowVector = new SplitVectorT(NavigationViewSplitVectorID.OverflowList, lambda);

            InitializeSplitVectors(new List<SplitVectorT> { primaryVector, overflowVector });
        }

        public IList GetPrimaryItems()
        {
            return GetVector(NavigationViewSplitVectorID.PrimaryList).GetVector();
        }

        public IList GetOverflowItems()
        {
            return GetVector(NavigationViewSplitVectorID.OverflowList).GetVector();
        }

        // The raw data is from MenuItems or MenuItemsSource
        public void SetDataSource(object rawData)
        {
            if (ShouldChangeDataSource(rawData)) // avoid to create multiple of datasource for the same raw data
            {
                ItemsSourceView dataSource = null;
                if (rawData != null)
                {
                    dataSource = new InspectingDataSource(rawData);
                }
                ChangeDataSource(dataSource);
                m_rawDataSource = rawData;
                if (dataSource != null)
                {
                    MoveAllItemsToPrimaryList();
                }
            }
        }

        public bool ShouldChangeDataSource(object rawData)
        {
            return rawData != m_rawDataSource;
        }

        public void OnRawDataChanged(Action<NotifyCollectionChangedEventArgs> dataChangeCallback)
        {
            m_dataChangeCallback = dataChangeCallback;
        }

        internal override int IndexOf(object value)
        {
            if (m_dataSource is { } dataSource)
            {
                return dataSource.IndexOf(value);
            }
            return -1;
        }

        internal override object GetAt(int index)
        {
            if (m_dataSource is { } dataSource)
            {
                return dataSource.GetAt(index);
            }
            return null;
        }

        internal override int Size()
        {
            if (m_dataSource is { } dataSource)
            {
                return dataSource.Count;
            }
            return 0;
        }

        internal override NavigationViewSplitVectorID DefaultVectorIDOnInsert()
        {
            return NavigationViewSplitVectorID.NotInitialized;
        }

        internal override double DefaultAttachedData()
        {
            return double.MinValue;
        }

        public void MoveAllItemsToPrimaryList()
        {
            for (int i = 0; i < Size(); i++)
            {
                MoveItemToVector(i, NavigationViewSplitVectorID.PrimaryList);
            }
        }

        public List<int> ConvertPrimaryIndexToIndex(List<int> indexesInPrimary)
        {
            List<int> indexes = null;
            if (indexesInPrimary.Count > 0)
            {
                var vector = GetVector(NavigationViewSplitVectorID.PrimaryList);
                if (vector != null)
                {
                    // transform PrimaryList index to OrignalVector index
                    indexes = indexesInPrimary.Select(index =>
                    {
                        return vector.IndexToIndexInOriginalVector(index);
                    }).ToList();
                }
            }
            return indexes ?? new List<int>();
        }

        public int ConvertOriginalIndexToIndex(int originalIndex)
        {
            var vector = GetVector(IsItemInPrimaryList(originalIndex) ? NavigationViewSplitVectorID.PrimaryList : NavigationViewSplitVectorID.OverflowList);
            return vector.IndexFromIndexInOriginalVector(originalIndex);
        }

        public void MoveItemsOutOfPrimaryList(List<int> indexes)
        {
            MoveItemsToList(indexes, NavigationViewSplitVectorID.OverflowList);
        }

        public void MoveItemsToPrimaryList(List<int> indexes)
        {
            MoveItemsToList(indexes, NavigationViewSplitVectorID.PrimaryList);
        }

        public void MoveItemsToList(List<int> indexes, NavigationViewSplitVectorID vectorID)
        {
            foreach (var index in indexes)
            {
                MoveItemToVector(index, vectorID);
            };
        }

        public int GetPrimaryListSize()
        {
            return GetPrimaryItems().Count;
        }

        public int GetNavigationViewItemCountInPrimaryList()
        {
            int count = 0;
            for (int i = 0; i < Size(); i++)
            {
                if (IsItemInPrimaryList(i) && IsContainerNavigationViewItem(i))
                {
                    count++;
                }
            }
            return count;
        }

        public int GetNavigationViewItemCountInTopNav()
        {
            int count = 0;
            for (int i = 0; i < Size(); i++)
            {
                if (IsContainerNavigationViewItem(i))
                {
                    count++;
                }
            }
            return count;
        }

        public void UpdateWidthForPrimaryItem(int indexInPrimary, double width)
        {
            var vector = GetVector(NavigationViewSplitVectorID.PrimaryList);
            if (vector != null)
            {
                var index = vector.IndexToIndexInOriginalVector(indexInPrimary);
                SetWidthForItem(index, width);
            }
        }

        public double WidthRequiredToRecoveryAllItemsToPrimary()
        {
            var width = 0.0;
            for (int i = 0; i < Size(); i++)
            {
                if (!IsItemInPrimaryList(i))
                {
                    width += GetWidthForItem(i);
                }
            }
            width -= m_overflowButtonCachedWidth;
            return Math.Max(0f, width);
        }

        public bool HasInvalidWidth(List<int> items)
        {
            bool hasInvalidWidth = false;
            foreach (var index in items)
            {
                if (!IsValidWidthForItem(index))
                {
                    hasInvalidWidth = true;
                    break;
                }
            }
            return hasInvalidWidth;
        }

        public double GetWidthForItem(int index)
        {
            var width = AttachedData(index);
            if (!IsValidWidth(width))
            {
                width = 0;
            }
            return width;
        }

        public double CalculateWidthForItems(List<int> items)
        {
            double width = 0;
            foreach (var index in items)
            {
                width += GetWidthForItem(index);
            }
            return width;
        }

        public void InvalidWidthCache()
        {
            ResetAttachedData(-1.0f);
        }

        public double OverflowButtonWidth()
        {
            return m_overflowButtonCachedWidth;
        }

        public void OverflowButtonWidth(double width)
        {
            m_overflowButtonCachedWidth = width;
        }

        public bool IsItemSelectableInPrimaryList(object value)
        {
            int index = IndexOf(value);
            return (index != -1);
        }

        public int IndexOf(object value, NavigationViewSplitVectorID vectorID)
        {
            return IndexOfImpl(value, vectorID);
        }

        private void OnDataSourceChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        OnInsertAt(args.NewStartingIndex, args.NewItems.Count);
                        break;
                    }

                case NotifyCollectionChangedAction.Remove:
                    {
                        OnRemoveAt(args.OldStartingIndex, args.OldItems.Count);
                        break;
                    }

                case NotifyCollectionChangedAction.Reset:
                    {
                        OnClear();
                        break;
                    }

                case NotifyCollectionChangedAction.Replace:
                    {
                        OnRemoveAt(args.OldStartingIndex, args.OldItems.Count);
                        OnInsertAt(args.NewStartingIndex, args.NewItems.Count);
                        break;
                    }
            }
            if (m_dataChangeCallback != null)
            {
                m_dataChangeCallback(args);
            }
        }

        private bool IsValidWidth(double width)
        {
            return (width >= 0) && (width < double.MaxValue);
        }

        public bool IsValidWidthForItem(int index)
        {
            var width = AttachedData(index);
            return IsValidWidth(width);
        }

        private void SetWidthForItem(int index, double width)
        {
            if (IsValidWidth(width))
            {
                AttachedData(index, width);
            }
        }

        private void ChangeDataSource(ItemsSourceView newValue)
        {
            var oldValue = m_dataSource;
            if (oldValue != newValue)
            {
                // update to the new datasource.

                if (oldValue != null)
                {
                    oldValue.CollectionChanged -= OnDataSourceChanged;
                }

                Clear();

                m_dataSource = newValue;
                SyncAndInitVectorFlagsWithID(NavigationViewSplitVectorID.NotInitialized, DefaultAttachedData());

                if (newValue != null)
                {
                    newValue.CollectionChanged += OnDataSourceChanged;
                }
            }

            // Move all to primary list
            MoveItemsToVector(NavigationViewSplitVectorID.NotInitialized);
        }

        public bool IsItemInPrimaryList(int index)
        {
            return GetVectorIDForItem(index) == NavigationViewSplitVectorID.PrimaryList;
        }

        private bool IsContainerNavigationViewItem(int index)
        {
            bool isContainerNavigationViewItem = true;

            var item = GetAt(index);
            if (item != null && (item is NavigationViewItemHeader || item is NavigationViewItemSeparator))
            {
                isContainerNavigationViewItem = false;
            }
            return isContainerNavigationViewItem;
        }

        private bool IsContainerNavigationViewHeader(int index)
        {
            bool isContainerNavigationViewHeader = false;

            var item = GetAt(index);
            if (item != null && item is NavigationViewItemHeader)
            {
                isContainerNavigationViewHeader = true;
            }
            return isContainerNavigationViewHeader;
        }

        ItemsSourceView m_dataSource;
        // If the raw datasource is the same, we don't need to create new ItemsSourceView object.
        object m_rawDataSource;
        Action<NotifyCollectionChangedEventArgs> m_dataChangeCallback;
        double m_overflowButtonCachedWidth;
    }
}