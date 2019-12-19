// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;

namespace ModernWpf.Controls
{
    // Manages virtualization windows (visible/realization). 
    // This class does the equivalent behavior as ViewportManagerWithPlatformFeatures class
    // except that here we do not use EffectiveViewport and ScrollAnchoring features added to the framework in RS5. 
    // Instead we use the IRepeaterScrollingSurface internal API. This class is used when building in MUX and 
    // should work down-level.
    internal class ViewportManagerDownLevel : ViewportManager
    {
        // Pixel delta by which to inflate the cache buffer on each side.  Rather than fill the entire
        // cache buffer all at once, we chunk the work to make the UI thread more responsive.  We inflate
        // the cache buffer from 0 to a max value determined by the Maximum[Horizontal,Vertical]CacheLength
        // properties.
        private const double CacheBufferPerSideInflationPixelDelta = 40.0;

        public ViewportManagerDownLevel(ItemsRepeater owner)
        {
            m_owner = owner;
            // ItemsRepeater is not fully constructed yet. Don't interact with it.
        }

        public override UIElement SuggestedAnchor
        {
            get
            {
                // The element generated during the ItemsRepeater.MakeAnchor call has precedence over the next tick.
                UIElement suggestedAnchor = m_makeAnchorElement;
                UIElement owner = m_owner;

                if (suggestedAnchor == null)
                {
                    // We only care about what the first scrollable scroller is tracking (i.e. inner most).
                    // A scroller is considered scrollable if IRepeaterScrollingSurface.IsHorizontallyScrollable
                    // or IsVerticallyScroller is true.
                    var anchorElement = m_innerScrollableScroller?.AnchorElement;

                    if (anchorElement != null)
                    {
                        // We can't simply return anchorElement because, in case of nested Repeaters, it may not
                        // be a direct child of ours, or even an indirect child. We need to walk up the tree starting
                        // from anchorElement to figure out what child of ours (if any) to use as the suggested element.

                        var child = anchorElement;
                        var parent = CachedVisualTreeHelpers.GetParent(child) as UIElement;
                        while (parent != null)
                        {
                            if (parent == owner)
                            {
                                suggestedAnchor = child;
                                break;
                            }

                            child = parent;
                            parent = CachedVisualTreeHelpers.GetParent(child) as UIElement;
                        }
                    }
                }

                return suggestedAnchor;
            }
        }

        public override double HorizontalCacheLength
        {
            get => m_maximumHorizontalCacheLength;
            set
            {
                if (m_maximumHorizontalCacheLength != value)
                {
                    ValidateCacheLength(value);
                    m_maximumHorizontalCacheLength = value;
                    ResetCacheBuffer();
                }
            }
        }

        public override double VerticalCacheLength
        {
            get => m_maximumVerticalCacheLength;
            set
            {
                if (m_maximumVerticalCacheLength != value)
                {
                    ValidateCacheLength(value);
                    m_maximumVerticalCacheLength = value;
                    ResetCacheBuffer();
                }
            }
        }

        public override Rect GetLayoutVisibleWindow()
        {
            var visibleWindow = m_visibleWindow;

            if (m_makeAnchorElement != null)
            {
                // The anchor is not necessarily laid out yet. Its position should default
                // to zero and the layout origin is expected to change once layout is done.
                // Until then, we need a window that's going to protect the anchor from
                // getting recycled.
                visibleWindow.X = 0.0;
                visibleWindow.Y = 0.0;
            }
            else if (HasScrollers)
            {
                visibleWindow.X += m_layoutExtent.X + m_expectedViewportShift.X;
                visibleWindow.Y += m_layoutExtent.Y + m_expectedViewportShift.Y;
            }

            return visibleWindow;
        }

        public override Rect GetLayoutRealizationWindow()
        {
            var realizationWindow = GetLayoutVisibleWindow();
            if (HasScrollers)
            {
                realizationWindow.X -= m_horizontalCacheBufferPerSide;
                realizationWindow.Y -= m_verticalCacheBufferPerSide;
                realizationWindow.Width += m_horizontalCacheBufferPerSide * 2.0;
                realizationWindow.Height += m_verticalCacheBufferPerSide * 2.0;
            }

            return realizationWindow;
        }

        public override void SetLayoutExtent(Rect extent)
        {
            m_expectedViewportShift.X += m_layoutExtent.X - extent.X;
            m_expectedViewportShift.Y += m_layoutExtent.Y - extent.Y;

            m_layoutExtent = extent;

            // We just finished a measure pass and have a new extent.
            // Let's make sure the scrollers will run its arrange so that they track the anchor.
            var outerScroller = GetOuterScroller();
            if (outerScroller != null) { ((UIElement)outerScroller).InvalidateArrange(); }
            if (m_horizontalScroller != null && m_horizontalScroller != outerScroller) { ((UIElement)m_horizontalScroller).InvalidateArrange(); }
            if (m_verticalScroller != null && m_verticalScroller != outerScroller) { ((UIElement)m_verticalScroller).InvalidateArrange(); }
        }

