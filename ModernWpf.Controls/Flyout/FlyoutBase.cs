using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;

namespace ModernWpf.Controls.Primitives
{
    public abstract class FlyoutBase : DependencyObject
    {
        protected FlyoutBase()
        {
        }

        #region Placement

        public static readonly DependencyProperty PlacementProperty =
            DependencyProperty.Register(
                nameof(Placement),
                typeof(FlyoutPlacementMode),
                typeof(FlyoutBase),
                new PropertyMetadata(FlyoutPlacementMode.Top));

        public FlyoutPlacementMode Placement
        {
            get => (FlyoutPlacementMode)GetValue(PlacementProperty);
            set => SetValue(PlacementProperty, value);
        }

        #endregion

        #region AttachedFlyout

        public static readonly DependencyProperty AttachedFlyoutProperty =
            DependencyProperty.RegisterAttached(
                "AttachedFlyout",
                typeof(FlyoutBase),
                typeof(FlyoutBase));

        public static FlyoutBase GetAttachedFlyout(FrameworkElement element)
        {
            return (FlyoutBase)element.GetValue(AttachedFlyoutProperty);
        }

        public static void SetAttachedFlyout(FrameworkElement element, FlyoutBase value)
        {
            element.SetValue(AttachedFlyoutProperty, value);
        }

        public static void ShowAttachedFlyout(FrameworkElement flyoutOwner)
        {
            var flyout = GetAttachedFlyout(flyoutOwner);
            if (flyout != null)
            {
                flyout.ShowAt(flyoutOwner);
            }
        }

        #endregion

        public event EventHandler<object> Closed;
        public event EventHandler<object> Opened;
        public event EventHandler<object> Opening;

        public void ShowAt(FrameworkElement placementTarget)
        {
            if (placementTarget is null)
            {
                throw new ArgumentNullException(nameof(placementTarget));
            }

            if (m_popup != null &&
                m_popup.IsOpen &&
                m_target == placementTarget)
            {
                return;
            }

            if (m_presenter == null)
            {
                m_presenter = CreatePresenter();
            }

            if (m_popup == null)
            {
                m_popup = new Popup
                {
                    Child = m_presenter,
                    StaysOpen = false,
                    AllowsTransparency = true,
                    PopupAnimation = PopupAnimation.Fade
                };
                m_popup.Opened += OnPopupOpened;
                m_popup.Closed += OnPopupClosed;
                m_popup.PreviewMouseLeftButtonDown += OnPopupPreviewMouseLeftButtonDown;
            }
            else if (m_popup.IsOpen)
            {
                m_popup.IsOpen = false;
            }

            var placement = Placement;
            m_popup.Placement = GetPopupPlacement(placement);

            if (placement == FlyoutPlacementMode.Full)
            {
                var window = Window.GetWindow(placementTarget);
                if (window != null)
                {
                    m_popup.PlacementTarget = window;

                    m_popup.SetBinding(
                        FrameworkElement.WidthProperty,
                        new MultiBinding
                        {
                            Converter = s_fullPlacementWidthConverter,
                            Bindings =
                            {
                                new Binding { Path = new PropertyPath(FrameworkElement.ActualWidthProperty), Source = window },
                                new Binding { Path = new PropertyPath(Control.BorderThicknessProperty), Source = window },
                            }
                        });

                    m_popup.SetBinding(
                        FrameworkElement.HeightProperty,
                        new MultiBinding
                        {
                            Converter = s_fullPlacementHeightConverter,
                            Bindings =
                            {
                                new Binding { Path = new PropertyPath(FrameworkElement.ActualHeightProperty), Source = window },
                                new Binding { Path = new PropertyPath(Control.BorderThicknessProperty), Source = window },
                            }
                        });
                }
                else
                {
                    m_popup.PlacementTarget = placementTarget;
                    m_popup.ClearValue(FrameworkElement.WidthProperty);
                    m_popup.ClearValue(FrameworkElement.HeightProperty);
                }

                m_popup.ClearValue(Popup.PlacementRectangleProperty);
                m_popup.ClearValue(Popup.HorizontalOffsetProperty);
                m_popup.ClearValue(Popup.VerticalOffsetProperty);
            }
            else
            {
                m_popup.PlacementTarget = placementTarget;

                m_popup.SetBinding(
                    Popup.PlacementRectangleProperty,
                    new MultiBinding
                    {
                        Converter = s_placementRectangleConverter,
                        Bindings =
                        {
                            new Binding { Path = new PropertyPath(FrameworkElement.ActualWidthProperty), Source = placementTarget },
                            new Binding { Path = new PropertyPath(FrameworkElement.ActualHeightProperty), Source = placementTarget },
                        }
                    });

                if (placement == FlyoutPlacementMode.Top ||
                    placement == FlyoutPlacementMode.Bottom)
                {
                    m_popup.ClearValue(Popup.VerticalOffsetProperty);
                    m_popup.SetBinding(
                        Popup.HorizontalOffsetProperty,
                        new MultiBinding
                        {
                            Converter = s_horizontalOffsetConverter,
                            Bindings =
                            {
                                new Binding { Path = new PropertyPath(FrameworkElement.ActualWidthProperty), Source = placementTarget },
                                new Binding { Path = new PropertyPath(FrameworkElement.ActualWidthProperty), Source = m_presenter },
                            }
                        });
                }
                else
                {
                    m_popup.ClearValue(Popup.HorizontalOffsetProperty);
                    m_popup.SetBinding(
                        Popup.VerticalOffsetProperty,
                        new MultiBinding
                        {
                            Converter = s_verticalOffsetConverter,
                            Bindings =
                            {
                                new Binding { Path = new PropertyPath(FrameworkElement.ActualHeightProperty), Source = placementTarget },
                                new Binding { Path = new PropertyPath(FrameworkElement.ActualHeightProperty), Source = m_presenter },
                            }
                        });
                }
            }

            m_target = placementTarget;
            Opening?.Invoke(this, s_sharedEventArgs);
            m_popup.IsOpen = true;
        }

