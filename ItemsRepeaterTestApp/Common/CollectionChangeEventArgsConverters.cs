// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace ItemsRepeaterTestApp.Common
{
    public static class CollectionChangeEventArgsConverters
    {
        public static NotifyCollectionChangedEventArgs CreateNotifyArgs(
            NotifyCollectionChangedAction action,
            int oldStartingIndex,
            int oldItemsCount,
            int newStartingIndex,
            int newItemsCount)
        {
            NotifyCollectionChangedEventArgs newArgs = null;

            switch (action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        List<object> addedItems = new List<object>();
                        for (int i = 0; i < newItemsCount; i++)
                        {
                            addedItems.Add(null);
                        }

                        newArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, addedItems, newStartingIndex);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    {
                        List<object> removedItems = new List<object>();
                        for (int i = 0; i < oldItemsCount; i++)
                        {
                            removedItems.Add(null);
                        }
                        newArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removedItems, oldStartingIndex);
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    {
                        List<object> addedItems = new List<object>();
                        for (int i = 0; i < newItemsCount; i++)
                        {
                            addedItems.Add(null);
                        }
                        List<object> removedItems = new List<object>();
                        for (int i = 0; i < oldItemsCount; i++)
                        {
                            removedItems.Add(null);
                        }
                        newArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, addedItems, removedItems, oldStartingIndex);
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    newArgs = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
                    break;
                default:
                    throw new InvalidOperationException();
            }

            return newArgs;
        }
    }
}
