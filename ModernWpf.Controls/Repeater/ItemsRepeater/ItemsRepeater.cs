// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using ModernWpf.Automation.Peers;

namespace ModernWpf.Controls
{
    [ContentProperty(nameof(ItemTemplate))]
    public partial class ItemsRepeater : Panel
    {
        internal static readonly Point ClearedElementsArrangePosition = new Point(-10000.0, -10000.0);
        // A convention we use in the ItemsRepeater codebase for an invalid Rect value.
        internal static readonly Rect InvalidRect = Rect.Empty;

        static ItemsRepeater()
        {
            KeyboardNavigation.TabNavigationProperty.OverrideMetadata(typeof(ItemsRepeater),
                new FrameworkPropertyMetadata(KeyboardNavigationMode.Once));
        }

        public ItemsRepeater()
        {
            AnimationManager = new AnimationManager(this);
            ViewManager = new ViewManager(this);
            m_viewportManager = new ViewportManagerDownLevel(this);

            Loaded += OnLoaded;
            Unloaded += OnUnloaded;

            SetCurrentValue(LayoutProperty, new StackLayout());

            // Initialize the cached layout to the default value
            var layout = Layout as VirtualizingLayout;
            OnLayoutChanged(null, layout);
        }

        ~ItemsRepeater()
        {
            m_itemTemplate = null;
            m_animator = null;
            m_layout = null;
        }

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new RepeaterAutomationPeer(this);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            if (m_isLayoutInProgress)
            {
                throw new InvalidOperationException("Reentrancy detected during layout.");
            }

            if (IsProcessingCollectionChange)
            {
                throw new InvalidOperationException("Cannot run layout in the middle of a collection change.");
            }

            m_viewportManager.OnOwnerMeasuring();

            m_isLayoutInProgress = true;
            try
            {
                ViewManager.PrunePinnedElements();
                Rect extent = default;
                Size desiredSize = default;

                if (Layout is Layout layout)
                {
                    var layoutContext = GetLayoutContext();

                    // Checking if we have an data template and it is empty
                    if (m_isItemTemplateEmpty)
                    {
                        // Has no content, so we will draw nothing and request zero space
                        extent = new Rect(m_layoutOrigin.X, m_layoutOrigin.Y, 0, 0);
                    }
                    else
                    {
                        desiredSize = layout.Measure(layoutContext, availableSize);
                        extent = new Rect(m_layoutOrigin.X, m_layoutOrigin.Y, desiredSize.Width, desiredSize.Height);
                    }

                    // Clear auto recycle candidate elements that have not been kept alive by layout - i.e layout did not
                    // call GetElementAt(index).
                    var children = Children;
                    for (int i = 0; i < children.Count; ++i)
                    {
                        var element = children[i];
                        var virtInfo = GetVirtualizationInfo(element);

                        if (virtInfo.Owner == ElementOwner.Layout &&
                            virtInfo.AutoRecycleCandidate &&
                            !virtInfo.KeepAlive)
                        {
                            ClearElementImpl(element);
                        }
                    }
                }

                m_viewportManager.SetLayoutExtent(extent);
                m_lastAvailableSize = availableSize;
                return desiredSize;
            }
            finally
            {
                m_isLayoutInProgress = false;
            }
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (m_isLayoutInProgress)
            {
                throw new InvalidOperationException("Reentrancy detected during layout.");
            }

            if (IsProcessingCollectionChange)
            {
                throw new InvalidOperationException("Cannot run layout in the middle of a collection change.");
            }

