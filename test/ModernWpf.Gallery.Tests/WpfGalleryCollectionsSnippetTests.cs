using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModernWpf.Gallery.Pages.WpfGallery.Collections;
using static ModernWpf.Gallery.Tests.WpfGallerySnippetTestHelpers;

namespace ModernWpf.Gallery.Tests
{
    [TestClass]
    public class WpfGalleryCollectionsSnippetTests
    {
        [TestMethod]
        public void CollectionsControlExamplesMatchOfficialWpfGallerySampleCode()
        {
            WpfTestHost.Run(() =>
            {
                AssertExamples(
                    new DataGridPage(),
                    new ExpectedExample(
                        "Default DataGrid with ItemsSource.",
                        "<DataGrid ItemsSource=\"{Binding ViewModel.ProductsCollection, Mode=TwoWay}\" />"));

                AssertExamples(
                    new ListBoxPage(),
                    new ExpectedExample(
                        "ListBox with items defined inline.",
                        Lines(
                            "<ListBox SelectedIndex=\"0\">",
                            "<ListBoxItem>Blue</ListBoxItem>",
                            "<ListBoxItem>Green</ListBoxItem>",
                            "<ListBoxItem>Red</ListBoxItem>",
                            "<ListBoxItem>Yellow</ListBoxItem>",
                            "</ListBox>")),
                    new ExpectedExample(
                        "A ListBox with its ItemsSource and Height set.",
                        "<ListBox Height=\"100\" ItemsSource=\"{Binding ViewModel.MyItems}\" SelectedIndex=\"2\" />"));

                AssertExamples(
                    new ListViewPage(),
                    new ExpectedExample(
                        "Basic ListView with Simple DataTemplate.",
                        Lines(
                            "<ListView",
                            "Height=\"200\"",
                            "ItemsSource=\"{Binding ViewModel.BasicListViewItems, Mode=TwoWay}\"",
                            "SelectedIndex=\"2\"",
                            "SelectionMode=\"Single\">",
                            "<ListView.ItemTemplate>",
                            "<DataTemplate DataType=\"{x:Type models:Person}\">",
                            "<TextBlock Margin=\"8,4\" Text=\"{Binding Name, Mode=OneWay}\" />",
                            "</DataTemplate>",
                            "</ListView.ItemTemplate>",
                            "</ListView>")),
                    new ExpectedExample(
                        "ListView with Selection Support.",
                        Lines(
                            "<Grid>",
                            "<Grid.ColumnDefinitions>",
                            "<ColumnDefinition Width=\"*\" />",
                            "<ColumnDefinition Width=\"Auto\" />",
                            "</Grid.ColumnDefinitions>",
                            "<ListView",
                            "Grid.Column=\"0\"",
                            "Height=\"200\"",
                            "ItemsSource=\"{Binding BasicListViewItems, Mode=TwoWay}\"",
                            "SelectedIndex=\"1\"",
                            "SelectionMode=\"{Binding ListViewSelectionMode, Mode=OneWay}\">",
                            "<ListView.ItemTemplate>",
                            "<DataTemplate DataType=\"{x:Type models:Person}\">",
                            "<Grid Margin=\"8,0\">",
                            "<Grid.RowDefinitions>",
                            "<RowDefinition Height=\"*\" />",
                            "<RowDefinition Height=\"*\" />",
                            "</Grid.RowDefinitions>",
                            "<Grid.ColumnDefinitions>",
                            "<ColumnDefinition Width=\"Auto\" />",
                            "<ColumnDefinition Width=\"*\" />",
                            "</Grid.ColumnDefinitions>",
                            "<Ellipse",
                            "x:Name=\"Ellipse\"",
                            "Grid.RowSpan=\"2\"",
                            "Width=\"32\"",
                            "Height=\"32\"",
                            "Margin=\"6\"",
                            "HorizontalAlignment=\"Center\"",
                            "VerticalAlignment=\"Center\"",
                            "Fill=\"{DynamicResource SystemAccentColorPrimaryBrush}\" />",
                            "<TextBlock",
                            "Grid.Row=\"0\"",
                            "Grid.Column=\"1\"",
                            "Margin=\"12,6,0,0\"",
                            "FontWeight=\"Bold\"",
                            "Text=\"{Binding Name, Mode=OneWay}\" />",
                            "<TextBlock",
                            "Grid.Row=\"1\"",
                            "Grid.Column=\"1\"",
                            "Margin=\"12,0,0,6\"",
                            "Foreground=\"{DynamicResource TextFillColorPrimaryBrush}\"",
                            "Text=\"{Binding Company, Mode=OneWay}\" />",
                            "</Grid>",
                            "</DataTemplate>",
                            "</ListView.ItemTemplate>",
                            "</ListView>",
                            "<StackPanel",
                            "Grid.Column=\"1\"",
                            "MinWidth=\"120\"",
                            "Margin=\"12,0,0,0\"",
                            "VerticalAlignment=\"Top\">",
                            "<Label Content=\"Selection mode\" Target=\"{Binding ElementName=SelectionModeComboBox}\" />",
                            "<ComboBox x:Name=\"SelectionModeComboBox\" SelectedIndex=\"{Binding ListViewSelectionModeComboBoxSelectedIndex, Mode=TwoWay}\">",
                            "<ComboBoxItem Content=\"Single\" />",
                            "<ComboBoxItem Content=\"Multiple\" />",
                            "<ComboBoxItem Content=\"Extended\" />",
                            "</ComboBox>",
                            "</StackPanel>",
                            "</Grid>")),
                    new ExpectedExample(
                        "ListView with GridView.",
                        Lines(
                            "<ListView",
                            "Height=\"280\"",
                            "ItemsSource=\"{Binding ViewModel.GridViewItems}\">",
                            "<ListView.View>",
                            "<GridView>",
                            "<GridViewColumn",
                            "Header=\"First Name\"",
                            "Width=\"150\"",
                            "DisplayMemberBinding=\"{Binding FirstName}\" />",
                            "<GridViewColumn",
                            "Header=\"Last Name\"",
                            "Width=\"150\"",
                            "DisplayMemberBinding=\"{Binding LastName}\" />",
                            "<GridViewColumn",
                            "Header=\"Company\"",
                            "Width=\"200\"",
                            "DisplayMemberBinding=\"{Binding Company}\" />",
                            "</GridView>",
                            "</ListView.View>",
                            "</ListView>")));

                AssertExamples(
                    new TreeViewPage(),
                    new ExpectedExample(
                        "Simple TreeView.",
                        Lines(
                            "<TreeView AllowDrop=\"True\" ScrollViewer.CanContentScroll=\"False\">",
                            "<TreeViewItem",
                            "Header=\"Work Documents\"",
                            "IsExpanded=\"True\"",
                            "IsSelected=\"True\">",
                            "<TreeViewItem Header=\"Feature Schedule\" />",
                            "<TreeViewItem Header=\"Overall Project Plan\" />",
                            "</TreeViewItem>",
                            "<TreeViewItem Header=\"Personal Documents\">",
                            "<TreeViewItem Header=\"Contractor contact info\" />",
                            "<TreeViewItem Header=\"Home Remodel\">",
                            "<TreeViewItem Header=\"Paint Color Scheme\" />",
                            "<TreeViewItem Header=\"Flooring Woodgrain Type\" />",
                            "<TreeViewItem Header=\"Kitchen Cabinet Style\" />",
                            "</TreeViewItem>",
                            "</TreeViewItem>",
                            "</TreeView>")));
            });
        }
    }
}
