using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Controls;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.Repeater;

[TestClass]
public class SelectionModelApiTests
{
    [TestMethod]
    public void ValidateOneLevelSingleSelectionNoSource()
    {
        WpfTestHost.Run(() =>
        {
            var selectionModel = new SelectionModel { SingleSelect = true };
            Select(selectionModel, 4, true);
            ValidateSelection(selectionModel, [Path(4)]);
            Select(selectionModel, 4, false);
            ValidateSelection(selectionModel, []);
        });
    }

    [TestMethod]
    public void ValidateOneLevelSingleSelection()
    {
        WpfTestHost.Run(() =>
        {
            var selectionModel = new SelectionModel { SingleSelect = true };
            selectionModel.Source = Enumerable.Range(0, 10).ToList();

            Select(selectionModel, 3, true);
            ValidateSelection(selectionModel, [Path(3)], [Path()]);
            Select(selectionModel, 3, false);
            ValidateSelection(selectionModel, []);

            Select(selectionModel, Path(4), true);
            ValidateSelection(selectionModel, [Path(4)], [Path()]);
            Select(selectionModel, Path(4), false);
            ValidateSelection(selectionModel, []);
        });
    }

    [TestMethod]
    public void ValidateSelectionChangedEventSingleSelection()
    {
        WpfTestHost.Run(() =>
        {
            var selectionModel = new SelectionModel { SingleSelect = true };
            selectionModel.Source = Enumerable.Range(0, 10).ToList();

            var select = true;
            var selectionChangedFiredCount = 0;
            selectionModel.SelectionChanged += (sender, args) =>
            {
                selectionChangedFiredCount++;

                if (select)
                {
                    ValidateSelection(selectionModel, [Path(4)], [Path()]);
                }
                else
                {
                    ValidateSelection(selectionModel, []);
                }
            };

            Select(selectionModel, Path(4), select);
            Assert.AreEqual(1, selectionChangedFiredCount);

            select = false;
            Select(selectionModel, Path(4), select);
            Assert.AreEqual(2, selectionChangedFiredCount);
        });
    }

    [TestMethod]
    public void ValidateSelectionChangedEventMultipleSelection()
    {
        WpfTestHost.Run(() =>
        {
            var selectionModel = new SelectionModel();
            selectionModel.Source = Enumerable.Range(0, 10).ToList();

            var selectionChangedFiredCount = 0;
            selectionModel.SelectionChanged += (sender, args) =>
            {
                selectionChangedFiredCount++;
                ValidateSelection(selectionModel, [Path(4)], [Path()]);
            };

            Select(selectionModel, 4, true);
            Assert.AreEqual(1, selectionChangedFiredCount);
        });
    }

    [TestMethod]
    public void ValidateCanSetSelectedIndex()
    {
        WpfTestHost.Run(() =>
        {
            var model = new SelectionModel();
            var index = IndexPath.CreateFrom(34);
            model.SelectedIndex = index;
            AssertIndexPathEqual(index, model.SelectedIndex);
        });
    }

    [TestMethod]
    public void ValidateOneLevelMultipleSelection()
    {
        WpfTestHost.Run(() =>
        {
            var selectionModel = new SelectionModel();
            selectionModel.Source = Enumerable.Range(0, 10).ToList();

            Select(selectionModel, 4, true);
            ValidateSelection(selectionModel, [Path(4)], [Path()]);
            SelectRangeFromAnchor(selectionModel, 8, true);
            ValidateSelection(selectionModel, [Path(4), Path(5), Path(6), Path(7), Path(8)], [Path()]);

            ClearSelection(selectionModel);
            SetAnchorIndex(selectionModel, 6);
            SelectRangeFromAnchor(selectionModel, 3, true);
            ValidateSelection(selectionModel, [Path(3), Path(4), Path(5), Path(6)], [Path()]);

            SetAnchorIndex(selectionModel, 4);
            SelectRangeFromAnchor(selectionModel, 5, false);
            ValidateSelection(selectionModel, [Path(3), Path(6)], [Path()]);
        });
    }

    [TestMethod]
    public void ValidateTwoLevelSingleSelection()
    {
        WpfTestHost.Run(() =>
        {
            var selectionModel = new SelectionModel();
            selectionModel.Source = CreateNestedData(levels: 1, groupsAtLevel: 2, countAtLeaf: 2);

            Select(selectionModel, 1, 1, true);
            ValidateSelection(selectionModel, [Path(1, 1)], [Path(), Path(1)]);
            Select(selectionModel, 1, 1, false);
            ValidateSelection(selectionModel, []);
        });
    }

