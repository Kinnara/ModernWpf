using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ModernWpf.SampleApp.ControlPages
{
    public partial class ThemeResourcesPage
    {
        private List<ScrollViewer> _scrollViewers;
        private bool _syncingScroll;

        public ThemeResourcesPage()
        {
            InitializeComponent();

            _scrollViewers = new List<ScrollViewer> { SV1, SV2 };
        }

        ~ThemeResourcesPage()
        {
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

    public class ColorResources : List<ColorResourceInfo>
    {
        public ColorResources()
        {
            AddColor("BaseLow", "Base");
            AddColor("BaseMediumLow", "Base");
            AddColor("BaseMedium", "Base");
            AddColor("BaseMediumHigh", "Base");
            AddColor("BaseHigh", "Base");
            AddColor("AltLow", "Alt");
            AddColor("AltMediumLow", "Alt");
            AddColor("AltMedium", "Alt");
            AddColor("AltMediumHigh", "Alt");
            AddColor("AltHigh", "Alt");
            AddColor("ListLow", "List");
            AddColor("ListMedium", "List");
            AddColor("ListAccentLow", "List");
            AddColor("ListAccentMedium", "List");
            AddColor("ListAccentHigh", "List");
            AddColor("ChromeLow", "Chrome");
            AddColor("ChromeMediumLow", "Chrome");
            AddColor("ChromeMedium", "Chrome");
            AddColor("ChromeMediumHigh", "Chrome");
            AddColor("ChromeHigh", "Chrome");
            AddColor("ChromeAltLow", "Chrome");
            AddColor("ChromeDisabledLow", "Chrome");
            AddColor("ChromeDisabledHigh", "Chrome");
            AddColor("ChromeBlackLow", "Chrome");
            AddColor("ChromeBlackMediumLow", "Chrome");
            AddColor("ChromeBlackMedium", "Chrome");
            AddColor("ChromeBlackHigh", "Chrome");
            AddColor("ChromeWhite", "Chrome");
        }

        private void AddColor(string simpleName, string groupName)
        {
            Add(new ColorResourceInfo(simpleName, groupName, Count));
        }
    }

    public class ColorResourceInfo : IHasKey
    {
        public ColorResourceInfo(string simpleName, string groupName, int order)
        {
            SimpleName = simpleName;
            GroupName = groupName;
            Order = order;

            if (simpleName.Contains("ListAccent"))
            {
                Key = simpleName;
            }
            else
            {
                Key = "System" + simpleName + "Color";
            }
        }

        public string SimpleName { get; }

        public string GroupName { get; }

        public string Key { get; }

        public int Order { get; }

        public int GroupOrder
        {
            get
            {
                switch (GroupName)
                {
                    case "Base":
                        return 1;
                    case "Alt":
                        return 2;
                    case "List":
                        return 3;
                    case "Chrome":
                        return 4;
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        public bool CanCopyKey => !SimpleName.Contains("ListAccent");

        public bool IsAlt => GroupName == "Alt";
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
            "AcrylicBackgroundFillColorDefaultBrush",
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

    public static class ColorResourceHelper
    {
        #region BackgroundColorKey

        public static readonly DependencyProperty BackgroundColorKeyProperty =
            DependencyProperty.RegisterAttached(
                "BackgroundColorKey",
                typeof(string),
                typeof(ColorResourceHelper),
                new PropertyMetadata(default(string), OnBackgroundColorKeyChanged));

        public static string GetBackgroundColorKey(Border border)
        {
            return (string)border.GetValue(BackgroundColorKeyProperty);
        }

        public static void SetBackgroundColorKey(Border border, string value)
        {
            border.SetValue(BackgroundColorKeyProperty, value);
        }

        private static void OnBackgroundColorKeyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var border = (Border)d;
            var key = e.NewValue as string;
            if (string.IsNullOrEmpty(key))
            {
                border.ClearValue(Border.BackgroundProperty);
            }
            else if (key.Contains("ListAccent"))
            {
                border.Background = (SolidColorBrush)border.FindResource($"SystemControlHighlight{key}Brush");
            }
            else
            {
                border.Background = new SolidColorBrush((Color)border.FindResource(key));
            }
        }

        #endregion
    }
}
