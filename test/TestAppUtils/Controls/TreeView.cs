using ModernWpf.Controls.Primitives;
using System;
using System.Windows;
using System.Windows.Controls;

namespace ModernWpf.Controls
{
    public class TreeView : System.Windows.Controls.TreeView
    {
        #region CornerRadius

        public static readonly DependencyProperty CornerRadiusProperty = ControlHelper.CornerRadiusProperty.AddOwner(typeof(TreeView));

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        #endregion

        public ItemCollection RootNodes => Items;

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            this.InitializeStyle(DefaultStyleKey);
        }
    }

    public class TreeViewNode : TreeViewItem
    {
        #region Content

        public static readonly DependencyProperty ContentProperty =
            DependencyProperty.Register(
                nameof(Content),
                typeof(object),
                typeof(TreeViewNode),
                new PropertyMetadata(null, OnContentPropertyChanged));

        public object Content
        {
            get => GetValue(ContentProperty);
            set => SetValue(ContentProperty, value);
        }

        private static void OnContentPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((TreeViewNode)sender).OnContentPropertyChanged(args);
        }

        private void OnContentPropertyChanged(DependencyPropertyChangedEventArgs args)
        {
            Header = args.NewValue;
        }

        #endregion

        public ItemCollection Children => Items;

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            this.InitializeStyle(DefaultStyleKey);
        }
    }
}