    [TestMethod]
    public void ValidateTwoLevelMultipleSelection()
    {
        WpfTestHost.Run(() =>
        {
            var selectionModel = new SelectionModel();
            selectionModel.Source = CreateNestedData(levels: 1, groupsAtLevel: 3, countAtLeaf: 3);

            Select(selectionModel, 1, 2, true);
            ValidateSelection(selectionModel, [Path(1, 2)], [Path(), Path(1)]);
            SelectRangeFromAnchor(selectionModel, 2, 2, true);
            ValidateSelection(
                selectionModel,
                [Path(1, 2), Path(2), Path(2, 0), Path(2, 1), Path(2, 2)],
                [Path(), Path(1)],
                selectedInnerNodes: 1);

            ClearSelection(selectionModel);
            SetAnchorIndex(selectionModel, 2, 1);
            SelectRangeFromAnchor(selectionModel, 0, 1, true);
            ValidateSelection(
                selectionModel,
                [Path(0, 1), Path(0, 2), Path(1, 0), Path(1, 1), Path(1, 2), Path(1), Path(2, 0), Path(2, 1)],
                [Path(), Path(0), Path(2)],
                selectedInnerNodes: 1);

            SetAnchorIndex(selectionModel, 1, 1);
            SelectRangeFromAnchor(selectionModel, 2, 0, false);
            ValidateSelection(
                selectionModel,
                [Path(0, 1), Path(0, 2), Path(1, 0), Path(2, 1)],
                [Path(), Path(1), Path(0), Path(2)]);

            ClearSelection(selectionModel);
            ValidateSelection(selectionModel, []);
        });
    }

    [TestMethod]
    public void ValidateNestedSingleSelection()
    {
        WpfTestHost.Run(() =>
        {
            var selectionModel = new SelectionModel { SingleSelect = true };
            selectionModel.Source = CreateNestedData(levels: 3, groupsAtLevel: 2, countAtLeaf: 2);

            var path = Path(1, 0, 1, 1);
            Select(selectionModel, path, true);
            ValidateSelection(selectionModel, [path], [Path(), Path(1), Path(1, 0), Path(1, 0, 1)]);

            var nextPath = Path(0, 0, 1, 0);
            Select(selectionModel, nextPath, true);
            ValidateSelection(selectionModel, [nextPath], [Path(), Path(0), Path(0, 0), Path(0, 0, 1)]);

            Select(selectionModel, nextPath, false);
            ValidateSelection(selectionModel, []);
        });
    }

    [TestMethod]
    public void ValidateInserts()
    {
        WpfTestHost.Run(() =>
        {
            var data = new ObservableCollection<int>(Enumerable.Range(0, 10));
            var selectionModel = new SelectionModel { Source = data };

            selectionModel.Select(3);
            selectionModel.Select(4);
            selectionModel.Select(5);
            ValidateSelection(selectionModel, [Path(3), Path(4), Path(5)], [Path()]);

            data.Insert(4, 41);
            data.Insert(4, 42);
            data.Insert(4, 43);
            ValidateSelection(selectionModel, [Path(3), Path(7), Path(8)], [Path()]);

            data.Insert(0, 100);
            data.Insert(0, 101);
            data.Insert(0, 102);
            ValidateSelection(selectionModel, [Path(6), Path(10), Path(11)], [Path()]);

            data.Insert(12, 1000);
            data.Insert(12, 1001);
            data.Insert(12, 1002);
            ValidateSelection(selectionModel, [Path(6), Path(10), Path(11)], [Path()]);
        });
    }

    [TestMethod]
    public void ValidateRemoves()
    {
        WpfTestHost.Run(() =>
        {
            var data = new ObservableCollection<int>(Enumerable.Range(0, 10));
            var selectionModel = new SelectionModel { Source = data };

            selectionModel.Select(6);
            selectionModel.Select(7);
            selectionModel.Select(8);
            ValidateSelection(selectionModel, [Path(6), Path(7), Path(8)], [Path()]);

            data.RemoveAt(0);
            ValidateSelection(selectionModel, [Path(5), Path(6), Path(7)], [Path()]);

            data.RemoveAt(3);
            data.RemoveAt(3);
            data.RemoveAt(3);
            ValidateSelection(selectionModel, [Path(3), Path(4)], [Path()]);

            data.RemoveAt(5);
            ValidateSelection(selectionModel, [Path(3), Path(4)], [Path()]);
        });
    }

