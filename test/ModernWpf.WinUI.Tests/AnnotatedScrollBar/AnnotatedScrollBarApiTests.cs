using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Controls;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.AnnotatedScrollBar;

[TestClass]
public class AnnotatedScrollBarApiTests
{
    [TestMethod]
    public void VerifyDefaultPropertyValues()
    {
        WpfTestHost.Run(() =>
        {
            var annotatedScrollBar = new ModernWpf.Controls.AnnotatedScrollBar();

            Assert.IsNotNull(annotatedScrollBar);
            Assert.IsNotNull(annotatedScrollBar.Labels);
            Assert.IsNull(annotatedScrollBar.LabelTemplate);
            Assert.IsNull(annotatedScrollBar.DetailLabelTemplate);
            Assert.AreEqual(0d, annotatedScrollBar.SmallChange);
            Assert.AreSame(annotatedScrollBar, annotatedScrollBar.ScrollController);
        });
    }

    [TestMethod]
    public void VerifyLabelCollectionIsInstanceScoped()
    {
        WpfTestHost.Run(() =>
        {
            var first = new ModernWpf.Controls.AnnotatedScrollBar();
            var second = new ModernWpf.Controls.AnnotatedScrollBar();

            first.Labels.Add(new AnnotatedScrollBarLabel("A", 10));

            Assert.AreEqual(1, first.Labels.Count);
            Assert.AreEqual(0, second.Labels.Count);
            Assert.AreNotSame(first.Labels, second.Labels);
        });
    }

    [TestMethod]
    public void VerifyPropertyGettersAndSetters()
    {
        WpfTestHost.Run(() =>
        {
            var labels = new ObservableCollection<AnnotatedScrollBarLabel>
            {
                new AnnotatedScrollBarLabel("First", 0),
                new AnnotatedScrollBarLabel("Second", 100)
            };
            var labelTemplate = new DataTemplate();
            var detailLabelTemplate = new DataTemplate();
            var annotatedScrollBar = new ModernWpf.Controls.AnnotatedScrollBar
            {
                Labels = labels,
                LabelTemplate = labelTemplate,
                DetailLabelTemplate = detailLabelTemplate,
                SmallChange = 24
            };

            Assert.AreSame(labels, annotatedScrollBar.Labels);
            Assert.AreSame(labelTemplate, annotatedScrollBar.LabelTemplate);
            Assert.AreSame(detailLabelTemplate, annotatedScrollBar.DetailLabelTemplate);
            Assert.AreEqual(24d, annotatedScrollBar.SmallChange);
        });
    }

    [TestMethod]
    public void VerifyLabelProperties()
    {
        WpfTestHost.Run(() =>
        {
            var label = new AnnotatedScrollBarLabel("Important section", 42);

            Assert.AreEqual("Important section", label.Content);
            Assert.AreEqual(42d, label.ScrollOffset);
            Assert.AreEqual("Important section", label.ToString());
        });
    }

    [TestMethod]
    public void VerifyTemplateUsesLabels()
    {
        WpfTestHost.Run(() =>
        {
            var labels = new ObservableCollection<AnnotatedScrollBarLabel>
            {
                new AnnotatedScrollBarLabel("A", 0),
                new AnnotatedScrollBarLabel("B", 50)
            };
            var annotatedScrollBar = new ModernWpf.Controls.AnnotatedScrollBar
            {
                Labels = labels
            };

            using var host = new TestWindowHost(annotatedScrollBar, width: 160, height: 200);

            var itemsControl = VisualTreeTestHelper
                .EnumerateDescendants(annotatedScrollBar)
                .OfType<ItemsControl>()
                .FirstOrDefault();

            Assert.IsNotNull(itemsControl);
            Assert.AreSame(labels, itemsControl!.ItemsSource);

            labels.Add(new AnnotatedScrollBarLabel("C", 100));
            host.UpdateLayout();

            Assert.AreEqual(3, itemsControl.Items.Count);
        });
    }
}
