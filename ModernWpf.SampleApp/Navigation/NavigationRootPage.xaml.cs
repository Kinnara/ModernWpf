using ModernWpf.Controls;
using ModernWpf.SampleApp.ControlPages;
using ModernWpf.SampleApp.Presets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;

namespace ModernWpf.SampleApp
{
    public partial class NavigationRootPage
    {
        public static NavigationRootPage Current { get; private set; }
        public static Frame RootFrame { get; private set; }

        private bool _ignoreSelectionChange;
        private readonly ControlPagesData _controlPagesData = new ControlPagesData();

        public NavigationRootPage()
        {
            InitializeComponent();

            Current = this;
            RootFrame = rootFrame;

            //PagesList.SelectedItem = PagesList.Items.OfType<ControlInfoDataItem>().FirstOrDefault(
            //    x => x.NavigateUri.ToString().EndsWith(nameof(AutoSuggestBoxPage) + ".xaml"));
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
                RootFrame?.Navigate(source);
            }
        }

        private void PagesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_ignoreSelectionChange)
            {
                NavigateToSelectedPage();
            }
        }

        private void OnControlsSearchBoxTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            var suggestions = new List<ControlInfoDataItem>();

            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                var querySplit = sender.Text.Split(' ');
                var matchingItems = _controlPagesData.Where(
                    item =>
                    {
                        // Idea: check for every word entered (separated by space) if it is in the name,  
                        // e.g. for query "split button" the only result should "SplitButton" since its the only query to contain "split" and "button" 
                        // If any of the sub tokens is not in the string, we ignore the item. So the search gets more precise with more words 
                        bool flag = true;
                        foreach (string queryToken in querySplit)
                        {
                            // Check if token is not in string 
                            if (item.Title.IndexOf(queryToken, StringComparison.CurrentCultureIgnoreCase) < 0)
                            {
                                // Token is not in string, so we ignore this item. 
                                flag = false;
                            }
                        }
                        return flag;
                    });
                foreach (var item in matchingItems)
                {
                    suggestions.Add(item);
                }
                if (suggestions.Count > 0)
                {
                    controlsSearchBox.ItemsSource = suggestions.OrderByDescending(i => i.Title.StartsWith(sender.Text, StringComparison.CurrentCultureIgnoreCase)).ThenBy(i => i.Title);
                }
                else
                {
                    controlsSearchBox.ItemsSource = new string[] { "No results found" };
                }
            }
        }

        private void OnControlsSearchBoxQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.ChosenSuggestion != null && args.ChosenSuggestion is ControlInfoDataItem)
            {
                var navigateUri = (args.ChosenSuggestion as ControlInfoDataItem).NavigateUri;
                RootFrame.Navigate(navigateUri);
            }
            else if (!string.IsNullOrEmpty(args.QueryText))
            {
                var item = _controlPagesData.FirstOrDefault(i => i.Title.Equals(args.QueryText, StringComparison.OrdinalIgnoreCase));
                if (item != null)
                {
                    RootFrame.Navigate(item.NavigateUri);
                }
            }
        }

        private void Frame_Navigated(object sender, NavigationEventArgs e)
        {
            _ignoreSelectionChange = true;
            PagesList.SelectedValue = RootFrame.CurrentSource;
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

        private void OnThemeButtonClick(object sender, RoutedEventArgs e)
        {
            if (ThemeManager.Current.ActualApplicationTheme == ApplicationTheme.Dark)
            {
                ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light;
            }
            else
            {
                ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark;
            }
        }

        private void OnThemeButtonMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Right)
            {
                AutoSuggestArea.Visibility = Visibility.Visible;
            }
        }
    }

    public class ControlPagesData : List<ControlInfoDataItem>
    {
        public ControlPagesData()
        {
            AddPage(typeof(ControlPalettePage), "Control Palette");
            AddPage(typeof(ThemesPage));
            AddPage(typeof(ThemeResourcesPage), "Theme Resources");
            AddPage(typeof(CompactSizingPage), "Compact Sizing");
            AddPage(typeof(PageTransitionsPage), "Page Transitions");
            AddPage(typeof(AppBarButtonPage));
            AddPage(typeof(AppBarSeparatorPage));
            AddPage(typeof(AppBarToggleButtonPage));
            AddPage(typeof(AutoSuggestBoxPage));
            AddPage(typeof(ButtonsPage));
            AddPage(typeof(CalendarPage));
            //AddPage(typeof(CheckBoxPage));
            AddPage(typeof(ComboBoxPage));
            AddPage(typeof(CommandBarPage));
            AddPage(typeof(CommandBarFlyoutPage));
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
            AddPage(typeof(MenuFlyoutPage));
            AddPage(typeof(NumberBoxPage));
            AddPage(typeof(PasswordBoxPage));
            //AddPage(typeof(PopupPlacementPage));
            AddPage(typeof(ProgressPage), "Progress Controls");
            AddPage(typeof(RadioButtonsPage));
            AddPage(typeof(RatingControlPage));
            AddPage(typeof(SimpleStackPanelPage));
            AddPage(typeof(SliderPage));
            //AddPage(typeof(StatusBarPage));
            AddPage(typeof(TabControlPage));
            AddPage(typeof(TextBoxPage));
            AddPage(typeof(ToggleSwitchPage));
            AddPage(typeof(ToolTipPage));
            AddPage(typeof(TreeViewPage));
            AddPage(typeof(WindowPage));
        }

        private void AddPage(Type pageType, string displayName = null)
        {
            Add(new ControlInfoDataItem(pageType, displayName));
        }
    }

    public class ControlInfoDataItem
    {
        public ControlInfoDataItem(Type pageType, string title = null)
        {
            Title = title ?? pageType.Name.Replace("Page", null);
            NavigateUri = new Uri($"ControlPages/{pageType.Name}.xaml", UriKind.Relative);
        }

        public string Title { get; }

        public Uri NavigateUri { get; }

        public override string ToString()
        {
            return Title;
        }
    }
}
