using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using ModernWpf.Controls.Primitives;
using ModernWpf.Media.Animation;

namespace ModernWpf.Controls
{
    [ContentProperty(nameof(Children))]
    public class StackPanelEx : Panel, IScrollSnapPointsInfo
    {
        public StackPanelEx()
        {
            _itemsHost = new ItemsHost(this)
            {
                Orientation = Orientation,
                Spacing = Spacing
            };
            _border = new LayoutChromeDecorator { Child = _itemsHost };
            UpdateBorder();
            AddVisualChild(_border);
        }

        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register(
                nameof(Orientation),
                typeof(Orientation),
                typeof(StackPanelEx),
                new FrameworkPropertyMetadata(
                    System.Windows.Controls.Orientation.Vertical,
                    FrameworkPropertyMetadataOptions.AffectsMeasure,
                    OnOrientationPropertyChanged));

        public Orientation Orientation
        {
            get => (Orientation)GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }

        public static readonly DependencyProperty SpacingProperty =
            DependencyProperty.Register(
                nameof(Spacing),
                typeof(double),
                typeof(StackPanelEx),
                new FrameworkPropertyMetadata(
                    0.0,
                    FrameworkPropertyMetadataOptions.AffectsMeasure,
                    OnSpacingPropertyChanged));

        public double Spacing
        {
            get => (double)GetValue(SpacingProperty);
            set => SetValue(SpacingProperty, value);
        }

        public static readonly DependencyProperty AreScrollSnapPointsRegularProperty =
            DependencyProperty.Register(
                nameof(AreScrollSnapPointsRegular),
                typeof(bool),
                typeof(StackPanelEx),
                new PropertyMetadata(false, OnAreScrollSnapPointsRegularPropertyChanged));

        public bool AreScrollSnapPointsRegular
        {
            get => (bool)GetValue(AreScrollSnapPointsRegularProperty);
            set => SetValue(AreScrollSnapPointsRegularProperty, value);
        }

        public bool AreHorizontalSnapPointsRegular =>
            Orientation == System.Windows.Controls.Orientation.Horizontal && AreScrollSnapPointsRegular;

        public bool AreVerticalSnapPointsRegular =>
            Orientation == System.Windows.Controls.Orientation.Vertical && AreScrollSnapPointsRegular;

        public event EventHandler<object> HorizontalSnapPointsChanged
        {
            add
            {
                _horizontalSnapPointsChanged += value;
                CaptureSnapPointSignature(true, out _horizontalSnapPointSignature);
                _hasHorizontalSnapPointSignature = true;
            }
            remove
            {
                _horizontalSnapPointsChanged -= value;
                if (_horizontalSnapPointsChanged == null)
                {
                    _hasHorizontalSnapPointSignature = false;
                }
            }
        }

        public event EventHandler<object> VerticalSnapPointsChanged
        {
            add
            {
                _verticalSnapPointsChanged += value;
                CaptureSnapPointSignature(false, out _verticalSnapPointSignature);
                _hasVerticalSnapPointSignature = true;
            }
            remove
            {
                _verticalSnapPointsChanged -= value;
                if (_verticalSnapPointsChanged == null)
                {
                    _hasVerticalSnapPointSignature = false;
                }
            }
        }

        public static readonly DependencyProperty BackgroundSizingProperty =
            DependencyProperty.Register(
                nameof(BackgroundSizing),
                typeof(ModernWpf.Controls.BackgroundSizing),
                typeof(StackPanelEx),
                new FrameworkPropertyMetadata(
                    ModernWpf.Controls.BackgroundSizing.InnerBorderEdge,
                    FrameworkPropertyMetadataOptions.AffectsRender,
                    OnBorderPropertyChanged));

        public BackgroundSizing BackgroundSizing
        {
            get => (BackgroundSizing)GetValue(BackgroundSizingProperty);
            set => SetValue(BackgroundSizingProperty, value);
        }

