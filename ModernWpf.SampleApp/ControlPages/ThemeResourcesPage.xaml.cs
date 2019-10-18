using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ModernWpf.SampleApp.ControlPages
{
    /// <summary>
    /// Interaction logic for ThemeResourcesPage.xaml
    /// </summary>
    public partial class ThemeResourcesPage : Page
    {
        private List<ScrollViewer> _scrollViewers;
        private bool _syncingScroll;

        public ThemeResourcesPage()
        {
            InitializeComponent();

            _scrollViewers = new List<ScrollViewer> { SV1, SV2 };
        }

        private void OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (_syncingScroll)
            {
                return;
            }

            if (e.VerticalChange != 0)
            {
                try
                {
                    _syncingScroll = true;

                    foreach (var sv in _scrollViewers)
                    {
                        if (sv != sender)
                        {
                            sv.ScrollToVerticalOffset(e.VerticalOffset);
                        }
                    }
                }
                finally
                {
                    _syncingScroll = false;
                }
            }
        }

        private void CopyKey(object sender, RoutedEventArgs e)
        {
            var menuItem = (MenuItem)sender;
            if (menuItem.DataContext is IHasKey item)
            {
                Clipboard.SetText(item.Key);
            }
        }
    }

    public class BrushResources : List<BrushResourceInfo>
    {
        private static readonly string[] _keys =
        {
            "SystemControlBackgroundAccentBrush",
            "SystemControlBackgroundAltHighBrush",
            "SystemControlBackgroundAltMediumHighBrush",
            "SystemControlBackgroundAltMediumBrush",
            "SystemControlBackgroundAltMediumLowBrush",
            "SystemControlBackgroundBaseHighBrush",
            "SystemControlBackgroundBaseLowBrush",
            "SystemControlBackgroundBaseMediumBrush",
            "SystemControlBackgroundBaseMediumHighBrush",
            "SystemControlBackgroundBaseMediumLowBrush",
            "SystemControlBackgroundChromeBlackHighBrush",
            "SystemControlBackgroundChromeBlackMediumBrush",
            "SystemControlBackgroundChromeBlackLowBrush",
            "SystemControlBackgroundChromeBlackMediumLowBrush",
            "SystemControlBackgroundChromeMediumBrush",
            "SystemControlBackgroundChromeMediumLowBrush",
            "SystemControlBackgroundChromeWhiteBrush",
            "SystemControlBackgroundListLowBrush",
            "SystemControlBackgroundListMediumBrush",
            "SystemControlDisabledAccentBrush",
            "SystemControlDisabledBaseHighBrush",
            "SystemControlDisabledBaseLowBrush",
            "SystemControlDisabledBaseMediumLowBrush",
            "SystemControlDisabledChromeDisabledHighBrush",
            "SystemControlDisabledChromeDisabledLowBrush",
            "SystemControlDisabledChromeHighBrush",
            "SystemControlDisabledChromeMediumLowBrush",
            "SystemControlDisabledListMediumBrush",
            "SystemControlDisabledTransparentBrush",
            "SystemControlFocusVisualPrimaryBrush",
            "SystemControlFocusVisualSecondaryBrush",
            "SystemControlRevealFocusVisualBrush",
            "SystemControlForegroundAccentBrush",
            "SystemControlForegroundAltHighBrush",
            "SystemControlForegroundAltMediumHighBrush",
            "SystemControlForegroundBaseHighBrush",
            "SystemControlForegroundBaseLowBrush",
            "SystemControlForegroundBaseMediumBrush",
            "SystemControlForegroundBaseMediumHighBrush",
            "SystemControlForegroundBaseMediumLowBrush",
            "SystemControlForegroundChromeBlackHighBrush",
            "SystemControlForegroundChromeHighBrush",
            "SystemControlForegroundChromeMediumBrush",
            "SystemControlForegroundChromeWhiteBrush",
            "SystemControlForegroundChromeDisabledLowBrush",
            "SystemControlForegroundChromeGrayBrush",
            "SystemControlForegroundListLowBrush",
            "SystemControlForegroundListMediumBrush",
            "SystemControlForegroundTransparentBrush",
            "SystemControlForegroundChromeBlackMediumBrush",
            "SystemControlForegroundChromeBlackMediumLowBrush",
            "SystemControlHighlightAccentBrush",
            "SystemControlHighlightAltAccentBrush",
            "SystemControlHighlightAltAltHighBrush",
            "SystemControlHighlightAltBaseHighBrush",
            "SystemControlHighlightAltBaseLowBrush",
            "SystemControlHighlightAltBaseMediumBrush",
            "SystemControlHighlightAltBaseMediumHighBrush",
            "SystemControlHighlightAltAltMediumHighBrush",
            "SystemControlHighlightAltBaseMediumLowBrush",
            "SystemControlHighlightAltListAccentHighBrush",
            "SystemControlHighlightAltListAccentLowBrush",
            "SystemControlHighlightAltListAccentMediumBrush",
            "SystemControlHighlightAltChromeWhiteBrush",
            "SystemControlHighlightAltTransparentBrush",
            "SystemControlHighlightBaseHighBrush",
            "SystemControlHighlightBaseLowBrush",
            "SystemControlHighlightBaseMediumBrush",
            "SystemControlHighlightBaseMediumHighBrush",
            "SystemControlHighlightBaseMediumLowBrush",
            "SystemControlHighlightChromeAltLowBrush",
            "SystemControlHighlightChromeHighBrush",
            "SystemControlHighlightListAccentHighBrush",
            "SystemControlHighlightListAccentLowBrush",
            "SystemControlHighlightListAccentMediumBrush",
            "SystemControlHighlightListMediumBrush",
            "SystemControlHighlightListLowBrush",
            "SystemControlHighlightChromeWhiteBrush",
            "SystemControlHighlightTransparentBrush",
            "SystemControlHyperlinkTextBrush",
            "SystemControlHyperlinkBaseHighBrush",
            "SystemControlHyperlinkBaseMediumBrush",
            "SystemControlHyperlinkBaseMediumHighBrush",
            "SystemControlPageBackgroundAltMediumBrush",
            "SystemControlPageBackgroundAltHighBrush",
            "SystemControlPageBackgroundMediumAltMediumBrush",
            "SystemControlPageBackgroundBaseLowBrush",
            "SystemControlPageBackgroundBaseMediumBrush",
            "SystemControlPageBackgroundListLowBrush",
            "SystemControlPageBackgroundChromeLowBrush",
            "SystemControlPageBackgroundChromeMediumLowBrush",
            "SystemControlPageBackgroundTransparentBrush",
            "SystemControlPageTextBaseHighBrush",
            "SystemControlPageTextBaseMediumBrush",
            "SystemControlPageTextChromeBlackMediumLowBrush",
            "SystemControlTransparentBrush",
            "SystemControlErrorTextForegroundBrush",
            "SystemControlTransientBorderBrush",
            "SystemControlTransientBackgroundBrush",
            "SystemControlDescriptionTextForegroundBrush"
        };

        public BrushResources()
        {
            foreach (var key in _keys)
            {
                Add(new BrushResourceInfo(key));
            }
        }        
    }

    public class BrushResourceInfo : IHasKey
    {
        public BrushResourceInfo(string key)
        {
            Key = key;
        }

        public string Key { get; }
    }

    public class TextStyles : List<TextStyleInfo>
    {
        public TextStyles()
        {
            Add(new TextStyleInfo("HeaderTextBlockStyle"));
            Add(new TextStyleInfo("SubheaderTextBlockStyle"));
            Add(new TextStyleInfo("TitleTextBlockStyle"));
            Add(new TextStyleInfo("SubtitleTextBlockStyle"));
            Add(new TextStyleInfo("BaseTextBlockStyle"));
            Add(new TextStyleInfo("BodyTextBlockStyle"));
            Add(new TextStyleInfo("CaptionTextBlockStyle"));
        }
    }

    public class TextStyleInfo : IHasKey
    {
        public TextStyleInfo(string styleKey)
        {
            Key = styleKey;
            Text = styleKey.Replace("TextBlockStyle", null);
        }

        public string Text { get; }

        public string Key { get; }
    }

    public interface IHasKey
    {
        string Key { get; }
    }
}
