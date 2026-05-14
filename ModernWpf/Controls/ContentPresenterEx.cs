using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;
using ModernWpf.Media.Animation;

namespace ModernWpf.Controls
{
    /// <summary>
    /// ContentPresenter is used within the template of a content control to denote the
    /// place in the control's visual tree (control template) where the content
    /// is to be added.
    /// </summary>
    [ContentProperty(nameof(Content))]
    public class ContentPresenterEx : ContentPresenter
    {
        #region Public Properties

        public static readonly DependencyProperty BackgroundProperty =
            Panel.BackgroundProperty.AddOwner(
                typeof(ContentPresenterEx),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.AffectsRender));

        public Brush Background
        {
            get => (Brush)GetValue(BackgroundProperty);
            set => SetValue(BackgroundProperty, value);
        }

        public static readonly DependencyProperty BackgroundSizingProperty =
            DependencyProperty.Register(
                nameof(BackgroundSizing),
                typeof(ModernWpf.Controls.BackgroundSizing),
                typeof(ContentPresenterEx),
                new FrameworkPropertyMetadata(
                    ModernWpf.Controls.BackgroundSizing.InnerBorderEdge,
                    FrameworkPropertyMetadataOptions.AffectsRender));

        public BackgroundSizing BackgroundSizing
        {
            get => (BackgroundSizing)GetValue(BackgroundSizingProperty);
            set => SetValue(BackgroundSizingProperty, value);
        }

        public static readonly DependencyProperty BackgroundTransitionProperty =
            DependencyProperty.Register(
                nameof(BackgroundTransition),
                typeof(BrushTransition),
                typeof(ContentPresenterEx),
                new PropertyMetadata(null));

        public BrushTransition BackgroundTransition
        {
            get => (BrushTransition)GetValue(BackgroundTransitionProperty);
            set => SetValue(BackgroundTransitionProperty, value);
        }