            m_isLayoutInProgress = true;
            try
            {
                Size arrangeSize = default;

                if (Layout is Layout layout)
                {
                    arrangeSize = layout.Arrange(GetLayoutContext(), finalSize);
                }

                // The view manager might clear elements during this call.
                // That's why we call it before arranging cleared elements
                // off screen.
                ViewManager.OnOwnerArranged();

                var children = Children;
                for (int i = 0; i < children.Count; ++i)
                {
                    var element = children[i];
                    var virtInfo = GetVirtualizationInfo(element);
                    virtInfo.KeepAlive = false;

                    if (virtInfo.Owner == ElementOwner.ElementFactory ||
                        virtInfo.Owner == ElementOwner.PinnedPool)
                    {
                        // Toss it away. And arrange it with size 0 so that XYFocus won't use it.
                        element.Arrange(new Rect(
                            ClearedElementsArrangePosition.X - element.DesiredSize.Width,
                            ClearedElementsArrangePosition.Y - element.DesiredSize.Height,
                            0.0,
                            0.0));
                    }
                    else
                    {
                        var newBounds = CachedVisualTreeHelpers.GetLayoutSlot(element as FrameworkElement);

                        if (virtInfo.ArrangeBounds != InvalidRect &&
                            newBounds != virtInfo.ArrangeBounds)
                        {
                            AnimationManager.OnElementBoundsChanged(element, virtInfo.ArrangeBounds, newBounds);
                        }

                        virtInfo.ArrangeBounds = newBounds;
                        virtInfo.DesiredSize = element.DesiredSize;
                    }
                }

                m_viewportManager.OnOwnerArranged();
                AnimationManager.OnOwnerArranged();

                return arrangeSize;
            }
            finally
            {
                m_isLayoutInProgress = false;
            }
        }

        #region Properties

        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(
                nameof(ItemsSource),
                typeof(object),
                typeof(ItemsRepeater),
                new PropertyMetadata(OnPropertyChanged));

        public object ItemsSource
        {
            get => GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public ItemsSourceView ItemsSourceView { get; private set; }

        public static readonly DependencyProperty ItemTemplateProperty =
            DependencyProperty.Register(
                nameof(ItemTemplate),
                typeof(object),
                typeof(ItemsRepeater),
                new PropertyMetadata(OnPropertyChanged));

        public object ItemTemplate
        {
            get => GetValue(ItemTemplateProperty);
            set => SetValue(ItemTemplateProperty, value);
        }

        public static readonly DependencyProperty LayoutProperty =
            DependencyProperty.Register(
                nameof(Layout),
                typeof(Layout),
                typeof(ItemsRepeater),
                new PropertyMetadata(OnPropertyChanged));

        public Layout Layout
        {
            get => (Layout)GetValue(LayoutProperty);
            set => SetValue(LayoutProperty, value);
        }

        public static readonly DependencyProperty AnimatorProperty =
            DependencyProperty.Register(
                nameof(Animator),
                typeof(ElementAnimator),
                typeof(ItemsRepeater),
                new PropertyMetadata(OnPropertyChanged));

        internal ElementAnimator Animator
        {
            get => (ElementAnimator)GetValue(AnimatorProperty);
            set => SetValue(AnimatorProperty, value);
        }

        public static readonly DependencyProperty HorizontalCacheLengthProperty =
            DependencyProperty.Register(
                nameof(HorizontalCacheLength),
                typeof(double),
                typeof(ItemsRepeater),
                new PropertyMetadata(2.0, OnPropertyChanged));

        public double HorizontalCacheLength
        {
            get => (double)GetValue(HorizontalCacheLengthProperty);
            set => SetValue(HorizontalCacheLengthProperty, value);
        }

        public static readonly DependencyProperty VerticalCacheLengthProperty =
            DependencyProperty.Register(
                nameof(VerticalCacheLength),
                typeof(double),
                typeof(ItemsRepeater),
                new PropertyMetadata(2.0, OnPropertyChanged));

        public double VerticalCacheLength
        {
            get => (double)GetValue(VerticalCacheLengthProperty);
            set => SetValue(VerticalCacheLengthProperty, value);
        }

        #endregion

        public int GetElementIndex(UIElement element)
        {
            return GetElementIndexImpl(element);
        }

        public UIElement TryGetElement(int index)
        {
            return GetElementFromIndexImpl(index);
        }

        internal void PinElement(UIElement element)
        {
            ViewManager.UpdatePin(element, true /* addPin */);
        }

        internal void UnpinElement(UIElement element)
        {
            ViewManager.UpdatePin(element, false /* addPin */);
        }

        public UIElement GetOrCreateElement(int index)
        {
            return GetOrCreateElementImpl(index);
        }

        public event TypedEventHandler<ItemsRepeater, ItemsRepeaterElementClearingEventArgs> ElementClearing;
        public event TypedEventHandler<ItemsRepeater, ItemsRepeaterElementIndexChangedEventArgs> ElementIndexChanged;
        public event TypedEventHandler<ItemsRepeater, ItemsRepeaterElementPreparedEventArgs> ElementPrepared;

