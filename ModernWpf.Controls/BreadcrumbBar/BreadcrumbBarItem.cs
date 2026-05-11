using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Input;
using ModernWpf.Automation.Peers;

namespace ModernWpf.Controls
{
    [TemplatePart(Name = ItemButtonName, Type = typeof(Button))]
    [TemplatePart(Name = ChevronTextBlockName, Type = typeof(TextBlock))]
    public class BreadcrumbBarItem : ContentControl
    {
        private const string ItemButtonName = "PART_ItemButton";
        private const string ChevronTextBlockName = "PART_ChevronTextBlock";

        static BreadcrumbBarItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BreadcrumbBarItem), new FrameworkPropertyMetadata(typeof(BreadcrumbBarItem)));
        }

        public override void OnApplyTemplate()
        {
            if (_button != null)
            {
                _button.Click -= OnButtonClick;
            }

            base.OnApplyTemplate();

            _button = GetTemplateChild(ItemButtonName) as Button;
            _chevronTextBlock = GetTemplateChild(ChevronTextBlockName) as TextBlock;

            if (_button != null)
            {
                _button.Click += OnButtonClick;
            }

            UpdateVisualState();
        }

        internal BreadcrumbBar Owner { get; set; }

        internal int Index { get; set; }

        internal object SourceItem { get; set; }

        internal bool IsCurrentItem
        {
            get => _isCurrentItem;
            set
            {
                if (_isCurrentItem != value)
                {
                    _isCurrentItem = value;
                    UpdateVisualState();
                }
            }
        }

        internal void Invoke()
        {
            if (!IsCurrentItem)
            {
                Owner?.RaiseItemClicked(SourceItem ?? Content, Index);
            }
        }

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new BreadcrumbBarItemAutomationPeer(this);
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
                Invoke();
                e.Handled = true;
                return;
            }

            if ((e.Key == Key.Left || e.Key == Key.Right) && Owner?.MoveFocusFrom(this, e.Key) == true)
            {
                e.Handled = true;
            }
        }

        private void OnButtonClick(object sender, RoutedEventArgs e)
        {
            Invoke();
        }

        private void UpdateVisualState()
        {
            if (_button != null)
            {
                _button.FontWeight = IsCurrentItem ? FontWeights.SemiBold : FontWeights.Normal;
            }

            if (_chevronTextBlock != null)
            {
                _chevronTextBlock.Visibility = IsCurrentItem ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        private Button _button;
        private TextBlock _chevronTextBlock;
        private bool _isCurrentItem;
    }
}
