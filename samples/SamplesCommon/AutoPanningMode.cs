using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace SamplesCommon
{
    public static class AutoPanningMode
    {
        #region IsEnabled

        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached(
                "IsEnabled",
                typeof(bool),
                typeof(AutoPanningMode),
                new PropertyMetadata(false, OnIsEnabledChanged));

        public static bool GetIsEnabled(ScrollViewer scrollViewer)
        {
            return (bool)scrollViewer.GetValue(IsEnabledProperty);
        }

        public static void SetIsEnabled(ScrollViewer scrollViewer, bool value)
        {
            scrollViewer.SetValue(IsEnabledProperty, value);
        }

        private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sv = (ScrollViewer)d;
            if ((bool)e.NewValue)
            {
                sv.Loaded += ScrollViewer_Loaded;
            }
            else
            {
                sv.Loaded -= ScrollViewer_Loaded;
            }
        }

        #endregion

        private static void ScrollViewer_Loaded(object sender, RoutedEventArgs e)
        {
            var sv = (ScrollViewer)sender;
            sv.Loaded -= ScrollViewer_Loaded;

            if (sv.TemplatedParent == null)
            {
                var valueSource = DependencyPropertyHelper.GetValueSource(sv, ScrollViewer.PanningModeProperty).BaseValueSource;
                if (valueSource == BaseValueSource.Default)
                {
                    sv.SetBinding(ScrollViewer.PanningModeProperty, new MultiBinding
                    {
                        Bindings =
                        {
                            new Binding
                            {
                                Path = new PropertyPath(ScrollViewer.ComputedHorizontalScrollBarVisibilityProperty),
                                RelativeSource = new RelativeSource(RelativeSourceMode.Self)
                            },
                            new Binding
                            {
                                Path = new PropertyPath(ScrollViewer.ComputedVerticalScrollBarVisibilityProperty),
                                RelativeSource = new RelativeSource(RelativeSourceMode.Self)
                            },
                        },
                        Converter = new PanningModeConverter()
                    });
                }
            }
        }

        private class PanningModeConverter : IMultiValueConverter
        {
            public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
            {
                PanningMode mode;

                var computedHorizontalScrollBarVisibility = (Visibility)values[0];
                var computedVerticalScrollBarVisibility = (Visibility)values[1];

                if (computedHorizontalScrollBarVisibility == Visibility.Visible &&
                    computedVerticalScrollBarVisibility == Visibility.Visible)
                {
                    mode = PanningMode.Both;
                }
                else if (computedHorizontalScrollBarVisibility != Visibility.Collapsed)
                {
                    mode = PanningMode.HorizontalOnly;
                }
                else if (computedVerticalScrollBarVisibility != Visibility.Collapsed)
                {
                    mode = PanningMode.VerticalOnly;
                }
                else
                {
                    mode = PanningMode.None;
                }

                return mode;
            }

            public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            {
                throw new NotImplementedException();
            }
        }
    }
}