        internal IElementFactoryShim ItemTemplateShim => m_itemTemplateWrapper;

        internal ViewManager ViewManager { get; }

        internal AnimationManager AnimationManager { get; }

        internal UIElement GetElementImpl(int index, bool forceCreate, bool suppressAutoRecycle)
        {
            var element = ViewManager.GetElement(index, forceCreate, suppressAutoRecycle);
            return element;
        }

        internal void ClearElementImpl(UIElement element)
        {
            // Clearing an element due to a collection change
            // is more strict in that pinned elements will be forcibly
            // unpinned and sent back to the view generator.
            bool isClearedDueToCollectionChange =
              IsProcessingCollectionChange &&
              (m_processingItemsSourceChange.Action == NotifyCollectionChangedAction.Remove ||
                  m_processingItemsSourceChange.Action == NotifyCollectionChangedAction.Replace ||
                  m_processingItemsSourceChange.Action == NotifyCollectionChangedAction.Reset);

            ViewManager.ClearElement(element, isClearedDueToCollectionChange);
            m_viewportManager.OnElementCleared(element);
        }

        internal int GetElementIndexImpl(UIElement element)
        {
            // Verify that element is actually a child of this ItemsRepeater
            var parent = VisualTreeHelper.GetParent(element);
            if (parent == this)
            {
                var virtInfo = TryGetVirtualizationInfo(element);
                return ViewManager.GetElementIndex(virtInfo);
            }
            return -1;
        }

        internal UIElement GetElementFromIndexImpl(int index)
        {
            UIElement result = null;

            var children = Children;
            for (int i = 0; i < children.Count && result == null; ++i)
            {
                var element = children[i];
                var virtInfo = TryGetVirtualizationInfo(element);
                if (virtInfo != null && virtInfo.IsRealized && virtInfo.Index == index)
                {
                    result = element;
                }
            }

            return result;
        }

        internal UIElement GetOrCreateElementImpl(int index)
        {
            if (index >= 0 && index >= ItemsSourceView.Count)
            {
                throw new ArgumentException(nameof(index), "Argument index is invalid.");
            }

            if (m_isLayoutInProgress)
            {
                throw new InvalidOperationException("GetOrCreateElement invocation is not allowed during layout.");
            }

            var element = GetElementFromIndexImpl(index);
            bool isAnchorOutsideRealizedRange = element == null;

            if (isAnchorOutsideRealizedRange)
            {
                if (Layout == null)
                {
                    throw new InvalidOperationException("Cannot make an Anchor when there is no attached layout.");
                }

                element = GetLayoutContext().GetOrCreateElementAt(index);
                element.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            }

            m_viewportManager.OnMakeAnchor(element, isAnchorOutsideRealizedRange);
            InvalidateMeasure();

            return element;
        }

        internal static VirtualizationInfo TryGetVirtualizationInfo(UIElement element)
        {
            var value = element.GetValue(VirtualizationInfoProperty);
            return (VirtualizationInfo)value;
        }

        internal static VirtualizationInfo GetVirtualizationInfo(UIElement element)
        {
            var result = TryGetVirtualizationInfo(element);

            if (result == null)
            {
                result = CreateAndInitializeVirtualizationInfo(element);
            }

            return result;
        }

        internal static VirtualizationInfo CreateAndInitializeVirtualizationInfo(UIElement element)
        {
            Debug.Assert(TryGetVirtualizationInfo(element) == null);
            var result = new VirtualizationInfo();
            element.SetValue(VirtualizationInfoProperty, result);
            return result;
        }

        private static void OnPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((ItemsRepeater)sender).PrivateOnPropertyChanged(args);
        }

        private void PrivateOnPropertyChanged(DependencyPropertyChangedEventArgs args)
        {
            DependencyProperty property = args.Property;

            if (property == ItemsSourceProperty)
            {
                if (args.NewValue != args.OldValue)
                {
                    var newValue = args.NewValue;
                    var newDataSource = newValue as ItemsSourceView;
                    if (newValue != null && newDataSource == null)
                    {
                        newDataSource = new InspectingDataSource(newValue);
                    }

                    OnDataSourcePropertyChanged(ItemsSourceView, newDataSource);
                }
            }
            else if (property == ItemTemplateProperty)
            {
                OnItemTemplateChanged(args.OldValue, args.NewValue);
            }
            else if (property == LayoutProperty)
            {
                OnLayoutChanged((Layout)args.OldValue, (Layout)args.NewValue);
            }
            else if (property == AnimatorProperty)
            {
                OnAnimatorChanged((ElementAnimator)args.OldValue, (ElementAnimator)args.NewValue);
            }
            else if (property == HorizontalCacheLengthProperty)
            {
                m_viewportManager.HorizontalCacheLength = (double)args.NewValue;
            }
            else if (property == VerticalCacheLengthProperty)
            {
                m_viewportManager.VerticalCacheLength = (double)args.NewValue;
            }
        }

