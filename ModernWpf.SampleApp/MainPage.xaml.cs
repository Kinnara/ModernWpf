using ModernWpf.Controls;
using ModernWpf.SampleApp.ControlPages;
using ModernWpf.SampleApp.Presets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace ModernWpf.SampleApp
{
    /// <summary>
    /// Interaction logic for MainPage.xaml
    /// </summary>
    public partial class MainPage
    {
        private bool _ignoreSelectionChange;

        public MainPage()
        {
            InitializeComponent();
            //PagesList.SelectedItem = PagesList.Items.OfType<ControlPageInfo>().FirstOrDefault(
            //    x => x.NavigateUri.ToString().Contains(nameof(FlyoutPage)));
            NavigateToSelectedPage();
        }

        private void ContextMenu_Loaded(object sender, RoutedEventArgs e)
        {
            //Debug.WriteLine(nameof(ContextMenu_Loaded));
            var menu = (ContextMenu)sender;
            var tabItem = (TabItem)menu.PlacementTarget;
            var content = (FrameworkElement)tabItem.Content;
            FindMenuItem(menu, ThemeManager.GetRequestedTheme(content)).IsChecked = true;
        }

        private void InvertTheme(object sender, RoutedEventArgs e)
        {
            GetTabItemContent(sender as MenuItem)?.InvertTheme();
        }

        private void ThemeMenuItem_Checked(object sender, RoutedEventArgs e)
        {
            //Debug.WriteLine($"{((RadioMenuItem)e.Source).Header} checked");
            var menuItem = (RadioMenuItem)e.Source;
            var tabItemContent = GetTabItemContent(menuItem);
            if (tabItemContent != null)
            {
                ThemeManager.SetRequestedTheme(tabItemContent, (ElementTheme)menuItem.Tag);
            }
        }

        private void ThemeMenuItem_Unchecked(object sender, RoutedEventArgs e)
        {
            //Debug.WriteLine($"{((RadioMenuItem)e.Source).Header} unchecked");
        }

        private RadioMenuItem FindMenuItem(ContextMenu menu, ElementTheme theme)
        {
            return menu.Items.OfType<RadioMenuItem>().First(x => (ElementTheme)x.Tag == theme);
        }

        private FrameworkElement GetTabItemContent(MenuItem menuItem)
        {
            return ((menuItem
                ?.Parent as ContextMenu)
                ?.PlacementTarget as TabItem)
                ?.Content as FrameworkElement;
        }

        private void NavigateToSelectedPage()
        {
            if (PagesList.SelectedValue is Uri source)
            {
                Frame?.Navigate(source);
            }
        }

        private void PagesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_ignoreSelectionChange)
            {
                NavigateToSelectedPage();
            }
        }

        private void Frame_Navigated(object sender, NavigationEventArgs e)
        {
            _ignoreSelectionChange = true;
            PagesList.SelectedValue = Frame.CurrentSource;
            _ignoreSelectionChange = false;
        }

        private void Default_Checked(object sender, RoutedEventArgs e)
        {
            ThemeManager.Current.ApplicationTheme = null;
        }

        private void Light_Checked(object sender, RoutedEventArgs e)
        {
            ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light;
        }

        private void Dark_Checked(object sender, RoutedEventArgs e)
        {
            ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark;
        }

        private void PresetMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is MenuItem menuItem)
            {
                PresetManager.Current.CurrentPreset = (string)menuItem.Header;
            }
        }

        private void SizingMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is MenuItem menuItem)
            {
                bool compact = menuItem.Tag as string == "Compact";

                var xcr = Application.Current.Resources.MergedDictionaries.OfType<XamlControlsResources>().FirstOrDefault();
                if (xcr != null)
                {
                    xcr.UseCompactResources = compact;
                }
            }
        }

        private void ShadowsAuto_Checked(object sender, RoutedEventArgs e)
        {
            Application.Current.Resources.Remove(SystemParameters.DropShadowKey);
        }

        private void ShadowsEnabled_Checked(object sender, RoutedEventArgs e)
        {
            Application.Current.Resources[SystemParameters.DropShadowKey] = true;
        }

        private void ShadowsDisabled_Checked(object sender, RoutedEventArgs e)
        {
            Application.Current.Resources[SystemParameters.DropShadowKey] = false;
        }

        private void ForceGC(object sender, RoutedEventArgs e)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }

    public class ControlPagesData : List<ControlPageInfo>
    {
        public ControlPagesData()
        {
            AddPage(typeof(ControlPalettePage), "Control Palette");
            AddPage(typeof(ThemesPage));
            AddPage(typeof(ThemeResourcesPage), "Theme Resources");
            AddPage(typeof(CompactSizingPage), "Compact Sizing");
            AddPage(typeof(PageTransitionsPage), "Page Transitions");
            AddPage(typeof(AppBarButtonPage));
            AddPage(typeof(AppBarToggleButtonPage));
            AddPage(typeof(ButtonsPage));
            AddPage(typeof(CalendarPage));
            AddPage(typeof(ComboBoxPage));
            AddPage(typeof(ContentDialogPage));
            AddPage(typeof(ContextMenuPage));
            AddPage(typeof(DataGridPage));
            AddPage(typeof(DatePickerPage));
            AddPage(typeof(ExpanderPage));
            AddPage(typeof(FlyoutPage));
            AddPage(typeof(GroupBoxPage));
            AddPage(typeof(HyperlinkButtonPage));
            AddPage(typeof(ItemsRepeaterPage));
            AddPage(typeof(ListBoxPage));
            AddPage(typeof(ListViewPage));
            AddPage(typeof(MenuPage));
            AddPage(typeof(PasswordBoxPage));
            AddPage(typeof(ProgressPage));
            AddPage(typeof(RadioButtonsPage));
            AddPage(typeof(SliderPage));
            //AddPage(typeof(StatusBarPage));
            AddPage(typeof(TabControlPage));
            AddPage(typeof(TextBoxPage));
            AddPage(typeof(ToggleSwitchPage));
            //AddPage(typeof(ToolBarPage));
            AddPage(typeof(ToolTipPage));
            AddPage(typeof(TreeViewPage));
            AddPage(typeof(WindowPage));
        }

        private void AddPage(Type pageType, string displayName = null)
        {
            Add(new ControlPageInfo(pageType, displayName));
        }
    }

    public class ControlPageInfo
    {
        public ControlPageInfo(Type pageType, string displayName = null)
        {
            DisplayName = displayName ?? pageType.Name.Replace("Page", null);
            NavigateUri = new Uri($"ControlPages/{pageType.Name}.xaml", UriKind.Relative);
        }

        public string DisplayName { get; }

        public Uri NavigateUri { get; }

        //public override string ToString()
        //{
        //    return base.ToString();
        //}
    }
}
