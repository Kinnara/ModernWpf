using ModernWpf.Controls;
using ModernWpf.SampleApp.ControlPages;
using ModernWpf.SampleApp.Presets;
using SamplesCommon;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Threading;
using Frame = ModernWpf.Controls.Frame;

namespace ModernWpf.SampleApp
{
    public partial class NavigationRootPage
    {
        private const string AutoHideScrollBarsKey = "AutoHideScrollBars";

        public static NavigationRootPage Current
        {
            get => _current.Value;
            private set => _current.Value = value;
        }

        public static Frame RootFrame
        {
            get => _rootFrame.Value;
            private set => _rootFrame.Value = value;
        }

        private static readonly ThreadLocal<NavigationRootPage> _current = new ThreadLocal<NavigationRootPage>();
        private static readonly ThreadLocal<Frame> _rootFrame = new ThreadLocal<Frame>();

        private bool _ignoreSelectionChange;
        private readonly ControlPagesData _controlPagesData = new ControlPagesData();
        private Type _startPage;

        public NavigationRootPage()
        {
            InitializeComponent();

            if (App.IsMultiThreaded)
            {
                PresetsMenu.Visibility = Visibility.Collapsed;
                NewWindowMenuItem.Visibility = Visibility.Visible;
            }

            Loaded += delegate
            {
                PresetManager.Current.ColorPresetChanged += OnColorPresetChanged;

                controlsSearchBox.Focus();
            };

            Unloaded += delegate
            {
                PresetManager.Current.ColorPresetChanged -= OnColorPresetChanged;
            };

            OnColorPresetChanged(null, null);

            Current = this;
            RootFrame = rootFrame;

            SetStartPage();
            if (_startPage != null)
            {
                PagesList.SelectedItem = PagesList.Items.OfType<ControlInfoDataItem>().FirstOrDefault(x => x.PageType == _startPage);
            }

            NavigateToSelectedPage();

            if (Debugger.IsAttached)
            {
                DebugMenuItem.Visibility = Visibility.Visible;
            }
        }

        partial void SetStartPage();

        private void ContextMenu_Loaded(object sender, RoutedEventArgs e)
        {
            //Debug.WriteLine(nameof(ContextMenu_Loaded));
            var menu = (ContextMenu)sender;
            var tabItem = (TabItem)menu.PlacementTarget;
            var content = (FrameworkElement)tabItem.Content;
            FindMenuItem(menu, ThemeManager.GetRequestedTheme(content)).IsChecked = true;
        }

        private void ToggleTheme(object sender, RoutedEventArgs e)
        {
            GetTabItemContent(sender as MenuItem)?.ToggleTheme();
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
            if (PagesList.SelectedValue is Type type)
            {
                RootFrame?.Navigate(type);
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
                var pageType = (args.ChosenSuggestion as ControlInfoDataItem).PageType;
                RootFrame.Navigate(pageType);
            }
            else if (!string.IsNullOrEmpty(args.QueryText))
            {
                var item = _controlPagesData.FirstOrDefault(i => i.Title.Equals(args.QueryText, StringComparison.OrdinalIgnoreCase));
                if (item != null)
                {
                    RootFrame.Navigate(item.PageType);
                }
            }
        }

