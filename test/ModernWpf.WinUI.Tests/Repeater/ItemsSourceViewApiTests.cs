using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Controls;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.Repeater;

[TestClass]
public class ItemsSourceViewApiTests
{
    [TestMethod]
    public void CanCreateFromEnumerable()
    {
        WpfTestHost.Run(() =>
        {
            var dataSource = new ItemsSourceView(Enumerable.Range(0, 100));
            Assert.AreEqual(100, dataSource.Count);
            Assert.AreEqual(4, dataSource.GetAt(4));
        });
    }

    [TestMethod]
    public void CanCreateFromNotifyCollectionChangedList()
    {
        WpfTestHost.Run(() =>
        {
            var data = new ObservableCollection<string>(Enumerable.Range(0, 100).Select(i => $"Item #{i}"));
            var dataSource = new ItemsSourceView(data);
            var recorder = new CollectionChangeRecorder(dataSource);
            Assert.AreEqual(100, dataSource.Count);
            Assert.AreEqual("Item #4", dataSource.GetAt(4));

            data.Insert(4, "Inserted Item");
            data.RemoveAt(7);
            data[15] = "Replaced Item";
            data.Clear();

            VerifyRecordedCollectionChanges(
                expected:
                [
                    CreateNotifyArgs(NotifyCollectionChangedAction.Add, -1, 0, 4, 1),
                    CreateNotifyArgs(NotifyCollectionChangedAction.Remove, 7, 1, -1, 0),
                    CreateNotifyArgs(NotifyCollectionChangedAction.Replace, 15, 1, 15, 1),
                    CreateNotifyArgs(NotifyCollectionChangedAction.Reset, -1, 0, -1, 0)
                ],
                actual: recorder.RecordedArgs);
        });
    }

    [TestMethod]
    public void VerifyUniqueIdMappingInterface()
    {
        WpfTestHost.Run(() =>
        {
            var data = new ObservableVectorWithUniqueIds(Enumerable.Range(0, 10));
            var dataSource = new ItemsSourceView(data);
            Assert.AreEqual(10, dataSource.Count);
            Assert.IsTrue(dataSource.HasKeyIndexMapping);
            Assert.AreEqual(5, dataSource.IndexFromKey("5"));
            Assert.AreEqual("5", dataSource.KeyFromIndex(5));
        });
    }

    [TestMethod]
    public void VerifyIndexOfBehavior()
    {
        WpfTestHost.Run(() =>
        {
            var collections = new List<IEnumerable>
            {
                new ObservableVectorWithUniqueIds(Enumerable.Range(0, 10)),
                new ObservableCollection<int>(Enumerable.Range(0, 10))
            };

            foreach (var collection in collections)
            {
                var dataSource = new ItemsSourceView(collection);
                foreach (int i in collection)
                {
                    Assert.AreEqual(i, dataSource.IndexOf(i));
                }

                Assert.AreEqual(-1, dataSource.IndexOf(11));
            }

            var nullContainingEnumerable = new CustomEnumerable();
            var testingItemsSourceView = new ItemsSourceView(nullContainingEnumerable);

            Assert.AreEqual(1, testingItemsSourceView.IndexOf(null));
        });
    }

    [TestMethod]
    public void VerifyReadOnlyListCompatibility()
    {
        WpfTestHost.Run(() =>
        {
            var collection = new ReadOnlyNotifyPropertyChangedCollection<object>();
            var firstItem = "something1";

            collection.Data = new ObservableCollection<object>();
            collection.Data.Add(firstItem);
            collection.Data.Add("something2");
            collection.Data.Add("something3");

            var itemsSourceView = new ItemsSourceView(collection);
            Assert.AreEqual(3, itemsSourceView.Count);

            collection.Data.Add("something4");
            Assert.AreEqual(4, itemsSourceView.Count);

            Assert.AreEqual(firstItem, itemsSourceView.GetAt(0));
            Assert.AreEqual(0, itemsSourceView.IndexOf(firstItem));
        });
    }

