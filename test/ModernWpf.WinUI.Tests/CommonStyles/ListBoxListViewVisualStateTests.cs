using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Controls;
using ModernWpf.Controls.Primitives;
using ModernWpf.WinUI.TestApp;
using ModernWpf.WinUI.TestInfra;
using WpfListView = System.Windows.Controls.ListView;
using WpfListViewItem = System.Windows.Controls.ListViewItem;

namespace ModernWpf.WinUI.Tests.CommonStyles;

[TestClass]
public class ListBoxListViewVisualStateTests
{
    [TestMethod]
    public void DefaultListBoxStylesUseOfficialWpfFluentTemplateShape()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var defaultListBoxStyle = (Style)Application.Current.FindResource("DefaultListBoxStyle");
            var implicitListBoxStyle = (Style)Application.Current.FindResource(typeof(ListBox));
            Assert.AreEqual(typeof(ListBox), defaultListBoxStyle.TargetType);
            Assert.AreSame(defaultListBoxStyle, implicitListBoxStyle.BasedOn);

            var defaultItemStyle = (Style)Application.Current.FindResource("DefaultListBoxItemStyle");
            var implicitItemStyle = (Style)Application.Current.FindResource(typeof(ListBoxItem));
            Assert.AreEqual(typeof(ListBoxItem), defaultItemStyle.TargetType);
            Assert.AreSame(defaultItemStyle, implicitItemStyle.BasedOn);

            var itemSetters = defaultItemStyle.Setters.OfType<Setter>().ToArray();
            AssertDynamicResourceSetter(itemSetters, Control.ForegroundProperty, "ListBoxItemForeground");
            AssertDynamicResourceSetter(itemSetters, Control.FocusVisualStyleProperty, "DefaultCollectionFocusVisualStyle");
            AssertSetter(itemSetters, System.Windows.Controls.Border.CornerRadiusProperty, new CornerRadius(0));
            AssertNoSetter(itemSetters, FocusVisualHelper.UseSystemFocusVisualsProperty);

            var item = new ListBoxItem
            {
                Content = "ListBox content",
                IsSelected = true
            };

            using var host = new TestWindowHost(item);
            host.UpdateLayout();

            var presenter = VisualTreeTestHelper.FindDescendant<ContentPresenter>(item)
                ?? throw new AssertFailedException("Expected official WPF Fluent ListBoxItem template to use ContentPresenter.");
            Assert.IsNotInstanceOfType(presenter, typeof(ContentPresenterEx));
            Assert.AreEqual(item.Content, presenter.Content);
            Assert.IsNull(VisualTreeTestHelper.FindDescendant<ContentPresenterEx>(item));