        public override Point GetOrigin()
        {
            return new Point(m_layoutExtent.X, m_layoutExtent.Y);
        }

        public override void OnLayoutChanged(bool isVirtualizing)
        {
            m_managingViewportDisabled = !isVirtualizing;
            m_layoutExtent = default;
            m_expectedViewportShift = default;
            ResetCacheBuffer();
        }

        public override void OnElementPrepared(UIElement element)
        {
        }

        public override void OnElementCleared(UIElement element)
        {
            if (m_horizontalScroller != null)
            {
                m_horizontalScroller.UnregisterAnchorCandidate(element);
            }

            if (m_verticalScroller != null && m_verticalScroller != m_horizontalScroller)
            {
                m_verticalScroller.UnregisterAnchorCandidate(element);
            }
        }

        public override void OnOwnerMeasuring()
        {
            if (m_managingViewportDisabled)
            {
                return;
            }

            // This is because of a bug that causes effective viewport to not 
            // fire if you register during arrange.
            // Bug 17411076: EffectiveViewport: registering for effective viewport in arrange should invalidate viewport
            EnsureScrollers();
        }

        public override void OnOwnerArranged()
        {
            if (m_managingViewportDisabled)
            {
                return;
            }

            m_expectedViewportShift = default;

            // This is because of a bug that causes effective viewport to not 
            // fire if you register during arrange.
            // Bug 17411076: EffectiveViewport: registering for effective viewport in arrange should invalidate viewport
            // EnsureScrollers();

            if (HasScrollers)
            {
                double maximumHorizontalCacheBufferPerSide = m_maximumHorizontalCacheLength * m_visibleWindow.Width / 2.0;
                double maximumVerticalCacheBufferPerSide = m_maximumVerticalCacheLength * m_visibleWindow.Height / 2.0;

                bool continueBuildingCache =
                    m_horizontalCacheBufferPerSide < maximumHorizontalCacheBufferPerSide ||
                    m_verticalCacheBufferPerSide < maximumVerticalCacheBufferPerSide;

                if (continueBuildingCache)
                {
                    m_horizontalCacheBufferPerSide += CacheBufferPerSideInflationPixelDelta;
                    m_verticalCacheBufferPerSide += CacheBufferPerSideInflationPixelDelta;

                    m_horizontalCacheBufferPerSide = Math.Min(m_horizontalCacheBufferPerSide, maximumHorizontalCacheBufferPerSide);
                    m_verticalCacheBufferPerSide = Math.Min(m_verticalCacheBufferPerSide, maximumVerticalCacheBufferPerSide);

                    // Since we grow the cache buffer at the end of the arrange pass,
                    // we need to register work even if we just reached cache potential.
                    RegisterCacheBuildWork();
                }
            }
        }

        public override void OnMakeAnchor(UIElement anchor, bool isAnchorOutsideRealizedRange)
        {
            m_makeAnchorElement = anchor;
            m_isAnchorOutsideRealizedRange = isAnchorOutsideRealizedRange;
        }

        public override void ResetScrollers()
        {
            m_parentScrollers.Clear();
            m_horizontalScroller = null;
            m_verticalScroller = null;
            m_innerScrollableScroller = null;

            m_ensuredScrollers = false;
        }

        public override UIElement MadeAnchor => m_makeAnchorElement;

        //struct ScrollerInfo;

        private void OnCacheBuildActionCompleted()
        {
            m_cacheBuildAction = null;
            m_owner.InvalidateMeasure();
        }

        private void OnViewportChanged(IRepeaterScrollingSurface sender, bool isFinal)
        {
            if (!m_managingViewportDisabled)
            {
                if (isFinal)
                {
                    // Note that isFinal will never be true for input based manipulations.
                    m_makeAnchorElement = null;
                    m_isAnchorOutsideRealizedRange = false;
                }

                TryInvalidateMeasure();
            }
        }

