using System.Windows;

namespace ModernWpf.Controls.Primitives
{
    public sealed class SplitViewTemplateSettings : DependencyObject
    {
        internal SplitViewTemplateSettings()
        {
        }

        #region CompactPaneGridLength

        private static readonly DependencyPropertyKey CompactPaneGridLengthPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(CompactPaneGridLength),
                typeof(GridLength),
                typeof(SplitViewTemplateSettings),
                null);

        public static readonly DependencyProperty CompactPaneGridLengthProperty =
            CompactPaneGridLengthPropertyKey.DependencyProperty;

        public GridLength CompactPaneGridLength
        {
            get => (GridLength)GetValue(CompactPaneGridLengthProperty);
            internal set => SetValue(CompactPaneGridLengthPropertyKey, value);
        }

        #endregion

        #region NegativeOpenPaneLength

        private static readonly DependencyPropertyKey NegativeOpenPaneLengthPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(NegativeOpenPaneLength),
                typeof(double),
                typeof(SplitViewTemplateSettings),
                null);

        public static readonly DependencyProperty NegativeOpenPaneLengthProperty =
            NegativeOpenPaneLengthPropertyKey.DependencyProperty;

        public double NegativeOpenPaneLength
        {
            get => (double)GetValue(NegativeOpenPaneLengthProperty);
            internal set => SetValue(NegativeOpenPaneLengthPropertyKey, value);
        }

        #endregion

        #region NegativeOpenPaneLengthMinusCompactLength

        private static readonly DependencyPropertyKey NegativeOpenPaneLengthMinusCompactLengthPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(NegativeOpenPaneLengthMinusCompactLength),
                typeof(double),
                typeof(SplitViewTemplateSettings),
                null);

        public static readonly DependencyProperty NegativeOpenPaneLengthMinusCompactLengthProperty =
            NegativeOpenPaneLengthMinusCompactLengthPropertyKey.DependencyProperty;

        public double NegativeOpenPaneLengthMinusCompactLength
        {
            get => (double)GetValue(NegativeOpenPaneLengthMinusCompactLengthProperty);
            internal set => SetValue(NegativeOpenPaneLengthMinusCompactLengthPropertyKey, value);
        }

        #endregion

        #region OpenPaneGridLength

        private static readonly DependencyPropertyKey OpenPaneGridLengthPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(OpenPaneGridLength),
                typeof(GridLength),
                typeof(SplitViewTemplateSettings),
                null);

        public static readonly DependencyProperty OpenPaneGridLengthProperty =
            OpenPaneGridLengthPropertyKey.DependencyProperty;

        public GridLength OpenPaneGridLength
        {
            get => (GridLength)GetValue(OpenPaneGridLengthProperty);
            internal set => SetValue(OpenPaneGridLengthPropertyKey, value);
        }

        #endregion

        #region OpenPaneLength

        private static readonly DependencyPropertyKey OpenPaneLengthPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(OpenPaneLength),
                typeof(double),
                typeof(SplitViewTemplateSettings),
                null);

        public static readonly DependencyProperty OpenPaneLengthProperty =
            OpenPaneLengthPropertyKey.DependencyProperty;

        public double OpenPaneLength
        {
            get => (double)GetValue(OpenPaneLengthProperty);
            internal set => SetValue(OpenPaneLengthPropertyKey, value);
        }

        #endregion

        #region OpenPaneLengthMinusCompactLength

        private static readonly DependencyPropertyKey OpenPaneLengthMinusCompactLengthPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(OpenPaneLengthMinusCompactLength),
                typeof(double),
                typeof(SplitViewTemplateSettings),
                null);

        public static readonly DependencyProperty OpenPaneLengthMinusCompactLengthProperty =
            OpenPaneLengthMinusCompactLengthPropertyKey.DependencyProperty;

        public double OpenPaneLengthMinusCompactLength
        {
            get => (double)GetValue(OpenPaneLengthMinusCompactLengthProperty);
            internal set => SetValue(OpenPaneLengthMinusCompactLengthPropertyKey, value);
        }

        #endregion
    }
}