    [TestMethod]
    public void CanReplaceItem()
    {
        WpfTestHost.Run(() =>
        {
            var data = new ObservableCollection<int>(Enumerable.Range(0, 10));
            var selectionModel = new SelectionModel { Source = data };

            selectionModel.Select(3);
            selectionModel.Select(4);
            selectionModel.Select(5);
            ValidateSelection(selectionModel, [Path(3), Path(4), Path(5)], [Path()]);

            data[3] = 300;
            data[4] = 400;
            ValidateSelection(selectionModel, [Path(5)], [Path()]);
        });
    }

    [TestMethod]
    public void ValidateClear()
    {
        WpfTestHost.Run(() =>
        {
            var data = new ObservableCollection<int>(Enumerable.Range(0, 10));
            var selectionModel = new SelectionModel { Source = data };

            selectionModel.Select(3);
            selectionModel.Select(4);
            selectionModel.Select(5);
            ValidateSelection(selectionModel, [Path(3), Path(4), Path(5)], [Path()]);

            data.Clear();
            ValidateSelection(selectionModel, []);
        });
    }

    [TestMethod]
    public void AlreadySelectedDoesNotRaiseEvent()
    {
        WpfTestHost.Run(() =>
        {
            var list = Enumerable.Range(0, 10).ToList();

            var selectionModel = new SelectionModel
            {
                Source = list,
                SingleSelect = true
            };
            selectionModel.Select(0);
            selectionModel.SelectionChanged += ThrowIfRaisedSelectionChanged;
            selectionModel.Select(0);

            selectionModel = new SelectionModel
            {
                Source = list,
                SingleSelect = true
            };
            selectionModel.SelectAt(IndexPath.CreateFrom(1));
            selectionModel.SelectionChanged += ThrowIfRaisedSelectionChanged;
            selectionModel.SelectAt(IndexPath.CreateFrom(1));

            selectionModel = new SelectionModel { Source = list };
            selectionModel.Select(1);
            selectionModel.Select(2);
            selectionModel.SelectionChanged += ThrowIfRaisedSelectionChanged;
            selectionModel.Select(1);
            selectionModel.Select(2);

            selectionModel = new SelectionModel { Source = list };
            selectionModel.SelectAt(IndexPath.CreateFrom(1));
            selectionModel.SelectAt(IndexPath.CreateFrom(2));
            selectionModel.SelectionChanged += ThrowIfRaisedSelectionChanged;
            selectionModel.SelectAt(IndexPath.CreateFrom(1));
            selectionModel.SelectAt(IndexPath.CreateFrom(2));
        });
    }

    [TestMethod]
    public void AlreadyDeselectedDoesNotRaiseEvent()
    {
        WpfTestHost.Run(() =>
        {
            var list = Enumerable.Range(0, 10).ToList();

            var selectionModel = new SelectionModel
            {
                Source = list,
                SingleSelect = true
            };
            selectionModel.SelectionChanged += ThrowIfRaisedSelectionChanged;
            selectionModel.Deselect(0);

            selectionModel = new SelectionModel
            {
                Source = list,
                SingleSelect = true
            };
            selectionModel.SelectionChanged += ThrowIfRaisedSelectionChanged;
            selectionModel.DeselectAt(IndexPath.CreateFrom(1));

            selectionModel = new SelectionModel { Source = list };
            selectionModel.SelectionChanged += ThrowIfRaisedSelectionChanged;
            selectionModel.Deselect(1);
            selectionModel.Deselect(2);

            selectionModel = new SelectionModel { Source = list };
            selectionModel.SelectionChanged += ThrowIfRaisedSelectionChanged;
            selectionModel.DeselectAt(IndexPath.CreateFrom(1));
            selectionModel.DeselectAt(IndexPath.CreateFrom(2));
        });
    }

