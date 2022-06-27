using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ModernWpf.Controls.Primitives
{
    public static class TextBlockHelper
    {
        #region IsEnabled

        public static bool GetIsEnabled(TextBlock element)
        {
            return (bool)element.GetValue(IsEnabledProperty);
        }

        public static void SetIsEnabled(TextBlock element, bool value)
        {
            element.SetValue(IsEnabledProperty, value);
        }

        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached(
                "IsEnabled",
                typeof(bool),
                typeof(TextBlockHelper),
                new PropertyMetadata(OnIsEnabledChanged));

        private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var element = (TextBlock)d;
            if ((bool)e.NewValue)
            {
                element.SizeChanged += OnSizeChanged;
                UpdateTextTrimmed(element);
            }
            else
            {
                element.SizeChanged -= OnSizeChanged;
            }
        }

        #endregion

        #region IsTextTrimmed

        public static bool GetIsTextTrimmed(TextBlock element)
        {
            return (bool)element.GetValue(IsTextTrimmedProperty);
        }

        private static void SetIsTextTrimmed(TextBlock element, bool value)
        {
            element.SetValue(IsTextTrimmedPropertyKey, value);
        }

        private static readonly DependencyPropertyKey IsTextTrimmedPropertyKey =
            DependencyProperty.RegisterAttachedReadOnly(
                "IsTextTrimmed",
                typeof(bool),
                typeof(TextBlockHelper),
                null);

        public static readonly DependencyProperty IsTextTrimmedProperty =
            IsTextTrimmedPropertyKey.DependencyProperty;

        #endregion

        private static void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateTextTrimmed((TextBlock)sender);
        }

        private static void UpdateTextTrimmed(TextBlock textBlock)
        {
            if (!textBlock.IsLoaded) { return; }

            Typeface typeface = new Typeface(
                textBlock.FontFamily,
                textBlock.FontStyle,
                textBlock.FontWeight,
                textBlock.FontStretch);

            FormattedText formattedText = new FormattedText(
                textBlock.Text,
                Thread.CurrentThread.CurrentCulture,
                textBlock.FlowDirection,
                typeface,
                textBlock.FontSize,
                textBlock.Foreground);

            formattedText.MaxTextWidth = textBlock.ActualWidth;

            bool isTrimmed = formattedText.Height > textBlock.ActualHeight ||
                             formattedText.Width > formattedText.MaxTextWidth;

            SetIsTextTrimmed(textBlock, isTrimmed);
        }
    }
}
