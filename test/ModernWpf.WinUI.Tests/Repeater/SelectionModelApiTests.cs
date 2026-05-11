using System;
using System.Collections;
using System.Collections.Generic;
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

    private static void ValidateSelection(
        SelectionModel selectionModel,
        IReadOnlyList<IndexPath> expectedSelected,
        IReadOnlyList<IndexPath>? expectedPartialSelected = null)
    {
        if (selectionModel.Source is IList source)
        {
            for (var i = 0; i < source.Count; i++)
            {
                var index = Path(i);
                var isSelected = selectionModel.IsSelectedAt(index);
                if (Contains(expectedSelected, index))
                {
                    Assert.IsTrue(isSelected!.Value, $"{index} is selected.");
                }
                else
                {
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

        if (expectedPartialSelected != null)
        {
            foreach (var index in expectedPartialSelected)
            {
                Assert.IsNull(selectionModel.IsSelectedAt(index), $"{index} is partially selected.");
            }
        }

        if (expectedSelected.Count > 0)
        {
            AssertIndexPathEqual(expectedSelected[0], selectionModel.SelectedIndex);

            if (selectionModel.Source != null)
            {
                Assert.AreEqual(GetData(selectionModel, expectedSelected[0]), selectionModel.SelectedItem);
            }

            Assert.AreEqual(selectionModel.Source != null ? expectedSelected.Count : 0, selectionModel.SelectedItems.Count);
            Assert.AreEqual(expectedSelected.Count, selectionModel.SelectedIndices.Count);
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

    private static void ThrowIfRaisedSelectionChanged(SelectionModel sender, SelectionModelSelectionChangedEventArgs args)
    {
        throw new InvalidOperationException("SelectionChanged was raised when selection did not change.");
    }
}