    [TestMethod]
    public void ValidateSelectionModeChangeFromMultipleToSingle()
    {
        WpfTestHost.Run(() =>
        {
            var selectionModel = new SelectionModel();
            selectionModel.Source = Enumerable.Range(0, 10).ToList();

            selectionModel.Select(4);
            selectionModel.SingleSelect = true;
            AssertIndexPathEqual(Path(4), selectionModel.SelectedIndex);

            selectionModel.SingleSelect = false;
            selectionModel.Select(5);
            selectionModel.Select(6);

            selectionModel.SingleSelect = true;

            Assert.AreEqual(1, selectionModel.SelectedIndices.Count);
            AssertIndexPathEqual(selectionModel.SelectedIndices[0], selectionModel.SelectedIndex);
            AssertIndexPathEqual(Path(4), selectionModel.SelectedIndex);
        });
    }

    [TestMethod]
    public void ValidateSelectionModeChangeFromMultipleToSingleSelectionChangedEvent()
    {
        WpfTestHost.Run(() =>
        {
            var selectionModel = new SelectionModel();
            selectionModel.Source = Enumerable.Range(0, 10).ToList();

            selectionModel.Select(4);

            var selectionChangedFiredCount = 0;
            selectionModel.SelectionChanged += IncreaseCountIfRaisedSelectionChanged;

            selectionModel.SingleSelect = true;
            Assert.AreEqual(0, selectionChangedFiredCount);

            selectionModel.SelectionChanged -= IncreaseCountIfRaisedSelectionChanged;
            selectionModel.SingleSelect = false;
            selectionModel.Select(5);
            selectionModel.SelectionChanged += IncreaseCountIfRaisedSelectionChanged;

            selectionModel.SingleSelect = true;
            Assert.AreEqual(1, selectionChangedFiredCount);

            void IncreaseCountIfRaisedSelectionChanged(SelectionModel sender, SelectionModelSelectionChangedEventArgs args)
            {
                selectionChangedFiredCount++;
            }
        });
    }

    private static void Select(SelectionModel manager, int index, bool select)
    {
        if (select)
        {
            manager.Select(index);
        }
        else
        {
            manager.Deselect(index);
        }
    }

    private static void Select(SelectionModel manager, int groupIndex, int itemIndex, bool select)
    {
        if (select)
        {
            manager.Select(groupIndex, itemIndex);
        }
        else
        {
            manager.Deselect(groupIndex, itemIndex);
        }
    }

    private static void Select(SelectionModel manager, IndexPath index, bool select)
    {
        if (select)
        {
            manager.SelectAt(index);
        }
        else
        {
            manager.DeselectAt(index);
        }
    }

    private static void SelectRangeFromAnchor(SelectionModel manager, int index, bool select)
    {
        if (select)
        {
            manager.SelectRangeFromAnchor(index);
        }
        else
        {
            manager.DeselectRangeFromAnchor(index);
        }
    }

    private static void SelectRangeFromAnchor(SelectionModel manager, int groupIndex, int itemIndex, bool select)
    {
        if (select)
        {
            manager.SelectRangeFromAnchor(groupIndex, itemIndex);
        }
        else
        {
            manager.DeselectRangeFromAnchor(groupIndex, itemIndex);
        }
    }

    private static void ClearSelection(SelectionModel manager)
    {
        manager.ClearSelection();
    }

    private static void SetAnchorIndex(SelectionModel manager, int index)
    {
        manager.SetAnchorIndex(index);
    }

    private static void SetAnchorIndex(SelectionModel manager, int groupIndex, int itemIndex)
    {
        manager.SetAnchorIndex(groupIndex, itemIndex);
    }

