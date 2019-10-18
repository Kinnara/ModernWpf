// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace ModernWpf.Controls
{
    public class LayoutPanel : Panel
    {
        public LayoutPanel()
        {
        }

        #region Layout

        public static readonly DependencyProperty LayoutProperty =
            DependencyProperty.Register(
                nameof(Layout),
                typeof(Layout),
                typeof(LayoutPanel),
                new FrameworkPropertyMetadata(OnLayoutChanged));

        public Layout Layout
        {
            get => m_layout;
            set => SetValue(LayoutProperty, value);
        }

        private static void OnLayoutChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((LayoutPanel)d).OnLayoutChanged((Layout)e.OldValue, (Layout)e.NewValue);
        }

        #endregion

        #region Padding

        public static readonly DependencyProperty PaddingProperty =
            DependencyProperty.Register(
                nameof(Padding),
                typeof(Thickness),
                typeof(LayoutPanel),
                new FrameworkPropertyMetadata(
                    new Thickness(0, 0, 0, 0),
                    FrameworkPropertyMetadataOptions.AffectsMeasure));

        public Thickness Padding
        {
            get => (Thickness)GetValue(PaddingProperty);
            set => SetValue(PaddingProperty, value);
        }

        #endregion

        internal object LayoutState { get; set; }

        protected override Size MeasureOverride(Size availableSize)
        {
            Size desiredSize;

            var padding = Padding;
            var effectiveHorizontalPadding = padding.Left + padding.Right;
            var effectiveVerticalPadding = padding.Top + padding.Bottom;

            var adjustedSize = availableSize;
            adjustedSize.Width -= effectiveHorizontalPadding;
            adjustedSize.Height -= effectiveVerticalPadding;

            adjustedSize.Width = Math.Max(0.0, adjustedSize.Width);
            adjustedSize.Height = Math.Max(0.0, adjustedSize.Height);

            if (Layout is Layout layout)
            {
                var layoutDesiredSize = layout.Measure(m_layoutContext, adjustedSize);
                layoutDesiredSize.Width += effectiveHorizontalPadding;
                layoutDesiredSize.Height += effectiveVerticalPadding;
                desiredSize = layoutDesiredSize;
            }
            else
            {
                Size desiredSizeUnpadded = default;
                foreach (UIElement child in Children)
                {
                    child.Measure(adjustedSize);
                    var childDesiredSize = child.DesiredSize;
                    desiredSizeUnpadded.Width = Math.Max(desiredSizeUnpadded.Width, childDesiredSize.Width);
                    desiredSizeUnpadded.Height = Math.Max(desiredSizeUnpadded.Height, childDesiredSize.Height);
                }
                desiredSize = desiredSizeUnpadded;
                desiredSize.Width += effectiveHorizontalPadding;
                desiredSize.Height += effectiveVerticalPadding;
            }
            return desiredSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            Size result = finalSize;

            var padding = Padding;

            var effectiveHorizontalPadding = padding.Left + padding.Right;
            var effectiveVerticalPadding = padding.Top + padding.Bottom;
            var leftAdjustment = padding.Left;
            var topAdjustment = padding.Top;

            var adjustedSize = finalSize;
            adjustedSize.Width -= effectiveHorizontalPadding;
            adjustedSize.Height -= effectiveVerticalPadding;

            adjustedSize.Width = Math.Max(0.0, adjustedSize.Width);
            adjustedSize.Height = Math.Max(0.0, adjustedSize.Height);

            if (Layout is Layout layout)
            {
                var layoutSize = layout.Arrange(m_layoutContext, adjustedSize);
                layoutSize.Width += effectiveHorizontalPadding;
                layoutSize.Height += effectiveVerticalPadding;

                if (leftAdjustment != 0 || topAdjustment != 0)
                {
                    foreach (UIElement child in Children)
                    {
                        if (child is FrameworkElement childAsFe)
                        {
                            var layoutSlot = LayoutInformation.GetLayoutSlot(childAsFe);
                            layoutSlot.X += leftAdjustment;
                            layoutSlot.Y += topAdjustment;
                            childAsFe.Arrange(layoutSlot);
                        }
                    }
                }

                result = layoutSize;
            }
            else
            {
                Rect arrangeRect = new Rect(leftAdjustment, topAdjustment, adjustedSize.Width, adjustedSize.Height);
                foreach (UIElement child in Children)
                {
                    child.Arrange(arrangeRect);
                }
            }

            return result;
        }

        private LayoutContext m_layoutContext = null;

        private Layout m_layout;

        private void OnLayoutChanged(Layout oldValue, Layout newValue)
        {
            if (m_layoutContext == null)
            {
                m_layoutContext = new LayoutPanelLayoutContext(this);
            }

            if (oldValue != null)
            {
                oldValue.UninitializeForContext(m_layoutContext);
                oldValue.MeasureInvalidated -= InvalidateMeasureForLayout;
                oldValue.ArrangeInvalidated -= InvalidateArrangeForLayout;
            }

            m_layout = newValue;

            if (newValue != null)
            {
                newValue.InitializeForContext(m_layoutContext);
                newValue.MeasureInvalidated += InvalidateMeasureForLayout;
                newValue.ArrangeInvalidated += InvalidateArrangeForLayout;
            }

            InvalidateMeasure();
        }

        private void InvalidateMeasureForLayout(Layout sender, object args)
        {
            InvalidateMeasure();
        }

        private void InvalidateArrangeForLayout(Layout sender, object args)
        {
            InvalidateArrange();
        }
    }
}
