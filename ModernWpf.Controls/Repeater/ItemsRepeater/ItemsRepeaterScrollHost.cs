// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;

namespace ModernWpf.Controls
{
    // TODO: move to framework level element tracking.
    [ContentProperty(nameof(ScrollViewer))]
    public class ItemsRepeaterScrollHost : Panel, IRepeaterScrollingSurface
    {
        public ItemsRepeaterScrollHost()
        {
            m_pendingBringIntoView = new BringIntoViewState(this);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            Size desiredSize = default;
            if (ScrollViewer is ScrollViewer scrollViewer)
            {
                scrollViewer.Measure(availableSize);
                desiredSize = scrollViewer.DesiredSize;
            }

            return desiredSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            Size result = finalSize;
            if (ScrollViewer is ScrollViewer scrollViewer)
            {
                var shouldApplyPendingChangeView = scrollViewer != null && HasPendingBringIntoView && !m_pendingBringIntoView.ChangeViewCalled;

                Rect anchorElementRelativeBounds = default;
                var anchorElement =
                    // BringIntoView takes precedence over tracking.
                    shouldApplyPendingChangeView ?
                    null :
                    // Pick the best candidate depending on HorizontalAnchorRatio and VerticalAnchorRatio.
                    // The best candidate is the element that's the closest to the edge of interest.
                    GetAnchorElement(ref anchorElementRelativeBounds);

                scrollViewer.Arrange(new Rect(0, 0, finalSize.Width, finalSize.Height));

                m_pendingViewportShift = 0.0;

                if (shouldApplyPendingChangeView)
                {
                    ApplyPendingChangeView(scrollViewer);
                }
                else if (anchorElement != null)
                {
                    // The anchor element might have changed its position relative to us.
                    // If that's the case, we should shift the viewport to follow it as much as possible.
                    m_pendingViewportShift = TrackElement(anchorElement, anchorElementRelativeBounds, scrollViewer);
                }
                else if (scrollViewer == null)
                {
                    m_pendingBringIntoView.Reset();
                }

                m_candidates.Clear();
                m_isAnchorElementDirty = true;

                PostArrange?.Invoke(this);
            }

            return result;
        }

        public double HorizontalAnchorRatio
        {
            get => m_horizontalEdge;
            set => m_horizontalEdge = value;
        }

        public double VerticalAnchorRatio
        {
            get => m_verticalEdge;
            set => m_verticalEdge = value;
        }

        public UIElement CurrentAnchor
        {
            get
            {
                var empty = Rect.Empty;
                return GetAnchorElement(ref empty);
            }
        }

        public ScrollViewer ScrollViewer
        {
            get
            {
                ScrollViewer value = null;
                var children = Children;
                if (children.Count > 0)
                {
                    value = children[0] as ScrollViewer;
                }

                return value;
            }
            set
            {
                if (ScrollViewer is ScrollViewer oldValue)
                {
                    oldValue.ScrollChanged -= OnScrollViewerScrollChanged;
                    oldValue.SizeChanged -= OnScrollViewerSizeChanged;
                }

                var children = Children;
                children.Clear();
                children.Add(value);

                value.ScrollChanged += OnScrollViewerScrollChanged;
                value.SizeChanged += OnScrollViewerSizeChanged;
            }
        }

        bool IRepeaterScrollingSurface.IsHorizontallyScrollable => true;

        bool IRepeaterScrollingSurface.IsVerticallyScrollable => true;

        UIElement IRepeaterScrollingSurface.AnchorElement
        {
            get
            {
                var empty = Rect.Empty;
                return GetAnchorElement(ref empty);
            }
        }

        event ViewportChangedEventHandler IRepeaterScrollingSurface.ViewportChanged
        {
            add { ViewportChanged += value; }
            remove { ViewportChanged -= value; }
        }

        event PostArrangeEventHandler IRepeaterScrollingSurface.PostArrange
        {
            add { PostArrange += value; }
            remove { PostArrange -= value; }
        }

        event ConfigurationChangedEventHandler IRepeaterScrollingSurface.ConfigurationChanged
        {
            add { ConfigurationChanged += value; }
            remove { ConfigurationChanged -= value; }
        }

