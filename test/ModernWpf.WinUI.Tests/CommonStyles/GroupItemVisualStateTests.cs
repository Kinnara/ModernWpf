using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Controls;
using ModernWpf.WinUI.TestApp;
using ModernWpf.WinUI.TestInfra;

namespace ModernWpf.WinUI.Tests.CommonStyles;

[TestClass]
public class GroupItemVisualStateTests
{
    [TestMethod]
    public void GroupItemStyleUsesOfficialWpfFluentTemplateShape()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var style = (Style)Application.Current.FindResource(typeof(GroupItem));
            Assert.AreEqual(typeof(GroupItem), style.TargetType);
            Assert.IsNull(style.BasedOn);
            Assert.IsNull(Application.Current.TryFindResource("DefaultGroupItemStyle"));

            var setters = style.Setters.OfType<Setter>().ToArray();
            Assert.AreEqual(1, setters.Length);
            Assert.AreEqual(Control.TemplateProperty, setters[0].Property);
            Assert.IsFalse(setters.Any(item => item.Property == Control.OverridesDefaultStyleProperty));

            var groupItem = new GroupItem { Content = "Group header" };
            using var host = new TestWindowHost(groupItem, width: 240, height: 120);
            host.UpdateLayout();

            var header = GetTemplateChild<ContentPresenter>(groupItem, "PART_Header");
            var itemsPresenter = GetTemplateChild<ItemsPresenter>(groupItem, "ItemsPresenter");

            Assert.AreEqual(typeof(ContentPresenter), header.GetType());
            Assert.AreEqual(typeof(ItemsPresenter), itemsPresenter.GetType());
            Assert.AreEqual(new Thickness(5, 0, 0, 0), itemsPresenter.Margin);
            Assert.IsNull(VisualTreeTestHelper.FindDescendant<ListViewHeaderItem>(groupItem));
        });
    }

    [TestMethod]
    public void CollectionViewGroupTemplateUsesOfficialWpfFluentHeaderBinding()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var templateKey = new DataTemplateKey(typeof(CollectionViewGroup));
            var template = Application.Current.FindResource(templateKey) as DataTemplate
                ?? throw new AssertFailedException("Expected official WPF Fluent CollectionViewGroup data template.");
            Assert.AreEqual(typeof(CollectionViewGroup), template.DataType);

            var presenter = template.LoadContent() as ContentPresenter
                ?? throw new AssertFailedException("Expected CollectionViewGroup template root to be a ContentPresenter.");
            var binding = BindingOperations.GetBinding(presenter, ContentControl.ContentProperty)
                ?? throw new AssertFailedException("Expected CollectionViewGroup template to bind ContentPresenter.Content.");
            Assert.AreEqual("Name", binding.Path.Path);
        });
    }

    private static T GetTemplateChild<T>(Control control, string name)
        where T : DependencyObject
    {
        return control.Template.FindName(name, control) as T
            ?? throw new AssertFailedException($"Expected {control.GetType().Name} template child '{name}' to be a {typeof(T).Name}.");
    }
}