        public void Hide()
        {
            if (m_popup != null)
            {
                m_popup.IsOpen = false;
            }
        }

        protected abstract Control CreatePresenter();

        private void OnPopupOpened(object sender, EventArgs e)
        {
            Opened?.Invoke(this, s_sharedEventArgs);
        }

        private void OnPopupClosed(object sender, EventArgs e)
        {
            Closed?.Invoke(this, s_sharedEventArgs);

            if (!m_popup.IsOpen)
            {
                m_popup.ClearValue(Popup.PlacementTargetProperty);
                m_popup.ClearValue(Popup.PlacementRectangleProperty);
                m_popup.ClearValue(Popup.HorizontalOffsetProperty);
                m_popup.ClearValue(Popup.VerticalOffsetProperty);
                m_popup.ClearValue(FrameworkElement.WidthProperty);
                m_popup.ClearValue(FrameworkElement.HeightProperty);
                m_target = null;
            }
        }

        private void OnPopupPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (m_target is Button button)
            {
                if (button.InputHitTest(e.GetPosition(button)) != null)
                {
                    e.Handled = true;
                }
            }
        }

        private static PlacementMode GetPopupPlacement(FlyoutPlacementMode flyoutPlacement)
        {
            switch (flyoutPlacement)
            {
                case FlyoutPlacementMode.Top:
                    return PlacementMode.Top;
                case FlyoutPlacementMode.Bottom:
                    return PlacementMode.Bottom;
                case FlyoutPlacementMode.Left:
                    return PlacementMode.Left;
                case FlyoutPlacementMode.Right:
                    return PlacementMode.Right;
                case FlyoutPlacementMode.Full:
                    return PlacementMode.Center;
                default:
                    throw new NotImplementedException();
            }
        }

        private static readonly object s_sharedEventArgs = new object();
        private static readonly IMultiValueConverter s_placementRectangleConverter = new PlacementRectangleConverter();
        private static readonly IMultiValueConverter s_horizontalOffsetConverter = new HorizontalOffsetConverter();
        private static readonly IMultiValueConverter s_verticalOffsetConverter = new VerticalOffsetConverter();
        private static readonly IMultiValueConverter s_fullPlacementWidthConverter = new FullPlacementWidthConverter();
        private static readonly IMultiValueConverter s_fullPlacementHeightConverter = new FullPlacementHeightConverter();

        private Control m_presenter;
        private Popup m_popup;
        private FrameworkElement m_target;

        private class PlacementRectangleConverter : IMultiValueConverter
        {
            private const double Margin = 4;

            public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
            {
                double width = (double)values[0];
                double height = (double)values[1];
                return new Rect(new Point(-Margin, -Margin), new Point(width + Margin, height + Margin));
            }

            public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }

        private class HorizontalOffsetConverter : IMultiValueConverter
        {
            public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
            {
                double placementTargetWidth = (double)values[0];
                double popupChildWidth = (double)values[1];
                return (placementTargetWidth - popupChildWidth) / 2;
            }

            public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }

        private class VerticalOffsetConverter : IMultiValueConverter
        {
            public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
            {
                double placementTargetHeight = (double)values[0];
                double popupChildHeight = (double)values[1];
                return (placementTargetHeight - popupChildHeight) / 2;
            }

            public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }

        private class FullPlacementWidthConverter : IMultiValueConverter
        {
            public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
            {
                double width = (double)values[0];
                Thickness border = (Thickness)values[1];
                return width - border.Left - border.Right;
            }

            public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }

        private class FullPlacementHeightConverter : IMultiValueConverter
        {
            public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
            {
                double height = (double)values[0];
                Thickness border = (Thickness)values[1];
                return height - border.Top - border.Bottom;
            }

            public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }
    }
}
