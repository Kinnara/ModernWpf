using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace ModernWpf.Controls
{
    /// <summary>
    /// Represents the base class for an icon UI element.
    /// </summary>
    public abstract class IconElement : FrameworkElement
    {
        internal IconElement()
        {
        }

        /// <summary>
        /// Identifies the Foreground dependency property.
        /// </summary>
        public static readonly DependencyProperty ForegroundProperty =
                TextElement.ForegroundProperty.AddOwner(
                        typeof(IconElement),
                        new FrameworkPropertyMetadata(SystemColors.ControlTextBrush,
                            FrameworkPropertyMetadataOptions.Inherits));

        /// <summary>
        /// Gets or sets a brush that describes the foreground color.
        /// </summary>
        /// <returns>
        /// The brush that paints the foreground of the control.
        /// </returns>
        [Bindable(true), Category("Appearance")]
        public Brush Foreground
        {
            get { return (Brush)GetValue(ForegroundProperty); }
            set { SetValue(ForegroundProperty, value); }
        }

        protected override int VisualChildrenCount => 1;

        protected override Visual GetVisualChild(int index)
        {
            if (index == 0)
            {
                EnsureLayoutRoot();
                return _layoutRoot;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            EnsureLayoutRoot();
            _layoutRoot.Measure(availableSize);
            return _layoutRoot.DesiredSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            EnsureLayoutRoot();
            _layoutRoot.Arrange(new Rect(new Point(), finalSize));
            return finalSize;
        }

        private void EnsureLayoutRoot()
        {
            if (_layoutRoot != null)
                return;

            _layoutRoot = new Grid
            {
                SnapsToDevicePixels = true,
                Children = { CreateIcon() }
            };

            AddVisualChild(_layoutRoot);
        }

        private Grid _layoutRoot;

        internal abstract UIElement CreateIcon();
    }
}
