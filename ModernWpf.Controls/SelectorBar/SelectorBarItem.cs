using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Input;
using ModernWpf.Automation.Peers;
using ModernWpf.Controls.Primitives;

namespace ModernWpf.Controls
{
    [TemplatePart(Name = ButtonName, Type = typeof(Button))]
    public class SelectorBarItem : ContentControl
    {
        private const string ButtonName = "PART_Button";

        static SelectorBarItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SelectorBarItem), new FrameworkPropertyMetadata(typeof(SelectorBarItem)));
        }

        #region BackgroundSizing

        public static readonly DependencyProperty BackgroundSizingProperty =
            ControlHelper.BackgroundSizingProperty.AddOwner(
                typeof(SelectorBarItem),
                new FrameworkPropertyMetadata(BackgroundSizing.OuterBorderEdge));

        public BackgroundSizing BackgroundSizing
        {
            get => (BackgroundSizing)GetValue(BackgroundSizingProperty);
            set => SetValue(BackgroundSizingProperty, value);
        }

        #endregion

        #region CornerRadius

        public static readonly DependencyProperty CornerRadiusProperty =
            ControlHelper.CornerRadiusProperty.AddOwner(typeof(SelectorBarItem));

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        #endregion

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(
                nameof(Text),
                typeof(string),
                typeof(SelectorBarItem),
                new FrameworkPropertyMetadata(string.Empty, OnVisualPropertyChanged));

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register(
                nameof(Icon),
                typeof(IconElement),
                typeof(SelectorBarItem),
                new FrameworkPropertyMetadata(null, OnVisualPropertyChanged));

        public IconElement Icon
        {
            get => (IconElement)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        public static readonly DependencyProperty ChildProperty =
            DependencyProperty.Register(
                nameof(Child),
                typeof(UIElement),
                typeof(SelectorBarItem),
                new FrameworkPropertyMetadata(null, OnVisualPropertyChanged));

        public UIElement Child
        {
            get => (UIElement)GetValue(ChildProperty);
            set => SetValue(ChildProperty, value);
        }

        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register(
                nameof(IsSelected),
                typeof(bool),
                typeof(SelectorBarItem),
                new FrameworkPropertyMetadata(false, OnIsSelectedPropertyChanged));

        public bool IsSelected
        {
            get => (bool)GetValue(IsSelectedProperty);
            set => SetValue(IsSelectedProperty, value);
        }

        public override void OnApplyTemplate()
        {
            if (_button != null)
            {
                _button.Click -= OnButtonClick;
            }

            base.OnApplyTemplate();

            _button = GetTemplateChild(ButtonName) as Button;
            if (_button != null)
            {
                _button.Click += OnButtonClick;
            }

            UpdateVisualState();
        }

        internal SelectorBar Owner { get; set; }

        internal void Select()
        {
            Owner?.SelectItem(this);
        }

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new SelectorBarItemAutomationPeer(this);
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);

            if (e.Handled)
            {
                return;
            }

            if (e.Key == Key.Enter || e.Key == Key.Space)
            {
                Select();
                e.Handled = true;
                return;
            }

            if ((e.Key == Key.Left || e.Key == Key.Right) && Owner?.MoveFocusFrom(this, e.Key) == true)
            {
                e.Handled = true;
            }
        }

        private static void OnIsSelectedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var item = (SelectorBarItem)d;
            item.Owner?.OnItemIsSelectedChanged(item, (bool)e.NewValue);
            item.UpdateVisualState();
        }

        private static void OnVisualPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((SelectorBarItem)d).UpdateVisualState();
        }

        private void OnButtonClick(object sender, RoutedEventArgs e)
        {
            Select();
        }

        private void UpdateVisualState()
        {
            if (_button != null)
            {
                _button.FontWeight = IsSelected ? FontWeights.SemiBold : FontWeights.Normal;
            }
        }

        private Button _button;
    }
}
