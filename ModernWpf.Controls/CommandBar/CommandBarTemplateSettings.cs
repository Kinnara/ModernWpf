using System.Windows;

namespace ModernWpf.Controls.Primitives
{
    public sealed class CommandBarTemplateSettings : DependencyObject
    {
        internal CommandBarTemplateSettings()
        {
        }

        private static readonly DependencyPropertyKey ContentHeightPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(ContentHeight),
                typeof(double),
                typeof(CommandBarTemplateSettings),
                new PropertyMetadata(0.0));

        public static readonly DependencyProperty ContentHeightProperty =
            ContentHeightPropertyKey.DependencyProperty;

        public double ContentHeight
        {
            get => (double)GetValue(ContentHeightProperty);
            internal set => SetValue(ContentHeightPropertyKey, value);
        }

        private static readonly DependencyPropertyKey OverflowContentClipRectPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(OverflowContentClipRect),
                typeof(Rect),
                typeof(CommandBarTemplateSettings),
                new PropertyMetadata(Rect.Empty));

        public static readonly DependencyProperty OverflowContentClipRectProperty =
            OverflowContentClipRectPropertyKey.DependencyProperty;

        public Rect OverflowContentClipRect
        {
            get => (Rect)GetValue(OverflowContentClipRectProperty);
            internal set => SetValue(OverflowContentClipRectPropertyKey, value);
        }

        private static readonly DependencyPropertyKey OverflowContentMinWidthPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(OverflowContentMinWidth),
                typeof(double),
                typeof(CommandBarTemplateSettings),
                new PropertyMetadata(0.0));

        public static readonly DependencyProperty OverflowContentMinWidthProperty =
            OverflowContentMinWidthPropertyKey.DependencyProperty;

        public double OverflowContentMinWidth
        {
            get => (double)GetValue(OverflowContentMinWidthProperty);
            internal set => SetValue(OverflowContentMinWidthPropertyKey, value);
        }

        private static readonly DependencyPropertyKey OverflowContentMaxWidthPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(OverflowContentMaxWidth),
                typeof(double),
                typeof(CommandBarTemplateSettings),
                new PropertyMetadata(0.0));

        public static readonly DependencyProperty OverflowContentMaxWidthProperty =
            OverflowContentMaxWidthPropertyKey.DependencyProperty;

        public double OverflowContentMaxWidth
        {
            get => (double)GetValue(OverflowContentMaxWidthProperty);
            internal set => SetValue(OverflowContentMaxWidthPropertyKey, value);
        }

        private static readonly DependencyPropertyKey OverflowContentMaxHeightPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(OverflowContentMaxHeight),
                typeof(double),
                typeof(CommandBarTemplateSettings),
                new PropertyMetadata(0.0));

        public static readonly DependencyProperty OverflowContentMaxHeightProperty =
            OverflowContentMaxHeightPropertyKey.DependencyProperty;

        public double OverflowContentMaxHeight
        {
            get => (double)GetValue(OverflowContentMaxHeightProperty);
            internal set => SetValue(OverflowContentMaxHeightPropertyKey, value);
        }

        private static readonly DependencyPropertyKey OverflowContentHorizontalOffsetPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(OverflowContentHorizontalOffset),
                typeof(double),
                typeof(CommandBarTemplateSettings),
                new PropertyMetadata(0.0));

        public static readonly DependencyProperty OverflowContentHorizontalOffsetProperty =
            OverflowContentHorizontalOffsetPropertyKey.DependencyProperty;

        public double OverflowContentHorizontalOffset
        {
            get => (double)GetValue(OverflowContentHorizontalOffsetProperty);
            internal set => SetValue(OverflowContentHorizontalOffsetPropertyKey, value);
        }

        private static readonly DependencyPropertyKey OverflowContentHeightPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(OverflowContentHeight),
                typeof(double),
                typeof(CommandBarTemplateSettings),
                new PropertyMetadata(0.0));

        public static readonly DependencyProperty OverflowContentHeightProperty =
            OverflowContentHeightPropertyKey.DependencyProperty;

        public double OverflowContentHeight
        {
            get => (double)GetValue(OverflowContentHeightProperty);
            internal set => SetValue(OverflowContentHeightPropertyKey, value);
        }

        private static readonly DependencyPropertyKey NegativeOverflowContentHeightPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(NegativeOverflowContentHeight),
                typeof(double),
                typeof(CommandBarTemplateSettings),
                new PropertyMetadata(0.0));

        public static readonly DependencyProperty NegativeOverflowContentHeightProperty =
            NegativeOverflowContentHeightPropertyKey.DependencyProperty;

        public double NegativeOverflowContentHeight
        {
            get => (double)GetValue(NegativeOverflowContentHeightProperty);
            internal set => SetValue(NegativeOverflowContentHeightPropertyKey, value);
        }

        private static readonly DependencyPropertyKey EffectiveOverflowButtonVisibilityPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(EffectiveOverflowButtonVisibility),
                typeof(Visibility),
                typeof(CommandBarTemplateSettings),
                new PropertyMetadata(Visibility.Collapsed));

        public static readonly DependencyProperty EffectiveOverflowButtonVisibilityProperty =
            EffectiveOverflowButtonVisibilityPropertyKey.DependencyProperty;

        public Visibility EffectiveOverflowButtonVisibility
        {
            get => (Visibility)GetValue(EffectiveOverflowButtonVisibilityProperty);
            internal set => SetValue(EffectiveOverflowButtonVisibilityPropertyKey, value);
        }

        private static readonly DependencyPropertyKey OverflowContentCompactYTranslationPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(OverflowContentCompactYTranslation),
                typeof(double),
                typeof(CommandBarTemplateSettings),
                new PropertyMetadata(0.0));

        public static readonly DependencyProperty OverflowContentCompactYTranslationProperty =
            OverflowContentCompactYTranslationPropertyKey.DependencyProperty;

        public double OverflowContentCompactYTranslation
        {
            get => (double)GetValue(OverflowContentCompactYTranslationProperty);
            internal set => SetValue(OverflowContentCompactYTranslationPropertyKey, value);
        }

        private static readonly DependencyPropertyKey OverflowContentMinimalYTranslationPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(OverflowContentMinimalYTranslation),
                typeof(double),
                typeof(CommandBarTemplateSettings),
                new PropertyMetadata(0.0));

        public static readonly DependencyProperty OverflowContentMinimalYTranslationProperty =
            OverflowContentMinimalYTranslationPropertyKey.DependencyProperty;

        public double OverflowContentMinimalYTranslation
        {
            get => (double)GetValue(OverflowContentMinimalYTranslationProperty);
            internal set => SetValue(OverflowContentMinimalYTranslationPropertyKey, value);
        }

        private static readonly DependencyPropertyKey OverflowContentHiddenYTranslationPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(OverflowContentHiddenYTranslation),
                typeof(double),
                typeof(CommandBarTemplateSettings),
                new PropertyMetadata(0.0));

        public static readonly DependencyProperty OverflowContentHiddenYTranslationProperty =
            OverflowContentHiddenYTranslationPropertyKey.DependencyProperty;

        public double OverflowContentHiddenYTranslation
        {
            get => (double)GetValue(OverflowContentHiddenYTranslationProperty);
            internal set => SetValue(OverflowContentHiddenYTranslationPropertyKey, value);
        }
    }
}