            var border = VisualTreeTestHelper.FindDescendant<Border>(item)
                ?? throw new AssertFailedException("Expected official WPF Fluent ListBoxItem template root Border.");
            Assert.AreSame(item.TryFindResource("ListBoxItemSelectedBackgroundThemeBrush"), border.Background);
            Assert.AreSame(item.TryFindResource("ListBoxItemSelectedForegroundThemeBrush"), item.Foreground);
        });
    }

    [TestMethod]
    public void DefaultListViewStylesUseOfficialWpfFluentTemplateShape()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var defaultListViewStyle = (Style)Application.Current.FindResource("DefaultListViewStyle");
            var implicitListViewStyle = (Style)Application.Current.FindResource(typeof(WpfListView));
            Assert.AreEqual(typeof(WpfListView), defaultListViewStyle.TargetType);
            Assert.AreSame(defaultListViewStyle, implicitListViewStyle.BasedOn);
            Assert.IsInstanceOfType(Application.Current.FindResource("ViewIsGridViewConverter"), typeof(IsGridViewConverter));
            Assert.IsInstanceOfType(Application.Current.FindResource("GridViewTemplate"), typeof(ControlTemplate));

            var defaultItemStyle = (Style)Application.Current.FindResource("DefaultListViewItemStyle");
            var implicitItemStyle = (Style)Application.Current.FindResource(typeof(WpfListViewItem));
            Assert.AreEqual(typeof(WpfListViewItem), defaultItemStyle.TargetType);
            Assert.AreSame(defaultItemStyle, implicitItemStyle.BasedOn);

            var itemSetters = defaultItemStyle.Setters.OfType<Setter>().ToArray();
            AssertDynamicResourceSetter(itemSetters, Control.ForegroundProperty, "ListViewItemForeground");
            AssertDynamicResourceSetter(itemSetters, System.Windows.Controls.Border.CornerRadiusProperty, "ControlCornerRadius");
            AssertDynamicResourceSetter(itemSetters, Control.FocusVisualStyleProperty, "DefaultCollectionFocusVisualStyle");
            AssertSetter(itemSetters, Control.OverridesDefaultStyleProperty, true);
            AssertNoSetter(itemSetters, FocusVisualHelper.UseSystemFocusVisualsProperty);

            var item = new WpfListViewItem
            {
                Content = "ListView content",
                IsSelected = true
            };

            using var host = new TestWindowHost(item);
            host.UpdateLayout();

            var presenter = VisualTreeTestHelper.FindDescendant<ContentPresenter>(item)
                ?? throw new AssertFailedException("Expected official WPF Fluent ListViewItem template to use ContentPresenter.");
            Assert.IsNotInstanceOfType(presenter, typeof(ContentPresenterEx));
            Assert.AreEqual(item.Content, presenter.Content);
            Assert.IsNull(VisualTreeTestHelper.FindDescendant<ContentPresenterEx>(item));

            var activeRectangle = item.Template.FindName("ActiveRectangle", item) as FrameworkElement
                ?? throw new AssertFailedException("Expected official WPF Fluent ListViewItem selection indicator.");
            Assert.AreEqual(Visibility.Visible, activeRectangle.Visibility);
        });
    }

    [TestMethod]
    public void DefaultGridViewColumnHeaderStyleUsesOfficialWpfFluentTemplateShape()
    {
        WpfTestHost.Run(() =>
        {
            TestApplication.EnsureInitialized();

            var defaultHeaderStyle = (Style)Application.Current.FindResource("DefaultGridViewColumnHeaderStyle");
            Assert.AreEqual(typeof(GridViewColumnHeader), defaultHeaderStyle.TargetType);

            var header = new GridViewColumnHeader
            {
                Style = defaultHeaderStyle,
                Content = "Header content"
            };

            using var host = new TestWindowHost(header);
            host.UpdateLayout();

            var presenter = VisualTreeTestHelper.FindDescendant<ContentPresenter>(header)
                ?? throw new AssertFailedException("Expected official WPF Fluent GridViewColumnHeader template to use ContentPresenter.");
            Assert.IsNotInstanceOfType(presenter, typeof(ContentPresenterEx));
            Assert.AreEqual(header.Content, presenter.Content);
            Assert.IsNull(VisualTreeTestHelper.FindDescendant<ContentPresenterEx>(header));
            Assert.IsInstanceOfType(header.Template.FindName("PART_HeaderGripper", header), typeof(Thumb));
        });
    }

    [TestMethod]
    public void ListBoxListViewFilesDeleteWinUIGuessedTemplateBranches()
    {
        var repoRoot = FindRepoRoot();
        var text = string.Join(
            "\n",
            new[] { "ListBox.xaml", "ListBoxItem.xaml", "GridView.xaml", "ListView.xaml", "ListViewItem.xaml" }
                .Select(file => File.ReadAllText(Path.Combine(repoRoot, "ModernWpf", "Styles", file))));

        Assert.IsFalse(text.Contains("VisualStateEx", System.StringComparison.Ordinal));
        Assert.IsFalse(text.Contains("ContentPresenterEx", System.StringComparison.Ordinal));
        Assert.IsFalse(text.Contains("ScrollViewerEx", System.StringComparison.Ordinal));
        Assert.IsFalse(text.Contains("FocusVisualHelper", System.StringComparison.Ordinal));
        Assert.IsFalse(text.Contains("ListViewBaseItemRoundedChromeEnabled", System.StringComparison.Ordinal));
        Assert.IsFalse(text.Contains("PressedBackground", System.StringComparison.Ordinal));
        Assert.IsFalse(text.Contains("Selector.IsSelectionActive", System.StringComparison.Ordinal));
        Assert.IsFalse(text.Contains("ControlHelper.CornerRadius", System.StringComparison.Ordinal));
        Assert.IsFalse(text.Contains("System.Runtime", System.StringComparison.Ordinal));
        Assert.IsFalse(text.Contains("Fluent.Controls", System.StringComparison.Ordinal));
    }

    private static void AssertSetter(Setter[] setters, DependencyProperty property, object value)
    {
        var setter = setters.Single(item => item.Property == property);
        Assert.AreEqual(value, setter.Value);
    }

    private static void AssertDynamicResourceSetter(Setter[] setters, DependencyProperty property, string resourceKey)
    {
        var setter = setters.Single(item => item.Property == property);
        Assert.IsInstanceOfType(setter.Value, typeof(DynamicResourceExtension));
        Assert.AreEqual(resourceKey, ((DynamicResourceExtension)setter.Value).ResourceKey);
    }

    private static void AssertNoSetter(Setter[] setters, DependencyProperty property)
    {
        Assert.IsFalse(setters.Any(item => item.Property == property), $"Unexpected setter for {property.Name}.");
    }

    private static string FindRepoRoot()
    {
        var directory = new DirectoryInfo(System.AppContext.BaseDirectory);

        while (directory != null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "ModernWpf.sln")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        Assert.Fail("Could not locate ModernWpf.sln from the test output directory.");
        return string.Empty;
    }
}