        void IRepeaterScrollingSurface.RegisterAnchorCandidate(UIElement element)
        {
            if (!double.IsNaN(HorizontalAnchorRatio) || !double.IsNaN(VerticalAnchorRatio))
            {
                if (ScrollViewer is ScrollViewer scrollViewer)
                {

#if DEBUG
                    // We should not be registring the same element twice. Even through it is functionally ok,
                    // we will end up spending more time during arrange than we must.
                    // However checking if an element is already in the list every time a new element is registered is worse for perf.
                    // So, I'm leaving an assert here to catch regression in our code but in release builds we run without the check.
                    var elem = element;
                    var i = m_candidates.FindIndex(c => c.Element == elem);
                    if (i >= 0)
                    {
                        //Debug.Assert(false);
                    }
#endif // _DEBUG

                    m_candidates.Add(new CandidateInfo(element));
                    m_isAnchorElementDirty = true;
                }
            }
        }

        void IRepeaterScrollingSurface.UnregisterAnchorCandidate(UIElement element)
        {
            var elem = element;
            var i = m_candidates.FindIndex(c => c.Element == elem);
            if (i >= 0)
            {
                m_candidates.RemoveAt(i);
                m_isAnchorElementDirty = true;
            }
        }

        Rect IRepeaterScrollingSurface.GetRelativeViewport(UIElement element)
        {
            if (ScrollViewer is ScrollViewer scrollViewer)
            {
                var elem = element;
                bool hasLockedViewport = HasPendingBringIntoView;
                var transformer = elem.SafeTransformToVisual(hasLockedViewport ? scrollViewer.GetContentTemplateRoot() : scrollViewer);
                var zoomFactor = 1.0;
                double viewportWidth = scrollViewer.ViewportWidth / zoomFactor;
                double viewportHeight = scrollViewer.ViewportHeight / zoomFactor;

                var elementOffset = transformer.Transform(new Point());

                elementOffset.X = -elementOffset.X;
                elementOffset.Y = -elementOffset.Y + m_pendingViewportShift;

                if (hasLockedViewport)
                {
                    elementOffset.X += m_pendingBringIntoView.ChangeViewOffset.X;
                    elementOffset.Y += m_pendingBringIntoView.ChangeViewOffset.Y;
                }

                return new Rect(elementOffset.X, elementOffset.Y, viewportWidth, viewportHeight);
            }

            return default;
        }

        // TODO: this API should go on UIElement.
        internal void StartBringIntoView(
            UIElement element,
            double alignmentX,
            double alignmentY,
            double offsetX,
            double offsetY,
            bool animate)
        {
            m_pendingBringIntoView = new BringIntoViewState(
                element,
                alignmentX,
                alignmentY,
                offsetX,
                offsetY,
                animate);
        }

        private void ApplyPendingChangeView(ScrollViewer scrollViewer)
        {
            var bringIntoView = m_pendingBringIntoView;
            Debug.Assert(!bringIntoView.ChangeViewCalled);

            bringIntoView.ChangeViewCalled = true;

            var layoutSlot = CachedVisualTreeHelpers.GetLayoutSlot((FrameworkElement)bringIntoView.TargetElement);

            // Arrange bounds are absolute.
            var arrangeBounds = bringIntoView
                .TargetElement
                .SafeTransformToVisual(scrollViewer.GetContentTemplateRoot())
                .TransformBounds(new Rect(0, 0, layoutSlot.Width, layoutSlot.Height));

            var scrollableArea = new Point(
                scrollViewer.ViewportWidth - arrangeBounds.Width,
                scrollViewer.ViewportHeight - arrangeBounds.Height);

            // Calculate the target offset based on the alignment and offset parameters.
            // Make sure that we are constrained to the ScrollViewer's extent.
            var changeViewOffset = new Point(
                Math.Max(0, Math.Min(
                    arrangeBounds.X + bringIntoView.OffsetX - scrollableArea.X * bringIntoView.AlignmentX,
                    scrollViewer.ExtentWidth - scrollViewer.ViewportWidth)),
                Math.Max(0, Math.Min(
                    arrangeBounds.Y + bringIntoView.OffsetY - scrollableArea.Y * bringIntoView.AlignmentY,
                    scrollViewer.ExtentHeight - scrollViewer.ViewportHeight)));
            bringIntoView.ChangeViewOffset = changeViewOffset;

            scrollViewer.ChangeView(
                changeViewOffset.X,
                changeViewOffset.Y,
                null,
                !bringIntoView.Animate);

            //m_pendingBringIntoView = std::move(bringIntoView);
        }

