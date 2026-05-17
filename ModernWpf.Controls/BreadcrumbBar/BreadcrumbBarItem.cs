using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using ModernWpf.Automation.Peers;
using ModernWpf.Controls.Primitives;
using ModernWpf.Media.Animation;

namespace ModernWpf.Controls
{
    [TemplatePart(Name = ItemButtonName, Type = typeof(Button))]
    [TemplatePart(Name = ChevronTextBlockName, Type = typeof(TextBlock))]
    public class BreadcrumbBarItem : ContentControl
    {
        private const string ItemButtonName = "PART_ItemButton";
        private const string ChevronTextBlockName = "PART_ChevronTextBlock";
        private const string EllipsisFlyoutResourceKey = "PART_EllipsisFlyout";
        private const string EllipsisItemsRepeaterName = "PART_EllipsisItemsRepeater";

        static BreadcrumbBarItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BreadcrumbBarItem), new FrameworkPropertyMetadata(typeof(BreadcrumbBarItem)));
            IsEnabledProperty.OverrideMetadata(typeof(BreadcrumbBarItem), new FrameworkPropertyMetadata(OnIsEnabledChanged));
        }

        #region ContentTransitions

        public static readonly DependencyProperty ContentTransitionsProperty =
            ControlHelper.ContentTransitionsProperty.AddOwner(typeof(BreadcrumbBarItem));

        public TransitionCollection ContentTransitions
        {
            get => (TransitionCollection)GetValue(ContentTransitionsProperty);
            set => SetValue(ContentTransitionsProperty, value);
        }

        #endregion

        #region CornerRadius

        public static readonly DependencyProperty CornerRadiusProperty =
            ControlHelper.CornerRadiusProperty.AddOwner(typeof(BreadcrumbBarItem));

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        #endregion

        #region FocusVisualMargin

        public static readonly DependencyProperty FocusVisualMarginProperty =
            FocusVisualHelper.FocusVisualMarginProperty.AddOwner(typeof(BreadcrumbBarItem));

        public Thickness FocusVisualMargin
        {
            get => (Thickness)GetValue(FocusVisualMarginProperty);
            set => SetValue(FocusVisualMarginProperty, value);
        }

        #endregion

        #region UseSystemFocusVisuals

        public static readonly DependencyProperty UseSystemFocusVisualsProperty =
            FocusVisualHelper.UseSystemFocusVisualsProperty.AddOwner(typeof(BreadcrumbBarItem));

        public bool UseSystemFocusVisuals
        {
            get => (bool)GetValue(UseSystemFocusVisualsProperty);
            set => SetValue(UseSystemFocusVisualsProperty, value);
        }

        #endregion

        public override void OnApplyTemplate()
        {
            RevokeButtonListeners();

            base.OnApplyTemplate();

            _button = GetTemplateChild(ItemButtonName) as Button;
            _chevronTextBlock = GetTemplateChild(ChevronTextBlockName) as TextBlock;

            if (_button != null)
            {
                _button.Click += OnButtonClick;
                _button.IsEnabledChanged += OnButtonIsEnabledChanged;
                _button.MouseEnter += OnButtonVisualPropertyChanged;
                _button.MouseLeave += OnButtonVisualPropertyChanged;
                _button.PreviewMouseLeftButtonDown += OnButtonVisualPropertyChanged;
                _button.PreviewMouseLeftButtonUp += OnButtonVisualPropertyChanged;
                _button.GotKeyboardFocus += OnButtonVisualPropertyChanged;
                _button.LostKeyboardFocus += OnButtonVisualPropertyChanged;
            }

            if (_isEllipsisItem)
            {
                SetPropertiesForEllipsisItem();
            }
            else if (_isLastItem)
            {
                SetPropertiesForLastItem();
            }
            else
            {
                ResetVisualProperties();
            }

            UpdateItemTypeVisualState(false);
        }

        internal BreadcrumbBar Owner { get; private set; }

        internal int Index { get; private set; }

        internal bool IsCurrentItem
        {
            get => _isLastItem;
            set
            {
                if (value)
                {
                    SetPropertiesForLastItem();
                }
                else
                {
                    ResetVisualProperties();
                }
            }
        }

        internal bool IsEllipsisDropDownItem => _isEllipsisDropDownItem;

        internal void SetParentBreadcrumb(BreadcrumbBar parent)
        {
            Owner = parent;
        }

        internal void SetIndex(int index)
        {
            Index = index;
        }

        internal void SetIsEllipsisDropDownItem(bool isEllipsisDropDownItem)
        {
            _isEllipsisDropDownItem = isEllipsisDropDownItem;
            UpdateItemTypeVisualState(false);
        }

        internal void SetEllipsisItem(BreadcrumbBarItem ellipsisItem)
        {
            _ellipsisItem = ellipsisItem;
        }

        internal void SetEllipsisDropDownItemDataTemplate(DataTemplate newDataTemplate)
        {
            _ellipsisDropDownItemDataTemplate = newDataTemplate;

            if (_ellipsisElementFactory != null)
            {
                _ellipsisElementFactory.UserElementFactory(newDataTemplate);
            }
        }

        internal void SetPropertiesForLastItem()
        {
            _isEllipsisItem = false;
            _isLastItem = true;

            UpdateButtonCommonVisualState(false);
            UpdateInlineItemTypeVisualState(false);
        }

        internal void SetPropertiesForEllipsisItem()
        {
            _isEllipsisItem = true;
            _isLastItem = false;

            InstantiateFlyout();

            UpdateButtonCommonVisualState(false);
            UpdateInlineItemTypeVisualState(false);
        }

        internal void ResetVisualProperties()
        {
            if (_isEllipsisDropDownItem)
            {
                _isPressed = false;
                _isPointerOver = false;
                UpdateEllipsisDropDownItemCommonVisualState(false);
                return;
            }

            _isEllipsisItem = false;
            _isLastItem = false;
            _isPressed = false;
            _isPointerOver = false;

            UpdateButtonCommonVisualState(false);
            UpdateInlineItemTypeVisualState(false);
            UpdateItemTypeVisualState(false);
        }

        internal void Invoke()
        {
            OnClickEvent();
        }

        internal void CloseFlyout()
        {
            _ellipsisFlyout?.Hide();
        }

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new BreadcrumbBarItemAutomationPeer(this);
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);

            if (_isEllipsisDropDownItem)
            {
                _isPointerOver = true;
                UpdateEllipsisDropDownItemCommonVisualState(true);
            }
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);

            if (_isEllipsisDropDownItem)
            {
                _isPointerOver = false;
                if (!_isPressed)
                {
                    UpdateEllipsisDropDownItemCommonVisualState(true);
                }
            }
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            if (_isEllipsisDropDownItem && IsEnabled && !e.Handled)
            {
                Focus();
                _isPressed = true;
                CaptureMouse();
                UpdateEllipsisDropDownItemCommonVisualState(true);
                e.Handled = true;
            }
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);

            if (_isEllipsisDropDownItem && _isPressed)
            {
                var shouldClick = IsMouseOver || IsMouseCaptured;
                _isPressed = false;
                if (IsMouseCaptured)
                {
                    ReleaseMouseCapture();
                }

                UpdateEllipsisDropDownItemCommonVisualState(true);
                if (shouldClick)
                {
                    OnClickEvent();
                }

                e.Handled = true;
            }
        }

        protected override void OnLostMouseCapture(MouseEventArgs e)
        {
            base.OnLostMouseCapture(e);

            if (_isEllipsisDropDownItem && _isPressed)
            {
                _isPressed = false;
                UpdateEllipsisDropDownItemCommonVisualState(true);
            }
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
                OnClickEvent();
                e.Handled = true;
                return;
            }

            if (!_isEllipsisDropDownItem &&
                (e.Key == Key.Left || e.Key == Key.Right) &&
                Owner?.MoveFocusFrom(this, e.Key) == true)
            {
                e.Handled = true;
            }
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.Property == FlowDirectionProperty && !_isEllipsisDropDownItem)
            {
                UpdateInlineItemTypeVisualState(true);
            }
        }

        private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var item = (BreadcrumbBarItem)d;
            item._isPressed = false;

            if (item._isEllipsisDropDownItem)
            {
                item.UpdateEllipsisDropDownItemCommonVisualState(true);
            }
            else
            {
                item.UpdateButtonCommonVisualState(true);
            }
        }

        private void RevokeButtonListeners()
        {
            if (_button == null)
            {
                return;
            }

            _button.Click -= OnButtonClick;
            _button.IsEnabledChanged -= OnButtonIsEnabledChanged;
            _button.MouseEnter -= OnButtonVisualPropertyChanged;
            _button.MouseLeave -= OnButtonVisualPropertyChanged;
            _button.PreviewMouseLeftButtonDown -= OnButtonVisualPropertyChanged;
            _button.PreviewMouseLeftButtonUp -= OnButtonVisualPropertyChanged;
            _button.GotKeyboardFocus -= OnButtonVisualPropertyChanged;
            _button.LostKeyboardFocus -= OnButtonVisualPropertyChanged;
        }

        private void OnButtonClick(object sender, RoutedEventArgs e)
        {
            OnClickEvent();
        }

        private void OnButtonVisualPropertyChanged(object sender, RoutedEventArgs e)
        {
            UpdateButtonCommonVisualState(true);
        }

        private void OnButtonIsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            UpdateButtonCommonVisualState(true);
        }

        private void OnClickEvent()
        {
            if (_isEllipsisDropDownItem)
            {
                if (_ellipsisItem != null)
                {
                    _ellipsisItem.CloseFlyout();
                    _ellipsisItem.RaiseItemClickedEvent(Content, Index - 1);
                }
            }
            else if (_isEllipsisItem)
            {
                OnEllipsisItemClick();
            }
            else if (!_isLastItem)
            {
                RaiseItemClickedEvent(Content, Index - 1);
            }
        }

        private void RaiseItemClickedEvent(object content, int index)
        {
            Owner?.RaiseItemClickedEvent(content, index);
        }

        private void OnEllipsisItemClick()
        {
            if (Owner == null)
            {
                return;
            }

            var hiddenElements = CloneEllipsisItemSource(Owner.HiddenElements());
            InstantiateFlyout();

            if (_ellipsisItemsRepeater != null)
            {
                _ellipsisItemsRepeater.ItemsSource = hiddenElements;
            }

            OpenFlyout();
        }

        private List<object> CloneEllipsisItemSource(IReadOnlyList<object> ellipsisItemsSource)
        {
            var newItemsSource = new List<object>();

            for (var i = ellipsisItemsSource.Count - 1; i >= 0; i--)
            {
                newItemsSource.Add(ellipsisItemsSource[i]);
            }

            return newItemsSource;
        }

        private void InstantiateFlyout()
        {
            if (_button == null)
            {
                return;
            }

            if (_ellipsisFlyout == null)
            {
                _ellipsisFlyout = TryFindResource(EllipsisFlyoutResourceKey) as Flyout
                    ?? new Flyout { Placement = FlyoutPlacementMode.Bottom };
                AutomationProperties.SetName(_ellipsisFlyout, "EllipsisFlyout");
            }

            if (_ellipsisItemsRepeater == null)
            {
                _ellipsisItemsRepeater = new ItemsRepeater
                {
                    Name = EllipsisItemsRepeaterName,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Layout = new StackLayout()
                };

                AutomationProperties.SetName(_ellipsisItemsRepeater, "EllipsisItemsRepeater");

                _ellipsisElementFactory = new BreadcrumbElementFactory();
                _ellipsisElementFactory.UserElementFactory(_ellipsisDropDownItemDataTemplate);
                _ellipsisItemsRepeater.ItemTemplate = _ellipsisElementFactory;
                _ellipsisItemsRepeater.ElementPrepared += OnFlyoutElementPrepared;
                _ellipsisItemsRepeater.ElementIndexChanged += OnFlyoutElementIndexChanged;
            }

            _ellipsisFlyout.Content = _ellipsisItemsRepeater;
            _ellipsisFlyout.Placement = FlyoutPlacementMode.Bottom;
        }

        private void OpenFlyout()
        {
            _ellipsisFlyout?.ShowAt(this, new FlyoutShowOptions { Placement = FlyoutPlacementMode.Bottom });
        }

        private void OnFlyoutElementPrepared(ItemsRepeater sender, ItemsRepeaterElementPreparedEventArgs args)
        {
            if (args.Element is BreadcrumbBarItem ellipsisDropDownItem)
            {
                ellipsisDropDownItem.SetIsEllipsisDropDownItem(true);
                UpdateFlyoutIndex(ellipsisDropDownItem, args.Index);
            }
        }

        private void OnFlyoutElementIndexChanged(ItemsRepeater sender, ItemsRepeaterElementIndexChangedEventArgs args)
        {
            UpdateFlyoutIndex(args.Element, args.NewIndex);
        }

        private void UpdateFlyoutIndex(UIElement element, int index)
        {
            if (_ellipsisItemsRepeater?.ItemsSourceView == null)
            {
                return;
            }

            var itemCount = _ellipsisItemsRepeater.ItemsSourceView.Count;

            if (element is BreadcrumbBarItem ellipsisDropDownItem)
            {
                ellipsisDropDownItem.SetParentBreadcrumb(Owner);
                ellipsisDropDownItem.SetEllipsisItem(this);
                ellipsisDropDownItem.SetIndex(itemCount - index);
            }

#if NET48_OR_NEWER
            AutomationProperties.SetPositionInSet(element, index + 1);
            AutomationProperties.SetSizeOfSet(element, itemCount);
#endif
        }

        private void UpdateItemTypeVisualState(bool useTransitions)
        {
            VisualStateManager.GoToState(this, _isEllipsisDropDownItem ? "EllipsisDropDown" : "Inline", useTransitions);
        }

        private void UpdateEllipsisDropDownItemCommonVisualState(bool useTransitions)
        {
            string stateName;

            if (!IsEnabled)
            {
                stateName = "Disabled";
            }
            else if (_isPressed)
            {
                stateName = "Pressed";
            }
            else if (_isPointerOver)
            {
                stateName = "PointerOver";
            }
            else
            {
                stateName = "Normal";
            }

            VisualStateManager.GoToState(this, stateName, useTransitions);
        }

        private void UpdateInlineItemTypeVisualState(bool useTransitions)
        {
            if (_isEllipsisDropDownItem)
            {
                return;
            }

            string visualStateName;
            var isLeftToRight = FlowDirection == FlowDirection.LeftToRight;

            if (_isEllipsisItem)
            {
                visualStateName = isLeftToRight ? "Ellipsis" : "EllipsisRTL";
            }
            else if (_isLastItem)
            {
                visualStateName = "LastItem";
            }
            else
            {
                visualStateName = isLeftToRight ? "Default" : "DefaultRTL";
            }

            VisualStateManager.GoToState(this, visualStateName, useTransitions);
        }

        private void UpdateButtonCommonVisualState(bool useTransitions)
        {
            if (_button == null)
            {
                return;
            }

            var commonVisualStateName = _isLastItem ? "Current" : string.Empty;

            if (!_button.IsEnabled)
            {
                commonVisualStateName += "Disabled";
            }
            else if (_button.IsPressed)
            {
                commonVisualStateName += "Pressed";
            }
            else if (_button.IsMouseOver)
            {
                commonVisualStateName += "PointerOver";
            }
            else if (_button.IsKeyboardFocused)
            {
                commonVisualStateName += "Focus";
            }
            else
            {
                commonVisualStateName += "Normal";
            }

            VisualStateManager.GoToState(_button, commonVisualStateName, useTransitions);
        }

        private Button _button;
        private TextBlock _chevronTextBlock;
        private Flyout _ellipsisFlyout;
        private ItemsRepeater _ellipsisItemsRepeater;
        private BreadcrumbElementFactory _ellipsisElementFactory;
        private DataTemplate _ellipsisDropDownItemDataTemplate;
        private BreadcrumbBarItem _ellipsisItem;
        private bool _isEllipsisDropDownItem;
        private bool _isEllipsisItem;
        private bool _isLastItem;
        private bool _isPressed;
        private bool _isPointerOver;
    }
}
