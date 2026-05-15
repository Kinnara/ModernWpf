using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using ModernWpf.Media.Animation;

namespace ModernWpf.Controls
{
    [ContentProperty(nameof(Children))]
    public class RelativePanel : Panel
    {
        static RelativePanel()
        {
            BackgroundProperty.OverrideMetadata(
                typeof(RelativePanel),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.AffectsRender,
                    OnBackgroundPropertyChanged));
        }

        public RelativePanel()
        {
            _itemsHost = new ItemsHost(this);
            _border = new LayoutChromeDecorator { Child = _itemsHost };
            UpdateBorder();
            AddVisualChild(_border);
        }

        public static readonly DependencyProperty BackgroundSizingProperty =
            DependencyProperty.Register(
                nameof(BackgroundSizing),
                typeof(ModernWpf.Controls.BackgroundSizing),
                typeof(RelativePanel),
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
                typeof(RelativePanel),
                new PropertyMetadata(null, OnBorderPropertyChanged));

        public BrushTransition BackgroundTransition
        {
            get => (BrushTransition)GetValue(BackgroundTransitionProperty);
            set => SetValue(BackgroundTransitionProperty, value);
        }

        public static readonly DependencyProperty BorderBrushProperty =
            Border.BorderBrushProperty.AddOwner(
                typeof(RelativePanel),
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
                typeof(RelativePanel),
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
                typeof(RelativePanel),
                new PropertyMetadata(null));

        public TransitionCollection ChildrenTransitions
        {
            get => (TransitionCollection)GetValue(ChildrenTransitionsProperty);
            set => SetValue(ChildrenTransitionsProperty, value);
        }

        public static readonly DependencyProperty CornerRadiusProperty =
            Border.CornerRadiusProperty.AddOwner(
                typeof(RelativePanel),
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
                typeof(RelativePanel),
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

        public static readonly DependencyProperty LeftOfProperty =
            RegisterReferenceConstraint("LeftOf");

        public static object GetLeftOf(UIElement element) => element.GetValue(LeftOfProperty);

        public static void SetLeftOf(UIElement element, object value) => element.SetValue(LeftOfProperty, value);

        public static readonly DependencyProperty AboveProperty =
            RegisterReferenceConstraint("Above");

        public static object GetAbove(UIElement element) => element.GetValue(AboveProperty);

        public static void SetAbove(UIElement element, object value) => element.SetValue(AboveProperty, value);

        public static readonly DependencyProperty RightOfProperty =
            RegisterReferenceConstraint("RightOf");

        public static object GetRightOf(UIElement element) => element.GetValue(RightOfProperty);

        public static void SetRightOf(UIElement element, object value) => element.SetValue(RightOfProperty, value);

        public static readonly DependencyProperty BelowProperty =
            RegisterReferenceConstraint("Below");

        public static object GetBelow(UIElement element) => element.GetValue(BelowProperty);

        public static void SetBelow(UIElement element, object value) => element.SetValue(BelowProperty, value);

        public static readonly DependencyProperty AlignHorizontalCenterWithProperty =
            RegisterReferenceConstraint("AlignHorizontalCenterWith");

        public static object GetAlignHorizontalCenterWith(UIElement element) => element.GetValue(AlignHorizontalCenterWithProperty);

        public static void SetAlignHorizontalCenterWith(UIElement element, object value) => element.SetValue(AlignHorizontalCenterWithProperty, value);

        public static readonly DependencyProperty AlignVerticalCenterWithProperty =
            RegisterReferenceConstraint("AlignVerticalCenterWith");

        public static object GetAlignVerticalCenterWith(UIElement element) => element.GetValue(AlignVerticalCenterWithProperty);

        public static void SetAlignVerticalCenterWith(UIElement element, object value) => element.SetValue(AlignVerticalCenterWithProperty, value);

        public static readonly DependencyProperty AlignLeftWithProperty =
            RegisterReferenceConstraint("AlignLeftWith");

        public static object GetAlignLeftWith(UIElement element) => element.GetValue(AlignLeftWithProperty);

        public static void SetAlignLeftWith(UIElement element, object value) => element.SetValue(AlignLeftWithProperty, value);

        public static readonly DependencyProperty AlignTopWithProperty =
            RegisterReferenceConstraint("AlignTopWith");

        public static object GetAlignTopWith(UIElement element) => element.GetValue(AlignTopWithProperty);

        public static void SetAlignTopWith(UIElement element, object value) => element.SetValue(AlignTopWithProperty, value);

        public static readonly DependencyProperty AlignRightWithProperty =
            RegisterReferenceConstraint("AlignRightWith");

        public static object GetAlignRightWith(UIElement element) => element.GetValue(AlignRightWithProperty);

        public static void SetAlignRightWith(UIElement element, object value) => element.SetValue(AlignRightWithProperty, value);

        public static readonly DependencyProperty AlignBottomWithProperty =
            RegisterReferenceConstraint("AlignBottomWith");

        public static object GetAlignBottomWith(UIElement element) => element.GetValue(AlignBottomWithProperty);

        public static void SetAlignBottomWith(UIElement element, object value) => element.SetValue(AlignBottomWithProperty, value);

        public static readonly DependencyProperty AlignLeftWithPanelProperty =
            RegisterPanelConstraint("AlignLeftWithPanel");

        public static bool GetAlignLeftWithPanel(UIElement element) => (bool)element.GetValue(AlignLeftWithPanelProperty);

        public static void SetAlignLeftWithPanel(UIElement element, bool value) => element.SetValue(AlignLeftWithPanelProperty, value);

        public static readonly DependencyProperty AlignTopWithPanelProperty =
            RegisterPanelConstraint("AlignTopWithPanel");

        public static bool GetAlignTopWithPanel(UIElement element) => (bool)element.GetValue(AlignTopWithPanelProperty);

        public static void SetAlignTopWithPanel(UIElement element, bool value) => element.SetValue(AlignTopWithPanelProperty, value);

        public static readonly DependencyProperty AlignRightWithPanelProperty =
            RegisterPanelConstraint("AlignRightWithPanel");

        public static bool GetAlignRightWithPanel(UIElement element) => (bool)element.GetValue(AlignRightWithPanelProperty);

        public static void SetAlignRightWithPanel(UIElement element, bool value) => element.SetValue(AlignRightWithPanelProperty, value);

        public static readonly DependencyProperty AlignBottomWithPanelProperty =
            RegisterPanelConstraint("AlignBottomWithPanel");

        public static bool GetAlignBottomWithPanel(UIElement element) => (bool)element.GetValue(AlignBottomWithPanelProperty);

        public static void SetAlignBottomWithPanel(UIElement element, bool value) => element.SetValue(AlignBottomWithPanelProperty, value);

        public static readonly DependencyProperty AlignHorizontalCenterWithPanelProperty =
            RegisterPanelConstraint("AlignHorizontalCenterWithPanel");

        public static bool GetAlignHorizontalCenterWithPanel(UIElement element) => (bool)element.GetValue(AlignHorizontalCenterWithPanelProperty);

        public static void SetAlignHorizontalCenterWithPanel(UIElement element, bool value) => element.SetValue(AlignHorizontalCenterWithPanelProperty, value);

        public static readonly DependencyProperty AlignVerticalCenterWithPanelProperty =
            RegisterPanelConstraint("AlignVerticalCenterWithPanel");

        public static bool GetAlignVerticalCenterWithPanel(UIElement element) => (bool)element.GetValue(AlignVerticalCenterWithPanelProperty);

        public static void SetAlignVerticalCenterWithPanel(UIElement element, bool value) => element.SetValue(AlignVerticalCenterWithPanelProperty, value);

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

        internal Brush EffectiveBackground => _border?.EffectiveBackground ?? Background;

        private static DependencyProperty RegisterReferenceConstraint(string name)
        {
            return DependencyProperty.RegisterAttached(
                name,
                typeof(object),
                typeof(RelativePanel),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.AffectsParentMeasure |
                    FrameworkPropertyMetadataOptions.AffectsParentArrange,
                    OnConstraintPropertyChanged),
                IsValidReferenceConstraint);
        }

