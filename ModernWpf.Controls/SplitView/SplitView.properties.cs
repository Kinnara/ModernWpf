using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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
                new PropertyMetadata(48d, OnCompactPaneLengthPropertyChanged));

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
            UpdateVisualState();
        }

        #endregion

        #region Content

        public static readonly DependencyProperty ContentProperty =
            DependencyProperty.Register(
                nameof(Content),
                typeof(UIElement),
                typeof(SplitView),
                new PropertyMetadata(null, OnContentPropertyChanged));

        public UIElement Content
        {
            get => (UIElement)GetValue(ContentProperty);
            set => SetValue(ContentProperty, value);
        }

        private static void OnContentPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((SplitView)sender).OnContentPropertyChanged(args);
        }

        private void OnContentPropertyChanged(DependencyPropertyChangedEventArgs args)
        {

        }

        #endregion

        #region DisplayMode

        public static readonly DependencyProperty DisplayModeProperty =
            DependencyProperty.Register(
                nameof(DisplayMode),
                typeof(SplitViewDisplayMode),
                typeof(SplitView),
                new PropertyMetadata(SplitViewDisplayMode.Overlay, OnDisplayModePropertyChanged));

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
            UpdateLightDismiss();
            UpdateVisualState();
        }

        #endregion

        #region IsPaneOpen

        public static readonly DependencyProperty IsPaneOpenProperty =
            DependencyProperty.Register(
                nameof(IsPaneOpen),
                typeof(bool),
                typeof(SplitView),
                new PropertyMetadata(false, OnIsPaneOpenPropertyChanged));

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
                _paneClosing = false;
                OpenPane();
            }
            else
            {
                _paneOpening = false;
                ClosePane();
            }

            UpdateLightDismiss();
            UpdateOverlayVisibilityState();
        }

        #endregion

        #region OpenPaneLength

        public static readonly DependencyProperty OpenPaneLengthProperty =
            DependencyProperty.Register(
                nameof(OpenPaneLength),
                typeof(double),
                typeof(SplitView),
                new PropertyMetadata(320d, OnOpenPaneLengthPropertyChanged));

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
        }

        #endregion

        #region PaneBackground

        public static readonly DependencyProperty PaneBackgroundProperty =
            DependencyProperty.Register(
                nameof(PaneBackground),
                typeof(Brush),
                typeof(SplitView),
                new PropertyMetadata(null, OnPaneBackgroundPropertyChanged));

        public Brush PaneBackground
        {
            get => (Brush)GetValue(PaneBackgroundProperty);
            set => SetValue(PaneBackgroundProperty, value);
        }

        private static void OnPaneBackgroundPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((SplitView)sender).OnPaneBackgroundPropertyChanged(args);
        }

        private void OnPaneBackgroundPropertyChanged(DependencyPropertyChangedEventArgs args)
        {

        }

        #endregion

        #region PanePlacement

        public static readonly DependencyProperty PanePlacementProperty =
            DependencyProperty.Register(
                nameof(PanePlacement),
                typeof(SplitViewPanePlacement),
                typeof(SplitView),
                new PropertyMetadata(SplitViewPanePlacement.Left, OnPanePlacementPropertyChanged));

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
                new PropertyMetadata(null, OnPanePropertyChanged));

        public UIElement Pane
        {
            get => (UIElement)GetValue(PaneProperty);
            set => SetValue(PaneProperty, value);
        }

        private static void OnPanePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((SplitView)sender).OnPanePropertyChanged(args);
        }

        private void OnPanePropertyChanged(DependencyPropertyChangedEventArgs args)
        {

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
