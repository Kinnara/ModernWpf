// Ported from https://github.com/sourcechord/FluentWPF/blob/master/FluentWPF/ParallaxView.cs

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows;

namespace ModernWpf.Controls
{
    public class ParallaxView : ContentControl
    {
        static ParallaxView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ParallaxView), new FrameworkPropertyMetadata(typeof(ParallaxView)));
        }

        #region VerticalShift

        public double VerticalShift
        {
            get { return (double)GetValue(VerticalShiftProperty); }
            set { SetValue(VerticalShiftProperty, value); }
        }

        // Using a DependencyProperty as the backing store for VerticalShift.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VerticalShiftProperty =
            DependencyProperty.Register(
                "VerticalShift",
                typeof(double),
                typeof(ParallaxView),
                new PropertyMetadata(0.0));

        #endregion

        #region HorizontalShift

        public double HorizontalShift
        {
            get { return (double)GetValue(HorizontalShiftProperty); }
            set { SetValue(HorizontalShiftProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HorizontalShift.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HorizontalShiftProperty =
            DependencyProperty.Register(
                "HorizontalShift",
                typeof(double),
                typeof(ParallaxView),
                new PropertyMetadata(0.0));

        #endregion

        #region OffsetMargin

        public Thickness OffsetMargin
        {
            get { return (Thickness)GetValue(OffsetMarginProperty); }
            private set { SetValue(OffsetMarginProperty, value); }
        }

        // Using a DependencyProperty as the backing store for OffsetMargin.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OffsetMarginProperty =
            DependencyProperty.Register(
                "OffsetMargin",
                typeof(Thickness),
                typeof(ParallaxView),
                new PropertyMetadata(new Thickness(0)));

        private void OnScrollUpdated(ScrollViewer scrollViewer)
        {
            var posX = scrollViewer.ScrollableWidth == 0 ? 0 : scrollViewer.HorizontalOffset / scrollViewer.ScrollableWidth;
            var posY = scrollViewer.ScrollableHeight == 0 ? 0 : scrollViewer.VerticalOffset / scrollViewer.ScrollableHeight;

            this.OffsetMargin = new Thickness(-posX * HorizontalShift, -posY * VerticalShift, 0, 0);
        }

        #endregion

        #region Source

        public UIElement Source
        {
            get { return (UIElement)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Source.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register(
                "Source",
                typeof(UIElement),
                typeof(ParallaxView),
                new PropertyMetadata(OnSourceChanged));

        private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var parallax = d as ParallaxView;
            var ctrl = e.NewValue as FrameworkElement;
            ctrl.Loaded += (_, __) =>
            {
                var viewer = GetScrollViewer(ctrl);

                if (viewer != null)
                {
                    viewer.ScrollChanged += (sender, e) => { parallax.OnScrollUpdated(sender as ScrollViewer); };
                    viewer.SizeChanged += (sender, e) => { parallax.OnScrollUpdated(sender as ScrollViewer); };
                }
            };
        }

        #endregion

        private static ScrollViewer GetScrollViewer(DependencyObject obj)
        {
            var viewer = obj as ScrollViewer ?? obj.FindDescendant<ScrollViewer>();
            return viewer;
        }

        private static ChildItem FindVisualChild<ChildItem>(DependencyObject obj)
            where ChildItem : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is ChildItem)
                    return (ChildItem)child;
                else
                {
                    ChildItem childOfChild = FindVisualChild<ChildItem>(child);
                    if (childOfChild != null)
                        return childOfChild;
                }
            }
            return null;
        }
    }

    public class AddValueConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            double sum = 0;
            foreach (var v in values)
            {
                var isInvalid = v == DependencyProperty.UnsetValue;
                if (isInvalid) continue;

                var value = (double)v;
                sum += value;
            }
            return sum;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