    private static void ValidateSelection(
        SelectionModel selectionModel,
        IReadOnlyList<IndexPath> expectedSelected,
        IReadOnlyList<IndexPath>? expectedPartialSelected = null,
        int selectedInnerNodes = 0)
    {
        if (selectionModel.Source != null)
        {
            foreach (var index in GetIndexPathsInSource(selectionModel.Source))
            {
                var isSelected = selectionModel.IsSelectedAt(index);
                if (Contains(expectedSelected, index))
                {
                    Assert.IsTrue(isSelected!.Value, $"{index} is selected.");
                }
                else if (expectedPartialSelected != null && Contains(expectedPartialSelected, index))
                {
                    Assert.IsNull(isSelected, $"{index} is partially selected.");
                }
                else
                {
                    Assert.IsNotNull(isSelected, $"{index} has an explicit unselected state.");
                    Assert.IsFalse(isSelected!.Value, $"{index} is not selected.");
                }
            }
        }
        else
        {
            foreach (var index in expectedSelected)
            {
                Assert.IsTrue(selectionModel.IsSelectedAt(index)!.Value, $"{index} is selected.");
            }
        }

        if (expectedSelected.Count > 0)
        {
            AssertIndexPathEqual(expectedSelected[0], selectionModel.SelectedIndex);

            if (selectionModel.Source != null)
            {
                Assert.AreEqual(GetData(selectionModel, expectedSelected[0]), selectionModel.SelectedItem);
            }

            Assert.AreEqual(selectionModel.Source != null ? expectedSelected.Count - selectedInnerNodes : 0, selectionModel.SelectedItems.Count);
            Assert.AreEqual(expectedSelected.Count - selectedInnerNodes, selectionModel.SelectedIndices.Count);
        }
        else
        {
            Assert.AreEqual(0, selectionModel.SelectedItems.Count);
            Assert.AreEqual(0, selectionModel.SelectedIndices.Count);
        }
    }

    private static object? GetData(SelectionModel selectionModel, IndexPath indexPath)
    {
        object? data = selectionModel.Source;
        for (var i = 0; i < indexPath.GetSize(); i++)
        {
            if (data is not IList list)
            {
                throw new AssertFailedException("SelectionModel source path did not resolve to a list.");
            }

            data = list[indexPath.GetAt(i)];
        }

        return data;
    }

    private static IReadOnlyList<IndexPath> GetIndexPathsInSource(object source)
    {
        var paths = new List<IndexPath>();
        Traverse(source, nodeInfo =>
        {
            if (!Contains(paths, nodeInfo.Path))
            {
                paths.Add(nodeInfo.Path);
            }
        });

        return paths;
    }

    private static void Traverse(object root, Action<TreeWalkNodeInfo> nodeAction)
    {
        var pendingNodes = new Stack<TreeWalkNodeInfo>();
        pendingNodes.Push(new TreeWalkNodeInfo(root, Path()));

        while (pendingNodes.Count > 0)
        {
            var currentNode = pendingNodes.Pop();
            if (currentNode.Current is IList list)
            {
                for (var i = list.Count - 1; i >= 0; i--)
                {
                    var child = list[i];
                    if (child != null)
                    {
                        pendingNodes.Push(new TreeWalkNodeInfo(child, Append(currentNode.Path, i)));
                    }
                }
            }

            nodeAction(currentNode);
        }
    }

    private static bool Contains(IEnumerable<IndexPath> list, IndexPath index)
    {
        return list.Any(item => item.CompareTo(index) == 0);
    }

    private static void AssertIndexPathEqual(IndexPath expected, IndexPath actual)
    {
        Assert.IsNotNull(actual);
        Assert.AreEqual(0, expected.CompareTo(actual));
    }

    private static IndexPath Path(params int[] path)
    {
        return IndexPath.CreateFromIndices(path);
    }

    private static IndexPath Append(IndexPath path, int index)
    {
        var indices = new List<int>();
        for (var i = 0; i < path.GetSize(); i++)
        {
            indices.Add(path.GetAt(i));
        }

        indices.Add(index);
        return IndexPath.CreateFromIndices(indices);
    }

    private static List<object> CreateNestedData(int levels = 3, int groupsAtLevel = 5, int countAtLeaf = 10)
    {
        var data = new List<object>();
        if (levels != 0)
        {
            for (var i = 0; i < groupsAtLevel; i++)
            {
                data.Add(CreateNestedData(levels - 1, groupsAtLevel, countAtLeaf));
            }
        }
        else
        {
            for (var i = 0; i < countAtLeaf; i++)
            {
                data.Add(_nextData++);
            }
        }

        return data;
    }

    private static void ThrowIfRaisedSelectionChanged(SelectionModel sender, SelectionModelSelectionChangedEventArgs args)
    {
        throw new InvalidOperationException("SelectionChanged was raised when selection did not change.");
    }

    private static int _nextData;

    private readonly struct TreeWalkNodeInfo
    {
        public TreeWalkNodeInfo(object current, IndexPath path)
        {
            Current = current;
            Path = path;
        }

        public object Current { get; }

        public IndexPath Path { get; }
    }
}