        private void OnPostArrange(IRepeaterScrollingSurface sender)
        {
            if (!m_managingViewportDisabled)
            {
                UpdateViewport();

                if (m_visibleWindow == new Rect())
                {
                    // We got cleared.
                    m_layoutExtent = default;
                }
                else
                {
                    // Register our non-recycled children as candidates for element tracking.
                    if (m_horizontalScroller != null || m_verticalScroller != null)
                    {
                        var children = m_owner.Children;
                        for (int i = 0; i < children.Count; ++i)
                        {
                            var element = children[i];
                            var virtInfo = ItemsRepeater.GetVirtualizationInfo(element);
                            if (virtInfo.IsHeldByLayout)
                            {
                                if (m_horizontalScroller != null)
                                {
                                    m_horizontalScroller.RegisterAnchorCandidate(element);
                                }

                                if (m_verticalScroller != null && m_verticalScroller != m_horizontalScroller)
                                {
                                    m_verticalScroller.RegisterAnchorCandidate(element);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void OnConfigurationChanged(IRepeaterScrollingSurface sender)
        {
            m_ensuredScrollers = false;
            TryInvalidateMeasure();
        }

        private void EnsureScrollers()
        {
            if (!m_ensuredScrollers)
            {
                ResetScrollers();

                var parent = CachedVisualTreeHelpers.GetParent(m_owner);
                while (parent != null)
                {
                    var scroller = parent as IRepeaterScrollingSurface;
                    if (scroller != null && AddScroller(scroller))
                    {
                        break;
                    }

                    parent = CachedVisualTreeHelpers.GetParent(parent);
                }

                if (m_parentScrollers.Count == 0)
                {
                    // We usually update the viewport in the post arrange handler. But, since we don't have
                    // a scroller, let's do it now.
                    UpdateViewport();
                }
                else
                {
                    var outerScrollerInfo = m_parentScrollers[m_parentScrollers.Count - 1];
                    outerScrollerInfo.Scroller.PostArrange += OnPostArrange;
                    //outerScrollerInfo.PostArrangeToken = outerScrollerInfo.Scroller().PostArrange(winrt::auto_revoke, { this, &ViewportManagerDownLevel::OnPostArrange });
                }

                m_ensuredScrollers = true;
            }
        }

        private bool HasScrollers => m_horizontalScroller != null || m_verticalScroller != null;

        private bool AddScroller(IRepeaterScrollingSurface scroller)
        {
            Debug.Assert(!(m_horizontalScroller != null && m_verticalScroller != null));

            bool isHorizontallyScrollable = scroller.IsHorizontallyScrollable;
            bool isVerticallyScrollable = scroller.IsVerticallyScrollable;
            bool allScrollersSet = (m_horizontalScroller != null || isHorizontallyScrollable) && (m_verticalScroller != null || isVerticallyScrollable);
            bool setHorizontalScroller = m_horizontalScroller == null && isHorizontallyScrollable;
            bool setVerticalScroller = m_verticalScroller == null && isVerticallyScrollable;
            bool setInnerScrollableScroller = m_innerScrollableScroller == null && (setHorizontalScroller || setVerticalScroller);

            if (setHorizontalScroller) { m_horizontalScroller = scroller; }
            if (setVerticalScroller) { m_verticalScroller = scroller; }
            if (setInnerScrollableScroller) { m_innerScrollableScroller = scroller; }

            var scrollerInfo = new ScrollerInfo(scroller);

            scroller.ConfigurationChanged += OnConfigurationChanged;
            if (setHorizontalScroller || setVerticalScroller)
            {
                scroller.ViewportChanged += OnViewportChanged;
            }

            m_parentScrollers.Add(scrollerInfo);
            return allScrollersSet;
        }

        private void UpdateViewport()
        {
            Debug.Assert(!m_managingViewportDisabled);

            var previousVisibleWindow = m_visibleWindow;
            var horizontalVisibleWindow =
                m_horizontalScroller != null ?
                m_horizontalScroller.GetRelativeViewport(m_owner) :
                new Rect();
            var verticalVisibleWindow =
                m_verticalScroller != null ?
                (m_verticalScroller == m_horizontalScroller ?
                    horizontalVisibleWindow :
                    m_verticalScroller.GetRelativeViewport(m_owner)) :
                new Rect();
            var currentVisibleWindow =
                HasScrollers?
                new Rect
                (
                    m_horizontalScroller != null ? horizontalVisibleWindow.X : verticalVisibleWindow.X,
                    m_verticalScroller != null ? verticalVisibleWindow.Y : horizontalVisibleWindow.Y,
                    m_horizontalScroller != null ? horizontalVisibleWindow.Width : verticalVisibleWindow.Width,
                    m_verticalScroller != null ? verticalVisibleWindow.Height : horizontalVisibleWindow.Height
                ) :
                new Rect(0.0, 0.0, double.MaxValue, double.MaxValue);

            if (-currentVisibleWindow.X <= ItemsRepeater.ClearedElementsArrangePosition.X &&
                -currentVisibleWindow.Y <= ItemsRepeater.ClearedElementsArrangePosition.Y)
            {
                // We got cleared.
                m_visibleWindow = default;
            }
            else
            {
                m_visibleWindow = currentVisibleWindow;
            }

            bool viewportChanged =
                Math.Abs(m_visibleWindow.X - previousVisibleWindow.X) > 1 ||
                Math.Abs(m_visibleWindow.Y - previousVisibleWindow.Y) > 1 ||
                m_visibleWindow.Width != previousVisibleWindow.Width ||
                m_visibleWindow.Height != previousVisibleWindow.Height;

            if (viewportChanged)
            {
                TryInvalidateMeasure();
            }
        }

        private void ResetCacheBuffer()
        {
            m_horizontalCacheBufferPerSide = 0.0;
            m_verticalCacheBufferPerSide = 0.0;

            if (!m_managingViewportDisabled)
            {
                // We need to start building the realization buffer again.
                RegisterCacheBuildWork();
            }
        }

        private void ValidateCacheLength(double cacheLength)
        {
            if (cacheLength < 0.0 || double.IsInfinity(cacheLength) || double.IsNaN(cacheLength))
            {
                throw new ArgumentOutOfRangeException("The maximum cache length must be equal or superior to zero.");
            }
        }

        private void RegisterCacheBuildWork()
        {
            if (m_owner.Layout != null &&
                m_cacheBuildAction == null)
            {
                var strongOwner = m_owner;
                m_cacheBuildAction = m_owner
                    .Dispatcher
                    // We capture 'owner' (a strong refernce on ItemsRepeater) to make sure ItemsRepeater is still around
                    // when the async action completes. By protecting ItemsRepeater, we also ensure that this instance
                    // of ViewportManager (referenced by 'this' pointer) is valid because the lifetime of ItemsRepeater
                    // and ViewportManager is the same (see ItemsRepeater::m_viewportManager).
                    // We can't simply hold a strong reference on ViewportManager because it's not a COM object.
                    .BeginInvoke(() =>
                    {
                        OnCacheBuildActionCompleted();
                    }, DispatcherPriority.ApplicationIdle);
            }
        }

        private void TryInvalidateMeasure()
        {
            // Don't invalidate measure if we have an invalid window.
            if (m_visibleWindow != new Rect())
            {
                // We invalidate measure instead of just invalidating arrange because
                // we don't invalidate measure in UpdateViewport if the view is changing to
                // avoid layout cycles.
                m_owner.InvalidateMeasure();
            }
        }

        private IRepeaterScrollingSurface GetOuterScroller()
        {
            IRepeaterScrollingSurface scroller = null;

            if (m_parentScrollers.Count > 0)
            {
                scroller = m_parentScrollers[m_parentScrollers.Count - 1].Scroller;
            }

            return scroller;
        }

        private string GetLayoutId()
        {
            string layoutId = null;
            if (m_owner.Layout is Layout layout)
            {
                layoutId = layout.LayoutId;
            }

            return layoutId;
        }

        private readonly ItemsRepeater m_owner;

        // List of parent scrollers.
        // The list stops when we reach the root scroller OR when both m_horizontalScroller
        // and m_verticalScroller are set. In the latter case, we don't care about the other
        // scroller that we haven't reached yet.
        private bool m_ensuredScrollers = false;
        private readonly List<ScrollerInfo> m_parentScrollers = new List<ScrollerInfo>();

        // In order to support the Store scenario (vertical list of horizontal lists),
        // we need to build a synthetic virtualization window by taking the horizontal and
        // vertical components of the viewport from two different scrollers.
        private IRepeaterScrollingSurface m_horizontalScroller;
        private IRepeaterScrollingSurface m_verticalScroller;
        // Invariant: !m_innerScrollableScroller || m_horizontalScroller == m_innerScrollableScroller || m_verticalScroller == m_innerScrollableScroller.
        private IRepeaterScrollingSurface m_innerScrollableScroller;

        private UIElement m_makeAnchorElement;
        private bool m_isAnchorOutsideRealizedRange;  // Value is only valid when m_makeAnchorElement is set.

        private DispatcherOperation m_cacheBuildAction;

        private Rect m_visibleWindow;
        private Rect m_layoutExtent;
        private Point m_expectedViewportShift;

        // Realization window cache fields
        private double m_maximumHorizontalCacheLength = 2.0;
        private double m_maximumVerticalCacheLength = 2.0;
        private double m_horizontalCacheBufferPerSide;
        private double m_verticalCacheBufferPerSide;

        // For non-virtualizing layouts, we do not need to keep
        // updating viewports and invalidating measure often. So when
        // a non virtualizing layout is used, we stop doing all that work.
        private bool m_managingViewportDisabled = false;

        // Stores information about a parent scrolling surface.
        // We subscribe to...
        // - ViewportChanged only on scrollers that are scrollable in at least one direction.
        // - ConfigurationChanged on all scrollers.
        // - PostArrange only on the outer most scroller, because we need to wait for that one
        //   to arrange its children before we can reliably figure out our relative viewport.
        private struct ScrollerInfo
        {
            public ScrollerInfo(IRepeaterScrollingSurface scroller)
            {
                Scroller = scroller;
            }

            public IRepeaterScrollingSurface Scroller { get; }
        };
    }
}