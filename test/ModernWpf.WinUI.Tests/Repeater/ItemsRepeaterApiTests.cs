using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Controls;
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

            var exception = Assert.ThrowsException<InvalidOperationException>(() => repeater.GetOrCreateElement(0));
            StringAssert.Contains(exception.Message, "ItemSource doesn't have a value");
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
}
