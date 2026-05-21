using System;
using System.Diagnostics;
using System.Windows;

namespace ModernWpf.Gallery.Pages
{
    public sealed partial class WhatsNewPage
    {
        public WhatsNewPage()
        {
            InitializeComponent();
            GalleryAutomation.SetHeadingLevel(TitleLabel, GalleryAutomationHeadingLevel.Level1);
            GalleryAutomation.SetHeadingLevel(DescriptionLabel, GalleryAutomationHeadingLevel.Level2);
            DataContext = this;
        }

        public Action<string> ItemRequested { get; set; }

        public string GridShorthandSyntaxXamlCode
        {
            get
            {
                return "<Grid RowDefinitions=\"Auto,Auto,Auto\" ColumnDefinitions=\"Auto 80 *\" HorizontalAlignment=\"Left\">\n"
                    + "    <TextBlock Grid.Row=\"0\" Grid.Column=\"0\" FontWeight=\"Bold\" Margin=\"0 0 10 0\">Sl. No.</TextBlock>\n"
                    + "    <TextBlock Grid.Row=\"0\" Grid.Column=\"1\" FontWeight=\"Bold\">Name</TextBlock>\n"
                    + "    <TextBlock Grid.Row=\"0\" Grid.Column=\"2\" FontWeight=\"Bold\">Description</TextBlock>\n"
                    + "    <TextBlock Grid.Row=\"1\" Grid.Column=\"0\">1</TextBlock>\n"
                    + "    <TextBlock Grid.Row=\"1\" Grid.Column=\"1\">Rectangle</TextBlock>\n"
                    + "    <TextBlock Grid.Row=\"1\" Grid.Column=\"2\" TextWrapping=\"Wrap\">Quadrilateral where all the adjacent sides form a right angle.</TextBlock>\n"
                    + "    <TextBlock Grid.Row=\"2\" Grid.Column=\"0\">2</TextBlock>\n"
                    + "    <TextBlock Grid.Row=\"2\" Grid.Column=\"1\">Circle</TextBlock>\n"
                    + "    <TextBlock Grid.Row=\"2\" Grid.Column=\"2\" TextWrapping=\"Wrap\">Set of all points that are equidistant from a fixed point.</TextBlock>\n"
                    + "</Grid>";
            }
        }

        public string AccentColorXamlCode
        {
            get
            {
                return "<StackPanel Orientation=\"Horizontal\" Height=\"50\">\n"
                    + "    <StackPanel.Resources>\n"
                    + "        <Style TargetType=\"Border\">\n"
                    + "            <Setter Property=\"Height\" Value=\"50\" />\n"
                    + "            <Setter Property=\"Width\" Value=\"30\" />\n"
                    + "        </Style>\n"
                    + "    </StackPanel.Resources>\n"
                    + "    <Border CornerRadius=\"2 0 0 2\" Background=\"{DynamicResource {x:Static SystemColors.AccentColorDark3BrushKey}}\" />\n"
                    + "    <Border Background=\"{DynamicResource {x:Static SystemColors.AccentColorDark2BrushKey}}\" />\n"
                    + "    <Border Background=\"{DynamicResource {x:Static SystemColors.AccentColorDark1BrushKey}}\" />\n"
                    + "    <Border Background=\"{DynamicResource {x:Static SystemColors.AccentColorBrushKey}}\" />\n"
                    + "    <Border Background=\"{DynamicResource {x:Static SystemColors.AccentColorLight1BrushKey}}\" />\n"
                    + "    <Border Background=\"{DynamicResource {x:Static SystemColors.AccentColorLight2BrushKey}}\" />\n"
                    + "    <Border CornerRadius=\"0 2 2 0\" Background=\"{DynamicResource {x:Static SystemColors.AccentColorLight3BrushKey}}\" />\n"
                    + "</StackPanel>";
            }
        }

        public string HyphenBasedLigatureXamlCode
        {
            get
            {
                return "<StackPanel Orientation=\"Horizontal\">\n"
                    + "    <TextBlock Margin=\"0 0 16 0\" FontFamily=\"Cascadia Code\" Text=\"--&gt;\" />\n"
                    + "    <TextBlock Margin=\"0 0 16 0\" FontFamily=\"Cascadia Code\" Text=\"&lt;!--\" />\n"
                    + "    <TextBlock Margin=\"0 0 16 0\" FontFamily=\"Cascadia Code\" Text=\"&lt;--\" />\n"
                    + "</StackPanel>";
            }
        }

        private void OnMessageBoxSampleClick(object sender, RoutedEventArgs e)
        {
            ItemRequested?.Invoke("MessageBox");
        }

        private void OnMessageBoxSpecClick(object sender, RoutedEventArgs e)
        {
            OpenUri("https://github.com/dotnet/wpf/issues/9542");
        }

        private void OnWhatsNewNet10Click(object sender, RoutedEventArgs e)
        {
            OpenUri("https://learn.microsoft.com/dotnet/desktop/wpf/whats-new/net100");
        }

        private void OnWhatsNewNet9Click(object sender, RoutedEventArgs e)
        {
            OpenUri("https://learn.microsoft.com/dotnet/desktop/wpf/whats-new/net90");
        }

        private void OnUsingFluentInWpfClick(object sender, RoutedEventArgs e)
        {
            OpenUri("https://learn.microsoft.com/dotnet/desktop/wpf/whats-new/net90#using-fluent-theme-in-wpf-in-net-9");
        }

        private static void OpenUri(string uri)
        {
            Process.Start(new ProcessStartInfo(uri) { UseShellExecute = true });
        }
    }
}