    [TestMethod]
    public void VerifyNotifyCollectionChangeWithReadonlyListBehavior()
    {
        var invocationCount = 0;

        WpfTestHost.Run(() =>
        {
            var collection = new ReadOnlyNotifyPropertyChangedCollection<object>();

            var itemsSourceView = new ItemsSourceView(collection);
            itemsSourceView.CollectionChanged += ItemsSourceViewCollectionChanged;

            var underlyingData = new ObservableCollection<object>();
            collection.Data = underlyingData;

            Assert.AreEqual(1, invocationCount);
        });

        void ItemsSourceViewCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            invocationCount++;
        }
    }

    private static void VerifyRecordedCollectionChanges(
        NotifyCollectionChangedEventArgs[] expected,
        IReadOnlyList<NotifyCollectionChangedEventArgs> actual)
    {
        Assert.AreEqual(expected.Length, actual.Count);

        for (var i = 0; i < expected.Length; i++)
        {
            Assert.AreEqual(expected[i].Action, actual[i].Action);
            Assert.AreEqual(expected[i].NewStartingIndex, actual[i].NewStartingIndex);
            Assert.AreEqual(expected[i].OldStartingIndex, actual[i].OldStartingIndex);
            Assert.AreEqual(GetCount(expected[i].NewItems), GetCount(actual[i].NewItems));
            Assert.AreEqual(GetCount(expected[i].OldItems), GetCount(actual[i].OldItems));
        }
    }

    private static int GetCount(IList? list)
    {
        return list == null ? -1 : list.Count;
    }

    private static NotifyCollectionChangedEventArgs CreateNotifyArgs(
        NotifyCollectionChangedAction action,
        int oldStartingIndex,
        int oldItemsCount,
        int newStartingIndex,
        int newItemsCount)
    {
        switch (action)
        {
            case NotifyCollectionChangedAction.Add:
                return new NotifyCollectionChangedEventArgs(action, CreateItems(newItemsCount), newStartingIndex);

            case NotifyCollectionChangedAction.Remove:
                return new NotifyCollectionChangedEventArgs(action, CreateItems(oldItemsCount), oldStartingIndex);

            case NotifyCollectionChangedAction.Replace:
                return new NotifyCollectionChangedEventArgs(
                    action,
                    CreateItems(newItemsCount),
                    CreateItems(oldItemsCount),
                    oldStartingIndex);

            case NotifyCollectionChangedAction.Reset:
                return new NotifyCollectionChangedEventArgs(action);

            default:
                throw new InvalidOperationException();
        }
    }

    private static List<object?> CreateItems(int count)
    {
        var items = new List<object?>();
        for (var i = 0; i < count; i++)
        {
            items.Add(null);
        }

        return items;
    }

    private sealed class CollectionChangeRecorder
    {
        public CollectionChangeRecorder(ItemsSourceView source)
        {
            source.CollectionChanged += (sender, args) => RecordedArgs.Add(Clone(args));
        }

        public List<NotifyCollectionChangedEventArgs> RecordedArgs { get; } = [];

        private static NotifyCollectionChangedEventArgs Clone(NotifyCollectionChangedEventArgs args)
        {
            return CreateNotifyArgs(
                args.Action,
                args.OldStartingIndex,
                args.OldItems == null ? -1 : args.OldItems.Count,
                args.NewStartingIndex,
                args.NewItems == null ? -1 : args.NewItems.Count);
        }
    }

    private sealed class ObservableVectorWithUniqueIds : ObservableCollection<int>, IKeyIndexMapping
    {
        public ObservableVectorWithUniqueIds(IEnumerable<int> data) : base(data)
        {
        }

        public string KeyFromIndex(int index)
        {
            return index.ToString();
        }

        public int IndexFromKey(string key)
        {
            return int.Parse(key);
        }
    }

    private sealed class CustomEnumerable : IEnumerable<object?>
    {
        private readonly List<string?> _items =
        [
            "text",
            null,
            "foobar",
            "WinUI is awesome"
        ];

        public IEnumerator<object?> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    private sealed class ReadOnlyNotifyPropertyChangedCollection<T> : IReadOnlyList<T>, INotifyCollectionChanged, IKeyIndexMapping
    {
        private ObservableCollection<T>? _data;

        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        public ObservableCollection<T> Data
        {
            get
            {
                _data ??= [];
                return _data;
            }

            set
            {
                if (_data != value)
                {
                    if (_data != null)
                    {
                        _data.CollectionChanged -= OnCollectionChanged;
                    }

                    _data = value;

                    if (_data != null)
                    {
                        _data.CollectionChanged += OnCollectionChanged;
                    }
                }

                OnCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        public int Count => Data.Count;

        public T this[int index] => Data[index];

        public IEnumerator<T> GetEnumerator()
        {
            throw new NotImplementedException("This is not implemented and should not be used.");
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public string KeyFromIndex(int index)
        {
            return this[index]?.GetHashCode().ToString() ?? string.Empty;
        }

        public int IndexFromKey(string key)
        {
            throw new NotImplementedException();
        }

        private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            CollectionChanged?.Invoke(this, e);
        }
    }
}
