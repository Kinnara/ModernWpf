using ModernWpf.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Windows.Foundation;
using Windows.Storage.Pickers;
using Windows.Storage;
using Windows.System;
using ModernWpf.Controls;
using System.Windows.Controls;
using Page = ModernWpf.Controls.Page;
using ModernWpf.SampleApp.Helper;
using System.Reflection;

namespace ModernWpf.SampleApp
{
    /// <summary>
    /// SettingsPage.xaml 的交互逻辑
    /// </summary>
    public partial class SettingsPage : Page
    {
        public string Version
        {
            get
            {
                if (PackagedAppHelper.IsPackagedApp)
                {
                    var version = Windows.ApplicationModel.Package.Current.Id.Version;
                    return string.Format("{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision);
                }
                else
                {
                    var version = Assembly.GetEntryAssembly()?.GetName().Version;
                    return string.Format("{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision);
                }
            }
        }

        public SettingsPage()
        {
            this.InitializeComponent();
            Loaded += OnSettingsPageLoaded;

            //if (ElementSoundPlayer.State == ElementSoundPlayerState.On)
            //    soundToggle.IsOn = true;
            //if (ElementSoundPlayer.SpatialAudioMode == ElementSpatialAudioMode.On)
            //    spatialSoundBox.IsChecked = true;

            ScreenshotSettingsGrid.Visibility = Visibility.Collapsed;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            NavigationRootPage.Current.NavigationView.Header = "Settings";
        }

        private async void OnFeedbackButtonClick(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("feedback-hub:"));
        }

        private void OnSettingsPageLoaded(object sender, RoutedEventArgs e)
        {
            var currentTheme = ThemeHelper.RootTheme.ToString();
            (ThemePanel.Children.Cast<RadioButton>().FirstOrDefault(c => c?.Tag?.ToString() == currentTheme)).IsChecked = true;

            NavigationRootPage navigationRootPage = NavigationRootPage.GetForElement(this);
            if (navigationRootPage != null)
            {
                if (navigationRootPage.NavigationView.PaneDisplayMode == NavigationViewPaneDisplayMode.Auto)
                {
                    navigationLocation.SelectedIndex = 0;
                }
                else
                {
                    navigationLocation.SelectedIndex = 1;
                }
            }
        }

        private void OnThemeRadioButtonChecked(object sender, RoutedEventArgs e)
        {

        }

        private void OnThemeRadioButtonKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up)
            {
                NavigationRootPage.GetForElement(this).PageHeader.Focus();
            }
        }
        private void spatialSoundBox_Checked(object sender, RoutedEventArgs e)
        {
            if (soundToggle.IsOn == true)
            {
                //ElementSoundPlayer.SpatialAudioMode = ElementSpatialAudioMode.On;
            }
        }

        private void soundToggle_Toggled(object sender, RoutedEventArgs e)
        {
            if (soundToggle.IsOn == true)
            {
                spatialSoundBox.IsEnabled = true;
                //ElementSoundPlayer.State = ElementSoundPlayerState.On;
            }
            else
            {
                spatialSoundBox.IsEnabled = false;
                spatialSoundBox.IsChecked = false;

                //ElementSoundPlayer.State = ElementSoundPlayerState.Off;
                //ElementSoundPlayer.SpatialAudioMode = ElementSpatialAudioMode.Off;
            }
        }

        private void screenshotModeToggle_Toggled(object sender, RoutedEventArgs e)
        {
            //UIHelper.IsScreenshotMode = screenshotModeToggle.IsOn;
        }

        private void spatialSoundBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (soundToggle.IsOn == true)
            {
                //ElementSoundPlayer.SpatialAudioMode = ElementSpatialAudioMode.Off;
            }
        }

        private void navigationLocation_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //NavigationOrientationHelper.IsLeftModeForElement(navigationLocation.SelectedIndex == 0, this);
        }

        private async void FolderButton_Click(object sender, RoutedEventArgs e)
        {
            FolderPicker folderPicker = new FolderPicker();
            folderPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            folderPicker.FileTypeFilter.Add(".png"); // meaningless, but you have to have something
            StorageFolder folder = await folderPicker.PickSingleFolderAsync();

            if (folder != null)
            {
                //UIHelper.ScreenshotStorageFolder = folder;
                //screenshotFolderLink.Content = UIHelper.ScreenshotStorageFolder.Path;
            }
        }

        private async void screenshotFolderLink_Click(object sender, RoutedEventArgs e)
        {
            //await Launcher.LaunchFolderAsync(UIHelper.ScreenshotStorageFolder);
        }

        private void OnResetTeachingTipsButtonClick(object sender, RoutedEventArgs e)
        {
            //ProtocolActivationClipboardHelper.ShowCopyLinkTeachingTip = true;
        }

        private void soundPageHyperlink_Click(object sender, RoutedEventArgs args)
        {
            this.Frame.Navigate(typeof(ItemPage), "Sound");
        }
    }
}