        internal object LayoutState { get; set; }

        internal Rect VisibleWindow => m_viewportManager.GetLayoutVisibleWindow();

        internal Rect RealizationWindow => m_viewportManager.GetLayoutRealizationWindow();

        internal UIElement SuggestedAnchor => m_viewportManager.SuggestedAnchor;

        internal UIElement MadeAnchor => m_viewportManager.MadeAnchor;

        internal Point LayoutOrigin
        {
            get => m_layoutOrigin;
            set => m_layoutOrigin = value;
        }

        internal void OnElementPrepared(UIElement element, int index)
        {
            m_viewportManager.OnElementPrepared(element);
            if (ElementPrepared != null)
            {
                if (m_elementPreparedArgs == null)
                {
                    m_elementPreparedArgs = new ItemsRepeaterElementPreparedEventArgs(element, index);
                }
                else
                {
                    m_elementPreparedArgs.Update(element, index);
                }

                ElementPrepared(this, m_elementPreparedArgs);
            }
        }

        internal void OnElementClearing(UIElement element)
        {
            if (ElementClearing != null)
            {
                if (m_elementClearingArgs == null)
                {
                    m_elementClearingArgs = new ItemsRepeaterElementClearingEventArgs(element);
                }
                else
                {
                    m_elementClearingArgs.Update(element);
                }

                ElementClearing(this, m_elementClearingArgs);
            }
        }

        internal void OnElementIndexChanged(UIElement element, int oldIndex, int newIndex)
        {
            if (ElementIndexChanged != null)
            {
                if (m_elementIndexChangedArgs == null)
                {
                    m_elementIndexChangedArgs = new ItemsRepeaterElementIndexChangedEventArgs(element, oldIndex, newIndex);
                }
                else
                {
                    m_elementIndexChangedArgs.Update(element, oldIndex, newIndex);
                }

                ElementIndexChanged(this, m_elementIndexChangedArgs);
            }
        }

        internal static readonly DependencyProperty VirtualizationInfoProperty =
            DependencyProperty.RegisterAttached(
                "VirtualizationInfo",
                typeof(VirtualizationInfo),
                typeof(ItemsRepeater),
                null);

        private void OnLoaded(object sender, RoutedEventArgs args)
        {
            // If we skipped an unload event, reset the scrollers now and invalidate measure so that we get a new
            // layout pass during which we will hookup new scrollers.
            if (_loadedCounter > _unloadedCounter)
            {
                _loadedCounter = _unloadedCounter; // WPF-specific optimization
                InvalidateMeasure();
                m_viewportManager.ResetScrollers();
            }
            ++_loadedCounter;
        }

        private void OnUnloaded(object sender, RoutedEventArgs args)
        {
            ++_unloadedCounter;
            // Only reset the scrollers if this unload event is in-sync.
            if (_unloadedCounter == _loadedCounter)
            {
                m_viewportManager.ResetScrollers();
            }
        }