        private static DependencyProperty RegisterPanelConstraint(string name)
        {
            return DependencyProperty.RegisterAttached(
                name,
                typeof(bool),
                typeof(RelativePanel),
                new FrameworkPropertyMetadata(
                    false,
                    FrameworkPropertyMetadataOptions.AffectsParentMeasure |
                    FrameworkPropertyMetadataOptions.AffectsParentArrange,
                    OnConstraintPropertyChanged));
        }

        private static bool IsValidReferenceConstraint(object value)
        {
            return value == null || value is string || value is UIElement;
        }

        private static void OnBackgroundPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((RelativePanel)d).UpdateBorder();
        }

        private static void OnBorderPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((RelativePanel)d).UpdateBorder();
        }

        private static void OnConstraintPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UIElement element && FindOwner(element) is RelativePanel owner)
            {
                owner._itemsHost?.InvalidateMeasure();
                owner.InvalidateMeasure();
            }
        }

        private static RelativePanel FindOwner(DependencyObject element)
        {
            DependencyObject current = element;
            while (current != null)
            {
                if (current is RelativePanel relativePanel)
                {
                    return relativePanel;
                }

                if (current is ItemsHost itemsHost)
                {
                    return itemsHost.Owner;
                }

                current = LogicalTreeHelper.GetParent(current) ?? GetVisualParent(current);
            }

            return null;
        }

        private static DependencyObject GetVisualParent(DependencyObject element)
        {
            try
            {
                return VisualTreeHelper.GetParent(element);
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }

        private void UpdateBorder()
        {
            if (_border == null)
            {
                return;
            }

            _border.BackgroundTransition = BackgroundTransition;
            _border.Background = Background;
            _border.BackgroundSizing = BackgroundSizing;
            _border.BorderBrush = BorderBrush;
            _border.BorderThickness = BorderThickness;
            _border.CornerRadius = CornerRadius;
            _border.Padding = Padding;
        }

        private readonly LayoutChromeDecorator _border;
        private readonly ItemsHost _itemsHost;

        private sealed class ItemsHost : Panel
        {
            public ItemsHost(RelativePanel owner)
            {
                Owner = owner;
            }

            public RelativePanel Owner { get; }

            protected override Size MeasureOverride(Size availableSize)
            {
                _graph = RelativePanelGraph.Create(Owner, InternalChildren);
                _graph.MeasureNodes(availableSize);
                return _graph.CalculateDesiredSize();
            }

            protected override Size ArrangeOverride(Size finalSize)
            {
                if (_graph == null)
                {
                    _graph = RelativePanelGraph.Create(Owner, InternalChildren);
                    _graph.MeasureNodes(finalSize);
                }

                _graph.ArrangeNodes(new Rect(finalSize));
                return finalSize;
            }

            protected override UIElementCollection CreateUIElementCollection(FrameworkElement logicalParent)
            {
                return new UIElementCollection(this, Owner);
            }

            private RelativePanelGraph _graph;
        }

        [Flags]
        private enum RelativePanelConstraints
        {
            None = 0x00000,
            LeftOf = 0x00001,
            Above = 0x00002,
            RightOf = 0x00004,
            Below = 0x00008,
            AlignHorizontalCenterWith = 0x00010,
            AlignVerticalCenterWith = 0x00020,
            AlignLeftWith = 0x00040,
            AlignTopWith = 0x00080,
            AlignRightWith = 0x00100,
            AlignBottomWith = 0x00200,
            AlignLeftWithPanel = 0x00400,
            AlignTopWithPanel = 0x00800,
            AlignRightWithPanel = 0x01000,
            AlignBottomWithPanel = 0x02000,
            AlignHorizontalCenterWithPanel = 0x04000,
            AlignVerticalCenterWithPanel = 0x08000
        }

        [Flags]
        private enum RelativePanelState
        {
            Unresolved = 0x00,
            Pending = 0x01,
            Measured = 0x02,
            ArrangedHorizontally = 0x04,
            ArrangedVertically = 0x08,
            Arranged = ArrangedHorizontally | ArrangedVertically
        }

        private sealed class RelativePanelGraph
        {
            public static RelativePanelGraph Create(RelativePanel owner, UIElementCollection children)
            {
                var graph = new RelativePanelGraph();

                foreach (UIElement child in children)
                {
                    if (child != null)
                    {
                        graph._nodes.Add(new RelativePanelNode(child));
                    }
                }

                graph.ResolveConstraints();
                return graph;
            }

            public void MeasureNodes(Size availableSize)
            {
                foreach (var node in _nodes)
                {
                    MeasureNode(node, availableSize);
                }

                _availableSizeForNodeResolution = availableSize;
            }

            public void ArrangeNodes(Rect finalRect)
            {
                var finalSize = finalRect.Size;

                if (!AreClose(_availableSizeForNodeResolution.Width, finalSize.Width))
                {
                    foreach (var node in _nodes)
                    {
                        node.SetArrangedHorizontally(false);
                    }

                    foreach (var node in _nodes)
                    {
                        ArrangeNodeHorizontally(node, finalSize);
                    }
                }

                if (!AreClose(_availableSizeForNodeResolution.Height, finalSize.Height))
                {
                    foreach (var node in _nodes)
                    {
                        node.SetArrangedVertically(false);
                    }

                    foreach (var node in _nodes)
                    {
                        ArrangeNodeVertically(node, finalSize);
                    }
                }

                _availableSizeForNodeResolution = finalSize;

                foreach (var node in _nodes)
                {
                    var layoutSlot = new Rect(
                        Math.Max(node.ArrangeRect.X + finalRect.X, 0),
                        Math.Max(node.ArrangeRect.Y + finalRect.Y, 0),
                        Math.Max(node.ArrangeRect.Width, 0),
                        Math.Max(node.ArrangeRect.Height, 0));

                    node.Element.Arrange(layoutSlot);
                }
            }

            public Size CalculateDesiredSize()
            {
                var maxDesiredSize = new Size();

                MarkHorizontalAndVerticalLeaves();

                foreach (var node in _nodes)
                {
                    if (node.IsHorizontalLeaf)
                    {
                        _minX = 0.0;
                        _maxX = 0.0;
                        _isMinCapped = false;
                        _isMaxCapped = false;

                        AccumulatePositiveDesiredWidth(node, 0.0);
                        maxDesiredSize.Width = Math.Max(maxDesiredSize.Width, _maxX - _minX);
                    }

                    if (node.IsVerticalLeaf)
                    {
                        _minY = 0.0;
                        _maxY = 0.0;
                        _isMinCapped = false;
                        _isMaxCapped = false;

                        AccumulatePositiveDesiredHeight(node, 0.0);
                        maxDesiredSize.Height = Math.Max(maxDesiredSize.Height, _maxY - _minY);
                    }
                }

                return maxDesiredSize;
            }

            private RelativePanelGraph()
            {
            }

            private void ResolveConstraints()
            {
                foreach (var node in _nodes)
                {
                    node.LeftOfNode = GetNodeByValue(GetLeftOf(node.Element));
                    node.AboveNode = GetNodeByValue(GetAbove(node.Element));
                    node.RightOfNode = GetNodeByValue(GetRightOf(node.Element));
                    node.BelowNode = GetNodeByValue(GetBelow(node.Element));
                    node.AlignHorizontalCenterWithNode = GetNodeByValue(GetAlignHorizontalCenterWith(node.Element));
                    node.AlignVerticalCenterWithNode = GetNodeByValue(GetAlignVerticalCenterWith(node.Element));
                    node.AlignLeftWithNode = GetNodeByValue(GetAlignLeftWith(node.Element));
                    node.AlignTopWithNode = GetNodeByValue(GetAlignTopWith(node.Element));
                    node.AlignRightWithNode = GetNodeByValue(GetAlignRightWith(node.Element));
                    node.AlignBottomWithNode = GetNodeByValue(GetAlignBottomWith(node.Element));

                    node.SetConstraint(RelativePanelConstraints.LeftOf, node.LeftOfNode != null);
                    node.SetConstraint(RelativePanelConstraints.Above, node.AboveNode != null);
                    node.SetConstraint(RelativePanelConstraints.RightOf, node.RightOfNode != null);
                    node.SetConstraint(RelativePanelConstraints.Below, node.BelowNode != null);
                    node.SetConstraint(RelativePanelConstraints.AlignHorizontalCenterWith, node.AlignHorizontalCenterWithNode != null);
                    node.SetConstraint(RelativePanelConstraints.AlignVerticalCenterWith, node.AlignVerticalCenterWithNode != null);
                    node.SetConstraint(RelativePanelConstraints.AlignLeftWith, node.AlignLeftWithNode != null);
                    node.SetConstraint(RelativePanelConstraints.AlignTopWith, node.AlignTopWithNode != null);
                    node.SetConstraint(RelativePanelConstraints.AlignRightWith, node.AlignRightWithNode != null);
                    node.SetConstraint(RelativePanelConstraints.AlignBottomWith, node.AlignBottomWithNode != null);
                    node.SetConstraint(RelativePanelConstraints.AlignLeftWithPanel, GetAlignLeftWithPanel(node.Element));
                    node.SetConstraint(RelativePanelConstraints.AlignTopWithPanel, GetAlignTopWithPanel(node.Element));
                    node.SetConstraint(RelativePanelConstraints.AlignRightWithPanel, GetAlignRightWithPanel(node.Element));
                    node.SetConstraint(RelativePanelConstraints.AlignBottomWithPanel, GetAlignBottomWithPanel(node.Element));
                    node.SetConstraint(RelativePanelConstraints.AlignHorizontalCenterWithPanel, GetAlignHorizontalCenterWithPanel(node.Element));
                    node.SetConstraint(RelativePanelConstraints.AlignVerticalCenterWithPanel, GetAlignVerticalCenterWithPanel(node.Element));
                }
            }

            private RelativePanelNode GetNodeByValue(object value)
            {
                if (value == null)
                {
                    return null;
                }

                if (value is string name)
                {
                    if (name.Length == 0)
                    {
                        return null;
                    }

                    foreach (var node in _nodes)
                    {
                        if (node.Element is FrameworkElement frameworkElement &&
                            string.Equals(frameworkElement.Name, name, StringComparison.Ordinal))
                        {
                            return node;
                        }
                    }

                    throw new InvalidOperationException($"RelativePanel target '{name}' was not found among this panel's children.");
                }

                if (value is UIElement target)
                {
                    foreach (var node in _nodes)
                    {
                        if (ReferenceEquals(node.Element, target))
                        {
                            return node;
                        }
                    }

                    throw new InvalidOperationException("RelativePanel target element was not found among this panel's children.");
                }

                throw new InvalidOperationException("RelativePanel constraints must reference a child element or child element name.");
            }

            private void CalculateMeasureRectHorizontally(RelativePanelNode node, Size availableSize, out double x, out double width)
            {
                var isHorizontallyCenteredFromLeft = false;
                var isHorizontallyCenteredFromRight = false;

                x = 0.0;
                width = availableSize.Width;

                if (!double.IsInfinity(availableSize.Width))
                {
                    if (!node.IsAlignLeftWithPanel)
                    {
                        if (node.IsAlignLeftWith)
                        {
                            var restrictedHorizontalSpace = node.AlignLeftWithNode.ArrangeRect.X;

                            x = restrictedHorizontalSpace;
                            width -= restrictedHorizontalSpace;
                        }
                        else if (node.IsAlignHorizontalCenterWith)
                        {
                            isHorizontallyCenteredFromLeft = true;
                        }
                        else if (node.IsRightOf)
                        {
                            var restrictedHorizontalSpace = node.RightOfNode.ArrangeRect.X + node.RightOfNode.ArrangeRect.Width;

                            x = restrictedHorizontalSpace;
                            width -= restrictedHorizontalSpace;
                        }
                    }

                    if (!node.IsAlignRightWithPanel)
                    {
                        if (node.IsAlignRightWith)
                        {
                            width -= availableSize.Width - (node.AlignRightWithNode.ArrangeRect.X + node.AlignRightWithNode.ArrangeRect.Width);
                        }
                        else if (node.IsAlignHorizontalCenterWith)
                        {
                            isHorizontallyCenteredFromRight = true;
                        }
                        else if (node.IsLeftOf)
                        {
                            width -= availableSize.Width - node.LeftOfNode.ArrangeRect.X;
                        }
                    }

                    if (isHorizontallyCenteredFromLeft && isHorizontallyCenteredFromRight)
                    {
                        var centerOfNeighbor = node.AlignHorizontalCenterWithNode.ArrangeRect.X +
                            (node.AlignHorizontalCenterWithNode.ArrangeRect.Width / 2.0);
                        width = Math.Min(centerOfNeighbor, availableSize.Width - centerOfNeighbor) * 2.0;
                        x = centerOfNeighbor - (width / 2.0);
                    }
                }
            }

            private void CalculateMeasureRectVertically(RelativePanelNode node, Size availableSize, out double y, out double height)
            {
                var isVerticallyCenteredFromTop = false;
                var isVerticallyCenteredFromBottom = false;

                y = 0.0;
                height = availableSize.Height;

                if (!double.IsInfinity(availableSize.Height))
                {
                    if (!node.IsAlignTopWithPanel)
                    {
                        if (node.IsAlignTopWith)
                        {
                            var restrictedVerticalSpace = node.AlignTopWithNode.ArrangeRect.Y;

                            y = restrictedVerticalSpace;
                            height -= restrictedVerticalSpace;
                        }
                        else if (node.IsAlignVerticalCenterWith)
                        {
                            isVerticallyCenteredFromTop = true;
                        }
                        else if (node.IsBelow)
                        {
                            var restrictedVerticalSpace = node.BelowNode.ArrangeRect.Y + node.BelowNode.ArrangeRect.Height;

                            y = restrictedVerticalSpace;
                            height -= restrictedVerticalSpace;
                        }
                    }

                    if (!node.IsAlignBottomWithPanel)
                    {
                        if (node.IsAlignBottomWith)
                        {
                            height -= availableSize.Height - (node.AlignBottomWithNode.ArrangeRect.Y + node.AlignBottomWithNode.ArrangeRect.Height);
                        }
                        else if (node.IsAlignVerticalCenterWith)
                        {
                            isVerticallyCenteredFromBottom = true;
                        }
                        else if (node.IsAbove)
                        {
                            height -= availableSize.Height - node.AboveNode.ArrangeRect.Y;
                        }
                    }

                    if (isVerticallyCenteredFromTop && isVerticallyCenteredFromBottom)
                    {
                        var centerOfNeighbor = node.AlignVerticalCenterWithNode.ArrangeRect.Y +
                            (node.AlignVerticalCenterWithNode.ArrangeRect.Height / 2.0);
                        height = Math.Min(centerOfNeighbor, availableSize.Height - centerOfNeighbor) * 2.0;
                        y = centerOfNeighbor - (height / 2.0);
                    }
                }
            }

            private static void CalculateArrangeRectHorizontally(RelativePanelNode node, out double x, out double width)
            {
                var measureRect = node.MeasureRect;
                var desiredWidth = Math.Min(measureRect.Width, node.DesiredSize.Width);

                x = measureRect.X;
                width = desiredWidth;

                if (node.IsLeftAnchored)
                {
                    if (node.IsRightAnchored)
                    {
                        x = measureRect.X;
                        width = measureRect.Width;
                    }
                    else
                    {
                        x = measureRect.X;
                        width = desiredWidth;
                    }
                }
                else if (node.IsRightAnchored)
                {
                    x = measureRect.X + measureRect.Width - desiredWidth;
                    width = desiredWidth;
                }
                else if (node.IsHorizontalCenterAnchored)
                {
                    x = measureRect.X + (measureRect.Width / 2.0) - (desiredWidth / 2.0);
                    width = desiredWidth;
                }
            }

            private static void CalculateArrangeRectVertically(RelativePanelNode node, out double y, out double height)
            {
                var measureRect = node.MeasureRect;
                var desiredHeight = Math.Min(measureRect.Height, node.DesiredSize.Height);

                y = measureRect.Y;
                height = desiredHeight;

                if (node.IsTopAnchored)
                {
                    if (node.IsBottomAnchored)
                    {
                        y = measureRect.Y;
                        height = measureRect.Height;
                    }
                    else
                    {
                        y = measureRect.Y;
                        height = desiredHeight;
                    }
                }
                else if (node.IsBottomAnchored)
                {
                    y = measureRect.Y + measureRect.Height - desiredHeight;
                    height = desiredHeight;
                }
                else if (node.IsVerticalCenterAnchored)
                {
                    y = measureRect.Y + (measureRect.Height / 2.0) - (desiredHeight / 2.0);
                    height = desiredHeight;
                }
            }

            private void MarkHorizontalAndVerticalLeaves()
            {
                foreach (var node in _nodes)
                {
                    node.IsHorizontalLeaf = true;
                    node.IsVerticalLeaf = true;
                }

                foreach (var node in _nodes)
                {
                    node.UnmarkNeighborsAsHorizontalOrVerticalLeaves();
                }
            }

            private void AccumulatePositiveDesiredWidth(RelativePanelNode node, double x)
            {
                var initialX = x;
                var isHorizontallyCenteredFromLeft = false;
                var isHorizontallyCenteredFromRight = false;

                x += node.DesiredSize.Width;
                _maxX = Math.Max(_maxX, x);

                if (node.IsAlignLeftWithPanel)
                {
                    if (!_isMaxCapped)
                    {
                        _maxX = x;
                        _isMaxCapped = true;
                    }
                }
                else if (node.IsAlignLeftWith)
                {
                    if (!ReferenceEquals(node.AlignLeftWithNode, node.AlignRightWithNode))
                    {
                        AccumulateNegativeDesiredWidth(node.AlignLeftWithNode, x);
                    }
                }
                else if (node.IsAlignHorizontalCenterWith)
                {
                    isHorizontallyCenteredFromLeft = true;
                }
                else if (node.IsRightOf)
                {
                    AccumulatePositiveDesiredWidth(node.RightOfNode, x);
                }

                if (node.IsAlignRightWithPanel)
                {
                    if (_isMinCapped)
                    {
                        _minX = Math.Min(_minX, initialX);
                    }
                    else
                    {
                        _minX = initialX;
                        _isMinCapped = true;
                    }
                }
                else if (node.IsAlignRightWith)
                {
                    AccumulatePositiveDesiredWidth(node.AlignRightWithNode, initialX);
                }
                else if (node.IsAlignHorizontalCenterWith)
                {
                    isHorizontallyCenteredFromRight = true;
                }
                else if (node.IsLeftOf)
                {
                    AccumulateNegativeDesiredWidth(node.LeftOfNode, initialX);
                }

                if (isHorizontallyCenteredFromLeft && isHorizontallyCenteredFromRight)
                {
                    var centerX = x - (node.DesiredSize.Width / 2.0);
                    var edgeX = centerX - (node.AlignHorizontalCenterWithNode.DesiredSize.Width / 2.0);
                    _minX = Math.Min(_minX, edgeX);
                    AccumulatePositiveDesiredWidth(node.AlignHorizontalCenterWithNode, edgeX);
                }
                else if (node.IsHorizontalCenterAnchored)
                {
                    var centerX = x - (node.DesiredSize.Width / 2.0);
                    var upper = _maxX - centerX;
                    var lower = centerX - _minX;
                    _maxX = Math.Max(upper, lower) * 2.0;
                    _minX = 0.0;
                }
            }

            private void AccumulateNegativeDesiredWidth(RelativePanelNode node, double x)
            {
                var initialX = x;
                var isHorizontallyCenteredFromLeft = false;
                var isHorizontallyCenteredFromRight = false;

                x -= node.DesiredSize.Width;
                _minX = Math.Min(_minX, x);

                if (node.IsAlignRightWithPanel)
                {
                    if (!_isMinCapped)
                    {
                        _minX = x;
                        _isMinCapped = true;
                    }
                }
                else if (node.IsAlignRightWith)
                {
                    if (!ReferenceEquals(node.AlignRightWithNode, node.AlignLeftWithNode))
                    {
                        AccumulatePositiveDesiredWidth(node.AlignRightWithNode, x);
                    }
                }
                else if (node.IsAlignHorizontalCenterWith)
                {
                    isHorizontallyCenteredFromRight = true;
                }
                else if (node.IsLeftOf)
                {
                    AccumulateNegativeDesiredWidth(node.LeftOfNode, x);
                }

                if (node.IsAlignLeftWithPanel)
                {
                    if (_isMaxCapped)
                    {
                        _maxX = Math.Max(_maxX, initialX);
                    }
                    else
                    {
                        _maxX = initialX;
                        _isMaxCapped = true;
                    }
                }
                else if (node.IsAlignLeftWith)
                {
                    AccumulateNegativeDesiredWidth(node.AlignLeftWithNode, initialX);
                }
                else if (node.IsAlignHorizontalCenterWith)
                {
                    isHorizontallyCenteredFromLeft = true;
                }
                else if (node.IsRightOf)
                {
                    AccumulatePositiveDesiredWidth(node.RightOfNode, initialX);
                }

                if (isHorizontallyCenteredFromLeft && isHorizontallyCenteredFromRight)
                {
                    var centerX = x + (node.DesiredSize.Width / 2.0);
                    var edgeX = centerX + (node.AlignHorizontalCenterWithNode.DesiredSize.Width / 2.0);
                    _maxX = Math.Max(_maxX, edgeX);
                    AccumulateNegativeDesiredWidth(node.AlignHorizontalCenterWithNode, edgeX);
                }
                else if (node.IsHorizontalCenterAnchored)
                {
                    var centerX = x + (node.DesiredSize.Width / 2.0);
                    var upper = _maxX - centerX;
                    var lower = centerX - _minX;
                    _maxX = Math.Max(upper, lower) * 2.0;
                    _minX = 0.0;
                }
            }

            private void AccumulatePositiveDesiredHeight(RelativePanelNode node, double y)
            {
                var initialY = y;
                var isVerticallyCenteredFromTop = false;
                var isVerticallyCenteredFromBottom = false;

                y += node.DesiredSize.Height;
                _maxY = Math.Max(_maxY, y);

                if (node.IsAlignTopWithPanel)
                {
                    if (!_isMaxCapped)
                    {
                        _maxY = y;
                        _isMaxCapped = true;
                    }
                }
                else if (node.IsAlignTopWith)
                {
                    if (!ReferenceEquals(node.AlignTopWithNode, node.AlignBottomWithNode))
                    {
                        AccumulateNegativeDesiredHeight(node.AlignTopWithNode, y);
                    }
                }
                else if (node.IsAlignVerticalCenterWith)
                {
                    isVerticallyCenteredFromTop = true;
                }
                else if (node.IsBelow)
                {
                    AccumulatePositiveDesiredHeight(node.BelowNode, y);
                }

                if (node.IsAlignBottomWithPanel)
                {
                    if (_isMinCapped)
                    {
                        _minY = Math.Min(_minY, initialY);
                    }
                    else
                    {
                        _minY = initialY;
                        _isMinCapped = true;
                    }
                }
                else if (node.IsAlignBottomWith)
                {
                    AccumulatePositiveDesiredHeight(node.AlignBottomWithNode, initialY);
                }
                else if (node.IsAlignVerticalCenterWith)
                {
                    isVerticallyCenteredFromBottom = true;
                }
                else if (node.IsAbove)
                {
                    AccumulateNegativeDesiredHeight(node.AboveNode, initialY);
                }

                if (isVerticallyCenteredFromTop && isVerticallyCenteredFromBottom)
                {
                    var centerY = y - (node.DesiredSize.Height / 2.0);
                    var edgeY = centerY - (node.AlignVerticalCenterWithNode.DesiredSize.Height / 2.0);
                    _minY = Math.Min(_minY, edgeY);
                    AccumulatePositiveDesiredHeight(node.AlignVerticalCenterWithNode, edgeY);
                }
                else if (node.IsVerticalCenterAnchored)
                {
                    var centerY = y - (node.DesiredSize.Height / 2.0);
                    var upper = _maxY - centerY;
                    var lower = centerY - _minY;
                    _maxY = Math.Max(upper, lower) * 2.0;
                    _minY = 0.0;
                }
            }

            private void AccumulateNegativeDesiredHeight(RelativePanelNode node, double y)
            {
                var initialY = y;
                var isVerticallyCenteredFromTop = false;
                var isVerticallyCenteredFromBottom = false;

                y -= node.DesiredSize.Height;
                _minY = Math.Min(_minY, y);

                if (node.IsAlignBottomWithPanel)
                {
                    if (!_isMinCapped)
                    {
                        _minY = y;
                        _isMinCapped = true;
                    }
                }
                else if (node.IsAlignBottomWith)
                {
                    if (!ReferenceEquals(node.AlignBottomWithNode, node.AlignTopWithNode))
                    {
                        AccumulatePositiveDesiredHeight(node.AlignBottomWithNode, y);
                    }
                }
                else if (node.IsAlignVerticalCenterWith)
                {
                    isVerticallyCenteredFromBottom = true;
                }
                else if (node.IsAbove)
                {
                    AccumulateNegativeDesiredHeight(node.AboveNode, y);
                }

                if (node.IsAlignTopWithPanel)
                {
                    if (_isMaxCapped)
                    {
                        _maxY = Math.Max(_maxY, initialY);
                    }
                    else
                    {
                        _maxY = initialY;
                        _isMaxCapped = true;
                    }
                }
                else if (node.IsAlignTopWith)
                {
                    AccumulateNegativeDesiredHeight(node.AlignTopWithNode, initialY);
                }
                else if (node.IsAlignVerticalCenterWith)
                {
                    isVerticallyCenteredFromTop = true;
                }
                else if (node.IsBelow)
                {
                    AccumulatePositiveDesiredHeight(node.BelowNode, initialY);
                }

                if (isVerticallyCenteredFromTop && isVerticallyCenteredFromBottom)
                {
                    var centerY = y + (node.DesiredSize.Height / 2.0);
                    var edgeY = centerY + (node.AlignVerticalCenterWithNode.DesiredSize.Height / 2.0);
                    _maxY = Math.Max(_maxY, edgeY);
                    AccumulateNegativeDesiredHeight(node.AlignVerticalCenterWithNode, edgeY);
                }
                else if (node.IsVerticalCenterAnchored)
                {
                    var centerY = y + (node.DesiredSize.Height / 2.0);
                    var upper = _maxY - centerY;
                    var lower = centerY - _minY;
                    _maxY = Math.Max(upper, lower) * 2.0;
                    _minY = 0.0;
                }
            }

            private void MeasureNode(RelativePanelNode node, Size availableSize)
            {
                if (node == null)
                {
                    return;
                }

                if (node.IsPending)
                {
                    throw new InvalidOperationException("RelativePanel has a circular dependency.");
                }

                if (node.IsUnresolved)
                {
                    node.SetPending(true);

                    MeasureNode(node.LeftOfNode, availableSize);
                    MeasureNode(node.AboveNode, availableSize);
                    MeasureNode(node.RightOfNode, availableSize);
                    MeasureNode(node.BelowNode, availableSize);
                    MeasureNode(node.AlignLeftWithNode, availableSize);
                    MeasureNode(node.AlignTopWithNode, availableSize);
                    MeasureNode(node.AlignRightWithNode, availableSize);
                    MeasureNode(node.AlignBottomWithNode, availableSize);
                    MeasureNode(node.AlignHorizontalCenterWithNode, availableSize);
                    MeasureNode(node.AlignVerticalCenterWithNode, availableSize);

                    node.SetPending(false);

                    CalculateMeasureRectHorizontally(node, availableSize, out var x, out var width);
                    CalculateMeasureRectVertically(node, availableSize, out var y, out var height);
                    node.MeasureRect = new Rect(x, y, width, height);

                    node.Element.Measure(new Size(Math.Max(width, 0), Math.Max(height, 0)));
                    node.SetMeasured(true);

                    if (!double.IsInfinity(availableSize.Width))
                    {
                        CalculateArrangeRectHorizontally(node, out var arrangeX, out var arrangeWidth);
                        node.ArrangeRect = new Rect(arrangeX, node.ArrangeRect.Y, arrangeWidth, node.ArrangeRect.Height);
                        node.SetArrangedHorizontally(true);
                    }

                    if (!double.IsInfinity(availableSize.Height))
                    {
                        CalculateArrangeRectVertically(node, out var arrangeY, out var arrangeHeight);
                        node.ArrangeRect = new Rect(node.ArrangeRect.X, arrangeY, node.ArrangeRect.Width, arrangeHeight);
                        node.SetArrangedVertically(true);
                    }
                }
            }

            private void ArrangeNodeHorizontally(RelativePanelNode node, Size finalSize)
            {
                if (node == null)
                {
                    return;
                }

                if (!node.IsArrangedHorizontally)
                {
                    ArrangeNodeHorizontally(node.LeftOfNode, finalSize);
                    ArrangeNodeHorizontally(node.AboveNode, finalSize);
                    ArrangeNodeHorizontally(node.RightOfNode, finalSize);
                    ArrangeNodeHorizontally(node.BelowNode, finalSize);
                    ArrangeNodeHorizontally(node.AlignLeftWithNode, finalSize);
                    ArrangeNodeHorizontally(node.AlignTopWithNode, finalSize);
                    ArrangeNodeHorizontally(node.AlignRightWithNode, finalSize);
                    ArrangeNodeHorizontally(node.AlignBottomWithNode, finalSize);
                    ArrangeNodeHorizontally(node.AlignHorizontalCenterWithNode, finalSize);
                    ArrangeNodeHorizontally(node.AlignVerticalCenterWithNode, finalSize);

                    CalculateMeasureRectHorizontally(node, finalSize, out var x, out var width);
                    node.MeasureRect = new Rect(x, node.MeasureRect.Y, width, node.MeasureRect.Height);
                    CalculateArrangeRectHorizontally(node, out var arrangeX, out var arrangeWidth);
                    node.ArrangeRect = new Rect(arrangeX, node.ArrangeRect.Y, arrangeWidth, node.ArrangeRect.Height);

                    node.SetArrangedHorizontally(true);
                }
            }

            private void ArrangeNodeVertically(RelativePanelNode node, Size finalSize)
            {
                if (node == null)
                {
                    return;
                }

                if (!node.IsArrangedVertically)
                {
                    ArrangeNodeVertically(node.LeftOfNode, finalSize);
                    ArrangeNodeVertically(node.AboveNode, finalSize);
                    ArrangeNodeVertically(node.RightOfNode, finalSize);
                    ArrangeNodeVertically(node.BelowNode, finalSize);
                    ArrangeNodeVertically(node.AlignLeftWithNode, finalSize);
                    ArrangeNodeVertically(node.AlignTopWithNode, finalSize);
                    ArrangeNodeVertically(node.AlignRightWithNode, finalSize);
                    ArrangeNodeVertically(node.AlignBottomWithNode, finalSize);
                    ArrangeNodeVertically(node.AlignHorizontalCenterWithNode, finalSize);
                    ArrangeNodeVertically(node.AlignVerticalCenterWithNode, finalSize);

                    CalculateMeasureRectVertically(node, finalSize, out var y, out var height);
                    node.MeasureRect = new Rect(node.MeasureRect.X, y, node.MeasureRect.Width, height);
                    CalculateArrangeRectVertically(node, out var arrangeY, out var arrangeHeight);
                    node.ArrangeRect = new Rect(node.ArrangeRect.X, arrangeY, node.ArrangeRect.Width, arrangeHeight);

                    node.SetArrangedVertically(true);
                }
            }

            private static bool AreClose(double first, double second)
            {
                if (double.IsInfinity(first) || double.IsInfinity(second))
                {
                    return double.IsInfinity(first) && double.IsInfinity(second) &&
                        Math.Sign(first).Equals(Math.Sign(second));
                }

                return Math.Abs(first - second) < 0.000001;
            }

            private readonly List<RelativePanelNode> _nodes = new List<RelativePanelNode>();
            private Size _availableSizeForNodeResolution;
            private double _minX;
            private double _maxX;
            private double _minY;
            private double _maxY;
            private bool _isMinCapped;
            private bool _isMaxCapped;
        }

        private sealed class RelativePanelNode
        {
            public RelativePanelNode(UIElement element)
            {
                Element = element;
            }

            public UIElement Element { get; }

            public Size DesiredSize => Element.DesiredSize;

            public Rect MeasureRect { get; set; }

            public Rect ArrangeRect { get; set; }

            public RelativePanelNode LeftOfNode { get; set; }

            public RelativePanelNode AboveNode { get; set; }

            public RelativePanelNode RightOfNode { get; set; }

            public RelativePanelNode BelowNode { get; set; }

            public RelativePanelNode AlignHorizontalCenterWithNode { get; set; }

            public RelativePanelNode AlignVerticalCenterWithNode { get; set; }

            public RelativePanelNode AlignLeftWithNode { get; set; }

            public RelativePanelNode AlignTopWithNode { get; set; }

            public RelativePanelNode AlignRightWithNode { get; set; }

            public RelativePanelNode AlignBottomWithNode { get; set; }

            public bool IsHorizontalLeaf { get; set; } = true;

            public bool IsVerticalLeaf { get; set; } = true;

            public bool IsPending => (_state & RelativePanelState.Pending) == RelativePanelState.Pending;

            public bool IsUnresolved => _state == RelativePanelState.Unresolved;

            public bool IsArrangedHorizontally => (_state & RelativePanelState.ArrangedHorizontally) == RelativePanelState.ArrangedHorizontally;

            public bool IsArrangedVertically => (_state & RelativePanelState.ArrangedVertically) == RelativePanelState.ArrangedVertically;

            public bool IsLeftOf => HasConstraint(RelativePanelConstraints.LeftOf);

            public bool IsAbove => HasConstraint(RelativePanelConstraints.Above);

            public bool IsRightOf => HasConstraint(RelativePanelConstraints.RightOf);

            public bool IsBelow => HasConstraint(RelativePanelConstraints.Below);

            public bool IsAlignHorizontalCenterWith => HasConstraint(RelativePanelConstraints.AlignHorizontalCenterWith);

            public bool IsAlignVerticalCenterWith => HasConstraint(RelativePanelConstraints.AlignVerticalCenterWith);

            public bool IsAlignLeftWith => HasConstraint(RelativePanelConstraints.AlignLeftWith);

            public bool IsAlignTopWith => HasConstraint(RelativePanelConstraints.AlignTopWith);

            public bool IsAlignRightWith => HasConstraint(RelativePanelConstraints.AlignRightWith);

            public bool IsAlignBottomWith => HasConstraint(RelativePanelConstraints.AlignBottomWith);

            public bool IsAlignLeftWithPanel => HasConstraint(RelativePanelConstraints.AlignLeftWithPanel);

            public bool IsAlignTopWithPanel => HasConstraint(RelativePanelConstraints.AlignTopWithPanel);

            public bool IsAlignRightWithPanel => HasConstraint(RelativePanelConstraints.AlignRightWithPanel);

            public bool IsAlignBottomWithPanel => HasConstraint(RelativePanelConstraints.AlignBottomWithPanel);

            public bool IsAlignHorizontalCenterWithPanel => HasConstraint(RelativePanelConstraints.AlignHorizontalCenterWithPanel);

            public bool IsAlignVerticalCenterWithPanel => HasConstraint(RelativePanelConstraints.AlignVerticalCenterWithPanel);

            public bool IsLeftAnchored => IsAlignLeftWithPanel || IsAlignLeftWith || (IsRightOf && !IsAlignHorizontalCenterWith);

            public bool IsTopAnchored => IsAlignTopWithPanel || IsAlignTopWith || (IsBelow && !IsAlignVerticalCenterWith);

            public bool IsRightAnchored => IsAlignRightWithPanel || IsAlignRightWith || (IsLeftOf && !IsAlignHorizontalCenterWith);

            public bool IsBottomAnchored => IsAlignBottomWithPanel || IsAlignBottomWith || (IsAbove && !IsAlignVerticalCenterWith);

            public bool IsHorizontalCenterAnchored =>
                (IsAlignHorizontalCenterWithPanel &&
                    !IsAlignLeftWithPanel &&
                    !IsAlignRightWithPanel &&
                    !IsAlignLeftWith &&
                    !IsAlignRightWith &&
                    !IsLeftOf &&
                    !IsRightOf) ||
                (IsAlignHorizontalCenterWith &&
                    !IsAlignLeftWithPanel &&
                    !IsAlignRightWithPanel &&
                    !IsAlignLeftWith &&
                    !IsAlignRightWith);

            public bool IsVerticalCenterAnchored =>
                (IsAlignVerticalCenterWithPanel &&
                    !IsAlignTopWithPanel &&
                    !IsAlignBottomWithPanel &&
                    !IsAlignTopWith &&
                    !IsAlignBottomWith &&
                    !IsAbove &&
                    !IsBelow) ||
                (IsAlignVerticalCenterWith &&
                    !IsAlignTopWithPanel &&
                    !IsAlignBottomWithPanel &&
                    !IsAlignTopWith &&
                    !IsAlignBottomWith);

            public void SetConstraint(RelativePanelConstraints constraint, bool value)
            {
                if (value)
                {
                    _constraints |= constraint;
                }
                else
                {
                    _constraints &= ~constraint;
                }
            }

            public void SetPending(bool value)
            {
                SetState(RelativePanelState.Pending, value);
            }

            public void SetMeasured(bool value)
            {
                SetState(RelativePanelState.Measured, value);
            }

            public void SetArrangedHorizontally(bool value)
            {
                SetState(RelativePanelState.ArrangedHorizontally, value);
            }

            public void SetArrangedVertically(bool value)
            {
                SetState(RelativePanelState.ArrangedVertically, value);
            }

            public void UnmarkNeighborsAsHorizontalOrVerticalLeaves()
            {
                var isHorizontallyCenteredFromLeft = false;
                var isHorizontallyCenteredFromRight = false;
                var isVerticallyCenteredFromTop = false;
                var isVerticallyCenteredFromBottom = false;

                if (!IsAlignLeftWithPanel)
                {
                    if (IsAlignLeftWith)
                    {
                        AlignLeftWithNode.IsHorizontalLeaf = false;
                    }
                    else if (IsAlignHorizontalCenterWith)
                    {
                        isHorizontallyCenteredFromLeft = true;
                    }
                    else if (IsRightOf)
                    {
                        RightOfNode.IsHorizontalLeaf = false;
                    }
                }

                if (!IsAlignTopWithPanel)
                {
                    if (IsAlignTopWith)
                    {
                        AlignTopWithNode.IsVerticalLeaf = false;
                    }
                    else if (IsAlignVerticalCenterWith)
                    {
                        isVerticallyCenteredFromTop = true;
                    }
                    else if (IsBelow)
                    {
                        BelowNode.IsVerticalLeaf = false;
                    }
                }

                if (!IsAlignRightWithPanel)
                {
                    if (IsAlignRightWith)
                    {
                        AlignRightWithNode.IsHorizontalLeaf = false;
                    }
                    else if (IsAlignHorizontalCenterWith)
                    {
                        isHorizontallyCenteredFromRight = true;
                    }
                    else if (IsLeftOf)
                    {
                        LeftOfNode.IsHorizontalLeaf = false;
                    }
                }

                if (!IsAlignBottomWithPanel)
                {
                    if (IsAlignBottomWith)
                    {
                        AlignBottomWithNode.IsVerticalLeaf = false;
                    }
                    else if (IsAlignVerticalCenterWith)
                    {
                        isVerticallyCenteredFromBottom = true;
                    }
                    else if (IsAbove)
                    {
                        AboveNode.IsVerticalLeaf = false;
                    }
                }

                if (isHorizontallyCenteredFromLeft && isHorizontallyCenteredFromRight)
                {
                    AlignHorizontalCenterWithNode.IsHorizontalLeaf = false;
                }

                if (isVerticallyCenteredFromTop && isVerticallyCenteredFromBottom)
                {
                    AlignVerticalCenterWithNode.IsVerticalLeaf = false;
                }
            }

            private bool HasConstraint(RelativePanelConstraints constraint)
            {
                return (_constraints & constraint) == constraint;
            }

            private void SetState(RelativePanelState state, bool value)
            {
                if (value)
                {
                    _state |= state;
                }
                else
                {
                    _state &= ~state;
                }
            }

            private RelativePanelState _state = RelativePanelState.Unresolved;
            private RelativePanelConstraints _constraints = RelativePanelConstraints.None;
        }
    }
}
