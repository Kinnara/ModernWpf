using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Controls;
using ModernWpf.WinUI.TestApp;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.Repeater;

[TestClass]
public class ItemsRepeaterApiTests
{
    [TestMethod]
    public void ValidateElementToIndexMapping()
    {
        WpfTestHost.Run(() =>
        {
            var elementFactory = new RecyclingElementFactory
            {
                RecyclePool = new RecyclePool(),
                Templates = { { "Item", CreateTextBlockTemplate() } }
            };
            var repeater = new ItemsRepeater
            {
                ItemsSource = Enumerable.Range(0, 10).Select(i => $"Item #{i}"),
                ItemTemplate = elementFactory
            };

            using var host = CreateScrollHost(repeater);

            for (var i = 0; i < 10; i++)
            {
                var element = repeater.TryGetElement(i);
                Assert.IsNotNull(element);
                Assert.AreEqual($"Item #{i}", ((TextBlock)element).Text);
                Assert.AreEqual(i, repeater.GetElementIndex(element));
            }

            Assert.IsNull(repeater.TryGetElement(20));
        });
    }

    [TestMethod]
    public void ValidateRepeaterDefaults()
    {
        WpfTestHost.Run(() =>
        {
            var repeater = new ItemsRepeater
            {
                ItemsSource = Enumerable.Range(0, 10).Select(i => $"Item #{i}")
            };

            using var host = CreateScrollHost(repeater);

            for (var i = 0; i < 10; i++)
            {
                var element = repeater.TryGetElement(i);
                Assert.IsNotNull(element);
                Assert.AreEqual($"Item #{i}", ((TextBlock)element).Text);
                Assert.AreEqual(i, repeater.GetElementIndex(element));
            }

            Assert.IsNull(repeater.TryGetElement(20));
        });
    }

    [TestMethod]
    public void CanSetItemsSource()
    {
        WpfTestHost.Run(() =>
        {
            var repeater = new ItemsRepeater();
            repeater.ItemsSource = null;
            repeater.ItemsSource = Enumerable.Range(0, 5).Select(i => $"Item #{i}");

            repeater = new ItemsRepeater();
            repeater.ItemsSource = Enumerable.Range(0, 5).Select(i => $"Item #{i}");
            repeater.ItemsSource = Enumerable.Range(5, 5).Select(i => $"Item #{i}");
            repeater.ItemsSource = null;
            repeater.ItemsSource = Enumerable.Range(10, 5).Select(i => $"Item #{i}");
            repeater.ItemsSource = null;
        });
    }

    [TestMethod]
    public void ValidateGetSetItemsSource()
    {
        WpfTestHost.Run(() =>
        {
            var repeater = new ItemsRepeater();
            var dataSource = new ItemsSourceView(Enumerable.Range(0, 10).Select(i => $"Item #{i}"));

            repeater.SetValue(ItemsRepeater.ItemsSourceProperty, dataSource);

            Assert.AreSame(dataSource, repeater.GetValue(ItemsRepeater.ItemsSourceProperty));
            Assert.AreSame(dataSource, repeater.ItemsSourceView);
        });
    }

    [TestMethod]
    public void ValidateNullItemsSource()
    {
        WpfTestHost.Run(() =>
        {
            var repeater = new ItemsRepeater();

            var exception = Assert.ThrowsExactly<InvalidOperationException>(() => repeater.GetOrCreateElement(0));
            StringAssert.Contains(exception.Message, "ItemSource doesn't have a value");
        });
    }

    [TestMethod]
    public void GetOrCreateElementRejectsInvalidIndexesLikeWinUI()
    {
        WpfTestHost.Run(() =>
        {
            var repeater = new ItemsRepeater
            {
                ItemsSource = Enumerable.Range(0, 5).Select(i => $"Item #{i}")
            };

            var negativeException = Assert.ThrowsExactly<ArgumentException>(() => repeater.GetOrCreateElement(-1));
            StringAssert.Contains(negativeException.Message, "Argument index is invalid.");
            Assert.AreEqual("index", negativeException.ParamName);

            var pastEndException = Assert.ThrowsExactly<ArgumentException>(() => repeater.GetOrCreateElement(5));
            StringAssert.Contains(pastEndException.Message, "Argument index is invalid.");
            Assert.AreEqual("index", pastEndException.ParamName);
        });
    }

    [TestMethod]
    public void ResourceBackedItemTemplateSurvivesOuterListViewItemReplacement()
    {
        WpfTestHost.Run(() =>
        {
            var repeaterTemplate = (DataTemplate)XamlReader.Parse(
                @"<DataTemplate
                      xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
                      xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
                      xmlns:ui='clr-namespace:ModernWpf.Controls;assembly=ModernWpf.Controls'>
                      <StackPanel>
                          <StackPanel.Resources>
                              <DataTemplate x:Key='ItemsTemplate'>
                                  <TextBlock Text='{Binding}' />
                              </DataTemplate>
                          </StackPanel.Resources>
                          <ui:ItemsRepeater
                              ItemTemplate='{StaticResource ItemsTemplate}'
                              ItemsSource='{Binding Values}' />
                      </StackPanel>
                  </DataTemplate>");
            var plainTemplate = (DataTemplate)XamlReader.Parse(
                @"<DataTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'>
                      <TextBlock Text='Equal' />
                  </DataTemplate>");
            var items = new ObservableCollection<object>
            {
                new object(),
                new RepeaterValuesItem()
            };
            var listView = new System.Windows.Controls.ListView
            {
                ItemsSource = items,
                ItemTemplateSelector = new RepeaterItemTemplateSelector
                {
                    PlainTemplate = plainTemplate,
                    RepeaterTemplate = repeaterTemplate
                }
            };

            using var host = new TestWindowHost(listView, width: 400, height: 200);
            host.UpdateLayout();

            var nestedRepeater = VisualTreeTestHelper
                .EnumerateDescendants(listView)
                .OfType<ItemsRepeater>()
                .Single();
            Assert.IsInstanceOfType<DataTemplate>(nestedRepeater.ItemTemplate);
            Assert.IsNotNull(nestedRepeater.ItemTemplateShim);

            items[1] = new object();
            WpfTestHost.DoEvents();
            host.UpdateLayout();

            Assert.AreEqual(2, listView.Items.Count);
            Assert.IsNotNull(listView.ItemContainerGenerator.ContainerFromIndex(1));
            Assert.IsNull(nestedRepeater.ItemTemplate);
            Assert.IsNull(nestedRepeater.ItemTemplateShim);
            Assert.IsFalse(VisualTreeTestHelper
                .EnumerateDescendants(listView)
                .OfType<ItemsRepeater>()
                .Any());
        });
    }

    private static TestWindowHost CreateScrollHost(ItemsRepeater repeater)
    {
        var scrollViewer = new ScrollViewer
        {
            Width = 400,
            Height = 800,
            Content = repeater
        };
        var host = new TestWindowHost(scrollViewer, width: 400, height: 800);
        host.UpdateLayout();
        return host;
    }

    private static DataTemplate CreateTextBlockTemplate()
    {
        return (DataTemplate)XamlReader.Parse(
            @"<DataTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'>
                  <TextBlock Text='{Binding}' Height='50' />
              </DataTemplate>");
    }

    private sealed class RepeaterValuesItem
    {
        public ObservableCollection<string> Values { get; } = new() { "one", "two" };
    }

    private sealed class RepeaterItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? PlainTemplate { get; init; }

        public DataTemplate? RepeaterTemplate { get; init; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            return item is RepeaterValuesItem ? RepeaterTemplate! : PlainTemplate!;
        }
    }
}