        private void OnDataSourcePropertyChanged(ItemsSourceView oldValue, ItemsSourceView newValue)
        {
            if (m_isLayoutInProgress)
            {
                throw new InvalidOperationException("Cannot set ItemsSourceView during layout.");
            }

            ItemsSourceView = newValue;

            if (oldValue != null)
            {
                oldValue.CollectionChanged -= OnItemsSourceViewChanged;
            }

            if (newValue != null)
            {
                newValue.CollectionChanged += OnItemsSourceViewChanged;
            }

            var processingChange = false;

            try
            {
                if (Layout is Layout layout)
                {
                    var args = new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Reset);
                    //args.Action;
                    m_processingItemsSourceChange = args;
                    processingChange = true;

                    if (layout is VirtualizingLayout virtualLayout)
                    {
                        ((IVirtualizingLayoutOverrides)virtualLayout).OnItemsChangedCore(GetLayoutContext(), newValue, args);
                    }
                    else if (layout is NonVirtualizingLayout nonVirtualLayout)
                    {
                        // Walk through all the elements and make sure they are cleared for
                        // non-virtualizing layouts.
                        foreach (UIElement element in Children)
                        {
                            if (GetVirtualizationInfo(element).IsRealized)
                            {
                                ClearElementImpl(element);
                            }
                        }

                        Children.Clear();
                    }

                    InvalidateMeasure();
                }
            }
            finally
            {
                if (processingChange)
                {
                    m_processingItemsSourceChange = null;
                }
            }
        }

        private void OnItemTemplateChanged(object oldValue, object newValue)
        {
            if (m_isLayoutInProgress && oldValue != null)
            {
                throw new InvalidOperationException("ItemTemplate cannot be changed during layout.");
            }

            bool processingChange = false;

            try
            {
                // Since the ItemTemplate has changed, we need to re-evaluate all the items that
                // have already been created and are now in the tree. The easiest way to do that
                // would be to do a reset.. Note that this has to be done before we change the template
                // so that the cleared elements go back into the old template.
                if (Layout is Layout layout)
                {
                    var args = new NotifyCollectionChangedEventArgs(
                        NotifyCollectionChangedAction.Reset);
                    //args.Action;

                    m_processingItemsSourceChange = args;
                    processingChange = true;

                    if (layout is VirtualizingLayout virtualLayout)
                    {
                        ((IVirtualizingLayoutOverrides)virtualLayout).OnItemsChangedCore(GetLayoutContext(), newValue, args);
                    }
                    else if (layout is NonVirtualizingLayout nonVirtualLayout)
                    {
                        // Walk through all the elements and make sure they are cleared for
                        // non-virtualizing layouts.
                        foreach (UIElement child in Children)
                        {
                            if (GetVirtualizationInfo(child).IsRealized)
                            {
                                ClearElementImpl(child);
                            }
                        }
                    }
                }

                //if (!SharedHelpers.IsRS5OrHigher())
                {
                    // Bug in framework's reference tracking causes crash during
                    // UIAffinityQueue cleanup. To avoid that bug, take a strong ref
                    m_itemTemplate = newValue;
                }
                // Clear flag for bug #776
                m_isItemTemplateEmpty = false;
                m_itemTemplateWrapper = newValue as IElementFactoryShim;
                if (m_itemTemplateWrapper == null)
                {
                    // ItemTemplate set does not implement IElementFactoryShim. We also 
                    // want to support DataTemplate and DataTemplateSelectors automagically.
                    if (newValue is DataTemplate dataTemplate)
                    {
                        m_itemTemplateWrapper = new ItemTemplateWrapper(dataTemplate);
                        if ((dataTemplate.LoadContent() as FrameworkElement) == null)
                        {
                            // We have a DataTemplate which is empty, so we need to set it to true
                            m_isItemTemplateEmpty = true;
                        }
                    }
                    else if (newValue is DataTemplateSelector selector)
                    {
                        m_itemTemplateWrapper = new ItemTemplateWrapper(selector);
                    }
                    else
                    {
                        throw new ArgumentException(nameof(newValue), "ItemTemplate");
                    }
                }

                InvalidateMeasure();
            }
            finally
            {
                if (processingChange)
                {
                    m_processingItemsSourceChange = null;
                }
            }
        }

        private void OnLayoutChanged(Layout oldValue, Layout newValue)
        {
            if (m_isLayoutInProgress)
            {
                throw new InvalidOperationException("Layout cannot be changed during layout.");
            }

            ViewManager.OnLayoutChanging();
            AnimationManager.OnLayoutChanging();

            if (oldValue != null)
            {
                oldValue.UninitializeForContext(GetLayoutContext());
                oldValue.MeasureInvalidated -= InvalidateMeasureForLayout;
                oldValue.ArrangeInvalidated -= InvalidateArrangeForLayout;

                // Walk through all the elements and make sure they are cleared
                var children = Children;
                for (int i = 0; i < children.Count; ++i)
                {
                    var element = children[i];
                    if (GetVirtualizationInfo(element).IsRealized)
                    {
                        ClearElementImpl(element);
                    }
                }

                LayoutState = null;
            }

            //if (!SharedHelpers.IsRS5OrHigher())
            {
                // Bug in framework's reference tracking causes crash during
                // UIAffinityQueue cleanup. To avoid that bug, take a strong ref
                m_layout = newValue;
            }

            if (newValue != null)
            {
                newValue.InitializeForContext(GetLayoutContext());
                newValue.MeasureInvalidated += InvalidateMeasureForLayout;
                newValue.ArrangeInvalidated += InvalidateArrangeForLayout;
            }

            bool isVirtualizingLayout = newValue != null && newValue as VirtualizingLayout != null;
            m_viewportManager.OnLayoutChanged(isVirtualizingLayout);
            InvalidateMeasure();
        }

        private void OnAnimatorChanged(ElementAnimator oldValue, ElementAnimator newValue)
        {
            AnimationManager.OnAnimatorChanged(newValue);
            //if (!SharedHelpers.IsRS5OrHigher())
            {
                // Bug in framework's reference tracking causes crash during
                // UIAffinityQueue cleanup. To avoid that bug, take a strong ref
                m_animator = newValue;
            }
        }

        private void OnItemsSourceViewChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            if (m_isLayoutInProgress)
            {
                // Bad things will follow if the data changes while we are in the middle of a layout pass.
                throw new InvalidOperationException("Changes in data source are not allowed during layout.");
            }

            if (IsProcessingCollectionChange)
            {
                throw new InvalidOperationException("Changes in the data source are not allowed during another change in the data source.");
            }

            m_processingItemsSourceChange = args;
            try
            {
                AnimationManager.OnItemsSourceChanged(sender, args);
                ViewManager.OnItemsSourceChanged(sender, args);

                if (Layout is Layout layout)
                {
                    if (layout is VirtualizingLayout virtualLayout)
                    {
                        ((IVirtualizingLayoutOverrides)virtualLayout).OnItemsChangedCore(GetLayoutContext(), sender, args);
                    }
                    else
                    {
                        // NonVirtualizingLayout
                        InvalidateMeasure();
                    }
                }
            }
            finally
            {
                m_processingItemsSourceChange = null;
            }
        }

        private void InvalidateMeasureForLayout(object sender, object args)
        {
            InvalidateMeasure();
        }

        private void InvalidateArrangeForLayout(object sender, object args)
        {
            InvalidateArrange();
        }

        private VirtualizingLayoutContext GetLayoutContext()
        {
            if (m_layoutContext == null)
            {
                m_layoutContext = new RepeaterLayoutContext(this);
            }
            return m_layoutContext;
        }

        private bool IsProcessingCollectionChange => m_processingItemsSourceChange != null;

        private readonly ViewportManager m_viewportManager;

        private IElementFactoryShim m_itemTemplateWrapper;

        private VirtualizingLayoutContext m_layoutContext;
        // Value is different from null only while we are on the OnItemsSourceChanged call stack.
        private NotifyCollectionChangedEventArgs m_processingItemsSourceChange;

        private Size m_lastAvailableSize;
        private bool m_isLayoutInProgress = false;
        // The value of _layoutOrigin is expected to be set by the layout
        // when it gets measured. It should not be used outside of measure.
        private Point m_layoutOrigin;

        // Cached Event args to avoid creation cost every time
        private ItemsRepeaterElementPreparedEventArgs m_elementPreparedArgs;
        private ItemsRepeaterElementClearingEventArgs m_elementClearingArgs;
        private ItemsRepeaterElementIndexChangedEventArgs m_elementIndexChangedArgs;

        // Loaded events fire on the first tick after an element is put into the tree 
        // while unloaded is posted on the UI tree and may be processed out of sync with subsequent loaded
        // events. We keep these counters to detect out-of-sync unloaded events and take action to rectify.
        private int _loadedCounter;
        private int _unloadedCounter;

        // Bug in framework's reference tracking causes crash during
        // UIAffinityQueue cleanup. To avoid that bug, take a strong ref
        private object m_itemTemplate;
        private Layout m_layout;
        private ElementAnimator m_animator;

        // Bug where DataTemplate with no content causes a crash.
        // See: https://github.com/microsoft/microsoft-ui-xaml/issues/776
        // Solution: Have flag that is only true when DataTemplate exists but it is empty.
        private bool m_isItemTemplateEmpty = false;
    }
}
