using System.Windows;
using System.Windows.Media;
using ModernWpf.Controls.Primitives;

namespace ModernWpf.Controls
{
    partial class SplitView
    {
        #region CompactPaneLength

        public static readonly DependencyProperty CompactPaneLengthProperty =
            DependencyProperty.Register(
                nameof(CompactPaneLength),
                typeof(double),
                typeof(SplitView),
                new FrameworkPropertyMetadata(
                    0d,
                    FrameworkPropertyMetadataOptions.AffectsMeasure,
                    OnCompactPaneLengthPropertyChanged));

        public double CompactPaneLength
        {
            get => (double)GetValue(CompactPaneLengthProperty);
            set => SetValue(CompactPaneLengthProperty, value);
        }

        private static void OnCompactPaneLengthPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((SplitView)sender).OnCompactPaneLengthPropertyChanged(args);
        }

        private void OnCompactPaneLengthPropertyChanged(DependencyPropertyChangedEventArgs args)
        {
            UpdateTemplateSettings();
            UpdateVisualState(false);
            CompactPaneLengthChanged?.Invoke(this, args.Property);
        }

        #endregion

        #region Content

        public static readonly DependencyProperty ContentProperty =
            DependencyProperty.Register(
                nameof(Content),
                typeof(UIElement),
                typeof(SplitView),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.AffectsMeasure,
                    OnContentPropertyChanged));

        public UIElement Content
        {
            get => (UIElement)GetValue(ContentProperty);
            set => SetValue(ContentProperty, value);
        }

        private static void OnContentPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((SplitView)sender).OnContentPropertyChanged(args);
        }

        #endregion

        #region DisplayMode

        public static readonly DependencyProperty DisplayModeProperty =
            DependencyProperty.Register(
                nameof(DisplayMode),
                typeof(SplitViewDisplayMode),
                typeof(SplitView),
                new FrameworkPropertyMetadata(
                    SplitViewDisplayMode.Overlay,
                    FrameworkPropertyMetadataOptions.AffectsMeasure,
                    OnDisplayModePropertyChanged));

        public SplitViewDisplayMode DisplayMode
        {
            get => (SplitViewDisplayMode)GetValue(DisplayModeProperty);
            set => SetValue(DisplayModeProperty, value);
        }

        private static void OnDisplayModePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((SplitView)sender).OnDisplayModePropertyChanged(args);
        }

        private void OnDisplayModePropertyChanged(DependencyPropertyChangedEventArgs args)
        {
            RestoreSavedFocusElement();
            UpdateVisualState();
            SetupOuterDismissLayer();
            if (!CanLightDismiss())
            {
                TeardownOuterDismissLayer();
            }

            DisplayModeChanged?.Invoke(this, args.Property);
        }

        #endregion

        #region IsPaneOpen

        public static readonly DependencyProperty IsPaneOpenProperty =
            DependencyProperty.Register(
                nameof(IsPaneOpen),
                typeof(bool),
                typeof(SplitView),
                new FrameworkPropertyMetadata(
                    false,
                    FrameworkPropertyMetadataOptions.AffectsMeasure,
                    OnIsPaneOpenPropertyChanged));

        public bool IsPaneOpen
        {
            get => (bool)GetValue(IsPaneOpenProperty);
            set => SetValue(IsPaneOpenProperty, value);
        }

        private static void OnIsPaneOpenPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((SplitView)sender).OnIsPaneOpenPropertyChanged(args);
        }

        private void OnIsPaneOpenPropertyChanged(DependencyPropertyChangedEventArgs args)
        {
            if ((bool)args.NewValue)
            {
                _isPaneClosing = false;
                OpenPane();
            }
            else
            {
                _isPaneOpening = false;
                ClosePane();
            }

            UpdateOverlayVisibilityState();

            IsPaneOpenChanged?.Invoke(this, args.Property);
        }

        #endregion

        #region OpenPaneLength

        public static readonly DependencyProperty OpenPaneLengthProperty =
            DependencyProperty.Register(
                nameof(OpenPaneLength),
                typeof(double),
                typeof(SplitView),
                new FrameworkPropertyMetadata(
                    0d,
                    FrameworkPropertyMetadataOptions.AffectsMeasure,
                    OnOpenPaneLengthPropertyChanged));

        public double OpenPaneLength
        {
            get => (double)GetValue(OpenPaneLengthProperty);
            set => SetValue(OpenPaneLengthProperty, value);
        }

        private static void OnOpenPaneLengthPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((SplitView)sender).OnOpenPaneLengthPropertyChanged(args);
        }

        private void OnOpenPaneLengthPropertyChanged(DependencyPropertyChangedEventArgs args)
        {
            UpdateTemplateSettings();
            UpdatePaneClipRectangle();
            UpdateVisualState(false);
        }

        #endregion

        #region PaneBackground

        public static readonly DependencyProperty PaneBackgroundProperty =
            DependencyProperty.Register(
                nameof(PaneBackground),
                typeof(Brush),
                typeof(SplitView));

        public Brush PaneBackground
        {
            get => (Brush)GetValue(PaneBackgroundProperty);
            set => SetValue(PaneBackgroundProperty, value);
        }

        #endregion

        #region PanePlacement

        public static readonly DependencyProperty PanePlacementProperty =
            DependencyProperty.Register(
                nameof(PanePlacement),
                typeof(SplitViewPanePlacement),
                typeof(SplitView),
                new FrameworkPropertyMetadata(
                    SplitViewPanePlacement.Left,
                    FrameworkPropertyMetadataOptions.AffectsMeasure,
                    OnPanePlacementPropertyChanged));

        public SplitViewPanePlacement PanePlacement
        {
            get => (SplitViewPanePlacement)GetValue(PanePlacementProperty);
            set => SetValue(PanePlacementProperty, value);
        }

        private static void OnPanePlacementPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((SplitView)sender).OnPanePlacementPropertyChanged(args);
        }

        private void OnPanePlacementPropertyChanged(DependencyPropertyChangedEventArgs args)
        {
            UpdateVisualState();
        }

        #endregion

        #region Pane

        public static readonly DependencyProperty PaneProperty =
            DependencyProperty.Register(
                nameof(Pane),
                typeof(UIElement),
                typeof(SplitView),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.AffectsMeasure,
                    OnPanePropertyChanged));

        public UIElement Pane
        {
            get => (UIElement)GetValue(PaneProperty);
            set => SetValue(PaneProperty, value);
        }

        private static void OnPanePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((SplitView)sender).OnPanePropertyChanged(args);
        }

        private void OnContentPropertyChanged(DependencyPropertyChangedEventArgs args)
        {
            InvalidateMeasure();
        }

        private void OnPanePropertyChanged(DependencyPropertyChangedEventArgs args)
        {
            InvalidateMeasure();
            UpdateTemplateSettings(false);
            UpdatePaneClipRectangle();
        }

        #endregion

        #region TemplateSettings

        private static readonly DependencyPropertyKey TemplateSettingsPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(TemplateSettings),
                typeof(SplitViewTemplateSettings),
                typeof(SplitView),
                null);

        public static readonly DependencyProperty TemplateSettingsProperty =
            TemplateSettingsPropertyKey.DependencyProperty;

        public SplitViewTemplateSettings TemplateSettings
        {
            get => (SplitViewTemplateSettings)GetValue(TemplateSettingsProperty);
            private set => SetValue(TemplateSettingsPropertyKey, value);
        }

        #endregion

        #region LightDismissOverlayMode

        public static readonly DependencyProperty LightDismissOverlayModeProperty =
            DependencyProperty.Register(
                nameof(LightDismissOverlayMode),
                typeof(LightDismissOverlayMode),
                typeof(SplitView),
                new PropertyMetadata(LightDismissOverlayMode.Auto, OnLightDismissOverlayModePropertyChanged));

        public LightDismissOverlayMode LightDismissOverlayMode
        {
            get => (LightDismissOverlayMode)GetValue(LightDismissOverlayModeProperty);
            set => SetValue(LightDismissOverlayModeProperty, value);
        }

        private static void OnLightDismissOverlayModePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((SplitView)sender).OnLightDismissOverlayModePropertyChanged(args);
        }

        private void OnLightDismissOverlayModePropertyChanged(DependencyPropertyChangedEventArgs args)
        {
            UpdateOverlayVisibilityState();
        }

        #endregion
    }
}
