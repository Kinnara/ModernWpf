// Ported from https://github.com/MahApps/MahApps.Metro/blob/develop/src/MahApps.Metro/Controls/FlipView.cs

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace ModernWpf.Controls
{
    /// <summary>
    /// A control that imitate a slide show with back/forward buttons.
    /// </summary>
    [TemplatePart(Name = PART_Presenter, Type = typeof(TransitioningContentControl))]
    [TemplatePart(Name = PART_BackButton, Type = typeof(Button))]
    [TemplatePart(Name = PART_ForwardButton, Type = typeof(Button))]
    [TemplatePart(Name = PART_UpButton, Type = typeof(Button))]
    [TemplatePart(Name = PART_DownButton, Type = typeof(Button))]
    [TemplatePart(Name = PART_BannerGrid, Type = typeof(Grid))]
    [TemplatePart(Name = PART_BannerLabel, Type = typeof(Label))]
    [TemplatePart(Name = PART_Index, Type = typeof(ListBox))]
    [StyleTypedProperty(Property = nameof(NavigationButtonStyle), StyleTargetType = typeof(Button))]
    [StyleTypedProperty(Property = nameof(IndexItemContainerStyle), StyleTargetType = typeof(ListBoxItem))]
    public class FlipView : Selector
    {
        /// <summary>Identifies the <see cref="MouseHoverBorderBrush"/> dependency property.</summary>
        public static readonly DependencyProperty MouseHoverBorderBrushProperty
            = DependencyProperty.Register(
                nameof(MouseHoverBorderBrush),
                typeof(Brush),
                typeof(FlipView),
                new PropertyMetadata(Brushes.LightGray));

        /// <summary>
        /// Gets or sets the border brush of the mouse hover effect.
        /// </summary>
        public Brush MouseHoverBorderBrush
        {
            get => (Brush)GetValue(MouseHoverBorderBrushProperty);
            set => SetValue(MouseHoverBorderBrushProperty, value);
        }

        /// <summary>Identifies the <see cref="MouseHoverBorderEnabled"/> dependency property.</summary>
        public static readonly DependencyProperty MouseHoverBorderEnabledProperty
            = DependencyProperty.Register(
                nameof(MouseHoverBorderEnabled),
                typeof(bool),
                typeof(FlipView),
                new PropertyMetadata(true));

        /// <summary>
        /// Gets or sets a value indicating whether the border for mouse over effect is enabled or not.
        /// </summary>
        public bool MouseHoverBorderEnabled
        {
            get => (bool)GetValue(MouseHoverBorderEnabledProperty);
            set => SetValue(MouseHoverBorderEnabledProperty, value);
        }

        /// <summary>Identifies the <see cref="MouseHoverBorderThickness"/> dependency property.</summary>
        public static readonly DependencyProperty MouseHoverBorderThicknessProperty
            = DependencyProperty.Register(
                nameof(MouseHoverBorderThickness),
                typeof(Thickness),
                typeof(FlipView),
                new PropertyMetadata(new Thickness(4)));

        /// <summary>
        /// Gets or sets the border thickness for the border of the mouse hover effect.
        /// </summary>
        public Thickness MouseHoverBorderThickness
        {
            get => (Thickness)GetValue(MouseHoverBorderThicknessProperty);
            set => SetValue(MouseHoverBorderThicknessProperty, value);
        }

        /// <summary>Identifies the <see cref="ShowIndex"/> dependency property.</summary>
        public static readonly DependencyProperty ShowIndexProperty
            = DependencyProperty.Register(
                nameof(ShowIndex),
                typeof(bool),
                typeof(FlipView),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));

        /// <summary>
        /// Gets or sets a value indicating whether the navigation index should be visible.
        /// </summary>
        public bool ShowIndex
        {
            get => (bool)GetValue(ShowIndexProperty);
            set => SetValue(ShowIndexProperty, value);
        }

        /// <summary>Identifies the <see cref="IndexItemContainerStyle"/> dependency property.</summary>
        public static readonly DependencyProperty IndexItemContainerStyleProperty
            = DependencyProperty.Register(
                nameof(IndexItemContainerStyle),
                typeof(Style),
                typeof(FlipView),
                new FrameworkPropertyMetadata(default(Style), FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));

        /// <summary>
        /// Gets or sets a style for the navigation index items.
        /// </summary>
        public Style IndexItemContainerStyle
        {
            get => (Style)GetValue(IndexItemContainerStyleProperty);
            set => SetValue(IndexItemContainerStyleProperty, value);
        }

        /// <summary>Identifies the <see cref="IndexPlacement"/> dependency property.</summary>
        public static readonly DependencyProperty IndexPlacementProperty
            = DependencyProperty.Register(
                nameof(IndexPlacement),
                typeof(NavigationIndexPlacement),
                typeof(FlipView),
                new FrameworkPropertyMetadata(NavigationIndexPlacement.Bottom, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));

        /// <summary>
        /// Gets or sets a value specifying where the navigation index should be rendered.
        /// </summary>
        public NavigationIndexPlacement IndexPlacement
        {
            get => (NavigationIndexPlacement)GetValue(IndexPlacementProperty);
            set => SetValue(IndexPlacementProperty, value);
        }

        public static readonly DependencyProperty IndexHorizontalAlignmentProperty
            = DependencyProperty.Register(
                nameof(IndexHorizontalAlignment),
                typeof(HorizontalAlignment),
                typeof(FlipView),
                new FrameworkPropertyMetadata(HorizontalAlignment.Center, FrameworkPropertyMetadataOptions.AffectsArrange));

        /// <summary>
        /// Gets or sets the horizontal alignment characteristics applied to the navigation index.
        /// </summary>
        public HorizontalAlignment IndexHorizontalAlignment
        {
            get => (HorizontalAlignment)GetValue(IndexHorizontalAlignmentProperty);
            set => SetValue(IndexHorizontalAlignmentProperty, value);
        }

        public static readonly DependencyProperty IndexVerticalAlignmentProperty
            = DependencyProperty.Register(
                nameof(IndexVerticalAlignment),
                typeof(VerticalAlignment),
                typeof(FlipView),
                new FrameworkPropertyMetadata(VerticalAlignment.Center, FrameworkPropertyMetadataOptions.AffectsArrange));

        /// <summary>
        /// Gets or sets the vertical alignment characteristics applied to the navigation index.
        /// </summary>
        public VerticalAlignment IndexVerticalAlignment
        {
            get => (VerticalAlignment)GetValue(IndexVerticalAlignmentProperty);
            set => SetValue(IndexVerticalAlignmentProperty, value);
        }

        /// <summary>Identifies the <see cref="CircularNavigation"/> dependency property.</summary>
        public static readonly DependencyProperty CircularNavigationProperty
            = DependencyProperty.Register(
                nameof(CircularNavigation),
                typeof(bool),
                typeof(FlipView),
                new FrameworkPropertyMetadata(false,
                    FrameworkPropertyMetadataOptions.AffectsRender,
                    (d, e) => ((FlipView)d).DetectControlButtonsStatus()));

        /// <summary>
        /// Gets or sets a value indicating whether the navigation is circular, so you get the first after last and the last before first.
        /// </summary>
        public bool CircularNavigation
        {
            get => (bool)GetValue(CircularNavigationProperty);
            set => SetValue(CircularNavigationProperty, value);
        }

        /// <summary>Identifies the <see cref="NavigationButtonsPosition"/> dependency property.</summary>
        public static readonly DependencyProperty NavigationButtonsPositionProperty
            = DependencyProperty.Register(
                nameof(NavigationButtonsPosition),
                typeof(NavigationButtonsPosition),
                typeof(FlipView),
                new FrameworkPropertyMetadata(NavigationButtonsPosition.Inside,
                    FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure,
                    (d, e) => ((FlipView)d).DetectControlButtonsStatus()));

        /// <summary>
        /// Gets or sets the position of the navigation buttons.
        /// </summary>
        [Bindable(true)]
        [Category("Appearance")]
        [DefaultValue(NavigationButtonsPosition.Inside)]
        public NavigationButtonsPosition NavigationButtonsPosition
        {
            get => (NavigationButtonsPosition)GetValue(NavigationButtonsPositionProperty);
            set => SetValue(NavigationButtonsPositionProperty, value);
        }

        /// <summary>Identifies the <see cref="Orientation"/> dependency property.</summary>
        public static readonly DependencyProperty OrientationProperty
            = DependencyProperty.Register(
                nameof(Orientation),
                typeof(Orientation),
                typeof(FlipView),
                new PropertyMetadata(Orientation.Horizontal, (d, e) => ((FlipView)d).DetectControlButtonsStatus()));

        /// <summary>
        /// Gets or sets the orientation of the navigation.
        /// </summary>
        public Orientation Orientation
        {
            get => (Orientation)GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }

        /// <summary>Identifies the <see cref="LeftTransition"/> dependency property.</summary>
        public static readonly DependencyProperty LeftTransitionProperty
            = DependencyProperty.Register(
                nameof(LeftTransition),
                typeof(TransitionType),
                typeof(FlipView),
                new PropertyMetadata(TransitionType.LeftReplace));

        /// <summary>
        /// Gets or sets the transition of the left navigation.
        /// </summary>
        public TransitionType LeftTransition
        {
            get => (TransitionType)GetValue(LeftTransitionProperty);
            set => SetValue(LeftTransitionProperty, value);
        }

        /// <summary>Identifies the <see cref="RightTransition"/> dependency property.</summary>
        public static readonly DependencyProperty RightTransitionProperty
            = DependencyProperty.Register(
                nameof(RightTransition),
                typeof(TransitionType),
                typeof(FlipView),
                new PropertyMetadata(TransitionType.RightReplace));

        /// <summary>
        /// Gets or sets the transition of the right navigation.
        /// </summary>
        public TransitionType RightTransition
        {
            get => (TransitionType)GetValue(RightTransitionProperty);
            set => SetValue(RightTransitionProperty, value);
        }

        /// <summary>Identifies the <see cref="UpTransition"/> dependency property.</summary>
        public static readonly DependencyProperty UpTransitionProperty
            = DependencyProperty.Register(
                nameof(UpTransition),
                typeof(TransitionType),
                typeof(FlipView),
                new PropertyMetadata(TransitionType.Up));

        /// <summary>
        /// Gets or sets the transition of the up navigation.
        /// </summary>
        public TransitionType UpTransition
        {
            get => (TransitionType)GetValue(UpTransitionProperty);
            set => SetValue(UpTransitionProperty, value);
        }

        /// <summary>Identifies the <see cref="DownTransition"/> dependency property.</summary>
        public static readonly DependencyProperty DownTransitionProperty
            = DependencyProperty.Register(
                nameof(DownTransition),
                typeof(TransitionType),
                typeof(FlipView),
                new PropertyMetadata(TransitionType.Down));

        /// <summary>
        /// Gets or sets the transition of the down navigation.
        /// </summary>
        public TransitionType DownTransition
        {
            get => (TransitionType)GetValue(DownTransitionProperty);
            set => SetValue(DownTransitionProperty, value);
        }

        /// <summary>Identifies the <see cref="IsBannerEnabled"/> dependency property.</summary>
        public static readonly DependencyProperty IsBannerEnabledProperty
            = DependencyProperty.Register(
                nameof(IsBannerEnabled),
                typeof(bool),
                typeof(FlipView),
                new UIPropertyMetadata(true, OnIsBannerEnabledPropertyChangedCallback));

        /// <summary>
        /// Gets or sets whether the banner is visible or not.
        /// </summary>
        public bool IsBannerEnabled
        {
            get => (bool)GetValue(IsBannerEnabledProperty);
            set => SetValue(IsBannerEnabledProperty, value);
        }

        private static void OnIsBannerEnabledPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var flipView = (FlipView)d;

            void CheckFlipViewBannerVisibility()
            {
                if ((bool)e.NewValue)
                {
                    flipView.ShowBanner();
                }
                else
                {
                    flipView.HideBanner();
                }
            }

            if (flipView.IsLoaded)
            {
                CheckFlipViewBannerVisibility();
            }
            else
            {
                //wait to be loaded?
                flipView.ExecuteWhenLoaded(() =>
                {
                    flipView.ApplyTemplate();
                    CheckFlipViewBannerVisibility();
                });
            }
        }

        /// <summary>Identifies the <see cref="IsNavigationEnabled"/> dependency property.</summary>
        public static readonly DependencyProperty IsNavigationEnabledProperty
            = DependencyProperty.Register(
                nameof(IsNavigationEnabled),
                typeof(bool),
                typeof(FlipView),
                new PropertyMetadata(true, (d, e) => ((FlipView)d).DetectControlButtonsStatus()));

        /// <summary>
        /// Gets or sets whether the navigation button are visible or not.
        /// </summary>
        public bool IsNavigationEnabled
        {
            get => (bool)GetValue(IsNavigationEnabledProperty);
            set => SetValue(IsNavigationEnabledProperty, value);
        }

        /// <summary>Identifies the <see cref="BannerText"/> dependency property.</summary>
        public static readonly DependencyProperty BannerTextProperty
            = DependencyProperty.Register(
                nameof(BannerText),
                typeof(object),
                typeof(FlipView),
                new FrameworkPropertyMetadata("Banner",
                    FrameworkPropertyMetadataOptions.AffectsRender,
                    (d, e) => ((FlipView)d).ExecuteWhenLoaded(() => ((FlipView)d).ChangeBannerText(e.NewValue))));

        /// <summary>
        /// Gets or sets the banner text.
        /// </summary>
        public object BannerText
        {
            get => GetValue(BannerTextProperty);
            set => SetValue(BannerTextProperty, value);
        }

        /// <summary>Identifies the <see cref="BannerBackground"/> dependency property.</summary>
        public static readonly DependencyProperty BannerBackgroundProperty
            = DependencyProperty.Register(
                nameof(BannerBackground),
                typeof(Brush),
                typeof(FlipView),
                new FrameworkPropertyMetadata(null));

        /// <summary>Identifies the <see cref="BannerTextTemplate"/> dependency property.</summary>
        public static readonly DependencyProperty BannerTextTemplateProperty
            = DependencyProperty.Register(
                nameof(BannerTextTemplate),
                typeof(DataTemplate),
                typeof(FlipView));

        /// <summary>
        /// Gets or sets the DataTemplate used to display the banner's content.
        /// </summary>
        public DataTemplate BannerTextTemplate
        {
            get => (DataTemplate)GetValue(BannerTextTemplateProperty);
            set => SetValue(BannerTextTemplateProperty, value);
        }

        /// <summary>Identifies the <see cref="BannerTextTemplateSelector"/> dependency property.</summary>
        public static readonly DependencyProperty BannerTextTemplateSelectorProperty
            = DependencyProperty.Register(
                nameof(BannerTextTemplateSelector),
                typeof(DataTemplateSelector),
                typeof(FlipView),
                new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Gets or sets a template selector for BannerText property that enables an application writer to provide custom template-selection logic .
        /// </summary>
        /// <remarks> 
        /// This property is ignored if <seealso cref="BannerTextTemplate"/> is set.
        /// </remarks>
        [Bindable(true)]
        public DataTemplateSelector BannerTextTemplateSelector
        {
            get => (DataTemplateSelector)GetValue(BannerTextTemplateSelectorProperty);
            set => SetValue(BannerTextTemplateSelectorProperty, value);
        }

        /// <summary>Identifies the <see cref="BannerTextStringFormat"/> dependency property.</summary>
        public static readonly DependencyProperty BannerTextStringFormatProperty
            = DependencyProperty.Register(
                nameof(BannerTextStringFormat),
                typeof(string),
                typeof(FlipView),
                new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Gets or sets a composite string that specifies how to format the BannerText property if it is displayed as a string.
        /// </summary>
        /// <remarks> 
        /// This property is ignored if <seealso cref="BannerTextTemplate"/> is set.
        /// </remarks>
        [Bindable(true)]
        public string BannerTextStringFormat
        {
            get => (string)GetValue(BannerTextStringFormatProperty);
            set => SetValue(BannerTextStringFormatProperty, value);
        }

        /// <summary>
        /// Gets or sets a <see cref="Brush" /> that is used to fill the banner.
        /// </summary>
        public Brush BannerBackground
        {
            get => (Brush)GetValue(BannerBackgroundProperty);
            set => SetValue(BannerBackgroundProperty, value);
        }

        /// <summary>Identifies the <see cref="BannerForeground"/> dependency property.</summary>
        public static readonly DependencyProperty BannerForegroundProperty
            = DependencyProperty.Register(
                nameof(BannerForeground),
                typeof(Brush),
                typeof(FlipView),
                new FrameworkPropertyMetadata(SystemColors.ControlTextBrush));

        /// <summary>
        /// Gets or sets a <see cref="Brush" /> that describes the foreground color of the banner label.
        /// </summary>
        public Brush BannerForeground
        {
            get => (Brush)GetValue(BannerForegroundProperty);
            set => SetValue(BannerForegroundProperty, value);
        }

        /// <summary>Identifies the <see cref="BannerOpacity"/> dependency property.</summary>
        public static readonly DependencyProperty BannerOpacityProperty
            = DependencyProperty.Register(
                nameof(BannerOpacity),
                typeof(double),
                typeof(FlipView),
                new UIPropertyMetadata(1.0));

        /// <summary>
        /// Gets or sets the opacity factor applied to the entire banner when it is rendered in the user interface (UI).
        /// </summary>
        public double BannerOpacity
        {
            get => (double)GetValue(BannerOpacityProperty);
            set => SetValue(BannerOpacityProperty, value);
        }

        /// <summary>Identifies the <see cref="NavigationButtonStyle"/> dependency property.</summary>
        public static readonly DependencyProperty NavigationButtonStyleProperty
            = DependencyProperty.Register(
                nameof(NavigationButtonStyle),
                typeof(Style),
                typeof(FlipView),
                new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the <see cref="FrameworkElement.Style"/> for the navigation buttons.
        /// </summary>
        public Style NavigationButtonStyle
        {
            get => (Style)GetValue(NavigationButtonStyleProperty);
            set => SetValue(NavigationButtonStyleProperty, value);
        }

        /// <summary>Identifies the <see cref="ButtonBackContent"/> dependency property.</summary>
        public static readonly DependencyProperty ButtonBackContentProperty
            = DependencyProperty.Register(
                nameof(ButtonBackContent),
                typeof(object),
                typeof(FlipView),
                new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Provides the object content that should be displayed on the Back Button.
        /// </summary>
        public object ButtonBackContent
        {
            get => GetValue(ButtonBackContentProperty);
            set => SetValue(ButtonBackContentProperty, value);
        }

        /// <summary>Identifies the <see cref="ButtonBackContentTemplate"/> dependency property.</summary>
        public static readonly DependencyProperty ButtonBackContentTemplateProperty
            = DependencyProperty.Register(
                nameof(ButtonBackContentTemplate),
                typeof(DataTemplate),
                typeof(FlipView));

        /// <summary>
        /// Gets or sets the DataTemplate used to display the Back button's content.
        /// </summary>
        public DataTemplate ButtonBackContentTemplate
        {
            get => (DataTemplate)GetValue(ButtonBackContentTemplateProperty);
            set => SetValue(ButtonBackContentTemplateProperty, value);
        }

        /// <summary>Identifies the <see cref="ButtonBackContentStringFormat"/> dependency property.</summary>
        public static readonly DependencyProperty ButtonBackContentStringFormatProperty
            = DependencyProperty.Register(
                nameof(ButtonBackContentStringFormat),
                typeof(string),
                typeof(FlipView),
                new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Gets or sets a composite string that specifies how to format the ButtonBackContent property if it is displayed as a string.
        /// </summary>
        /// <remarks> 
        /// This property is ignored if <seealso cref="ButtonBackContentTemplate"/> is set.
        /// </remarks>
        [Bindable(true)]
        public string ButtonBackContentStringFormat
        {
            get => (string)GetValue(ButtonBackContentStringFormatProperty);
            set => SetValue(ButtonBackContentStringFormatProperty, value);
        }

        /// <summary>Identifies the <see cref="ButtonForwardContent"/> dependency property.</summary>
        public static readonly DependencyProperty ButtonForwardContentProperty
            = DependencyProperty.Register(
                nameof(ButtonForwardContent),
                typeof(object),
                typeof(FlipView),
                new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Provides the object content that should be displayed on the Forward Button.
        /// </summary>
        public object ButtonForwardContent
        {
            get => GetValue(ButtonForwardContentProperty);
            set => SetValue(ButtonForwardContentProperty, value);
        }

        /// <summary>Identifies the <see cref="ButtonForwardContentTemplate"/> dependency property.</summary>
        public static readonly DependencyProperty ButtonForwardContentTemplateProperty
            = DependencyProperty.Register(
                nameof(ButtonForwardContentTemplate),
                typeof(DataTemplate),
                typeof(FlipView));

        /// <summary>
        /// Gets or sets the DataTemplate used to display the Forward button's content.
        /// </summary>
        public DataTemplate ButtonForwardContentTemplate
        {
            get => (DataTemplate)GetValue(ButtonForwardContentTemplateProperty);
            set => SetValue(ButtonForwardContentTemplateProperty, value);
        }

        /// <summary>Identifies the <see cref="ButtonForwardContentStringFormat"/> dependency property.</summary>
        public static readonly DependencyProperty ButtonForwardContentStringFormatProperty
            = DependencyProperty.Register(
                nameof(ButtonForwardContentStringFormat),
                typeof(string),
                typeof(FlipView),
                new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Gets or sets a composite string that specifies how to format the ButtonForwardContent property if it is displayed as a string.
        /// </summary>
        /// <remarks> 
        /// This property is ignored if <seealso cref="ButtonForwardContentTemplate"/> is set.
        /// </remarks>
        [Bindable(true)]
        public string ButtonForwardContentStringFormat
        {
            get => (string)GetValue(ButtonForwardContentStringFormatProperty);
            set => SetValue(ButtonForwardContentStringFormatProperty, value);
        }

        /// <summary>Identifies the <see cref="ButtonUpContent"/> dependency property.</summary>
        public static readonly DependencyProperty ButtonUpContentProperty
            = DependencyProperty.Register(
                nameof(ButtonUpContent),
                typeof(object),
                typeof(FlipView),
                new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Provides the object content that should be displayed on the Up Button.
        /// </summary>
        public object ButtonUpContent
        {
            get => GetValue(ButtonUpContentProperty);
            set => SetValue(ButtonUpContentProperty, value);
        }

        /// <summary>Identifies the <see cref="ButtonUpContentTemplate"/> dependency property.</summary>
        public static readonly DependencyProperty ButtonUpContentTemplateProperty
            = DependencyProperty.Register(
                nameof(ButtonUpContentTemplate),
                typeof(DataTemplate),
                typeof(FlipView));

        /// <summary>
        /// Gets or sets the DataTemplate used to display the Up button's content.
        /// </summary>
        public DataTemplate ButtonUpContentTemplate
        {
            get => (DataTemplate)GetValue(ButtonUpContentTemplateProperty);
            set => SetValue(ButtonUpContentTemplateProperty, value);
        }

        /// <summary>Identifies the <see cref="ButtonUpContentStringFormat"/> dependency property.</summary>
        public static readonly DependencyProperty ButtonUpContentStringFormatProperty
            = DependencyProperty.Register(
                nameof(ButtonUpContentStringFormat),
                typeof(string),
                typeof(FlipView),
                new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Gets or sets a composite string that specifies how to format the ButtonUpContent property if it is displayed as a string.
        /// </summary>
        /// <remarks> 
        /// This property is ignored if <seealso cref="ButtonUpContentTemplate"/> is set.
        /// </remarks>
        [Bindable(true)]
        public string ButtonUpContentStringFormat
        {
            get => (string)GetValue(ButtonUpContentStringFormatProperty);
            set => SetValue(ButtonUpContentStringFormatProperty, value);
        }

        /// <summary>Identifies the <see cref="ButtonDownContent"/> dependency property.</summary>
        public static readonly DependencyProperty ButtonDownContentProperty
            = DependencyProperty.Register(
                nameof(ButtonDownContent),
                typeof(object),
                typeof(FlipView),
                new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Provides the object content that should be displayed on the Down Button.
        /// </summary>
        public object ButtonDownContent
        {
            get => GetValue(ButtonDownContentProperty);
            set => SetValue(ButtonDownContentProperty, value);
        }

        /// <summary>Identifies the <see cref="ButtonDownContentTemplate"/> dependency property.</summary>
        public static readonly DependencyProperty ButtonDownContentTemplateProperty
            = DependencyProperty.Register(
                nameof(ButtonDownContentTemplate),
                typeof(DataTemplate),
                typeof(FlipView));

        /// <summary>
        /// Gets or sets the DataTemplate used to display the Down button's content.
        /// </summary>
        public DataTemplate ButtonDownContentTemplate
        {
            get => (DataTemplate)GetValue(ButtonDownContentTemplateProperty);
            set => SetValue(ButtonDownContentTemplateProperty, value);
        }

        /// <summary>Identifies the <see cref="ButtonDownContentStringFormat"/> dependency property.</summary>
        public static readonly DependencyProperty ButtonDownContentStringFormatProperty
            = DependencyProperty.Register(
                nameof(ButtonDownContentStringFormat),
                typeof(string),
                typeof(FlipView),
                new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Gets or sets a composite string that specifies how to format the ButtonDownContent property if it is displayed as a string.
        /// </summary>
        /// <remarks> 
        /// This property is ignored if <seealso cref="ButtonDownContentTemplate"/> is set.
        /// </remarks>
        [Bindable(true)]
        public string ButtonDownContentStringFormat
        {
            get => (string)GetValue(ButtonDownContentStringFormatProperty);
            set => SetValue(ButtonDownContentStringFormatProperty, value);
        }

        private const string PART_BackButton = "PART_BackButton";
        private const string PART_BannerGrid = "PART_BannerGrid";
        private const string PART_BannerLabel = "PART_BannerLabel";
        private const string PART_DownButton = "PART_DownButton";
        private const string PART_ForwardButton = "PART_ForwardButton";
        private const string PART_Presenter = "PART_Presenter";
        private const string PART_UpButton = "PART_UpButton";
        private const string PART_Index = "PART_Index";
        /// <summary>
        /// To counteract the double Loaded event issue.
        /// </summary>
        private bool loaded;
        private bool allowSelectedIndexChangedCallback = true;
        private Grid bannerGrid;
        private Label bannerLabel;
        private ListBox indexListBox;
        private Button backButton;
        private Button forwardButton;
        private Button downButton;
        private Button upButton;
        private Storyboard hideBannerStoryboard;
        private Storyboard hideControlStoryboard;
        private EventHandler hideControlStoryboardCompletedHandler;
        private TransitioningContentControl presenter;
        private Storyboard showBannerStoryboard;
        private Storyboard showControlStoryboard;
        private object savedBannerText;

        static FlipView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FlipView), new FrameworkPropertyMetadata(typeof(FlipView)));

            /* Hook SelectedIndexProperty's coercion.
             * Coercion is called whenever the value of the DependencyProperty is being re-evaluated or coercion is specifically requested.
             * Coercion has access to current and proposed value to enforce compliance.
             * It is called after ValidateCallback.
             * So one can ultimately use this callback like a value is changing event.
             * As this control doesn't implement System.ComponentModel.INotifyPropertyChanging,
             * it's the only way to determine from/to index and ensure Transition consistency in any use case of the control.
             */
            var previousSelectedIndexPropertyMetadata = SelectedIndexProperty.GetMetadata(typeof(FlipView));
            SelectedIndexProperty.OverrideMetadata(typeof(FlipView),
                new FrameworkPropertyMetadata
                {
                    /* Coercion being behavior critical, we don't want to replace inherited or added callbacks.
                     * They must be called before ours and most of all : their result must be our input.
                     * But since delegates are multicast (meaning they can have more than on target), each target would sequentially be executed with the same original input
                     * thus not chaining invocations' inputs/outputs. So the caller would retrieve the sole last target's return value, ignoring previous computations.
                     * Hence, we chain coerions inputs/outputs until our callback to preserve the behavior of the control
                     * and be sure the value won't change anymore before being actually set.
                     */
                    CoerceValueCallback = (d, value) =>
                    {
                        /* Chain actual coercions... */
                        if (previousSelectedIndexPropertyMetadata.CoerceValueCallback != null)
                        {
                            foreach (var item in previousSelectedIndexPropertyMetadata.CoerceValueCallback.GetInvocationList())
                            {
                                value = ((CoerceValueCallback)item)(d, value);
                            }
                        }

                        /* ...'til our new one. */
                        return CoerceSelectedIndexProperty(d, value);
                    }
                });
        }

        public FlipView()
        {
            Loaded += FlipViewLoaded;
        }

        /// <summary>
        /// Coerce SelectedIndexProperty's value.
        /// </summary>
        /// <param name="d">The object that the property exists on.</param>
        /// <param name="value">The new value of the property, prior to any coercion attempt.</param>
        /// <returns>The coerced value (with appropriate type). </returns>
        private static object CoerceSelectedIndexProperty(DependencyObject d, object value)
        {
            // call ComputeTransition only if SelectedIndex is changed from outside and not from GoBack or GoForward
            if (d is FlipView flipView && flipView.allowSelectedIndexChangedCallback)
            {
                flipView.ComputeTransition(flipView.SelectedIndex, value as int? ?? flipView.SelectedIndex);
            }

            return value;
        }

        /// <summary>
        /// Changes the current slide to the previous item.
        /// </summary>
        public void GoBack()
        {
            allowSelectedIndexChangedCallback = false;
            try
            {
                if (presenter is not null)
                {
                    presenter.Transition = Orientation == Orientation.Horizontal ? RightTransition : UpTransition;
                }

                if (SelectedIndex > 0)
                {
                    SelectedIndex--;
                }
                else
                {
                    if (CircularNavigation)
                    {
                        SelectedIndex = Items.Count - 1;
                    }
                }
            }
            finally
            {
                allowSelectedIndexChangedCallback = true;
            }
        }

        /// <summary>
        /// Changes the current to the next item.
        /// </summary>
        public void GoForward()
        {
            allowSelectedIndexChangedCallback = false;
            try
            {
                if (presenter is not null)
                {
                    presenter.Transition = Orientation == Orientation.Horizontal ? LeftTransition : DownTransition;
                }

                if (SelectedIndex < Items.Count - 1)
                {
                    SelectedIndex++;
                }
                else
                {
                    if (CircularNavigation)
                    {
                        SelectedIndex = 0;
                    }
                }
            }
            finally
            {
                allowSelectedIndexChangedCallback = true;
            }
        }

        /// <summary>
        /// Brings the control buttons (next/previous) into view.
        /// </summary>
        public void ShowControlButtons()
        {
            this.ExecuteWhenLoaded(() => DetectControlButtonsStatus());
        }

        /// <summary>
        /// Removes the control buttons (next/previous) from view.
        /// </summary>
        public void HideControlButtons()
        {
            this.ExecuteWhenLoaded(() => DetectControlButtonsStatus(Visibility.Hidden));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            showBannerStoryboard = ((Storyboard)Template.Resources["ShowBannerStoryboard"]).Clone();
            hideBannerStoryboard = ((Storyboard)Template.Resources["HideBannerStoryboard"]).Clone();

            showControlStoryboard = ((Storyboard)Template.Resources["ShowControlStoryboard"]).Clone();
            hideControlStoryboard = ((Storyboard)Template.Resources["HideControlStoryboard"]).Clone();

            presenter = GetTemplateChild(PART_Presenter) as TransitioningContentControl;

            if (indexListBox != null)
            {
                indexListBox.SelectionChanged -= OnIndexListBoxSelectionChanged;
            }

            if (forwardButton != null)
            {
                forwardButton.Click -= NextButtonClick;
            }

            if (backButton != null)
            {
                backButton.Click -= PrevButtonClick;
            }

            if (upButton != null)
            {
                upButton.Click -= PrevButtonClick;
            }

            if (downButton != null)
            {
                downButton.Click -= NextButtonClick;
            }

            indexListBox = GetTemplateChild(PART_Index) as ListBox;

            forwardButton = GetTemplateChild(PART_ForwardButton) as Button;
            backButton = GetTemplateChild(PART_BackButton) as Button;
            upButton = GetTemplateChild(PART_UpButton) as Button;
            downButton = GetTemplateChild(PART_DownButton) as Button;

            bannerGrid = GetTemplateChild(PART_BannerGrid) as Grid;
            bannerLabel = GetTemplateChild(PART_BannerLabel) as Label;

            if (forwardButton != null)
            {
                forwardButton.Click += NextButtonClick;
            }

            if (backButton != null)
            {
                backButton.Click += PrevButtonClick;
            }

            if (upButton != null)
            {
                upButton.Click += PrevButtonClick;
            }

            if (downButton != null)
            {
                downButton.Click += NextButtonClick;
            }

            if (bannerLabel != null)
            {
                bannerLabel.Opacity = IsBannerEnabled ? 1d : 0d;
            }

            this.ExecuteWhenLoaded(() =>
            {
                if (indexListBox != null)
                {
                    indexListBox.SelectionChanged += OnIndexListBoxSelectionChanged;
                }
            });
        }

        private void OnIndexListBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ReferenceEquals(e.OriginalSource, indexListBox))
            {
                e.Handled = true;
            }
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new FlipViewItem { HorizontalAlignment = HorizontalAlignment.Stretch };
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is FlipViewItem;
        }

        protected override void OnItemsChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);
            DetectControlButtonsStatus();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            var isHorizontal = Orientation == Orientation.Horizontal;
            var isVertical = Orientation == Orientation.Vertical;
            var canGoPrev = (e.Key == Key.Left && isHorizontal && backButton != null && backButton.Visibility == Visibility.Visible && backButton.IsEnabled)
                            || (e.Key == Key.Up && isVertical && upButton != null && upButton.Visibility == Visibility.Visible && upButton.IsEnabled);
            var canGoNext = (e.Key == Key.Right && isHorizontal && forwardButton != null && forwardButton.Visibility == Visibility.Visible && forwardButton.IsEnabled)
                            || (e.Key == Key.Down && isVertical && downButton != null && downButton.Visibility == Visibility.Visible && downButton.IsEnabled);

            if (canGoPrev)
            {
                GoBack();
                e.Handled = true;
                Focus();
            }
            else if (canGoNext)
            {
                GoForward();
                e.Handled = true;
                Focus();
            }
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            Focus();
        }

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            base.OnSelectionChanged(e);

            var oldItem = e.RemovedItems?.OfType<FlipViewItem>().FirstOrDefault();
            if (oldItem is null)
            {
                savedBannerText = BannerText;
            }

            var newItem = e.AddedItems?.OfType<FlipViewItem>().FirstOrDefault();
            SetCurrentValue(BannerTextProperty, newItem is null ? savedBannerText : newItem.BannerText);

            DetectControlButtonsStatus();
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            if (element != item)
            {
                element.SetValue(DataContextProperty, item); // don't want to set the DataContext to itself.
            }

            base.PrepareContainerForItemOverride(element, item);
        }

        /// <summary>
        /// Applies actions to navigation buttons.
        /// </summary>
        /// <param name="prevButtonApply">Action applied to the previous button.</param>
        /// <param name="nextButtonApply">Action applied to the next button.</param>
        /// <param name="inactiveButtonsApply">Action applied to the inactive buttons.</param>
        /// <exception cref="ArgumentNullException">Any action is null.</exception>
        private void ApplyToNavigationButtons(Action<Button> prevButtonApply, Action<Button> nextButtonApply, Action<Button> inactiveButtonsApply)
        {
            if (prevButtonApply == null)
            {
                throw new ArgumentNullException(nameof(prevButtonApply));
            }

            if (nextButtonApply == null)
            {
                throw new ArgumentNullException(nameof(nextButtonApply));
            }

            if (inactiveButtonsApply == null)
            {
                throw new ArgumentNullException(nameof(inactiveButtonsApply));
            }

            GetNavigationButtons(out var prevButton, out var nextButton, out var inactiveButtons);

            foreach (var button in inactiveButtons.OfType<Button>())
            {
                inactiveButtonsApply(button);
            }

            if (prevButton != null)
            {
                prevButtonApply(prevButton);
            }

            if (nextButton != null)
            {
                nextButtonApply(nextButton);
            }
        }

        private void ChangeBannerText(object value = null)
        {
            if (IsBannerEnabled)
            {
                var newValue = value ?? BannerText;
                if (newValue is null
                    || hideControlStoryboard is null
                    || showControlStoryboard is null
                    || bannerLabel is null)
                {
                    return;
                }

                if (hideControlStoryboardCompletedHandler != null)
                {
                    hideControlStoryboard.Completed -= hideControlStoryboardCompletedHandler;
                }

                hideControlStoryboardCompletedHandler = (sender, e) =>
                {
                    try
                    {
                        hideControlStoryboard.Completed -= hideControlStoryboardCompletedHandler;

                        bannerLabel.Content = newValue;

                        bannerLabel.BeginStoryboard(showControlStoryboard, HandoffBehavior.SnapshotAndReplace);
                    }
                    catch (Exception)
                    {
                    }
                };

                hideControlStoryboard.Completed += hideControlStoryboardCompletedHandler;

                bannerLabel.BeginStoryboard(hideControlStoryboard, HandoffBehavior.SnapshotAndReplace);
            }
            else
            {
                this.ExecuteWhenLoaded(() =>
                {
                    if (bannerLabel is not null)
                    {
                        bannerLabel.Content = value ?? BannerText;
                    }
                });
            }
        }

        /// <summary>
        /// Computes the transition when changing selected index.
        /// </summary>
        /// <param name="fromIndex">Previous selected index.</param>
        /// <param name="toIndex">New selected index.</param>
        private void ComputeTransition(int fromIndex, int toIndex)
        {
            if (presenter != null)
            {
                if (fromIndex < toIndex)
                {
                    presenter.Transition = Orientation == Orientation.Horizontal ? LeftTransition : DownTransition;
                }
                else if (fromIndex > toIndex)
                {
                    presenter.Transition = Orientation == Orientation.Horizontal ? RightTransition : UpTransition;
                }
                else
                {
                    presenter.Transition = TransitionType.Default;
                }
            }
        }

        /// <summary>
        /// Sets the visibility of navigation buttons.
        /// </summary>
        /// <param name="activeButtonsVisibility">Visibility of active buttons.</param>
        private void DetectControlButtonsStatus(Visibility activeButtonsVisibility = Visibility.Visible)
        {
            if (!IsNavigationEnabled)
            {
                activeButtonsVisibility = Visibility.Hidden;
            }

            ApplyToNavigationButtons(
                prev => prev.Visibility = CircularNavigation || (Items.Count > 0 && SelectedIndex > 0) ? activeButtonsVisibility : Visibility.Hidden,
                next => next.Visibility = CircularNavigation || (Items.Count > 0 && SelectedIndex < Items.Count - 1) ? activeButtonsVisibility : Visibility.Hidden,
                inactive => inactive.Visibility = Visibility.Hidden);
        }

        private void FlipViewLoaded(object sender, RoutedEventArgs e)
        {
            /* Loaded event fires twice if its a child of a TabControl.
             * Once because the TabControl seems to initiali(z|s)e everything.
             * And a second time when the Tab (housing the FlipView) is switched to. */

            // if OnApplyTemplate hasn't been called yet.
            if (backButton == null || forwardButton == null || upButton == null || downButton == null)
            {
                ApplyTemplate();
            }

            // Counteracts the double 'Loaded' event issue.
            if (loaded)
            {
                return;
            }

            Unloaded += FlipViewUnloaded;

            if (SelectedIndex < 0)
            {
                SelectedIndex = 0;
            }

            DetectControlButtonsStatus();

            ShowBanner();

            loaded = true;
        }

        private void FlipViewUnloaded(object sender, RoutedEventArgs e)
        {
            Unloaded -= FlipViewUnloaded;

            if (hideControlStoryboard != null && hideControlStoryboardCompletedHandler != null)
            {
                hideControlStoryboard.Completed -= hideControlStoryboardCompletedHandler;
            }

            loaded = false;
        }

        /// <summary>
        /// Gets the navigation buttons.
        /// </summary>
        /// <param name="prevButton">Previous button.</param>
        /// <param name="nextButton">Next button.</param>
        /// <param name="inactiveButtons">Inactive buttons.</param>
        private void GetNavigationButtons(out Button? prevButton, out Button? nextButton, out IEnumerable<Button?> inactiveButtons)
        {
            if (Orientation == Orientation.Horizontal)
            {
                prevButton = backButton;
                nextButton = forwardButton;
                inactiveButtons = new[] { upButton, downButton };
            }
            else
            {
                prevButton = upButton;
                nextButton = downButton;
                inactiveButtons = new[] { backButton, forwardButton };
            }
        }

        private void HideBanner()
        {
            if (ActualHeight > 0.0)
            {
                if (hideControlStoryboard is not null)
                {
                    bannerLabel?.BeginStoryboard(hideControlStoryboard);
                }

                if (hideBannerStoryboard is not null)
                {
                    bannerGrid?.BeginStoryboard(hideBannerStoryboard);
                }
            }
        }

        private void NextButtonClick(object sender, RoutedEventArgs e)
        {
            GoForward();
        }

        private void PrevButtonClick(object sender, RoutedEventArgs e)
        {
            GoBack();
        }

        private void ShowBanner()
        {
            if (IsBannerEnabled)
            {
                ChangeBannerText(BannerText);
                bannerGrid?.BeginStoryboard(showBannerStoryboard);
            }
        }
    }
}
