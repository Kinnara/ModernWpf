using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;

namespace ModernWpf.Controls
{
    [ContentProperty(nameof(Child))]
    public class ParallaxView : Decorator
    {
        private const double Epsilon = 0.000001;

        public ParallaxView()
        {
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        public static readonly DependencyProperty ChildProperty =
            DependencyProperty.Register(
                nameof(Child),
                typeof(UIElement),
                typeof(ParallaxView),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange,
                    OnChildPropertyChanged));

        public new UIElement Child
        {
            get => (UIElement)GetValue(ChildProperty);
            set => SetValue(ChildProperty, value);
        }

        public static readonly DependencyProperty HorizontalShiftProperty =
            DependencyProperty.Register(
                nameof(HorizontalShift),
                typeof(double),
                typeof(ParallaxView),
                new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));

        public double HorizontalShift
        {
            get => (double)GetValue(HorizontalShiftProperty);
            set => SetValue(HorizontalShiftProperty, value);
        }

        public static readonly DependencyProperty HorizontalSourceEndOffsetProperty =
            DependencyProperty.Register(
                nameof(HorizontalSourceEndOffset),
                typeof(double),
                typeof(ParallaxView),
                new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsArrange));

        public double HorizontalSourceEndOffset
        {
            get => (double)GetValue(HorizontalSourceEndOffsetProperty);
            set => SetValue(HorizontalSourceEndOffsetProperty, value);
        }

        public static readonly DependencyProperty HorizontalSourceOffsetKindProperty =
            DependencyProperty.Register(
                nameof(HorizontalSourceOffsetKind),
                typeof(ParallaxSourceOffsetKind),
                typeof(ParallaxView),
                new FrameworkPropertyMetadata(ParallaxSourceOffsetKind.Relative, FrameworkPropertyMetadataOptions.AffectsArrange));

        public ParallaxSourceOffsetKind HorizontalSourceOffsetKind
        {
            get => (ParallaxSourceOffsetKind)GetValue(HorizontalSourceOffsetKindProperty);
            set => SetValue(HorizontalSourceOffsetKindProperty, value);
        }

        public static readonly DependencyProperty HorizontalSourceStartOffsetProperty =
            DependencyProperty.Register(
                nameof(HorizontalSourceStartOffset),
                typeof(double),
                typeof(ParallaxView),
                new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsArrange));

        public double HorizontalSourceStartOffset
        {
            get => (double)GetValue(HorizontalSourceStartOffsetProperty);
            set => SetValue(HorizontalSourceStartOffsetProperty, value);
        }

        public static readonly DependencyProperty IsHorizontalShiftClampedProperty =
            DependencyProperty.Register(
                nameof(IsHorizontalShiftClamped),
                typeof(bool),
                typeof(ParallaxView),
                new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsArrange));

        public bool IsHorizontalShiftClamped
        {
            get => (bool)GetValue(IsHorizontalShiftClampedProperty);
            set => SetValue(IsHorizontalShiftClampedProperty, value);
        }

        public static readonly DependencyProperty IsVerticalShiftClampedProperty =
            DependencyProperty.Register(
                nameof(IsVerticalShiftClamped),
                typeof(bool),
                typeof(ParallaxView),
                new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsArrange));

        public bool IsVerticalShiftClamped
        {
            get => (bool)GetValue(IsVerticalShiftClampedProperty);
            set => SetValue(IsVerticalShiftClampedProperty, value);
        }

        public static readonly DependencyProperty MaxHorizontalShiftRatioProperty =
            DependencyProperty.Register(
                nameof(MaxHorizontalShiftRatio),
                typeof(double),
                typeof(ParallaxView),
                new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.AffectsArrange));

        public double MaxHorizontalShiftRatio
        {
            get => (double)GetValue(MaxHorizontalShiftRatioProperty);
            set => SetValue(MaxHorizontalShiftRatioProperty, value);
        }

        public static readonly DependencyProperty MaxVerticalShiftRatioProperty =
            DependencyProperty.Register(
                nameof(MaxVerticalShiftRatio),
                typeof(double),
                typeof(ParallaxView),
                new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.AffectsArrange));

        public double MaxVerticalShiftRatio
        {
            get => (double)GetValue(MaxVerticalShiftRatioProperty);
            set => SetValue(MaxVerticalShiftRatioProperty, value);
        }

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register(
                nameof(Source),
                typeof(UIElement),
                typeof(ParallaxView),
                new FrameworkPropertyMetadata(null, OnSourcePropertyChanged));

        public UIElement Source
        {
            get => (UIElement)GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        public static readonly DependencyProperty VerticalShiftProperty =
            DependencyProperty.Register(
                nameof(VerticalShift),
                typeof(double),
                typeof(ParallaxView),
                new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));

        public double VerticalShift
        {
            get => (double)GetValue(VerticalShiftProperty);
            set => SetValue(VerticalShiftProperty, value);
        }

        public static readonly DependencyProperty VerticalSourceEndOffsetProperty =
            DependencyProperty.Register(
                nameof(VerticalSourceEndOffset),
                typeof(double),
                typeof(ParallaxView),
                new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsArrange));

        public double VerticalSourceEndOffset
        {
            get => (double)GetValue(VerticalSourceEndOffsetProperty);
            set => SetValue(VerticalSourceEndOffsetProperty, value);
        }

        public static readonly DependencyProperty VerticalSourceOffsetKindProperty =
            DependencyProperty.Register(
                nameof(VerticalSourceOffsetKind),
                typeof(ParallaxSourceOffsetKind),
                typeof(ParallaxView),
                new FrameworkPropertyMetadata(ParallaxSourceOffsetKind.Relative, FrameworkPropertyMetadataOptions.AffectsArrange));

        public ParallaxSourceOffsetKind VerticalSourceOffsetKind
        {
            get => (ParallaxSourceOffsetKind)GetValue(VerticalSourceOffsetKindProperty);
            set => SetValue(VerticalSourceOffsetKindProperty, value);
        }

        public static readonly DependencyProperty VerticalSourceStartOffsetProperty =
            DependencyProperty.Register(
                nameof(VerticalSourceStartOffset),
                typeof(double),
                typeof(ParallaxView),
                new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsArrange));

        public double VerticalSourceStartOffset
        {
            get => (double)GetValue(VerticalSourceStartOffsetProperty);
            set => SetValue(VerticalSourceStartOffsetProperty, value);
        }

        public void RefreshAutomaticHorizontalOffsets()
        {
            InvalidateArrange();
        }

        public void RefreshAutomaticVerticalOffsets()
        {
            InvalidateArrange();
        }

        protected override Size MeasureOverride(Size constraint)
        {
            var child = Child;
            if (child == null)
            {
                return new Size();
            }

            var childConstraint = new Size(
                AddShift(constraint.Width, HorizontalShift),
                AddShift(constraint.Height, VerticalShift));

            child.Measure(childConstraint);

            return new Size(
                double.IsInfinity(constraint.Width) ? child.DesiredSize.Width : constraint.Width,
                double.IsInfinity(constraint.Height) ? child.DesiredSize.Height : constraint.Height);
        }

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            var child = Child;
            if (child != null)
            {
                var childRect = GetChildArrangeRect(child, arrangeSize);
                childRect.Offset(CalculateHorizontalOffset(), CalculateVerticalOffset());
                child.Arrange(childRect);
            }

            Clip = new RectangleGeometry(new Rect(arrangeSize));
            return arrangeSize;
        }

        private static void OnChildPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ParallaxView)d).OnChildChanged((UIElement)e.OldValue, (UIElement)e.NewValue);
        }

        private void OnChildChanged(UIElement oldChild, UIElement newChild)
        {
            UnhookChildAlignmentChanged(oldChild as FrameworkElement);
            base.Child = newChild;
            HookChildAlignmentChanged(newChild as FrameworkElement);
        }

        private static void OnSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ParallaxView)d).OnSourceChanged((UIElement)e.OldValue, (UIElement)e.NewValue);
        }

        private void OnSourceChanged(UIElement oldSource, UIElement newSource)
        {
            if (oldSource is FrameworkElement oldSourceElement)
            {
                oldSourceElement.Loaded -= OnSourceLoaded;
            }

            if (newSource is FrameworkElement newSourceElement)
            {
                newSourceElement.Loaded += OnSourceLoaded;
            }

            ResolveSourceScrollViewer();
            InvalidateArrange();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            ResolveSourceScrollViewer();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            SetSourceScrollViewer(null);
        }

        private void OnSourceLoaded(object sender, RoutedEventArgs e)
        {
            ResolveSourceScrollViewer();
        }

        private void OnSourceScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            InvalidateArrange();
        }

        private void HookChildAlignmentChanged(FrameworkElement child)
        {
            if (child == null)
            {
                return;
            }

            _horizontalAlignmentDescriptor = DependencyPropertyDescriptor.FromProperty(FrameworkElement.HorizontalAlignmentProperty, typeof(FrameworkElement));
            _verticalAlignmentDescriptor = DependencyPropertyDescriptor.FromProperty(FrameworkElement.VerticalAlignmentProperty, typeof(FrameworkElement));
            _horizontalAlignmentDescriptor?.AddValueChanged(child, OnChildAlignmentChanged);
            _verticalAlignmentDescriptor?.AddValueChanged(child, OnChildAlignmentChanged);
        }

        private void UnhookChildAlignmentChanged(FrameworkElement child)
        {
            if (child == null)
            {
                return;
            }

            _horizontalAlignmentDescriptor?.RemoveValueChanged(child, OnChildAlignmentChanged);
            _verticalAlignmentDescriptor?.RemoveValueChanged(child, OnChildAlignmentChanged);
            _horizontalAlignmentDescriptor = null;
            _verticalAlignmentDescriptor = null;
        }

        private void OnChildAlignmentChanged(object sender, EventArgs e)
        {
            InvalidateArrange();
        }

        private void ResolveSourceScrollViewer()
        {
            var source = Source;
            var scrollViewer = source as ScrollViewer ?? FindDescendant<ScrollViewer>(source);
            SetSourceScrollViewer(scrollViewer);
        }

        private void SetSourceScrollViewer(ScrollViewer scrollViewer)
        {
            if (ReferenceEquals(_sourceScrollViewer, scrollViewer))
            {
                return;
            }

            if (_sourceScrollViewer != null)
            {
                _sourceScrollViewer.ScrollChanged -= OnSourceScrollChanged;
            }

            _sourceScrollViewer = scrollViewer;

            if (_sourceScrollViewer != null)
            {
                _sourceScrollViewer.ScrollChanged += OnSourceScrollChanged;
            }
        }

        private double CalculateHorizontalOffset()
        {
            if (_sourceScrollViewer == null)
            {
                return 0;
            }

            return CalculateParallaxOffset(
                _sourceScrollViewer.HorizontalOffset,
                HorizontalSourceStartOffset,
                GetHorizontalSourceEndOffset(),
                HorizontalShift,
                MaxHorizontalShiftRatio,
                IsHorizontalShiftClamped);
        }

        private double CalculateVerticalOffset()
        {
            if (_sourceScrollViewer == null)
            {
                return 0;
            }

            return CalculateParallaxOffset(
                _sourceScrollViewer.VerticalOffset,
                VerticalSourceStartOffset,
                GetVerticalSourceEndOffset(),
                VerticalShift,
                MaxVerticalShiftRatio,
                IsVerticalShiftClamped);
        }

        private double GetHorizontalSourceEndOffset()
        {
            return HorizontalSourceOffsetKind == ParallaxSourceOffsetKind.Absolute
                ? HorizontalSourceEndOffset
                : _sourceScrollViewer.ScrollableWidth + HorizontalSourceEndOffset;
        }

        private double GetVerticalSourceEndOffset()
        {
            return VerticalSourceOffsetKind == ParallaxSourceOffsetKind.Absolute
                ? VerticalSourceEndOffset
                : _sourceScrollViewer.ScrollableHeight + VerticalSourceEndOffset;
        }

        private static double CalculateParallaxOffset(
            double sourceOffset,
            double startOffset,
            double endOffset,
            double shift,
            double maxShiftRatio,
            bool isShiftClamped)
        {
            var span = endOffset - startOffset;
            if (Math.Abs(shift) < Epsilon || span <= Epsilon)
            {
                return 0;
            }

            var maxRatio = Math.Max(0.0, maxShiftRatio);
            if (maxRatio < Epsilon)
            {
                return 0;
            }

            if (shift > 0)
            {
                if (isShiftClamped)
                {
                    if (sourceOffset <= startOffset)
                    {
                        return 0;
                    }

                    if (sourceOffset < endOffset)
                    {
                        return -Math.Min(maxRatio, shift / span) * (sourceOffset - startOffset);
                    }

                    return -Math.Min(maxRatio * span, shift);
                }

                return -Math.Min(maxRatio, shift / span) * (sourceOffset - startOffset);
            }

            if (isShiftClamped)
            {
                if (sourceOffset <= startOffset)
                {
                    return -Math.Min(maxRatio * span, -shift);
                }

                if (sourceOffset < endOffset)
                {
                    return Math.Min(maxRatio, shift / (startOffset - endOffset)) * (sourceOffset - endOffset);
                }

                return 0;
            }

            return Math.Min(maxRatio, shift / (startOffset - endOffset)) * (sourceOffset - endOffset);
        }

        private Rect GetChildArrangeRect(UIElement child, Size finalSize)
        {
            var childSize = child.DesiredSize;
            var width = childSize.Width;
            var height = childSize.Height;
            var childAsFrameworkElement = child as FrameworkElement;

            if (Math.Abs(HorizontalShift) > Epsilon && width < finalSize.Width + Math.Abs(HorizontalShift))
            {
                var stretchRatio = width > Epsilon ? (finalSize.Width + Math.Abs(HorizontalShift)) / width : 0;
                width = finalSize.Width + Math.Abs(HorizontalShift);

                if (stretchRatio > Epsilon &&
                    childAsFrameworkElement != null &&
                    double.IsNaN(childAsFrameworkElement.Height) &&
                    childAsFrameworkElement.VerticalAlignment == VerticalAlignment.Stretch)
                {
                    height *= stretchRatio;
                }
            }

            if (Math.Abs(VerticalShift) > Epsilon && height < finalSize.Height + Math.Abs(VerticalShift))
            {
                var stretchRatio = height > Epsilon ? (finalSize.Height + Math.Abs(VerticalShift)) / height : 0;
                height = finalSize.Height + Math.Abs(VerticalShift);

                if (stretchRatio > Epsilon &&
                    childAsFrameworkElement != null &&
                    double.IsNaN(childAsFrameworkElement.Width) &&
                    childAsFrameworkElement.HorizontalAlignment == HorizontalAlignment.Stretch)
                {
                    width *= stretchRatio;
                }
            }

            var x = 0.0;
            var y = 0.0;

            if (childAsFrameworkElement != null)
            {
                x = GetAlignedOffset(finalSize.Width, width, childAsFrameworkElement.HorizontalAlignment);
                y = GetAlignedOffset(finalSize.Height, height, childAsFrameworkElement.VerticalAlignment);
            }

            return new Rect(x, y, width, height);
        }

        private static double GetAlignedOffset(double finalLength, double childLength, HorizontalAlignment alignment)
        {
            switch (alignment)
            {
                case HorizontalAlignment.Center:
                    return (finalLength - childLength) / 2.0;

                case HorizontalAlignment.Right:
                    return finalLength - childLength;

                case HorizontalAlignment.Stretch:
                    return childLength < finalLength ? (finalLength - childLength) / 2.0 : 0.0;

                default:
                    return 0.0;
            }
        }

        private static double GetAlignedOffset(double finalLength, double childLength, VerticalAlignment alignment)
        {
            switch (alignment)
            {
                case VerticalAlignment.Center:
                    return (finalLength - childLength) / 2.0;

                case VerticalAlignment.Bottom:
                    return finalLength - childLength;

                case VerticalAlignment.Stretch:
                    return childLength < finalLength ? (finalLength - childLength) / 2.0 : 0.0;

                default:
                    return 0.0;
            }
        }

        private static double AddShift(double length, double shift)
        {
            return double.IsInfinity(length) ? length : Math.Max(0.0, length + Math.Abs(shift));
        }

        private static T FindDescendant<T>(DependencyObject root)
            where T : DependencyObject
        {
            if (root == null)
            {
                return null;
            }

            var count = VisualTreeHelper.GetChildrenCount(root);
            for (var i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(root, i);
                if (child is T match)
                {
                    return match;
                }

                var descendant = FindDescendant<T>(child);
                if (descendant != null)
                {
                    return descendant;
                }
            }

            return null;
        }

        private ScrollViewer _sourceScrollViewer;
        private DependencyPropertyDescriptor _horizontalAlignmentDescriptor;
        private DependencyPropertyDescriptor _verticalAlignmentDescriptor;
    }
}
