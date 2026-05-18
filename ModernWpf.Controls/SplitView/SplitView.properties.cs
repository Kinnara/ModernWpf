using System.Windows;

namespace ModernWpf.Controls
{
    partial class SplitView
    {
        private void OnCompactPaneLengthPropertyChanged(DependencyPropertyChangedEventArgs args)
        {
            UpdateTemplateSettings();
            UpdateVisualState(false);
            CompactPaneLengthChanged?.Invoke(this, args.Property);
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

        private void OnOpenPaneLengthPropertyChanged(DependencyPropertyChangedEventArgs args)
        {
            UpdateTemplateSettings();
            UpdatePaneClipRectangle();
            UpdateVisualState(false);
        }

        private void OnPanePlacementPropertyChanged(DependencyPropertyChangedEventArgs args)
        {
            UpdateVisualState();
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

        private void OnLightDismissOverlayModePropertyChanged(DependencyPropertyChangedEventArgs args)
        {
            UpdateOverlayVisibilityState();
        }
    }
}