        private double TrackElement(UIElement element, Rect previousBounds, ScrollViewer scrollViewer)
        {
            var bounds = LayoutInformation.GetLayoutSlot((FrameworkElement)element);
            var transformer = element.SafeTransformToVisual(scrollViewer.GetContentTemplateRoot());
            var newBounds = transformer.TransformBounds(new Rect(
                0.0,
                0.0,
                bounds.Width,
                bounds.Height));

            var oldEdgeOffset = previousBounds.Y + HorizontalAnchorRatio * previousBounds.Height;
            var newEdgeOffset = newBounds.Y + HorizontalAnchorRatio * newBounds.Height;

            var unconstrainedPendingViewportShift = newEdgeOffset - oldEdgeOffset;
            var pendingViewportShift = unconstrainedPendingViewportShift;

            // ScrollViewer.ChangeView is not synchronous, so we need to account for the pending ChangeView call
            // and make sure we are locked on the target viewport.
            var verticalOffset =
                HasPendingBringIntoView && !m_pendingBringIntoView.Animate ?
                m_pendingBringIntoView.ChangeViewOffset.Y :
                scrollViewer.VerticalOffset;

            // Constrain the viewport shift to the extent
            if (verticalOffset + pendingViewportShift < 0)
            {
                pendingViewportShift = -verticalOffset;
            }
            else if (verticalOffset + scrollViewer.ViewportHeight + pendingViewportShift > scrollViewer.ExtentHeight)
            {
                pendingViewportShift = scrollViewer.ExtentHeight - scrollViewer.ViewportHeight - verticalOffset;
            }

            // WPF-specific
            if (scrollViewer.ScrollableHeight == 0)
            {
                pendingViewportShift = 0;
            }

            if (Math.Abs(pendingViewportShift) > 1)
            {
                // TODO: do we need to account for the zoom factor?
                // BUG:
                //  Unfortunately, if we have to correct while animating, we almost never
                //  update the ongoing animation correctly and we end up missing our target
                //  viewport. We should address that when building element tracking as part
                //  of the framework.
                scrollViewer.ChangeView(
                    null,
                    verticalOffset + pendingViewportShift,
                    null,
                    true /* disableAnimation */);
            }
            else
            {
                pendingViewportShift = 0.0;

                // We can't shift the viewport to follow the tracked element. The viewport relative
                // to the tracked element will have changed. We need to raise ViewportChanged to make
                // sure the repeaters will get a second layout pass to fill any empty space they have.
                if (Math.Abs(unconstrainedPendingViewportShift) > 1)
                {
                    ViewportChanged?.Invoke(this, true /* isFinal */);
                }
            }

            return pendingViewportShift;
        }

        private UIElement GetAnchorElement(ref Rect relativeBounds)
        {
            if (m_isAnchorElementDirty)
            {
                if (ScrollViewer is ScrollViewer scrollViewer)
                {
                    // ScrollViewer.ChangeView is not synchronous, so we need to account for the pending ChangeView call
                    // and make sure we are locked on the target viewport.
                    var verticalOffset =
                        HasPendingBringIntoView && !m_pendingBringIntoView.Animate ?
                        m_pendingBringIntoView.ChangeViewOffset.Y :
                        scrollViewer.VerticalOffset;
                    double viewportEdgeOffset = verticalOffset + HorizontalAnchorRatio * scrollViewer.ViewportHeight + m_pendingViewportShift;

                    CandidateInfo bestCandidate = null;
                    double bestCandidateDistance = float.MaxValue;

                    foreach (var candidate in m_candidates)
                    {
                        var element = candidate.Element;

                        if (!candidate.IsRelativeBoundsSet)
                        {
                            var bounds = LayoutInformation.GetLayoutSlot((FrameworkElement)element);
                            var transformer = element.SafeTransformToVisual(scrollViewer.GetContentTemplateRoot());
                            candidate.RelativeBounds = transformer.TransformBounds(new Rect(
                                0.0,
                                0.0,
                                bounds.Width,
                                bounds.Height));
                        }

                        double elementEdgeOffset = candidate.RelativeBounds.Y + HorizontalAnchorRatio * candidate.RelativeBounds.Height;
                        double candidateDistance = Math.Abs(elementEdgeOffset - viewportEdgeOffset);
                        if (candidateDistance < bestCandidateDistance)
                        {
                            bestCandidate = candidate;
                            bestCandidateDistance = candidateDistance;
                        }
                    }

                    if (bestCandidate != null)
                    {
                        m_anchorElement = bestCandidate.Element;
                        m_anchorElementRelativeBounds = bestCandidate.RelativeBounds;
                    }
                    else
                    {
                        m_anchorElement = null;
                        m_anchorElementRelativeBounds = CandidateInfo.InvalidBounds;
                    }
                }

                m_isAnchorElementDirty = false;
            }

            if (relativeBounds != Rect.Empty)
            {
                relativeBounds = m_anchorElementRelativeBounds;
            }

            return m_anchorElement;
        }

