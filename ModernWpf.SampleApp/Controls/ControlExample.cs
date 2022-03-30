using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Windows.Foundation.Metadata;
using System.Windows.Markup;
using ModernWpf.SampleApp.Controls;

namespace ModernWpf.SampleApp
{
    /// <summary>
    /// Describes a textual substitution in sample content.
    /// If enabled (default), then $(Key) is replaced with the stringified value.
    /// If disabled, then $(Key) is replaced with the empty string.
    /// </summary>
    public sealed class ControlExampleSubstitution : DependencyObject
    {
        public event TypedEventHandler<ControlExampleSubstitution, object> ValueChanged;

        public static readonly DependencyProperty KeyProperty = DependencyProperty.Register("Key", typeof(string), typeof(ControlExampleSubstitution), new PropertyMetadata(default));
        public string Key
        {
            get { return (string)GetValue(KeyProperty); }
            set { SetValue(KeyProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(object), typeof(ControlExampleSubstitution), new PropertyMetadata(null, OnDependencyPropertyChanged));
        public object Value
        {
            get { return GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.Register("IsEnabled", typeof(bool), typeof(ControlExampleSubstitution), new PropertyMetadata(true, OnDependencyPropertyChanged));
        public bool IsEnabled
        {
            get { return (bool)GetValue(IsEnabledProperty); }
            set { SetValue(IsEnabledProperty, value); }
        }

        private static void OnDependencyPropertyChanged(DependencyObject target, DependencyPropertyChangedEventArgs args)
        {
            if (target is ControlExampleSubstitution presenter)
            {
                presenter.ValueChanged?.Invoke(presenter, null);
            }
        }

        public string ValueAsString()
        {
            if (!IsEnabled)
            {
                return string.Empty;
            }

            object value = Value;

            // For solid color brushes, use the underlying color.
            if (value is SolidColorBrush)
            {
                value = ((SolidColorBrush)value).Color;
            }

            if (value == null)
            {
                return string.Empty;
            }

            return value.ToString();
        }
    }

    [ContentProperty(nameof(Example))]
    [TemplatePart(Name = nameof(RootGrid), Type = typeof(Grid))]
    [TemplatePart(Name = nameof(ExampleContainer), Type = typeof(Border))]
    [TemplatePart(Name = nameof(OptionsPresenterBorder), Type = typeof(Border))]
    [TemplatePart(Name = nameof(ScreenshotButton), Type = typeof(Button))]
    [TemplatePart(Name = nameof(ScreenshotDelayButton), Type = typeof(Button))]
    [TemplatePart(Name = nameof(ControlPaddingBox), Type = typeof(TextBox))]
    [TemplatePart(Name = nameof(ErrorTextBlock), Type = typeof(TextBlock))]
    [TemplatePart(Name = nameof(ScreenshotStatusTextBlock), Type = typeof(TextBlock))]
    [TemplatePart(Name = nameof(ControlPresenter), Type = typeof(ContentPresenter))]
    [TemplatePart(Name = nameof(OptionsPresenter), Type = typeof(ContentPresenter))]
    [TemplatePart(Name = nameof(XamlPresenter), Type = typeof(SampleCodePresenter))]
    [TemplatePart(Name = nameof(CSharpPresenter), Type = typeof(SampleCodePresenter))]
    public class ControlExample : Control
    {
        private Grid RootGrid;

        public Border ExampleContainer;
        private Border OptionsPresenterBorder;

        private Button ScreenshotButton;
        private Button ScreenshotDelayButton;

        private TextBox ControlPaddingBox;

        private TextBlock ErrorTextBlock;
        private TextBlock ScreenshotStatusTextBlock;

        private ContentPresenter ControlPresenter;
        private ContentPresenter OptionsPresenter;

        private SampleCodePresenter XamlPresenter;
        private SampleCodePresenter CSharpPresenter;

        static ControlExample()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ControlExample), new FrameworkPropertyMetadata(typeof(ControlExample)));
        }

        public ControlExample()
        {
            this.Loaded += ControlExample_Loaded;
        }

        public static readonly DependencyProperty HeaderTextProperty = DependencyProperty.Register("HeaderText", typeof(string), typeof(ControlExample), new PropertyMetadata(null));
        public string HeaderText
        {
            get { return (string)GetValue(HeaderTextProperty); }
            set { SetValue(HeaderTextProperty, value); }
        }

        public static readonly DependencyProperty ExampleProperty = DependencyProperty.Register("Example", typeof(object), typeof(ControlExample), new PropertyMetadata(null));
        public object Example
        {
            get { return GetValue(ExampleProperty); }
            set { SetValue(ExampleProperty, value); }
        }

        public static readonly DependencyProperty OutputProperty = DependencyProperty.Register("Output", typeof(object), typeof(ControlExample), new PropertyMetadata(null));
        public object Output
        {
            get { return GetValue(OutputProperty); }
            set { SetValue(OutputProperty, value); }
        }

        public static readonly DependencyProperty OptionsProperty = DependencyProperty.Register("Options", typeof(object), typeof(ControlExample), new PropertyMetadata(null));
        public object Options
        {
            get { return GetValue(OptionsProperty); }
            set { SetValue(OptionsProperty, value); }
        }

        public static readonly DependencyProperty XamlProperty = DependencyProperty.Register("Xaml", typeof(string), typeof(ControlExample), new PropertyMetadata(null));
        public string Xaml
        {
            get { return (string)GetValue(XamlProperty); }
            set { SetValue(XamlProperty, value); }
        }

        public static readonly DependencyProperty XamlSourceProperty = DependencyProperty.Register("XamlSource", typeof(object), typeof(ControlExample), new PropertyMetadata(null));
        public Uri XamlSource
        {
            get { return (Uri)GetValue(XamlSourceProperty); }
            set { SetValue(XamlSourceProperty, value); }
        }

        public static readonly DependencyProperty CSharpProperty = DependencyProperty.Register("CSharp", typeof(string), typeof(ControlExample), new PropertyMetadata(null));
        public string CSharp
        {
            get { return (string)GetValue(CSharpProperty); }
            set { SetValue(CSharpProperty, value); }
        }

        public static readonly DependencyProperty CSharpSourceProperty = DependencyProperty.Register("CSharpSource", typeof(object), typeof(ControlExample), new PropertyMetadata(null));
        public Uri CSharpSource
        {
            get { return (Uri)GetValue(CSharpSourceProperty); }
            set { SetValue(CSharpSourceProperty, value); }
        }

        public static readonly DependencyProperty SubstitutionsProperty = DependencyProperty.Register("Substitutions", typeof(IList<ControlExampleSubstitution>), typeof(ControlExample), new PropertyMetadata(default));
        public IList<ControlExampleSubstitution> Substitutions
        {
            get { return (IList<ControlExampleSubstitution>)GetValue(SubstitutionsProperty); }
            set { SetValue(SubstitutionsProperty, value); }
        }

        public static readonly DependencyProperty ExampleHeightProperty = DependencyProperty.Register("ExampleHeight", typeof(GridLength), typeof(ControlExample), new PropertyMetadata(new GridLength(1, GridUnitType.Star)));
        public GridLength ExampleHeight
        {
            get { return (GridLength)GetValue(ExampleHeightProperty); }
            set { SetValue(ExampleHeightProperty, value); }
        }

        public static readonly DependencyProperty WebViewHeightProperty = DependencyProperty.Register("WebViewHeight", typeof(int), typeof(ControlExample), new PropertyMetadata(400));
        public int WebViewHeight
        {
            get { return (int)GetValue(WebViewHeightProperty); }
            set { SetValue(WebViewHeightProperty, value); }
        }

        public static readonly DependencyProperty WebViewWidthProperty = DependencyProperty.Register("WebViewWidth", typeof(int), typeof(ControlExample), new PropertyMetadata(800));
        public int WebViewWidth
        {
            get { return (int)GetValue(WebViewWidthProperty); }
            set { SetValue(WebViewWidthProperty, value); }
        }

        public new static readonly DependencyProperty HorizontalContentAlignmentProperty = DependencyProperty.Register("HorizontalContentAlignment", typeof(HorizontalAlignment), typeof(ControlExample), new PropertyMetadata(HorizontalAlignment.Left));
        public new HorizontalAlignment HorizontalContentAlignment
        {
            get { return (HorizontalAlignment)GetValue(HorizontalContentAlignmentProperty); }
            set { SetValue(HorizontalContentAlignmentProperty, value); }
        }

        public static readonly DependencyProperty MinimumUniversalAPIContractProperty = DependencyProperty.Register("MinimumUniversalAPIContract", typeof(int), typeof(ControlExample), new PropertyMetadata(null));
        public int MinimumUniversalAPIContract
        {
            get { return (int)GetValue(MinimumUniversalAPIContractProperty); }
            set { SetValue(MinimumUniversalAPIContractProperty, value); }
        }

        public override void OnApplyTemplate()
        {
            if (RootGrid != null)
            {
                RootGrid.Loaded -= RootGrid_Loaded;
            }
            if (ScreenshotButton != null)
            {
                ScreenshotButton.Click -= ScreenshotButton_Click;
            }
            if (ScreenshotDelayButton != null)
            {
                ScreenshotDelayButton.Click -= ScreenshotDelayButton_Click;
            }
            if (ControlPaddingBox != null)
            {
                ControlPaddingBox.KeyUp -= ControlPaddingBox_KeyUp;
                ControlPaddingBox.LostFocus -= ControlPaddingBox_LostFocus;
            }

            base.OnApplyTemplate();

            RootGrid = GetTemplateChild(nameof(RootGrid)) as Grid;
            ExampleContainer = GetTemplateChild(nameof(ExampleContainer)) as Border;
            OptionsPresenterBorder = GetTemplateChild(nameof(OptionsPresenterBorder)) as Border;
            ScreenshotButton = GetTemplateChild(nameof(ScreenshotButton)) as Button;
            ScreenshotDelayButton = GetTemplateChild(nameof(ScreenshotDelayButton)) as Button;
            ControlPaddingBox = GetTemplateChild(nameof(ControlPaddingBox)) as TextBox;
            ErrorTextBlock = GetTemplateChild(nameof(ErrorTextBlock)) as TextBlock;
            ScreenshotStatusTextBlock = GetTemplateChild(nameof(ScreenshotStatusTextBlock)) as TextBlock;
            ControlPresenter = GetTemplateChild(nameof(ControlPresenter)) as ContentPresenter;
            OptionsPresenter = GetTemplateChild(nameof(OptionsPresenter)) as ContentPresenter;
            XamlPresenter = GetTemplateChild(nameof(XamlPresenter)) as SampleCodePresenter;
            CSharpPresenter = GetTemplateChild(nameof(CSharpPresenter)) as SampleCodePresenter;

            if (RootGrid != null)
            {
                RootGrid.Loaded += RootGrid_Loaded;
            }
            if (ScreenshotButton != null)
            {
                ScreenshotButton.Click += ScreenshotButton_Click;
            }
            if (ScreenshotDelayButton != null)
            {
                ScreenshotDelayButton.Click += ScreenshotDelayButton_Click;
            }
            if (ControlPaddingBox != null)
            {
                ControlPaddingBox.KeyUp += ControlPaddingBox_KeyUp;
                ControlPaddingBox.LostFocus += ControlPaddingBox_LostFocus;
            }
        }

        private void ControlExample_Loaded(object sender, RoutedEventArgs e)
        {
            if (!XamlPresenter.IsEmpty && !CSharpPresenter.IsEmpty)
            {
                VisualStateManager.GoToState(this, "SeparatorVisible", false);
            }
        }

        private void RootGrid_Loaded(object sender, RoutedEventArgs e)
        {
            if (MinimumUniversalAPIContract != 0 && !(ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", (ushort)MinimumUniversalAPIContract)))
            {
                ErrorTextBlock.Visibility = Visibility.Visible;
            }
        }

        private enum SyntaxHighlightLanguage { Xml, CSharp };

        private void ScreenshotButton_Click(object sender, RoutedEventArgs e)
        {
            TakeScreenshot();
        }

        private void ScreenshotDelayButton_Click(object sender, RoutedEventArgs e)
        {
            TakeScreenshotWithDelay();
        }

        private async void TakeScreenshot()
        {
            // Using RTB doesn't capture popups; but in the non-delay case, that probably isn't necessary.
            // This method seems more robust than using AppRecordingManager and also will work on non-desktop devices.

            //RenderTargetBitmap rtb = new RenderTargetBitmap();
            //await rtb.RenderAsync(ControlPresenter);

            //var pixelBuffer = await rtb.GetPixelsAsync();
            //var pixels = pixelBuffer.ToArray();

            //var file = await UIHelper.ScreenshotStorageFolder.CreateFileAsync(GetBestScreenshotName(), CreationCollisionOption.ReplaceExisting);
            //using (var stream = await file.OpenAsync(FileAccessMode.ReadWrite))
            //{
            //    var displayInformation = DisplayInformation.GetForCurrentView();
            //    var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
            //    encoder.SetPixelData(BitmapPixelFormat.Bgra8,
            //        BitmapAlphaMode.Premultiplied,
            //        (uint)rtb.PixelWidth,
            //        (uint)rtb.PixelHeight,
            //        displayInformation.RawDpiX,
            //        displayInformation.RawDpiY,
            //        pixels);

            //    await encoder.FlushAsync();
            //}
        }

        public async void TakeScreenshotWithDelay()
        {
            // 3 second countdown
            for (int i = 3; i > 0; i--)
            {
                ScreenshotStatusTextBlock.Text = i.ToString();
                await Task.Delay(1000);
            }
            ScreenshotStatusTextBlock.Text = "Image captured";

            // AppRecordingManager is desktop-only, and its use here is quite hacky,
            // but it is able to capture popups (though not theme shadows).

            //bool isAppRecordingPresent = ApiInformation.IsTypePresent("Windows.Media.AppRecording.AppRecordingManager");
            //if (!isAppRecordingPresent)
            //{
            //    // Better than doing nothing
            //    TakeScreenshot();
            //}
            //else
            //{
            //    var manager = AppRecordingManager.GetDefault();
            //    if (manager.GetStatus().CanRecord)
            //    {
            //        var result = await manager.SaveScreenshotToFilesAsync(
            //            ApplicationData.Current.LocalFolder,
            //            "appScreenshot",
            //            AppRecordingSaveScreenshotOption.HdrContentVisible,
            //            manager.SupportedScreenshotMediaEncodingSubtypes);

            //        if (result.Succeeded)
            //        {
            //            // Open the screenshot back up
            //            var screenshotFile = await ApplicationData.Current.LocalFolder.GetFileAsync("appScreenshot.png");
            //            using (var stream = await screenshotFile.OpenAsync(FileAccessMode.Read))
            //            {
            //                var decoder = await BitmapDecoder.CreateAsync(stream);

            //                // Find the control in the picture
            //                GeneralTransform t = ControlPresenter.TransformToVisual(Window.Current.Content);
            //                Point pos = t.TransformPoint(new Point(0, 0));
            //                ;
            //                if (!CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar)
            //                {
            //                    // Add the height of the title bar, which I really wish was programmatically available anywhere.
            //                    pos.Y += 32.0;
            //                }

            //                // Crop the screenshot to the control area
            //                var transform = new BitmapTransform()
            //                {
            //                    Bounds = new BitmapBounds()
            //                    {
            //                        X = (uint)(Math.Ceiling(pos.X)) + 1, // Avoid the 1px window border
            //                        Y = (uint)(Math.Ceiling(pos.Y)) + 1,
            //                        Width = (uint)ControlPresenter.ActualWidth - 1, // Rounding issues -- this avoids capturing the control border
            //                        Height = (uint)ControlPresenter.ActualHeight - 1
            //                    }
            //                };

            //                var softwareBitmap = await decoder.GetSoftwareBitmapAsync(
            //                    decoder.BitmapPixelFormat,
            //                    BitmapAlphaMode.Ignore,
            //                    transform,
            //                    ExifOrientationMode.IgnoreExifOrientation,
            //                    ColorManagementMode.DoNotColorManage);

            //                // Save the cropped picture
            //                var file = await ApplicationData.Current.LocalFolder.CreateFileAsync(GetBestScreenshotName(), CreationCollisionOption.ReplaceExisting);
            //                using (var outStream = await file.OpenAsync(FileAccessMode.ReadWrite))
            //                {
            //                    BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, outStream);
            //                    encoder.SetSoftwareBitmap(softwareBitmap);
            //                    await encoder.FlushAsync();
            //                }
            //            }

            //            // Delete intermediate file
            //            await screenshotFile.DeleteAsync();
            //        }
            //    }
            //}

            await Task.Delay(1000);
            ScreenshotStatusTextBlock.Text = "";
        }

        string GetBestScreenshotName()
        {
            string imageName = "Screenshot.png";
            if (XamlSource != null)
            {
                // Most of them don't have this, but the xaml source name is a really good file name
                string xamlSource = XamlSource.LocalPath;
                string fileName = Path.GetFileNameWithoutExtension(xamlSource);
                if (!String.IsNullOrWhiteSpace(fileName))
                {
                    imageName = fileName + ".png";
                }
            }
            else if (!String.IsNullOrWhiteSpace(Name))
            {
                // Put together the page name and the control example name
                UIElement uie = this;
                while (uie != null && !(uie is Page))
                {
                    uie = VisualTreeHelper.GetParent(uie) as UIElement;
                }
                if (uie != null)
                {
                    string name = Name;
                    if (name.Equals("RootPanel"))
                    {
                        // This is the default name for the example; add an index on the end to disambiguate
                        imageName = uie.GetType().Name + "_" + ((Panel)this.Parent).Children.IndexOf(this).ToString() + ".png";
                    }
                    else
                    {
                        imageName = uie.GetType().Name + "_" + name + ".png";
                    }
                }
            }
            return imageName;
        }

        private void ControlPaddingChangedCallback(DependencyObject sender, DependencyProperty dp)
        {
            ControlPaddingBox.Text = ControlPresenter.Margin.ToString();
        }

        private void ControlPaddingBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !String.IsNullOrWhiteSpace(ControlPaddingBox.Text))
            {
                EvaluatePadding();
            }
        }

        private void ControlPaddingBox_LostFocus(object sender, RoutedEventArgs e)
        {
            EvaluatePadding();
        }

        private void EvaluatePadding()
        {
            // Evaluate the text in the ControlPaddingBox as padding
            string[] strs = ControlPaddingBox.Text.Split(new char[] { ' ', ',' });
            double[] nums = new double[4];
            for (int i = 0; i < strs.Length; i++)
            {
                if (!Double.TryParse(strs[i], out nums[i]))
                {
                    //  Bad format
                    return;
                }
            }

            switch (nums.Length)
            {
                case 1:
                    ControlPresenter.Margin = new Thickness(nums[0]);
                    break;

                case 2:
                    ControlPresenter.Margin = new Thickness(nums[0], nums[1], nums[0], nums[1]);
                    break;

                case 4:
                    ControlPresenter.Margin = new Thickness(nums[0], nums[1], nums[2], nums[3]);
                    break;
            }
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (Application.Current.MainWindow.ActualWidth < 740)
            {
                OptionsPresenter.HorizontalAlignment = HorizontalAlignment.Left;
                OptionsPresenterBorder.Margin = new Thickness(0, 24, 0, 0);
                Grid.SetRow(OptionsPresenterBorder, 1);
                Grid.SetColumn(OptionsPresenterBorder, 0);
                Grid.SetColumnSpan(OptionsPresenterBorder, 2);
            }
            else
            {
                OptionsPresenter.HorizontalAlignment = HorizontalAlignment.Stretch;
                OptionsPresenterBorder.Margin = new Thickness(0, 12, 12, 12);
                Grid.SetRow(OptionsPresenterBorder, 0);
                Grid.SetColumn(OptionsPresenterBorder, 2);
                Grid.SetColumnSpan(OptionsPresenterBorder, 1);
            }
        }
    }
}
