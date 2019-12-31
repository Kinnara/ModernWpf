using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace ModernWpf.Controls.Primitives
{
    public static class RichTextBoxHelper
    {
        #region IsEnabled

        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached(
                "IsEnabled",
                typeof(bool),
                typeof(RichTextBoxHelper),
                new PropertyMetadata(false, OnIsEnabledChanged));

        public static bool GetIsEnabled(RichTextBox richTextBox)
        {
            return (bool)richTextBox.GetValue(IsEnabledProperty);
        }

        public static void SetIsEnabled(RichTextBox richTextBox, bool value)
        {
            richTextBox.SetValue(IsEnabledProperty, value);
        }

        private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var richTextBox = (RichTextBox)d;
            var oldValue = (bool)e.OldValue;
            var newValue = (bool)e.NewValue;
            if (newValue)
            {
                richTextBox.TextChanged += OnTextChanged;
                UpdateIsEmpty(richTextBox);
            }
            else
            {
                richTextBox.TextChanged -= OnTextChanged;
                richTextBox.ClearValue(IsEmptyPropertyKey);
            }
        }

        #endregion

        #region IsEmpty

        private static readonly DependencyPropertyKey IsEmptyPropertyKey =
            DependencyProperty.RegisterAttachedReadOnly(
                "IsEmpty",
                typeof(bool),
                typeof(RichTextBoxHelper),
                new PropertyMetadata(false));

        public static readonly DependencyProperty IsEmptyProperty =
            IsEmptyPropertyKey.DependencyProperty;

        public static bool GetIsEmpty(RichTextBox richTextBox)
        {
            return (bool)richTextBox.GetValue(IsEmptyProperty);
        }

        private static void SetIsEmpty(RichTextBox richTextBox, bool value)
        {
            richTextBox.SetValue(IsEmptyPropertyKey, value);
        }

        private static void UpdateIsEmpty(RichTextBox rtb)
        {
            bool isEmpty;
            if (rtb.Document.Blocks.Count == 0)
            {
                isEmpty = true;
            }
            else
            {
                TextPointer startPointer = rtb.Document.ContentStart.GetNextInsertionPosition(LogicalDirection.Forward);
                TextPointer endPointer = rtb.Document.ContentEnd.GetNextInsertionPosition(LogicalDirection.Backward);
                isEmpty = startPointer.CompareTo(endPointer) == 0;
            }

            if (GetIsEmpty(rtb) != isEmpty)
            {
                SetIsEmpty(rtb, isEmpty);
            }
        }

        #endregion

        private static void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateIsEmpty((RichTextBox)sender);
        }
    }
}