        private void OnScrollViewerScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            // WPF: Workaround for an issue where only the first item is realized
            if (e.ViewportWidthChange != 0 || e.ViewportHeightChange != 0)
            {
                InvalidateArrange();
            }

            if (e.HorizontalChange == 0 && e.VerticalChange == 0)
            {
                return;
            }

            m_pendingViewportShift = 0.0;

            if (HasPendingBringIntoView &&
                m_pendingBringIntoView.ChangeViewCalled)
            {
                m_pendingBringIntoView.Reset();
            }

            ViewportChanged?.Invoke(this, true /* isFinal */);
        }

        private void OnScrollViewerSizeChanged(object sender, SizeChangedEventArgs args)
        {
            ViewportChanged?.Invoke(this, true /* isFinal */);
        }

        private bool HasPendingBringIntoView => m_pendingBringIntoView.TargetElement != null;

        private class CandidateInfo
        {
            public CandidateInfo(UIElement element)
            {
                RelativeBounds = InvalidBounds;
                Element = element;
            }

            public UIElement Element { get; }
            public Rect RelativeBounds { get; set; }
            public bool IsRelativeBoundsSet => RelativeBounds != InvalidBounds;

            public static Rect InvalidBounds = Rect.Empty;
        }

        private class BringIntoViewState
        {
            public BringIntoViewState(UIElement owner)
            {
                TargetElement = owner;
            }

            public BringIntoViewState(
                UIElement targetElement,
                double alignmentX,
                double alignmentY,
                double offsetX,
                double offsetY,
                bool animate)
            {
                TargetElement = targetElement;
                AlignmentX = alignmentX;
                AlignmentY = alignmentY;
                OffsetX = offsetX;
                OffsetY = offsetY;
                Animate = animate;
                ChangeViewCalled = default;
                ChangeViewOffset = default;
            }

            public UIElement TargetElement { get; private set; }
            public double AlignmentX { get; private set; }
            public double AlignmentY { get; private set; }
            public double OffsetX { get; private set; }
            public double OffsetY { get; private set; }
            public bool Animate { get; }
            public bool ChangeViewCalled { get; set; }
            public Point ChangeViewOffset { get; set; }

            public void Reset()
            {
                TargetElement = null;
                AlignmentX = AlignmentY = OffsetX = OffsetY = 0.0;
            }
        }

        private readonly List<CandidateInfo> m_candidates = new List<CandidateInfo>();

        private UIElement m_anchorElement;
        private Rect m_anchorElementRelativeBounds;
        // Whenever the m_candidates list changes, we set this to true.
        private bool m_isAnchorElementDirty = true;

        private double m_horizontalEdge;
        private double m_verticalEdge;    // Not used in this temporary implementation.

        // We can only bring an element into view after it got arranged and
        // we know its bounds as well as the viewport (so that we can account
        // for alignment and offset).
        // The BringIntoView call can however be made at any point, even
        // in the constructor of a page (deserialization scenario) so we
        // need to hold on the parameter that are passed in BringIntoViewOperation.
        private BringIntoViewState m_pendingBringIntoView;

        // A ScrollViewer.ChangeView operation, even if not animated, is not synchronous.
        // In other words, right after the call, ScrollViewer.[Vertical|Horizontal]Offset and
        // TransformToVisual are not going to reflect the new viewport. We need to keep
        // track of the pending viewport shift until the ChangeView operation completes
        // asynchronously.
        private double m_pendingViewportShift;

        private event ViewportChangedEventHandler ViewportChanged;
        private event PostArrangeEventHandler PostArrange;
        private event ConfigurationChangedEventHandler ConfigurationChanged;
    }
}