using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using ModernWpf.Controls.Primitives;

namespace ModernWpf.Controls
{
    public class TeachingTip : ContentControl
    {
        private const string ContainerName = "Container";
        private const string ContentRootGridName = "ContentRootGrid";
        private const string HeroContentBorderName = "HeroContentBorder";
        private const string MainContentPresenterName = "MainContentPresenter";
        private const string TitleTextBlockName = "TitleTextBlock";
        private const string SubtitleTextBlockName = "SubtitleTextBlock";
        private const string IconPresenterName = "IconPresenter";
        private const string ActionButtonName = "ActionButton";
        private const string CloseButtonName = "CloseButton";
        private const string AlternateCloseButtonName = "AlternateCloseButton";
        private const string TailPolygonName = "TailPolygon";

        static TeachingTip()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TeachingTip), new FrameworkPropertyMetadata(typeof(TeachingTip)));
        }

        public TeachingTip()
        {
            SetValue(TemplateSettingsPropertyKey, new TeachingTipTemplateSettings());
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(
                nameof(Title),
                typeof(string),
                typeof(TeachingTip),
                new PropertyMetadata(string.Empty, OnVisualStatePropertyChanged));

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public static readonly DependencyProperty SubtitleProperty =
            DependencyProperty.Register(
                nameof(Subtitle),
                typeof(string),
                typeof(TeachingTip),
                new PropertyMetadata(string.Empty, OnVisualStatePropertyChanged));

        public string Subtitle
        {
            get => (string)GetValue(SubtitleProperty);
            set => SetValue(SubtitleProperty, value);
        }

        public static readonly DependencyProperty IsOpenProperty =
            DependencyProperty.Register(
                nameof(IsOpen),
                typeof(bool),
                typeof(TeachingTip),
                new PropertyMetadata(false, OnIsOpenPropertyChanged));

        public bool IsOpen
        {
            get => (bool)GetValue(IsOpenProperty);
            set => SetValue(IsOpenProperty, value);
        }

        public static readonly DependencyProperty TargetProperty =
            DependencyProperty.Register(
                nameof(Target),
                typeof(FrameworkElement),
                typeof(TeachingTip),
                new PropertyMetadata(null, OnVisualStatePropertyChanged));

        public FrameworkElement Target
        {
            get => (FrameworkElement)GetValue(TargetProperty);
            set => SetValue(TargetProperty, value);
        }

        public static readonly DependencyProperty TailVisibilityProperty =
            DependencyProperty.Register(
                nameof(TailVisibility),
                typeof(TeachingTipTailVisibility),
                typeof(TeachingTip),
                new PropertyMetadata(TeachingTipTailVisibility.Auto, OnVisualStatePropertyChanged));

        public TeachingTipTailVisibility TailVisibility
        {
            get => (TeachingTipTailVisibility)GetValue(TailVisibilityProperty);
            set => SetValue(TailVisibilityProperty, value);
        }

        public static readonly DependencyProperty ActionButtonContentProperty =
            DependencyProperty.Register(
                nameof(ActionButtonContent),
                typeof(object),
                typeof(TeachingTip),
                new PropertyMetadata(null, OnVisualStatePropertyChanged));

        public object ActionButtonContent
        {
            get => GetValue(ActionButtonContentProperty);
            set => SetValue(ActionButtonContentProperty, value);
        }

        public static readonly DependencyProperty ActionButtonStyleProperty =
            DependencyProperty.Register(
                nameof(ActionButtonStyle),
                typeof(Style),
                typeof(TeachingTip));

        public Style ActionButtonStyle
        {
            get => (Style)GetValue(ActionButtonStyleProperty);
            set => SetValue(ActionButtonStyleProperty, value);
        }

        public static readonly DependencyProperty ActionButtonCommandProperty =
            DependencyProperty.Register(
                nameof(ActionButtonCommand),
                typeof(ICommand),
                typeof(TeachingTip));

        public ICommand ActionButtonCommand
        {
            get => (ICommand)GetValue(ActionButtonCommandProperty);
            set => SetValue(ActionButtonCommandProperty, value);
        }

        public static readonly DependencyProperty ActionButtonCommandParameterProperty =
            DependencyProperty.Register(
                nameof(ActionButtonCommandParameter),
                typeof(object),
                typeof(TeachingTip));

        public object ActionButtonCommandParameter
        {
            get => GetValue(ActionButtonCommandParameterProperty);
            set => SetValue(ActionButtonCommandParameterProperty, value);
        }

        public static readonly DependencyProperty CloseButtonContentProperty =
            DependencyProperty.Register(
                nameof(CloseButtonContent),
                typeof(object),
                typeof(TeachingTip),
                new PropertyMetadata(null, OnVisualStatePropertyChanged));

        public object CloseButtonContent
        {
            get => GetValue(CloseButtonContentProperty);
            set => SetValue(CloseButtonContentProperty, value);
        }

        public static readonly DependencyProperty CloseButtonStyleProperty =
            DependencyProperty.Register(
                nameof(CloseButtonStyle),
                typeof(Style),
                typeof(TeachingTip));

        public Style CloseButtonStyle
        {
            get => (Style)GetValue(CloseButtonStyleProperty);
            set => SetValue(CloseButtonStyleProperty, value);
        }

        public static readonly DependencyProperty CloseButtonCommandProperty =
            DependencyProperty.Register(
                nameof(CloseButtonCommand),
                typeof(ICommand),
                typeof(TeachingTip));

        public ICommand CloseButtonCommand
        {
            get => (ICommand)GetValue(CloseButtonCommandProperty);
            set => SetValue(CloseButtonCommandProperty, value);
        }

        public static readonly DependencyProperty CloseButtonCommandParameterProperty =
            DependencyProperty.Register(
                nameof(CloseButtonCommandParameter),
                typeof(object),
                typeof(TeachingTip));

        public object CloseButtonCommandParameter
        {
            get => GetValue(CloseButtonCommandParameterProperty);
            set => SetValue(CloseButtonCommandParameterProperty, value);
        }

        public static readonly DependencyProperty PlacementMarginProperty =
            DependencyProperty.Register(
                nameof(PlacementMargin),
                typeof(Thickness),
                typeof(TeachingTip),
                new PropertyMetadata(new Thickness(), OnVisualStatePropertyChanged));

        public Thickness PlacementMargin
        {
            get => (Thickness)GetValue(PlacementMarginProperty);
            set => SetValue(PlacementMarginProperty, value);
        }

        public static readonly DependencyProperty ShouldConstrainToRootBoundsProperty =
            DependencyProperty.Register(
                nameof(ShouldConstrainToRootBounds),
                typeof(bool),
                typeof(TeachingTip),
                new PropertyMetadata(true));

        public bool ShouldConstrainToRootBounds
        {
            get => (bool)GetValue(ShouldConstrainToRootBoundsProperty);
            set => SetValue(ShouldConstrainToRootBoundsProperty, value);
        }

        public static readonly DependencyProperty IsLightDismissEnabledProperty =
            DependencyProperty.Register(
                nameof(IsLightDismissEnabled),
                typeof(bool),
                typeof(TeachingTip),
                new PropertyMetadata(false, OnVisualStatePropertyChanged));

        public bool IsLightDismissEnabled
        {
            get => (bool)GetValue(IsLightDismissEnabledProperty);
            set => SetValue(IsLightDismissEnabledProperty, value);
        }

        public static readonly DependencyProperty PreferredPlacementProperty =
            DependencyProperty.Register(
                nameof(PreferredPlacement),
                typeof(TeachingTipPlacementMode),
                typeof(TeachingTip),
                new PropertyMetadata(TeachingTipPlacementMode.Auto, OnVisualStatePropertyChanged));

        public TeachingTipPlacementMode PreferredPlacement
        {
            get => (TeachingTipPlacementMode)GetValue(PreferredPlacementProperty);
            set => SetValue(PreferredPlacementProperty, value);
        }

        public static readonly DependencyProperty HeroContentPlacementProperty =
            DependencyProperty.Register(
                nameof(HeroContentPlacement),
                typeof(TeachingTipHeroContentPlacementMode),
                typeof(TeachingTip),
                new PropertyMetadata(TeachingTipHeroContentPlacementMode.Auto, OnVisualStatePropertyChanged));

        public TeachingTipHeroContentPlacementMode HeroContentPlacement
        {
            get => (TeachingTipHeroContentPlacementMode)GetValue(HeroContentPlacementProperty);
            set => SetValue(HeroContentPlacementProperty, value);
        }

        public static readonly DependencyProperty HeroContentProperty =
            DependencyProperty.Register(
                nameof(HeroContent),
                typeof(UIElement),
                typeof(TeachingTip),
                new PropertyMetadata(null, OnVisualStatePropertyChanged));

        public UIElement HeroContent
        {
            get => (UIElement)GetValue(HeroContentProperty);
            set => SetValue(HeroContentProperty, value);
        }

        public static readonly DependencyProperty IconSourceProperty =
            DependencyProperty.Register(
                nameof(IconSource),
                typeof(IconSource),
                typeof(TeachingTip),
                new PropertyMetadata(null, OnIconSourcePropertyChanged));

        public IconSource IconSource
        {
            get => (IconSource)GetValue(IconSourceProperty);
            set => SetValue(IconSourceProperty, value);
        }

        public static readonly DependencyProperty CornerRadiusProperty =
            Border.CornerRadiusProperty.AddOwner(
                typeof(TeachingTip),
                new FrameworkPropertyMetadata(new CornerRadius()));

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        private static readonly DependencyPropertyKey TemplateSettingsPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(TemplateSettings),
                typeof(TeachingTipTemplateSettings),
                typeof(TeachingTip),
                new PropertyMetadata(null));

        public static readonly DependencyProperty TemplateSettingsProperty =
            TemplateSettingsPropertyKey.DependencyProperty;

        public TeachingTipTemplateSettings TemplateSettings =>
            (TeachingTipTemplateSettings)GetValue(TemplateSettingsProperty);

        public event TypedEventHandler<TeachingTip, object> ActionButtonClick;

        public event TypedEventHandler<TeachingTip, object> CloseButtonClick;

        public event TypedEventHandler<TeachingTip, TeachingTipClosingEventArgs> Closing;

        public event TypedEventHandler<TeachingTip, TeachingTipClosedEventArgs> Closed;

        public override void OnApplyTemplate()
        {
            UnhookButtonEvents();

            base.OnApplyTemplate();

            _container = GetTemplateChild(ContainerName) as FrameworkElement;
            _contentRootGrid = GetTemplateChild(ContentRootGridName) as Border;
            _heroContentBorder = GetTemplateChild(HeroContentBorderName) as Border;
            _mainContentPresenter = GetTemplateChild(MainContentPresenterName) as Border;
            _titleTextBlock = GetTemplateChild(TitleTextBlockName) as TextBlock;
            _subtitleTextBlock = GetTemplateChild(SubtitleTextBlockName) as TextBlock;
            _iconPresenter = GetTemplateChild(IconPresenterName) as FrameworkElement;
            _actionButton = GetTemplateChild(ActionButtonName) as Button;
            _closeButton = GetTemplateChild(CloseButtonName) as Button;
            _alternateCloseButton = GetTemplateChild(AlternateCloseButtonName) as Button;
            _tailPolygon = GetTemplateChild(TailPolygonName) as Polygon;

            HookButtonEvents();
            UpdateIcon();
            UpdateVisualState();
        }

        private static void OnIsOpenPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var teachingTip = (TeachingTip)d;
            if ((bool)e.NewValue)
            {
                teachingTip._lastCloseReason = TeachingTipCloseReason.Programmatic;
                teachingTip.UpdateVisualState();
            }
            else if ((bool)e.OldValue)
            {
                if (teachingTip._isCompletingClose)
                {
                    teachingTip.UpdateVisualState();
                    teachingTip.Closed?.Invoke(teachingTip, new TeachingTipClosedEventArgs(teachingTip._lastCloseReason));
                }
                else
                {
                    teachingTip.BeginClose(teachingTip._lastCloseReason);
                }
            }
            else
            {
                teachingTip.UpdateVisualState();
            }
        }

        private static void OnVisualStatePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((TeachingTip)d).UpdateVisualState();
        }

        private static void OnIconSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var teachingTip = (TeachingTip)d;
            teachingTip.UpdateIcon();
            teachingTip.UpdateVisualState();
        }

        private void HookButtonEvents()
        {
            if (_actionButton != null)
            {
                _actionButton.Click += OnActionButtonClick;
            }

            if (_closeButton != null)
            {
                _closeButton.Click += OnCloseButtonClick;
            }

            if (_alternateCloseButton != null)
            {
                _alternateCloseButton.Click += OnCloseButtonClick;

                if (string.IsNullOrEmpty(System.Windows.Automation.AutomationProperties.GetName(_alternateCloseButton)))
                {
                    System.Windows.Automation.AutomationProperties.SetName(_alternateCloseButton, "Close");
                }

                if (_alternateCloseButton.ToolTip == null)
                {
                    _alternateCloseButton.ToolTip = "Close";
                }
            }
        }

        private void UnhookButtonEvents()
        {
            if (_actionButton != null)
            {
                _actionButton.Click -= OnActionButtonClick;
            }

            if (_closeButton != null)
            {
                _closeButton.Click -= OnCloseButtonClick;
            }

            if (_alternateCloseButton != null)
            {
                _alternateCloseButton.Click -= OnCloseButtonClick;
            }
        }

        private void OnActionButtonClick(object sender, RoutedEventArgs e)
        {
            ExecuteCommand(ActionButtonCommand, ActionButtonCommandParameter);
            ActionButtonClick?.Invoke(this, e);
        }

        private void OnCloseButtonClick(object sender, RoutedEventArgs e)
        {
            _lastCloseReason = TeachingTipCloseReason.CloseButton;
            ExecuteCommand(CloseButtonCommand, CloseButtonCommandParameter);
            CloseButtonClick?.Invoke(this, e);
            IsOpen = false;
        }

        private static void ExecuteCommand(ICommand command, object parameter)
        {
            if (command?.CanExecute(parameter) == true)
            {
                command.Execute(parameter);
            }
        }

        private void BeginClose(TeachingTipCloseReason reason)
        {
            var args = new TeachingTipClosingEventArgs(reason);
            Closing?.Invoke(this, args);

            if (args.Cancel)
            {
                RestoreOpenState();
                return;
            }

            if (args.HasOutstandingDeferrals)
            {
                RestoreOpenState();
                args.DeferralsCompleted += OnDeferralsCompleted;
                return;
            }

            CompleteClose(reason);

            void OnDeferralsCompleted(object sender, EventArgs e)
            {
                args.DeferralsCompleted -= OnDeferralsCompleted;
                if (args.Cancel)
                {
                    RestoreOpenState();
                    return;
                }

                CompleteClose(reason);
            }
        }

        private void RestoreOpenState()
        {
            SetCurrentValue(IsOpenProperty, true);
            UpdateVisualState();
        }

        private void CompleteClose(TeachingTipCloseReason reason)
        {
            _lastCloseReason = reason;

            if (IsOpen)
            {
                _isCompletingClose = true;
                try
                {
                    SetCurrentValue(IsOpenProperty, false);
                }
                finally
                {
                    _isCompletingClose = false;
                }
            }
            else
            {
                UpdateVisualState();
                Closed?.Invoke(this, new TeachingTipClosedEventArgs(reason));
            }

            _lastCloseReason = TeachingTipCloseReason.Programmatic;
        }

        private void UpdateIcon()
        {
            TemplateSettings.IconElement = IconSource?.CreateIconElement();
        }

        private void UpdateVisualState()
        {
            if (_container != null)
            {
                _container.Visibility = IsOpen ? Visibility.Visible : Visibility.Collapsed;
                _container.Margin = PlacementMargin;
            }

            if (_titleTextBlock != null)
            {
                _titleTextBlock.Visibility = string.IsNullOrEmpty(Title) ? Visibility.Collapsed : Visibility.Visible;
            }

            if (_subtitleTextBlock != null)
            {
                _subtitleTextBlock.Visibility = string.IsNullOrEmpty(Subtitle) ? Visibility.Collapsed : Visibility.Visible;
            }

            if (_iconPresenter != null)
            {
                _iconPresenter.Visibility = IconSource == null ? Visibility.Collapsed : Visibility.Visible;
            }

            UpdateButtons();
            UpdateHeroContentPlacement();
            UpdateTailPlacement();
            UpdateBackgroundResources();
        }

        private void UpdateButtons()
        {
            var hasActionButton = !ControlHelper.IsNullOrEmptyString(ActionButtonContent) || ActionButtonCommand != null;
            var hasCloseButton = !ControlHelper.IsNullOrEmptyString(CloseButtonContent) || CloseButtonCommand != null;

            if (_actionButton != null)
            {
                _actionButton.Visibility = hasActionButton ? Visibility.Visible : Visibility.Collapsed;
                Grid.SetColumn(_actionButton, hasCloseButton ? 0 : 0);
                Grid.SetColumnSpan(_actionButton, hasCloseButton ? 1 : 2);
                _actionButton.Margin = hasCloseButton ? GetThicknessResource("TeachingTipLeftButtonMargin") : GetThicknessResource("TeachingTipButtonPanelMargin");
            }

            if (_closeButton != null)
            {
                _closeButton.Visibility = hasCloseButton ? Visibility.Visible : Visibility.Collapsed;
                Grid.SetColumn(_closeButton, hasActionButton ? 1 : 0);
                Grid.SetColumnSpan(_closeButton, hasActionButton ? 1 : 2);
                _closeButton.Margin = hasActionButton ? GetThicknessResource("TeachingTipRightButtonMargin") : GetThicknessResource("TeachingTipButtonPanelMargin");
            }

            if (_alternateCloseButton != null)
            {
                _alternateCloseButton.Visibility = hasCloseButton ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        private void UpdateHeroContentPlacement()
        {
            if (_heroContentBorder == null)
            {
                return;
            }

            _heroContentBorder.Visibility = HeroContent == null ? Visibility.Collapsed : Visibility.Visible;
            Grid.SetRow(_heroContentBorder, HeroContentPlacement == TeachingTipHeroContentPlacementMode.Bottom ? 2 : 0);
        }

        private void UpdateTailPlacement()
        {
            if (_tailPolygon == null)
            {
                return;
            }

            var showTail = TailVisibility == TeachingTipTailVisibility.Visible ||
                TailVisibility == TeachingTipTailVisibility.Auto && Target != null;

            if (!showTail)
            {
                _tailPolygon.Visibility = Visibility.Collapsed;
                return;
            }

            _tailPolygon.Visibility = Visibility.Visible;

            var placement = PreferredPlacement == TeachingTipPlacementMode.Auto
                ? TeachingTipPlacementMode.Bottom
                : PreferredPlacement;

            ApplyTailPlacement(placement);
        }

        private void ApplyTailPlacement(TeachingTipPlacementMode placement)
        {
            switch (placement)
            {
                case TeachingTipPlacementMode.Top:
                case TeachingTipPlacementMode.TopLeft:
                case TeachingTipPlacementMode.TopRight:
                case TeachingTipPlacementMode.Center:
                    SetTail("0,0 10,10 20,0", 4, 2, HorizontalAlignment.Center, VerticalAlignment.Bottom, "TeachingTipTailPolygonMarginTop");
                    break;

                case TeachingTipPlacementMode.Left:
                case TeachingTipPlacementMode.LeftTop:
                case TeachingTipPlacementMode.LeftBottom:
                    SetTail("0,0 10,10 0,20", 2, 4, HorizontalAlignment.Right, VerticalAlignment.Center, "TeachingTipTailPolygonMarginLeft");
                    break;

                case TeachingTipPlacementMode.Right:
                case TeachingTipPlacementMode.RightTop:
                case TeachingTipPlacementMode.RightBottom:
                    SetTail("10,0 0,10 10,20", 2, 0, HorizontalAlignment.Left, VerticalAlignment.Center, "TeachingTipTailPolygonMarginRight");
                    break;

                default:
                    SetTail("0,10 10,0 20,10", 0, 2, HorizontalAlignment.Center, VerticalAlignment.Top, "TeachingTipTailPolygonMarginBottom");
                    break;
            }
        }

        private void SetTail(string points, int row, int column, HorizontalAlignment horizontalAlignment, VerticalAlignment verticalAlignment, string marginResourceKey)
        {
            _tailPolygon.Points = PointCollection.Parse(points);
            Grid.SetRow(_tailPolygon, row);
            Grid.SetColumn(_tailPolygon, column);
            _tailPolygon.HorizontalAlignment = horizontalAlignment;
            _tailPolygon.VerticalAlignment = verticalAlignment;
            _tailPolygon.Margin = GetThicknessResource(marginResourceKey);
        }

        private void UpdateBackgroundResources()
        {
            if (IsLightDismissEnabled)
            {
                SetBackgroundResource(_contentRootGrid, Border.BackgroundProperty, "TeachingTipTransientBackground");
                SetBackgroundResource(_heroContentBorder, Border.BackgroundProperty, "TeachingTipTransientBackground");
                SetBackgroundResource(_mainContentPresenter, Border.BackgroundProperty, "TeachingTipTransientBackground");

                if (_tailPolygon != null)
                {
                    _tailPolygon.SetResourceReference(Shape.FillProperty, "TeachingTipTransientBackground");
                }
            }
            else
            {
                SetBackgroundBinding(_contentRootGrid, Border.BackgroundProperty);
                SetBackgroundBinding(_heroContentBorder, Border.BackgroundProperty);
                SetBackgroundBinding(_mainContentPresenter, Border.BackgroundProperty);

                if (_tailPolygon != null)
                {
                    _tailPolygon.SetBinding(Shape.FillProperty, new System.Windows.Data.Binding(nameof(Background)) { Source = this });
                }
            }
        }

        private static void SetBackgroundResource(FrameworkElement element, DependencyProperty property, object resourceKey)
        {
            element?.SetResourceReference(property, resourceKey);
        }

        private void SetBackgroundBinding(FrameworkElement element, DependencyProperty property)
        {
            element?.SetBinding(property, new System.Windows.Data.Binding(nameof(Background)) { Source = this });
        }

        private Thickness GetThicknessResource(object key)
        {
            return TryFindResource(key) is Thickness thickness ? thickness : new Thickness();
        }

        private TeachingTipCloseReason _lastCloseReason = TeachingTipCloseReason.Programmatic;
        private bool _isCompletingClose;
        private FrameworkElement _container;
        private Border _contentRootGrid;
        private Border _heroContentBorder;
        private Border _mainContentPresenter;
        private TextBlock _titleTextBlock;
        private TextBlock _subtitleTextBlock;
        private FrameworkElement _iconPresenter;
        private Button _actionButton;
        private Button _closeButton;
        private Button _alternateCloseButton;
        private Polygon _tailPolygon;
    }
}