        public static readonly DependencyProperty BackgroundTransitionProperty =
            DependencyProperty.Register(
                nameof(BackgroundTransition),
                typeof(BrushTransition),
                typeof(StackPanelEx),
                new PropertyMetadata(null));

        public BrushTransition BackgroundTransition
        {
            get => (BrushTransition)GetValue(BackgroundTransitionProperty);
            set => SetValue(BackgroundTransitionProperty, value);
        }

        public static readonly DependencyProperty BorderBrushProperty =
            Border.BorderBrushProperty.AddOwner(
                typeof(StackPanelEx),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.AffectsRender,
                    OnBorderPropertyChanged));

        public Brush BorderBrush
        {
            get => (Brush)GetValue(BorderBrushProperty);
            set => SetValue(BorderBrushProperty, value);
        }

        public static readonly DependencyProperty BorderThicknessProperty =
            Border.BorderThicknessProperty.AddOwner(
                typeof(StackPanelEx),
                new FrameworkPropertyMetadata(
                    new Thickness(),
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.AffectsArrange |
                    FrameworkPropertyMetadataOptions.AffectsRender,
                    OnBorderPropertyChanged));

        public Thickness BorderThickness
        {
            get => (Thickness)GetValue(BorderThicknessProperty);
            set => SetValue(BorderThicknessProperty, value);
        }

        public static readonly DependencyProperty ChildrenTransitionsProperty =
            DependencyProperty.Register(
                nameof(ChildrenTransitions),
                typeof(TransitionCollection),
                typeof(StackPanelEx),
                new PropertyMetadata(null));

        public TransitionCollection ChildrenTransitions
        {
            get => (TransitionCollection)GetValue(ChildrenTransitionsProperty);
            set => SetValue(ChildrenTransitionsProperty, value);
        }

        public static readonly DependencyProperty CornerRadiusProperty =
            Border.CornerRadiusProperty.AddOwner(
                typeof(StackPanelEx),
                new FrameworkPropertyMetadata(
                    new CornerRadius(),
                    FrameworkPropertyMetadataOptions.AffectsRender,
                    OnBorderPropertyChanged));

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        public static readonly DependencyProperty PaddingProperty =
            Control.PaddingProperty.AddOwner(
                typeof(StackPanelEx),
                new FrameworkPropertyMetadata(
                    new Thickness(),
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.AffectsArrange,
                    OnBorderPropertyChanged));

        public Thickness Padding
        {
            get => (Thickness)GetValue(PaddingProperty);
            set => SetValue(PaddingProperty, value);
        }