        private void RootFrame_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.Back)
            {
                RootFrame.RemoveBackEntry();
            }
        }

        private void RootFrame_Navigated(object sender, NavigationEventArgs e)
        {
            Debug.Assert(!RootFrame.CanGoForward);

            _ignoreSelectionChange = true;
            PagesList.SelectedValue = RootFrame.CurrentSourcePageType;
            _ignoreSelectionChange = false;
        }

        private void Default_Checked(object sender, RoutedEventArgs e)
        {
            SetApplicationTheme(null);
        }

        private void Light_Checked(object sender, RoutedEventArgs e)
        {
            SetApplicationTheme(ApplicationTheme.Light);
        }

        private void Dark_Checked(object sender, RoutedEventArgs e)
        {
            SetApplicationTheme(ApplicationTheme.Dark);
        }

        private void PresetMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is MenuItem menuItem)
            {
                PresetManager.Current.ColorPreset = (string)menuItem.Header;
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

        private void AutoHideScrollBarsAuto_Checked(object sender, RoutedEventArgs e)
        {
            Application.Current.Resources.Remove(AutoHideScrollBarsKey);
        }

        private void AutoHideScrollBarsOn_Checked(object sender, RoutedEventArgs e)
        {
            Application.Current.Resources[AutoHideScrollBarsKey] = true;
        }

        private void AutoHideScrollBarsOff_Checked(object sender, RoutedEventArgs e)
        {
            Application.Current.Resources[AutoHideScrollBarsKey] = false;
        }

        private void ForceGC(object sender, RoutedEventArgs e)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        private void NewWindow(object sender, RoutedEventArgs e)
        {
            var thread = new Thread(() =>
            {
                var window = new MainWindow();
                window.Closed += delegate
                {
                    Dispatcher.CurrentDispatcher.BeginInvokeShutdown(DispatcherPriority.Normal);
                };
                window.Show();
                Dispatcher.Run();
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.IsBackground = true;
            thread.Start();
        }

        private void OnThemeButtonClick(object sender, RoutedEventArgs e)
        {
            DispatcherHelper.RunOnMainThread(() =>
            {
                if (ThemeManager.Current.ActualApplicationTheme == ApplicationTheme.Dark)
                {
                    ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light;
                }
                else
                {
                    ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark;
                }
            });
        }

        private void SetApplicationTheme(ApplicationTheme? theme)
        {
            DispatcherHelper.RunOnMainThread(() =>
            {
                ThemeManager.Current.ApplicationTheme = theme;
            });
        }

        private void OnColorPresetChanged(object sender, EventArgs e)
        {
            this.RunOnUIThread(() =>
            {
                PresetsMenu.Items
                .OfType<RadioMenuItem>()
                .Single(mi => mi.Header.ToString() == PresetManager.Current.ColorPreset)
                .IsChecked = true;
            });
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
            AddPage(typeof(PageTransitionPage), "Page Transitions");
            AddPage(typeof(ThreadedUIPage), "Threaded UI");
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
            //AddPage(typeof(GridSplitterPage));
            AddPage(typeof(GridViewPage));
            AddPage(typeof(GroupBoxPage));
            AddPage(typeof(HyperlinkButtonPage));
            AddPage(typeof(ItemsRepeaterPage));
            AddPage(typeof(ListBoxPage));
            AddPage(typeof(ListViewPage));
            AddPage(typeof(ListView2Page), "ListView (ModernWPF)");
            AddPage(typeof(MenuPage));
            AddPage(typeof(MenuFlyoutPage));
            AddPage(typeof(NavigationViewPage));
            AddPage(typeof(NumberBoxPage));
            AddPage(typeof(PasswordBoxPage));
            AddPage(typeof(PersonPicturePage));
            //AddPage(typeof(PopupPlacementPage));
            AddPage(typeof(ProgressPage), "Progress Controls");
            AddPage(typeof(RadioButtonsPage));
            AddPage(typeof(RatingControlPage));
            AddPage(typeof(RichTextBoxPage));
            AddPage(typeof(ShadowPage));
            AddPage(typeof(SimpleStackPanelPage));
            AddPage(typeof(SliderPage));
            AddPage(typeof(SplitViewPage));
            //AddPage(typeof(StatusBarPage));
            AddPage(typeof(TabControlPage));
            AddPage(typeof(PivotPage), "TabControlPivotStyle");
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
            PageType = pageType;
            Title = title ?? pageType.Name.Replace("Page", null);
        }

        public string Title { get; }

        public Type PageType { get; }

        public override string ToString()
        {
            return Title;
        }
    }
}