        public static readonly DependencyProperty BorderBrushProperty =
            Border.BorderBrushProperty.AddOwner(
                typeof(ContentPresenterEx),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.AffectsRender));

        public Brush BorderBrush
        {
            get => (Brush)GetValue(BorderBrushProperty);
            set => SetValue(BorderBrushProperty, value);
        }

        public static readonly DependencyProperty BorderThicknessProperty =
            Border.BorderThicknessProperty.AddOwner(
                typeof(ContentPresenterEx),
                new FrameworkPropertyMetadata(
                    new Thickness(),
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.AffectsArrange |
                    FrameworkPropertyMetadataOptions.AffectsRender));

        public Thickness BorderThickness
        {
            get => (Thickness)GetValue(BorderThicknessProperty);
            set => SetValue(BorderThicknessProperty, value);
        }

        public static readonly DependencyProperty ContentTransitionsProperty =
            DependencyProperty.Register(
                nameof(ContentTransitions),
                typeof(TransitionCollection),
                typeof(ContentPresenterEx),
                new PropertyMetadata(null));

        public TransitionCollection ContentTransitions
        {
            get => (TransitionCollection)GetValue(ContentTransitionsProperty);
            set => SetValue(ContentTransitionsProperty, value);
        }

        public static readonly DependencyProperty HorizontalContentAlignmentProperty =
            DependencyProperty.Register(
                nameof(HorizontalContentAlignment),
                typeof(HorizontalAlignment),
                typeof(ContentPresenterEx),
                new FrameworkPropertyMetadata(
                    HorizontalAlignment.Stretch,
                    FrameworkPropertyMetadataOptions.AffectsArrange));

        public HorizontalAlignment HorizontalContentAlignment
        {
            get => (HorizontalAlignment)GetValue(HorizontalContentAlignmentProperty);
            set => SetValue(HorizontalContentAlignmentProperty, value);
        }

        public static readonly DependencyProperty VerticalContentAlignmentProperty =
            DependencyProperty.Register(
                nameof(VerticalContentAlignment),
                typeof(VerticalAlignment),
                typeof(ContentPresenterEx),
                new FrameworkPropertyMetadata(
                    VerticalAlignment.Stretch,
                    FrameworkPropertyMetadataOptions.AffectsArrange));

        public VerticalAlignment VerticalContentAlignment
        {
            get => (VerticalAlignment)GetValue(VerticalContentAlignmentProperty);
            set => SetValue(VerticalContentAlignmentProperty, value);
        }

        public static readonly DependencyProperty CornerRadiusProperty =
            Border.CornerRadiusProperty.AddOwner(
                typeof(ContentPresenterEx),
                new FrameworkPropertyMetadata(
                    new CornerRadius(),
                    FrameworkPropertyMetadataOptions.AffectsRender));

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        /// <summary>
        /// DependencyProperty for <see cref="FontFamily" /> property.
        /// </summary>
        public static readonly DependencyProperty FontFamilyProperty =
                TextElement.FontFamilyProperty.AddOwner(typeof(ContentPresenterEx));

        /// <summary>
        /// The FontFamily property specifies the name of font family.
        /// </summary>
        [Localizability(LocalizationCategory.Font)]
        public FontFamily FontFamily
        {
            get { return (FontFamily)GetValue(FontFamilyProperty); }
            set { SetValue(FontFamilyProperty, value); }
        }

        /// <summary>
        /// DependencyProperty for <see cref="FontStyle" /> property.
        /// </summary>
        public static readonly DependencyProperty FontStyleProperty =
                TextElement.FontStyleProperty.AddOwner(typeof(ContentPresenterEx));

        /// <summary>
        /// The FontStyle property requests normal, italic, and oblique faces within a font family.
        /// </summary>
        public FontStyle FontStyle
        {
            get { return (FontStyle)GetValue(FontStyleProperty); }
            set { SetValue(FontStyleProperty, value); }
        }

        /// <summary>
        /// DependencyProperty for <see cref="FontWeight" /> property.
        /// </summary>
        public static readonly DependencyProperty FontWeightProperty =
                TextElement.FontWeightProperty.AddOwner(typeof(ContentPresenterEx));

        /// <summary>
        /// The FontWeight property specifies the weight of the font.
        /// </summary>
        public FontWeight FontWeight
        {
            get { return (FontWeight)GetValue(FontWeightProperty); }
            set { SetValue(FontWeightProperty, value); }
        }

        /// <summary>
        /// DependencyProperty for <see cref="FontStretch" /> property.
        /// </summary>
        public static readonly DependencyProperty FontStretchProperty =
                TextElement.FontStretchProperty.AddOwner(typeof(ContentPresenterEx));

        /// <summary>
        /// The FontStretch property selects a normal, condensed, or extended face from a font family.
        /// </summary>
        public FontStretch FontStretch
        {
            get { return (FontStretch)GetValue(FontStretchProperty); }
            set { SetValue(FontStretchProperty, value); }
        }

        /// <summary>
        /// DependencyProperty for <see cref="FontSize" /> property.
        /// </summary>
        public static readonly DependencyProperty FontSizeProperty =
                TextElement.FontSizeProperty.AddOwner(typeof(ContentPresenterEx));

        /// <summary>
        /// The FontSize property specifies the size of the font.
        /// </summary>
        [TypeConverter(typeof(FontSizeConverter))]
        [Localizability(LocalizationCategory.None)]
        public double FontSize
        {
            get { return (double)GetValue(FontSizeProperty); }
            set { SetValue(FontSizeProperty, value); }
        }

        /// <summary>
        /// DependencyProperty for <see cref="Foreground" /> property.
        /// </summary>
        public static readonly DependencyProperty ForegroundProperty =
                TextElement.ForegroundProperty.AddOwner(typeof(ContentPresenterEx));

        /// <summary>
        /// The Foreground property specifies the foreground brush of an element's text content.
        /// </summary>
        public Brush Foreground
        {
            get { return (Brush)GetValue(ForegroundProperty); }
            set { SetValue(ForegroundProperty, value); }
        }

        public static readonly DependencyProperty CharacterSpacingProperty =
            DependencyProperty.Register(
                nameof(CharacterSpacing),
                typeof(int),
                typeof(ContentPresenterEx),
                new FrameworkPropertyMetadata(
                    0,
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.AffectsRender));

        public int CharacterSpacing
        {
            get => (int)GetValue(CharacterSpacingProperty);
            set => SetValue(CharacterSpacingProperty, value);
        }

        public static readonly DependencyProperty IsTextScaleFactorEnabledProperty =
            DependencyProperty.Register(
                nameof(IsTextScaleFactorEnabled),
                typeof(bool),
                typeof(ContentPresenterEx),
                new FrameworkPropertyMetadata(
                    true,
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.AffectsRender));

        public bool IsTextScaleFactorEnabled
        {
            get => (bool)GetValue(IsTextScaleFactorEnabledProperty);
            set => SetValue(IsTextScaleFactorEnabledProperty, value);
        }

        /// <summary>
        /// DependencyProperty for <see cref="LineHeight" /> property.
        /// </summary>
        public static readonly DependencyProperty LineHeightProperty =
            DependencyProperty.Register(
                nameof(LineHeight),
                typeof(double),
                typeof(ContentPresenterEx),
                new FrameworkPropertyMetadata(
                    0.0,
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.AffectsRender,
                    OnTextBlockPropertyChanged),
                IsValidLineHeight);

        /// <summary>
        /// The LineHeight property specifies the height of each generated line box.
        /// </summary>
        [TypeConverter(typeof(LengthConverter))]
        public double LineHeight
        {
            get { return (double)GetValue(LineHeightProperty); }
            set { SetValue(LineHeightProperty, value); }
        }

        /// <summary>
        /// DependencyProperty for <see cref="LineStackingStrategy" /> property.
        /// </summary>
        public static readonly DependencyProperty LineStackingStrategyProperty =
                Block.LineStackingStrategyProperty.AddOwner(
                    typeof(ContentPresenterEx),
                    new FrameworkPropertyMetadata(
                        LineStackingStrategy.MaxHeight,
                        FrameworkPropertyMetadataOptions.AffectsMeasure |
                        FrameworkPropertyMetadataOptions.AffectsRender,
                        OnTextBlockPropertyChanged));

        /// <summary>
        /// The LineStackingStrategy property specifies how lines are placed
        /// </summary>
        public LineStackingStrategy LineStackingStrategy
        {
            get { return (LineStackingStrategy)GetValue(LineStackingStrategyProperty); }
            set { SetValue(LineStackingStrategyProperty, value); }
        }

        public static readonly DependencyProperty MaxLinesProperty =
            DependencyProperty.Register(
                nameof(MaxLines),
                typeof(int),
                typeof(ContentPresenterEx),
                new FrameworkPropertyMetadata(
                    0,
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.AffectsRender,
                    OnTextBlockPropertyChanged),
                IsValidMaxLines);

        public int MaxLines
        {
            get => (int)GetValue(MaxLinesProperty);
            set => SetValue(MaxLinesProperty, value);
        }

        public static readonly DependencyProperty OpticalMarginAlignmentProperty =
            DependencyProperty.Register(
                nameof(OpticalMarginAlignment),
                typeof(ModernWpf.OpticalMarginAlignment),
                typeof(ContentPresenterEx),
                new FrameworkPropertyMetadata(
                    ModernWpf.OpticalMarginAlignment.None,
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.AffectsRender));

        public ModernWpf.OpticalMarginAlignment OpticalMarginAlignment
        {
            get => (ModernWpf.OpticalMarginAlignment)GetValue(OpticalMarginAlignmentProperty);
            set => SetValue(OpticalMarginAlignmentProperty, value);
        }

        public static readonly DependencyProperty PaddingProperty =
            Control.PaddingProperty.AddOwner(
                typeof(ContentPresenterEx),
                new FrameworkPropertyMetadata(
                    new Thickness(),
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.AffectsArrange));

        public Thickness Padding
        {
            get => (Thickness)GetValue(PaddingProperty);
            set => SetValue(PaddingProperty, value);
        }

        public static readonly DependencyProperty TextLineBoundsProperty =
            DependencyProperty.Register(
                nameof(TextLineBounds),
                typeof(ModernWpf.TextLineBounds),
                typeof(ContentPresenterEx),
                new FrameworkPropertyMetadata(
                    ModernWpf.TextLineBounds.Full,
                    FrameworkPropertyMetadataOptions.AffectsMeasure |
                    FrameworkPropertyMetadataOptions.AffectsRender));

        public ModernWpf.TextLineBounds TextLineBounds
        {
            get => (ModernWpf.TextLineBounds)GetValue(TextLineBoundsProperty);
            set => SetValue(TextLineBoundsProperty, value);
        }

        /// <summary>
        /// DependencyProperty for <see cref="TextWrapping" /> property.
        /// </summary>
        public static readonly DependencyProperty TextWrappingProperty =
                TextBlock.TextWrappingProperty.AddOwner(
                        typeof(ContentPresenterEx),
                        new FrameworkPropertyMetadata(
                                TextWrapping.NoWrap,
                                FrameworkPropertyMetadataOptions.AffectsMeasure |
                                FrameworkPropertyMetadataOptions.AffectsRender,
                                OnTextBlockPropertyChanged));

        /// <summary>
        /// The TextWrapping property controls whether or not text wraps 
        /// when it reaches the flow edge of its containing block box.
        /// </summary>
        public TextWrapping TextWrapping
        {
            get { return (TextWrapping)GetValue(TextWrappingProperty); }
            set { SetValue(TextWrappingProperty, value); }
        }

        private static bool IsValidMaxLines(object value)
        {
            return value is int maxLines && maxLines >= 0;
        }

        private static bool IsValidLineHeight(object value)
        {
            return value is double lineHeight && lineHeight >= 0 && !double.IsInfinity(lineHeight) && !double.IsNaN(lineHeight);
        }

        private static void OnTextBlockPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ContentPresenterEx)d).ApplyTextProperties();
        }

        #endregion

        #region Private Properties

        private bool IsUsingDefaultTemplate { get; set; }

        private TextBlock _textBlock;
        private TextBlock TextBlock
        {
            get => _textBlock;
            set
            {
                if (_textBlock != null)
                {
                    _textBlock.ClearValue(TextBlock.TextWrappingProperty);
                    _textBlock.ClearValue(TextBlock.LineHeightProperty);
                    _textBlock.ClearValue(TextBlock.LineStackingStrategyProperty);
                    _textBlock.ClearValue(MaxHeightProperty);
                    _textBlock.ClearValue(ClipToBoundsProperty);
                }

                _textBlock = value;

                ApplyTextProperties();
            }
        }

        private AccessText _accessText;
        private AccessText AccessText
        {
            get => _accessText;
            set
            {
                if (_accessText != null)
                {
                    _accessText.ClearValue(AccessText.TextWrappingProperty);
                }

                _accessText = value;

                ApplyTextProperties();
            }
        }

        #endregion

        #region Protected Methods

        protected override DataTemplate ChooseTemplate()
        {
            DataTemplate template = null;
            object content = Content;

            // ContentTemplate has first stab
            template = ContentTemplate;

            // no ContentTemplate set, try ContentTemplateSelector
            if (template == null)
            {
                if (ContentTemplateSelector != null)
                {
                    template = ContentTemplateSelector.SelectTemplate(content, this);
                }
            }

            // if that failed, try the default TemplateSelector
            if (template == null)
            {
                template = base.ChooseTemplate();
                IsUsingDefaultTemplate = true;
            }
            else
            {
                IsUsingDefaultTemplate = false;
            }

            return template;
        }

        protected override Size MeasureOverride(Size constraint)
        {
            var chrome = GetChromeThickness();
            var desired = base.MeasureOverride(LayoutChromeHelper.Deflate(constraint, chrome));
            return LayoutChromeHelper.Inflate(desired, chrome);
        }

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            var chrome = GetChromeThickness();
            var contentSize = LayoutChromeHelper.Deflate(arrangeSize, chrome);

            base.ArrangeOverride(contentSize);

            for (int i = 0; i < VisualChildrenCount; i++)
            {
                if (GetVisualChild(i) is UIElement child)
                {
                    child.Arrange(GetContentArrangeRect(child, contentSize, chrome));
                }
            }

            return arrangeSize;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            DrawChrome(drawingContext, RenderSize);
            base.OnRender(drawingContext);
        }

        protected override Geometry GetLayoutClip(Size layoutSlotSize)
        {
            return LayoutChromeHelper.CreateRoundedLayoutClip(
                layoutSlotSize,
                CornerRadius,
                base.GetLayoutClip(layoutSlotSize));
        }

        protected override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters)
        {
            if (!LayoutChromeHelper.FillContainsRoundedRectangle(RenderSize, CornerRadius, hitTestParameters.HitPoint))
            {
                return null;
            }

            return base.HitTestCore(hitTestParameters);
        }

        protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
        {
            base.OnVisualChildrenChanged(visualAdded, visualRemoved);

            if (visualAdded != null && IsUsingDefaultTemplate)
            {
                if (visualAdded is TextBlock textBlock)
                {
                    TextBlock = textBlock;
                }
                else if (visualAdded is AccessText accessText)
                {
                    AccessText = accessText;
                }
            }
            else if (visualRemoved != null)
            {
                if (visualRemoved == TextBlock)
                {
                    TextBlock = null;
                }
                else if (visualRemoved == AccessText)
                {
                    AccessText = null;
                }
            }
        }

        #endregion

        private void ApplyTextProperties()
        {
            if (_textBlock != null)
            {
                _textBlock.TextWrapping = TextWrapping;
                if (LineHeight > 0)
                {
                    _textBlock.LineHeight = LineHeight;
                }
                else
                {
                    _textBlock.ClearValue(TextBlock.LineHeightProperty);
                }

                _textBlock.LineStackingStrategy = LineStackingStrategy;
                ApplyMaxLines(_textBlock);
            }

            if (_accessText != null)
            {
                _accessText.TextWrapping = TextWrapping;
            }
        }

        private void ApplyMaxLines(TextBlock textBlock)
        {
            if (MaxLines > 0)
            {
                var effectiveLineHeight = GetEffectiveLineHeight(textBlock);
                if (effectiveLineHeight > 0)
                {
                    textBlock.MaxHeight = MaxLines * effectiveLineHeight;
                    textBlock.ClipToBounds = true;
                    return;
                }
            }

            textBlock.ClearValue(MaxHeightProperty);
            textBlock.ClearValue(ClipToBoundsProperty);
        }

        private double GetEffectiveLineHeight(TextBlock textBlock)
        {
            if (LineHeight > 0)
            {
                return LineHeight;
            }

            var fontSize = textBlock.FontSize;
            if (fontSize <= 0 || double.IsNaN(fontSize) || double.IsInfinity(fontSize))
            {
                return 0;
            }

            var lineSpacing = textBlock.FontFamily?.LineSpacing ?? 1.0;
            if (lineSpacing <= 0 || double.IsNaN(lineSpacing) || double.IsInfinity(lineSpacing))
            {
                lineSpacing = 1.0;
            }

            return fontSize * lineSpacing;
        }

        private void DrawChrome(DrawingContext drawingContext, Size renderSize)
        {
            if (renderSize.Width <= 0 || renderSize.Height <= 0)
            {
                return;
            }

            var borderThickness = BorderThickness;
            LayoutChromeHelper.DrawChrome(
                drawingContext,
                renderSize,
                Background,
                BackgroundSizing,
                BorderBrush,
                borderThickness,
                CornerRadius);
        }

        private Thickness GetChromeThickness()
        {
            var borderThickness = BorderThickness;
            var padding = Padding;
            return LayoutChromeHelper.Add(borderThickness, padding);
        }

        private Rect GetContentArrangeRect(UIElement child, Size availableSize, Thickness chrome)
        {
            var desired = child.DesiredSize;
            var width = HorizontalContentAlignment == HorizontalAlignment.Stretch ? availableSize.Width : desired.Width;
            var height = VerticalContentAlignment == VerticalAlignment.Stretch ? availableSize.Height : desired.Height;

            return new Rect(
                chrome.Left + GetHorizontalAlignmentOffset(availableSize.Width, width),
                chrome.Top + GetVerticalAlignmentOffset(availableSize.Height, height),
                Math.Max(0, width),
                Math.Max(0, height));
        }

        private double GetHorizontalAlignmentOffset(double availableWidth, double contentWidth)
        {
            switch (HorizontalContentAlignment)
            {
                case HorizontalAlignment.Center:
                    return (availableWidth - contentWidth) / 2;
                case HorizontalAlignment.Right:
                    return availableWidth - contentWidth;
                default:
                    return 0;
            }
        }

        private double GetVerticalAlignmentOffset(double availableHeight, double contentHeight)
        {
            switch (VerticalContentAlignment)
            {
                case VerticalAlignment.Center:
                    return (availableHeight - contentHeight) / 2;
                case VerticalAlignment.Bottom:
                    return availableHeight - contentHeight;
                default:
                    return 0;
            }
        }
    }
}