        static StackPanelEx()
        {
            BackgroundProperty.OverrideMetadata(
                typeof(StackPanelEx),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.AffectsRender,
                    OnBackgroundPropertyChanged));
        }

        protected override bool HasLogicalOrientation => true;

        protected override Orientation LogicalOrientation => Orientation;

        protected override int VisualChildrenCount => 1;

        protected override Visual GetVisualChild(int index)
        {
            if (index != 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return _border;
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            _border.Measure(availableSize);
            return _border.DesiredSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            _border.Arrange(new Rect(finalSize));
            return finalSize;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
        }

        protected override Geometry GetLayoutClip(Size layoutSlotSize)
        {
            return LayoutChromeHelper.CreateRoundedLayoutClip(
                layoutSlotSize,
                CornerRadius,
                base.GetLayoutClip(layoutSlotSize));
        }

        protected override UIElementCollection CreateUIElementCollection(FrameworkElement logicalParent)
        {
            return _itemsHost.Children;
        }

        public IReadOnlyList<float> GetIrregularSnapPoints(Orientation orientation, SnapPointsAlignment alignment)
        {
            if (AreScrollSnapPointsRegular)
            {
                throw new InvalidOperationException("Irregular snap points are not available when AreScrollSnapPointsRegular is true.");
            }

            if (!HasSnapPointsForOrientation(orientation))
            {
                return Array.Empty<float>();
            }

            var snapPoints = new List<float>();
            double lowerMarginSnapPointKey = GetLowerMarginSnapPointKey();
            double cumulatedDim = 0.0;
            bool isFirstChild = true;

            foreach (UIElement child in Children)
            {
                if (child == null)
                {
                    continue;
                }

                double childDim = GetChildSnapDimension(child);
                double snapPoint;

                switch (alignment)
                {
                    case SnapPointsAlignment.Near:
                        snapPoint = cumulatedDim;
                        break;

                    case SnapPointsAlignment.Center:
                        snapPoint = cumulatedDim + childDim / 2.0;
                        break;

                    case SnapPointsAlignment.Far:
                        snapPoint = cumulatedDim + childDim;
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(alignment), alignment, null);
                }

                cumulatedDim += childDim;

                if (!(alignment == SnapPointsAlignment.Near && isFirstChild))
                {
                    snapPoint += lowerMarginSnapPointKey;
                }

                snapPoints.Add((float)snapPoint);
                isFirstChild = false;
            }

            return snapPoints.AsReadOnly();
        }

        public float GetRegularSnapPoints(Orientation orientation, SnapPointsAlignment alignment, out float offset)
        {
            offset = 0.0f;

            if (!AreScrollSnapPointsRegular)
            {
                throw new InvalidOperationException("Regular snap points are not available when AreScrollSnapPointsRegular is false.");
            }

            if (!HasSnapPointsForOrientation(orientation))
            {
                return 0.0f;
            }

            foreach (UIElement child in Children)
            {
                if (child == null)
                {
                    continue;
                }

                double childDim = GetChildSnapDimension(child);

                switch (alignment)
                {
                    case SnapPointsAlignment.Near:
                        offset = (float)GetLowerMarginSnapPointKey();
                        break;

                    case SnapPointsAlignment.Center:
                        offset = (float)(childDim / 2.0 + GetLowerMarginSnapPointKey());
                        break;

                    case SnapPointsAlignment.Far:
                        offset = (float)GetUpperMarginSnapPointKey();
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(alignment), alignment, null);
                }

                return (float)childDim;
            }

            return 0.0f;
        }

        private static void OnBackgroundPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((StackPanelEx)d).UpdateBorder();
        }

        private static void OnAreScrollSnapPointsRegularPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((StackPanelEx)d).NotifySnapPointsChangedIfNeeded();
        }

        private static void OnBorderPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((StackPanelEx)d).UpdateBorder();
        }

        private static void OnOrientationPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var stackPanel = (StackPanelEx)d;
            if (stackPanel._itemsHost != null)
            {
                stackPanel._itemsHost.Orientation = (Orientation)e.NewValue;
                stackPanel._itemsHost.InvalidateMeasure();
            }

            stackPanel.NotifySnapPointsChangedIfNeeded();
        }

        private static void OnSpacingPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var stackPanel = (StackPanelEx)d;
            if (stackPanel._itemsHost != null)
            {
                stackPanel._itemsHost.Spacing = (double)e.NewValue;
                stackPanel._itemsHost.InvalidateMeasure();
            }
        }

        private void UpdateBorder()
        {
            if (_border == null)
            {
                return;
            }

            _border.Background = Background;
            _border.BackgroundSizing = BackgroundSizing;
            _border.BorderBrush = BorderBrush;
            _border.BorderThickness = BorderThickness;
            _border.CornerRadius = CornerRadius;
            _border.Padding = Padding;
        }

        private bool HasSnapPointsForOrientation(Orientation orientation)
        {
            return (Orientation == System.Windows.Controls.Orientation.Horizontal && orientation == System.Windows.Controls.Orientation.Horizontal) ||
                   (Orientation == System.Windows.Controls.Orientation.Vertical && orientation == System.Windows.Controls.Orientation.Vertical);
        }

        private double GetChildSnapDimension(UIElement child)
        {
            return Orientation == System.Windows.Controls.Orientation.Vertical
                ? child.DesiredSize.Height
                : child.DesiredSize.Width;
        }

        private double GetLowerMarginSnapPointKey()
        {
            return Orientation == System.Windows.Controls.Orientation.Horizontal ? Margin.Left : Margin.Top;
        }

        private double GetUpperMarginSnapPointKey()
        {
            return Orientation == System.Windows.Controls.Orientation.Horizontal ? Margin.Right : Margin.Bottom;
        }

        private void NotifySnapPointsChangedIfNeeded()
        {
            NotifySnapPointsChangedIfNeeded(true);
            NotifySnapPointsChangedIfNeeded(false);
        }

        private void NotifySnapPointsChangedIfNeeded(bool isHorizontal)
        {
            var handlers = isHorizontal ? _horizontalSnapPointsChanged : _verticalSnapPointsChanged;
            if (handlers == null)
            {
                return;
            }

            CaptureSnapPointSignature(isHorizontal, out var current);
            bool hasPrevious = isHorizontal ? _hasHorizontalSnapPointSignature : _hasVerticalSnapPointSignature;
            var previous = isHorizontal ? _horizontalSnapPointSignature : _verticalSnapPointSignature;

            if (!hasPrevious || !current.Equals(previous))
            {
                if (isHorizontal)
                {
                    _horizontalSnapPointSignature = current;
                    _hasHorizontalSnapPointSignature = true;
                }
                else
                {
                    _verticalSnapPointSignature = current;
                    _hasVerticalSnapPointSignature = true;
                }

                handlers(this, EventArgs.Empty);
            }
        }

        private void CaptureSnapPointSignature(bool isHorizontal, out SnapPointSignature signature)
        {
            bool isActiveAxis = isHorizontal
                ? Orientation == System.Windows.Controls.Orientation.Horizontal
                : Orientation == System.Windows.Controls.Orientation.Vertical;

            if (!isActiveAxis)
            {
                signature = new SnapPointSignature(false, false, 0.0, 0.0, Array.Empty<double>());
                return;
            }

            signature = new SnapPointSignature(
                true,
                AreScrollSnapPointsRegular,
                GetLowerMarginSnapPointKey(),
                GetUpperMarginSnapPointKey(),
                GetSnapPointKeys());
        }

        private double[] GetSnapPointKeys()
        {
            if (AreScrollSnapPointsRegular)
            {
                foreach (UIElement child in Children)
                {
                    if (child != null)
                    {
                        return new[] { GetChildSnapDimension(child) };
                    }
                }

                return new[] { 0.0 };
            }

            var keys = new List<double>();
            foreach (UIElement child in Children)
            {
                if (child != null)
                {
                    keys.Add(GetChildSnapDimension(child));
                }
            }

            return keys.ToArray();
        }

        private readonly LayoutChromeDecorator _border;
        private readonly ItemsHost _itemsHost;
        private EventHandler<object> _horizontalSnapPointsChanged;
        private EventHandler<object> _verticalSnapPointsChanged;
        private SnapPointSignature _horizontalSnapPointSignature;
        private SnapPointSignature _verticalSnapPointSignature;
        private bool _hasHorizontalSnapPointSignature;
        private bool _hasVerticalSnapPointSignature;

        private readonly struct SnapPointSignature
        {
            public SnapPointSignature(bool isActiveAxis, bool areRegular, double lowerMargin, double upperMargin, double[] keys)
            {
                IsActiveAxis = isActiveAxis;
                AreRegular = areRegular;
                LowerMargin = lowerMargin;
                UpperMargin = upperMargin;
                Keys = keys;
            }

            public bool Equals(SnapPointSignature other)
            {
                return IsActiveAxis == other.IsActiveAxis &&
                    AreRegular == other.AreRegular &&
                    LowerMargin.Equals(other.LowerMargin) &&
                    UpperMargin.Equals(other.UpperMargin) &&
                    Keys.SequenceEqual(other.Keys);
            }

            private bool IsActiveAxis { get; }
            private bool AreRegular { get; }
            private double LowerMargin { get; }
            private double UpperMargin { get; }
            private double[] Keys { get; }
        }

        private class ItemsHost : Panel
        {
            public ItemsHost(StackPanelEx owner)
            {
                _owner = owner;
            }

            public Orientation Orientation { get; set; } = System.Windows.Controls.Orientation.Vertical;

            public double Spacing { get; set; }

            protected override bool HasLogicalOrientation => true;

            protected override Orientation LogicalOrientation => Orientation;

            protected override Size MeasureOverride(Size constraint)
            {
                Size stackDesiredSize = new Size();
                UIElementCollection children = InternalChildren;
                Size layoutSlotSize = constraint;
                bool horizontal = Orientation == System.Windows.Controls.Orientation.Horizontal;
                double spacing = Spacing;
                bool hasVisibleChild = false;

                if (horizontal)
                {
                    layoutSlotSize.Width = double.PositiveInfinity;
                }
                else
                {
                    layoutSlotSize.Height = double.PositiveInfinity;
                }

                for (int i = 0, count = children.Count; i < count; ++i)
                {
                    UIElement child = children[i];

                    if (child == null)
                    {
                        continue;
                    }

                    bool isVisible = child.Visibility != Visibility.Collapsed;
                    if (isVisible)
                    {
                        hasVisibleChild = true;
                    }

                    child.Measure(layoutSlotSize);
                    Size childDesiredSize = child.DesiredSize;

                    if (horizontal)
                    {
                        stackDesiredSize.Width += (isVisible ? spacing : 0) + childDesiredSize.Width;
                        stackDesiredSize.Height = Math.Max(stackDesiredSize.Height, childDesiredSize.Height);
                    }
                    else
                    {
                        stackDesiredSize.Width = Math.Max(stackDesiredSize.Width, childDesiredSize.Width);
                        stackDesiredSize.Height += (isVisible ? spacing : 0) + childDesiredSize.Height;
                    }
                }

                if (horizontal)
                {
                    stackDesiredSize.Width -= hasVisibleChild ? spacing : 0;
                }
                else
                {
                    stackDesiredSize.Height -= hasVisibleChild ? spacing : 0;
                }

                return stackDesiredSize;
            }

            protected override Size ArrangeOverride(Size arrangeSize)
            {
                UIElementCollection children = InternalChildren;
                bool horizontal = Orientation == System.Windows.Controls.Orientation.Horizontal;
                Rect childRect = new Rect(arrangeSize);
                double previousChildSize = 0.0;
                double spacing = Spacing;

                for (int i = 0, count = children.Count; i < count; ++i)
                {
                    UIElement child = children[i];

                    if (child == null)
                    {
                        continue;
                    }

                    if (horizontal)
                    {
                        childRect.X += previousChildSize;
                        previousChildSize = child.DesiredSize.Width;
                        childRect.Width = previousChildSize;
                        childRect.Height = Math.Max(arrangeSize.Height, child.DesiredSize.Height);
                    }
                    else
                    {
                        childRect.Y += previousChildSize;
                        previousChildSize = child.DesiredSize.Height;
                        childRect.Height = previousChildSize;
                        childRect.Width = Math.Max(arrangeSize.Width, child.DesiredSize.Width);
                    }

                    if (child.Visibility != Visibility.Collapsed)
                    {
                        previousChildSize += spacing;
                    }

                    child.Arrange(childRect);
                }

                _owner.NotifySnapPointsChangedIfNeeded();
                return arrangeSize;
            }

            protected override UIElementCollection CreateUIElementCollection(FrameworkElement logicalParent)
            {
                return new UIElementCollection(this, _owner);
            }

            private readonly StackPanelEx _owner;
        }
    }
}
