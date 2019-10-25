using System.Collections;
using System.Windows;
using System.Windows.Media;

namespace ModernWpf.Controls
{
    public abstract class ItemsRepeaterPanel : FrameworkElement
    {
        protected ItemsRepeaterPanel()
        {
            Children = new ItemsRepeaterChildCollection(this, this);
        }

        public Brush Background
        {
            get => (Brush)GetValue(BackgroundProperty);
            set => SetValue(BackgroundProperty, value);
        }

        public static readonly DependencyProperty BackgroundProperty =
            DependencyProperty.Register("Background",
                typeof(Brush),
                typeof(ItemsRepeaterPanel),
                new FrameworkPropertyMetadata(null,
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender));

        protected override void OnRender(DrawingContext dc)
        {
            Brush background = Background;
            if (background != null)
            {
                // Using the Background brush, draw a rectangle that fills the
                // render bounds of the panel.
                Size renderSize = RenderSize;
                dc.DrawRectangle(background,
                                 null,
                                 new Rect(0.0, 0.0, renderSize.Width, renderSize.Height));
            }
        }

        protected override IEnumerator LogicalChildren
        {
            get
            {
                if ((this.VisualChildrenCount == 0))
                {
                    // empty panel or a panel being used as the items
                    // host has *no* logical children; give empty enumerator
                    return EmptyEnumerator.Instance;
                }

                // otherwise, its logical children is its visual children
                return this.Children.GetEnumerator();
            }
        }

        protected override int VisualChildrenCount => Children.Count;

        protected internal ItemsRepeaterChildCollection Children { get; }

        protected override Visual GetVisualChild(int index)
        {
            return Children[index];
        }

        internal void AddLogicalChildInternal(object child)
        {
            AddLogicalChild(child);
        }

        internal void RemoveLogicalChildInternal(object child)
        {
            RemoveLogicalChild(child);
        }
    }
}
