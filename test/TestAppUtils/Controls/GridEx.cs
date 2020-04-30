using MS.Internal.Controls;
using System.Collections;
using System.Diagnostics;
using System.Windows.Media;

namespace System.Windows.Controls
{
    public class GridEx : Panel
    {
        public GridEx()
        {
            _itemsHost = new ItemsHost(this);
            _border = new Border { Child = _itemsHost };
            AddVisualChild(_border);
        }

        #region BorderBrush

        public static readonly DependencyProperty BorderBrushProperty
                = Border.BorderBrushProperty.AddOwner(typeof(GridEx),
                    new FrameworkPropertyMetadata(
                        Border.BorderBrushProperty.DefaultMetadata.DefaultValue,
                        FrameworkPropertyMetadataOptions.None,
                        OnBorderBrushPropertyChanged));

        public Brush BorderBrush
        {
            get => (Brush)GetValue(BorderBrushProperty);
            set => SetValue(BorderBrushProperty, value);
        }

        private static void OnBorderBrushPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((GridEx)sender).OnBorderBrushPropertyChanged(args);
        }

        private void OnBorderBrushPropertyChanged(DependencyPropertyChangedEventArgs args)
        {
            _border.BorderBrush = (Brush)args.NewValue;
        }

        #endregion

        #region BorderThickness

        public static readonly DependencyProperty BorderThicknessProperty
                = Border.BorderThicknessProperty.AddOwner(typeof(GridEx),
                    new FrameworkPropertyMetadata(
                        Border.BorderThicknessProperty.DefaultMetadata.DefaultValue,
                        FrameworkPropertyMetadataOptions.None,
                        OnBorderThicknessPropertyChanged));

        public Thickness BorderThickness
        {
            get => (Thickness)GetValue(BorderThicknessProperty);
            set => SetValue(BorderThicknessProperty, value);
        }

        private static void OnBorderThicknessPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((GridEx)sender).OnBorderThicknessPropertyChanged(args);
        }

        private void OnBorderThicknessPropertyChanged(DependencyPropertyChangedEventArgs args)
        {
            _border.BorderThickness = (Thickness)args.NewValue;
        }

        #endregion

        #region Padding

        public static readonly DependencyProperty PaddingProperty
                = Border.PaddingProperty.AddOwner(typeof(GridEx),
                    new FrameworkPropertyMetadata(
                        Border.PaddingProperty.DefaultMetadata.DefaultValue,
                        FrameworkPropertyMetadataOptions.None,
                        OnPaddingPropertyChanged));

        public Thickness Padding
        {
            get => (Thickness)GetValue(PaddingProperty);
            set => SetValue(PaddingProperty, value);
        }

        private static void OnPaddingPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((GridEx)sender).OnPaddingPropertyChanged(args);
        }

        private void OnPaddingPropertyChanged(DependencyPropertyChangedEventArgs args)
        {
            _border.Padding = (Thickness)args.NewValue;
        }

        #endregion

        public ColumnDefinitionCollection ColumnDefinitions => _itemsHost.ColumnDefinitions;

        public RowDefinitionCollection RowDefinitions => _itemsHost.RowDefinitions;

        protected override IEnumerator LogicalChildren
        {
            get
            {
                // empty panel or a panel being used as the items
                // host has *no* logical children; give empty enumerator
                bool noChildren = (base.VisualChildrenCount == 0) || IsItemsHost;

                if (noChildren)
                {
                    if (ColumnDefinitions.Count == 0 || RowDefinitions.Count == 0)
                    {
                        //  grid is empty
                        return EmptyEnumerator.Instance;
                    }
                }

                return (new GridChildrenCollectionEnumeratorSimple(this, !noChildren));
            }
        }

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

        protected override UIElementCollection CreateUIElementCollection(FrameworkElement logicalParent)
        {
            return _itemsHost.Children;
        }

        private readonly Border _border;
        private readonly ItemsHost _itemsHost;

        private class ItemsHost : Grid
        {
            public ItemsHost(GridEx owner)
            {
                _owner = owner;
            }

            protected override UIElementCollection CreateUIElementCollection(FrameworkElement logicalParent)
            {
                return new UIElementCollection(this, _owner);
            }

            private readonly GridEx _owner;
        }

        /// <summary>
        /// Implementation of a simple enumerator of grid's logical children
        /// </summary>
        private class GridChildrenCollectionEnumeratorSimple : IEnumerator
        {
            internal GridChildrenCollectionEnumeratorSimple(GridEx grid, bool includeChildren)
            {
                Debug.Assert(grid != null);
                _currentEnumerator = -1;
                _enumerator0 = ((IEnumerable)grid.ColumnDefinitions).GetEnumerator();
                _enumerator1 = ((IEnumerable)grid.RowDefinitions).GetEnumerator();
                // GridLineRenderer is NOT included into this enumerator.
                _enumerator2Index = 0;
                if (includeChildren)
                {
                    _enumerator2Collection = grid.Children;
                    _enumerator2Count = _enumerator2Collection.Count;
                }
                else
                {
                    _enumerator2Collection = null;
                    _enumerator2Count = 0;
                }
            }

            public bool MoveNext()
            {
                while (_currentEnumerator < 3)
                {
                    if (_currentEnumerator >= 0)
                    {
                        switch (_currentEnumerator)
                        {
                            case (0): if (_enumerator0.MoveNext()) { _currentChild = _enumerator0.Current; return (true); } break;
                            case (1): if (_enumerator1.MoveNext()) { _currentChild = _enumerator1.Current; return (true); } break;
                            case (2):
                                if (_enumerator2Index < _enumerator2Count)
                                {
                                    _currentChild = _enumerator2Collection[_enumerator2Index];
                                    _enumerator2Index++;
                                    return (true);
                                }
                                break;
                        }
                    }
                    _currentEnumerator++;
                }
                return (false);
            }

            public Object Current
            {
                get
                {
                    if (_currentEnumerator == -1)
                    {
#pragma warning suppress 6503 // IEnumerator.Current is documented to throw this exception
                        throw new InvalidOperationException(EnumeratorNotStarted);
                    }
                    if (_currentEnumerator >= 3)
                    {
#pragma warning suppress 6503 // IEnumerator.Current is documented to throw this exception
                        throw new InvalidOperationException(EnumeratorNotStarted);
                    }

                    //  assert below is not true anymore since UIElementCollection allowes for null children
                    //Debug.Assert(_currentChild != null);
                    return (_currentChild);
                }
            }

            public void Reset()
            {
                _currentEnumerator = -1;
                _currentChild = null;
                _enumerator0.Reset();
                _enumerator1.Reset();
                _enumerator2Index = 0;
            }

            private int _currentEnumerator;
            private Object _currentChild;
            private IEnumerator _enumerator0;
            private IEnumerator _enumerator1;
            private UIElementCollection _enumerator2Collection;
            private int _enumerator2Index;
            private int _enumerator2Count;

            private const string EnumeratorNotStarted = "Enumeration has not started. Call MoveNext.";
        }
    }
}
