// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ModernWpf.Automation.Peers;
using ModernWpf.Controls.Primitives;

namespace ModernWpf.Controls
{
    enum RatingControlStates
    {
        Disabled = 0,
        Set = 1,
        PointerOverSet = 2,
        PointerOverPlaceholder = 3, // Also functions as the pointer over unset state at the moment
        Placeholder = 4,
        Unset = 5,
        Null = 6
    }

    enum RatingInfoType
    {
        None,
        Font,
        Image
    }

    public partial class RatingControl : Control
    {
        const double c_scaleAnimationCenterPointXValue = 16.0;
        const double c_scaleAnimationCenterPointYValue = 16.0;
        static readonly Thickness c_focusVisualMargin = new Thickness(-8, -7, -8, 0);
        const double c_captionSpacing = 12;
        const double c_defaultFontSizeForRendering = 32; // (32 = 2 * [default fontsize] -- because of double size rendering), remove when MSFT #10030063 is done
        const double c_defaultItemSpacing = 8.0;
        const double c_defaultCaptionTopMargin = -6.0;
        const string c_fontSizeForRenderingKey = "RatingControlFontSizeForRendering";
        const string c_itemSpacingKey = "RatingControlItemSpacing";
        const string c_captionTopMarginKey = "RatingControlCaptionTopMargin";

        const double c_noValueSetSentinel = -1.0;

        static RatingControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RatingControl), new FrameworkPropertyMetadata(typeof(RatingControl)));
            FontFamilyProperty.OverrideMetadata(typeof(RatingControl), new FrameworkPropertyMetadata(OnFontFamilyPropertyChanged));
        }

        public RatingControl()
        {
        }

        double RenderingRatingFontSize
        {
            get
            {
                if (m_scaledFontSizeForRendering < 0)
                {
                    EnsureResourcesLoaded();
                    return m_fontSizeForRendering;
                }

                return m_scaledFontSizeForRendering;
            }
        }

        double ActualRatingFontSize => RenderingRatingFontSize / 2;

        double ItemSpacing
        {
            get
            {
                EnsureResourcesLoaded();
                return m_itemSpacing;
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            RecycleEvents();

            if (GetTemplateChild("CaptionStackPanel") is StackPanel captionStackPanel)
            {
                m_captionStackPanel = captionStackPanel;
            }

            if (GetTemplateChild("Caption") is TextBlock captionTextBlock)
            {
                m_captionTextBlock = captionTextBlock;
                captionTextBlock.SizeChanged += OnCaptionSizeChanged;
            }

            if (GetTemplateChild("RatingBackgroundStackPanel") is StackPanelEx backgroundStackPanel)
            {
                m_backgroundStackPanel = backgroundStackPanel;
                backgroundStackPanel.LostMouseCapture += OnPointerCaptureLostBackgroundStackPanel;
                backgroundStackPanel.MouseMove += OnPointerMovedOverBackgroundStackPanel;
                backgroundStackPanel.MouseEnter += OnPointerEnteredBackgroundStackPanel;
                backgroundStackPanel.MouseLeave += OnPointerExitedBackgroundStackPanel;
                backgroundStackPanel.MouseDown += OnPointerPressedBackgroundStackPanel;
                backgroundStackPanel.MouseUp += OnPointerReleasedBackgroundStackPanel;
            }

            m_foregroundStackPanel = GetTemplateChild("RatingForegroundStackPanel") as StackPanelEx;
            m_backgroundStackPanelTranslateTransform = GetTemplateChild("RatingBackgroundStackPanelTranslateTransform") as TranslateTransform;
            m_foregroundStackPanelTranslateTransform = GetTemplateChild("RatingForegroundStackPanelTranslateTransform") as TranslateTransform;

            // I've picked values so that these LOOK like the redlines, but these
            // values are not actually from the redlines because the redlines don't
            // consistently pick "distance from glyph"/"distance from edge of textbox"
            // so it's not possible to actually just have a consistent sizing model
            // here based on the redlines.
            SetValue(FocusVisualHelper.FocusVisualMarginProperty, c_focusVisualMargin);

            IsEnabledChanged += OnIsEnabledChanged;

            StampOutRatingItems();
        }

        double CoerceValueBetweenMinAndMax(double value)
        {
            if (value < 0.0) // Force all negative values to the sentinel "unset" value.
            {
                value = c_noValueSetSentinel;
            }
            else if (value <= 1.0)
            {
                value = 1.0;
            }
            else if (value > MaxRating)
            {
                value = MaxRating;
            }

            return value;
        }

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new RatingControlAutomationPeer(this);
        }

        // private methods 

        // TODO: call me when font size changes, and stuff like that, glyph, etc
        void StampOutRatingItems()
        {
            if (m_backgroundStackPanel == null || m_foregroundStackPanel == null)
            {
                // OnApplyTemplate() hasn't executed yet, this is being called 
                // from a property value changed handler for markup set values.

                return;
            }

            if (IsItemInfoPresentAndFontInfo())
            {
                EnsureResourcesLoaded();

                var textBlock = new TextBlock
                {
                    FontFamily = FontFamily,
                    Text = GetAppropriateGlyph(RatingControlStates.Set),
                    FontSize = m_fontSizeForRendering
                };
                textBlock.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                m_scaledFontSizeForRendering = textBlock.DesiredSize.Width;
            }
            else if (IsItemInfoPresentAndImageInfo())
            {
                EnsureResourcesLoaded();
                m_scaledFontSizeForRendering = m_fontSizeForRendering;
            }
            else
            {
                EnsureResourcesLoaded();
                m_scaledFontSizeForRendering = m_fontSizeForRendering;
            }

            // Background initialization:

            m_backgroundStackPanel.Children.Clear();

            if (IsItemInfoPresentAndFontInfo())
            {
                PopulateStackPanelWithItems("BackgroundGlyphDefaultTemplate", m_backgroundStackPanel, RatingControlStates.Unset);
            }
            else if (IsItemInfoPresentAndImageInfo())
            {
                PopulateStackPanelWithItems("BackgroundImageDefaultTemplate", m_backgroundStackPanel, RatingControlStates.Unset);
            }

            // Foreground initialization:
            m_foregroundStackPanel.Children.Clear();
            if (IsItemInfoPresentAndFontInfo())
            {
                PopulateStackPanelWithItems("ForegroundGlyphDefaultTemplate", m_foregroundStackPanel, RatingControlStates.Set);
            }
            else if (IsItemInfoPresentAndImageInfo())
            {
                PopulateStackPanelWithItems("ForegroundImageDefaultTemplate", m_foregroundStackPanel, RatingControlStates.Set);
            }

            double controlHeight = ActualHeight;
            if (double.IsNaN(controlHeight) || controlHeight <= 0)
            {
                controlHeight = m_fontSizeForRendering;
            }

            double yTranslation = (controlHeight - ActualRatingFontSize) / 2;
            if (m_backgroundStackPanelTranslateTransform != null)
            {
                m_backgroundStackPanelTranslateTransform.Y = yTranslation;
            }

            if (m_foregroundStackPanelTranslateTransform != null)
            {
                m_foregroundStackPanelTranslateTransform.Y = yTranslation;
            }

            if (MaxRating >= 1 && m_foregroundStackPanel.Children.Count > 0)
            {
                var firstItem = m_foregroundStackPanel.Children[0];
                firstItem.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                double defaultItemSpacing = firstItem.DesiredSize.Width - ActualRatingFontSize;
                double netItemSpacing = ItemSpacing - defaultItemSpacing;

                if (m_captionTextBlock != null)
                {
                    Thickness margin = m_captionTextBlock.Margin;
                    margin.Left = c_captionSpacing - defaultItemSpacing;
                    m_captionTextBlock.Margin = margin;
                }

                if (MaxRating >= 2)
                {
                    m_backgroundStackPanel.Spacing = netItemSpacing;
                    m_foregroundStackPanel.Spacing = netItemSpacing;
                }
            }

            UpdateRatingItemsAppearance();
        }

        void ReRenderCaption()
        {
            var captionTextBlock = m_captionTextBlock;
            if (captionTextBlock != null)
            {
                ResetControlSize();
            }
        }

        void UpdateRatingItemsAppearance()
        {
            if (m_foregroundStackPanel != null)
            {
                // TODO: MSFT 11521414 - complete disabled state functionality

                double placeholderValue = PlaceholderValue;
                double ratingValue = Value;
                double value = 0.0;

                if (m_isPointerOver)
                {
                    value = Math.Ceiling(m_mousePercentage * MaxRating);
                    if (ratingValue == c_noValueSetSentinel)
                    {
                        if (placeholderValue == -1)
                        {
                            VisualStateManager.GoToState(this, "PointerOverPlaceholder", false);
                            CustomizeStackPanel(m_foregroundStackPanel, RatingControlStates.PointerOverPlaceholder);
                        }
                        else
                        {
                            VisualStateManager.GoToState(this, "PointerOverUnselected", false);
                            // The API is locked, so we can't change this part to be consistent any more:
                            CustomizeStackPanel(m_foregroundStackPanel, RatingControlStates.PointerOverPlaceholder);
                        }
                    }
                    else
                    {
                        VisualStateManager.GoToState(this, "PointerOverSet", false);
                        CustomizeStackPanel(m_foregroundStackPanel, RatingControlStates.PointerOverSet);
                    }
                }
                else if (ratingValue > c_noValueSetSentinel)
                {
                    value = ratingValue;
                    VisualStateManager.GoToState(this, "Set", false);
                    CustomizeStackPanel(m_foregroundStackPanel, RatingControlStates.Set);
                }
                else if (placeholderValue > c_noValueSetSentinel)
                {
                    value = placeholderValue;
                    VisualStateManager.GoToState(this, "Placeholder", false);
                    CustomizeStackPanel(m_foregroundStackPanel, RatingControlStates.Placeholder);
                } // there's no "unset" state because the foreground items are simply cropped out

                if (!IsEnabled)
                {
                    // TODO: MSFT 11521414 - complete disabled state functionality [merge this code block with ifs above]
                    VisualStateManager.GoToState(this, "Disabled", false);
                    CustomizeStackPanel(m_foregroundStackPanel, RatingControlStates.Disabled);
                }

                int i = 0;
                foreach (var uiElement in m_foregroundStackPanel.Children)
                {
                    // Handle clips on stars
                    double width = RenderingRatingFontSize;
                    if (i + 1 > value)
                    {
                        if (i < value)
                        {
                            // partial stars
                            width *= value - Math.Floor(value);
                        }
                        else
                        {
                            // empty stars
                            width = 0.0;
                        }
                    }

                    Rect rect = new Rect(0, 0, width, RenderingRatingFontSize);

                    RectangleGeometry rg = new RectangleGeometry(rect);
                    ((UIElement)uiElement).Clip = rg;

                    i++;
                }

                ResetControlSize();
            }
        }

        void ApplyScaleExpressionAnimation(UIElement uiElement, int starIndex)
        {
            var transform = uiElement.RenderTransform as ScaleTransform;
            if (transform == null)
            {
                transform = new ScaleTransform();
                uiElement.RenderTransform = transform;
            }

            transform.ScaleX = 0.5;
            transform.ScaleY = 0.5;
            transform.CenterX = c_scaleAnimationCenterPointXValue;
            transform.CenterY = c_scaleAnimationCenterPointYValue;
        }

        void PopulateStackPanelWithItems(string templateName, Panel stackPanel, RatingControlStates state)
        {
            object lookup = Application.Current.FindResource(templateName);
            var dt = (DataTemplate)lookup;

            for (int i = 0; i < MaxRating; i++)
            {
                if (dt.LoadContent() is UIElement ui)
                {
                    CustomizeRatingItem(ui, state);
                    stackPanel.Children.Add(ui);
                    ApplyScaleExpressionAnimation(ui, i);
                }
            }
        }

        void CustomizeRatingItem(UIElement ui, RatingControlStates type)
        {
            if (IsItemInfoPresentAndFontInfo())
            {
                if (ui is TextBlock textBlock)
                {
                    textBlock.FontFamily = FontFamily;
                    textBlock.Text = GetAppropriateGlyph(type);
                }
            }
            else if (IsItemInfoPresentAndImageInfo())
            {
                if (ui is Image image)
                {
                    image.Source = GetAppropriateImageSource(type);
                    image.Width = RenderingRatingFontSize; // 
                    image.Height = RenderingRatingFontSize; // MSFT #10030063 Replacing with Rating size DPs
                }
            }
            else
            {
                Debug.Fail("Runtime error, ItemInfo property is null");
            }

        }

        void CustomizeStackPanel(Panel stackPanel, RatingControlStates state)
        {
            foreach (UIElement child in stackPanel.Children)
            {
                CustomizeRatingItem(child, state);
            }
        }

        bool IsItemInfoPresentAndFontInfo()
        {
            return m_infoType == RatingInfoType.Font;
        }
        bool IsItemInfoPresentAndImageInfo()
        {
            return m_infoType == RatingInfoType.Image;
        }

        string GetAppropriateGlyph(RatingControlStates type)
        {
            if (!IsItemInfoPresentAndFontInfo())
            {
                Debug.Fail("Runtime error, tried to retrieve a glyph when the ItemInfo is not a RatingItemGlyphInfo");
            }

            RatingItemFontInfo rifi = (RatingItemFontInfo)ItemInfo;

            switch (type)
            {
                case RatingControlStates.Disabled:
                    return GetNextGlyphIfNull(rifi.DisabledGlyph, RatingControlStates.Set);
                case RatingControlStates.PointerOverSet:
                    return GetNextGlyphIfNull(rifi.PointerOverGlyph, RatingControlStates.Set);
                case RatingControlStates.PointerOverPlaceholder:
                    return GetNextGlyphIfNull(rifi.PointerOverPlaceholderGlyph, RatingControlStates.Placeholder);
                case RatingControlStates.Placeholder:
                    return GetNextGlyphIfNull(rifi.PlaceholderGlyph, RatingControlStates.Set);
                case RatingControlStates.Unset:
                    return GetNextGlyphIfNull(rifi.UnsetGlyph, RatingControlStates.Set);
                case RatingControlStates.Null:
                    return string.Empty;
                default:
                    return rifi.Glyph; // "Set" state
            }
        }

        string GetNextGlyphIfNull(string glyph, RatingControlStates fallbackType)
        {
            if (string.IsNullOrEmpty(glyph))
            {
                if (fallbackType == RatingControlStates.Null)
                {
                    return string.Empty;
                }
                return GetAppropriateGlyph(fallbackType);
            }
            return glyph;
        }

        ImageSource GetAppropriateImageSource(RatingControlStates type)
        {
            if (!IsItemInfoPresentAndImageInfo())
            {
                Debug.Assert(false, "Runtime error, tried to retrieve an image when the ItemInfo is not a RatingItemImageInfo");
            }

            RatingItemImageInfo imageInfo = (RatingItemImageInfo)ItemInfo;

            switch (type)
            {
                case RatingControlStates.Disabled:
                    return GetNextImageIfNull(imageInfo.DisabledImage, RatingControlStates.Set);
                case RatingControlStates.PointerOverSet:
                    return GetNextImageIfNull(imageInfo.PointerOverImage, RatingControlStates.Set);
                case RatingControlStates.PointerOverPlaceholder:
                    return GetNextImageIfNull(imageInfo.PointerOverPlaceholderImage, RatingControlStates.Placeholder);
                case RatingControlStates.Placeholder:
                    return GetNextImageIfNull(imageInfo.PlaceholderImage, RatingControlStates.Set);
                case RatingControlStates.Unset:
                    return GetNextImageIfNull(imageInfo.UnsetImage, RatingControlStates.Set);
                case RatingControlStates.Null:
                    return null;
                default:
                    return imageInfo.Image; // "Set" state
            }
        }

        ImageSource GetNextImageIfNull(ImageSource image, RatingControlStates fallbackType)
        {
            if (image == null)
            {
                if (fallbackType == RatingControlStates.Null)
                {
                    return null;
                }
                return GetAppropriateImageSource(fallbackType);
            }
            return image;
        }

        void ResetControlSize()
        {
            Width = CalculateTotalRatingControlWidth();
            EnsureResourcesLoaded();
            Height = m_fontSizeForRendering;
        }

        void EnsureResourcesLoaded()
        {
            if (!m_resourcesLoaded)
            {
                m_fontSizeForRendering = GetResourceDouble(c_fontSizeForRenderingKey, c_defaultFontSizeForRendering);
                m_itemSpacing = GetResourceDouble(c_itemSpacingKey, c_defaultItemSpacing);
                m_captionTopMargin = GetResourceDouble(c_captionTopMarginKey, c_defaultCaptionTopMargin);
                m_resourcesLoaded = true;
            }

            double GetResourceDouble(string resourceKey, double fallbackValue)
            {
                object value = TryFindResource(resourceKey) ?? Application.Current?.TryFindResource(resourceKey);
                if (value is double doubleValue)
                {
                    return doubleValue;
                }

                if (value is IConvertible convertible)
                {
                    return convertible.ToDouble(CultureInfo.InvariantCulture);
                }

                return fallbackValue;
            }
        }

        void ChangeRatingBy(double change, bool originatedFromMouse)
        {
            if (change != 0.0)
            {
                double ratingValue = 0.0;
                double oldRatingValue = Value;
                if (oldRatingValue != c_noValueSetSentinel)
                {
                    // If the Value was programmatically set to a fraction, drop that fraction before we modify it
                    if ((int)Value != Value)
                    {
                        if (change == -1)
                        {
                            ratingValue = (int)Value;
                        }
                        else
                        {
                            ratingValue = (int)Value + change;
                        }
                    }
                    else
                    {
                        oldRatingValue = ratingValue = oldRatingValue;
                        ratingValue += change;
                    }
                }
                else
                {
                    ratingValue = InitialSetValue;
                }

                SetRatingTo(ratingValue, originatedFromMouse);
            }
        }

        void SetRatingTo(double newRating, bool originatedFromMouse)
        {
            double ratingValue = 0.0;
            double oldRatingValue = Value;

            ratingValue = Math.Min(newRating, MaxRating);
            ratingValue = Math.Max(ratingValue, 0.0);

            // The base case, and the you have no rating, and you pressed left case [wherein nothing should happen]
            if (oldRatingValue > c_noValueSetSentinel || ratingValue != 0.0)
            {
                if (!IsClearEnabled && ratingValue <= 0.0)
                {
                    SetCurrentValue(ValueProperty, 1.0);
                }
                else if (ratingValue == oldRatingValue && IsClearEnabled && (ratingValue != MaxRating || originatedFromMouse))
                {
                    // If you increase the Rating via the keyboard/gamepad when it's maxed, the value should stay stable.
                    // But if you click a star that represents the current Rating value, it should clear the rating.

                    SetCurrentValue(ValueProperty, c_noValueSetSentinel);
                }
                else if (ratingValue > 0.0)
                {
                    SetCurrentValue(ValueProperty, ratingValue);
                }
                else
                {
                    SetCurrentValue(ValueProperty, c_noValueSetSentinel);
                }

                // Notify that the Value has changed
                ValueChanged?.Invoke(this, null);
            }
        }

        void PrivateOnPropertyChanged(DependencyPropertyChangedEventArgs args)
        {
            var property = args.Property;
            // Do coercion first.
            if (property == MaxRatingProperty)
            {
                // Enforce minimum MaxRating
                var value = (int)args.NewValue;
                var coercedValue = Math.Max(1, value);

                if (Value > coercedValue)
                {
                    Value = coercedValue;
                }

                if (PlaceholderValue > coercedValue)
                {
                    PlaceholderValue = coercedValue;
                }

                if (coercedValue != value)
                {
                    SetValue(property, coercedValue);
                    return;
                }
            }
            else if (property == PlaceholderValueProperty || property == ValueProperty)
            {
                var value = (double)args.NewValue;
                var coercedValue = CoerceValueBetweenMinAndMax(value);
                if (value != coercedValue)
                {
                    SetValue(property, coercedValue);
                    // early return, we'll come back to handle the change to the corced value.
                    return;
                }
            }

            // Property value changed handling.
            if (property == CaptionProperty)
            {
                OnCaptionChanged(args);
            }
            else if (property == InitialSetValueProperty)
            {
                OnInitialSetValueChanged(args);
            }
            else if (property == IsClearEnabledProperty)
            {
                OnIsClearEnabledChanged(args);
            }
            else if (property == IsReadOnlyProperty)
            {
                OnIsReadOnlyChanged(args);
            }
            else if (property == ItemInfoProperty)
            {
                OnItemInfoChanged(args);
            }
            else if (property == MaxRatingProperty)
            {
                OnMaxRatingChanged(args);
            }
            else if (property == PlaceholderValueProperty)
            {
                OnPlaceholderValueChanged(args);
            }
            else if (property == ValueProperty)
            {
                OnValueChanged(args);
            }
        }

        void OnCaptionChanged(DependencyPropertyChangedEventArgs args)
        {
            ReRenderCaption();
        }

        static void OnFontFamilyPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((RatingControl)sender).OnFontFamilyChanged();
        }

        void OnFontFamilyChanged()
        {
            if (m_backgroundStackPanel != null) // We don't want to do this for the initial property set
            {
                for (int i = 0; i < MaxRating; i++)
                {
                    // FUTURE: handle image rating items
                    if (m_backgroundStackPanel.Children[i] is TextBlock backgroundTB)
                    {
                        CustomizeRatingItem(backgroundTB, RatingControlStates.Unset);
                    }

                    if (m_foregroundStackPanel.Children[i] is TextBlock foregroundTB)
                    {
                        CustomizeRatingItem(foregroundTB, RatingControlStates.Set);
                    }
                }
            }

            UpdateRatingItemsAppearance();
        }

        void OnInitialSetValueChanged(DependencyPropertyChangedEventArgs args)
        {

        }

        void OnIsClearEnabledChanged(DependencyPropertyChangedEventArgs args)
        {

        }

        void OnIsReadOnlyChanged(DependencyPropertyChangedEventArgs args)
        {
            // TODO: Colour changes - see spec
        }

        void OnItemInfoChanged(DependencyPropertyChangedEventArgs args)
        {
            bool changedType = false;

            if (ItemInfo == null)
            {
                m_infoType = RatingInfoType.None;
            }
            else if (ItemInfo is RatingItemFontInfo)
            {
                if (m_infoType != RatingInfoType.Font && m_backgroundStackPanel != null /* prevent calling StampOutRatingItems() twice at initialisation */)
                {
                    m_infoType = RatingInfoType.Font;
                    StampOutRatingItems();
                    changedType = true;
                }
            }
            else
            {
                if (m_infoType != RatingInfoType.Image)
                {
                    m_infoType = RatingInfoType.Image;
                    StampOutRatingItems();
                    changedType = true;
                }
            }

            // We don't want to do this for the initial property set
            // Or if we just stamped them out
            if (m_backgroundStackPanel != null && !changedType)
            {
                for (int i = 0; i < MaxRating; i++)
                {
                    CustomizeRatingItem(m_backgroundStackPanel.Children[i], RatingControlStates.Unset);
                    CustomizeRatingItem(m_foregroundStackPanel.Children[i], RatingControlStates.Set);
                }
            }

            UpdateRatingItemsAppearance();
        }

        void OnMaxRatingChanged(DependencyPropertyChangedEventArgs args)
        {
            StampOutRatingItems();
        }

        void OnPlaceholderValueChanged(DependencyPropertyChangedEventArgs args)
        {
            UpdateRatingItemsAppearance();
        }

        void OnValueChanged(DependencyPropertyChangedEventArgs args)
        {
            // Fire property change for UIA
            if (FrameworkElementAutomationPeer.FromElement(this) is AutomationPeer peer)
            {
                var ratingPeer = (RatingControlAutomationPeer)peer;
                ratingPeer.RaisePropertyChangedEvent(Value);
            }

            UpdateRatingItemsAppearance();
        }

        void OnIsEnabledChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            // MSFT 11521414 TODO: change states (add a state)
            UpdateRatingItemsAppearance();
        }

        void OnCaptionSizeChanged(object sender, SizeChangedEventArgs args)
        {
            // The caption's size changing means that the text scale factor has been updated and applied.
            // As such, we should re-run sizing and layout when this occurs.
            m_scaledFontSizeForRendering = -1;

            StampOutRatingItems();
            ResetControlSize();
        }

        void OnPointerCaptureLostBackgroundStackPanel(object sender, MouseEventArgs args)
        {
            // We capture the pointer because we want to support the drag off the
            // left side to clear the rating scenario. However, this means that
            // when we simply click to set values - we get here, but we don't want
            // to reset the scaling on the stars underneath the pointer.
            PointerExitedImpl(args, false /* resetScaleAnimation */);
            m_hasPointerCapture = false;
        }

        void OnPointerMovedOverBackgroundStackPanel(object sender, MouseEventArgs args)
        {
            if (!IsReadOnly)
            {
                var point = args.GetPosition(m_backgroundStackPanel);
                double xPosition = point.X;

                m_mousePercentage = (xPosition - m_firstItemOffset) / CalculateActualRatingWidth();

                UpdateRatingItemsAppearance();
                args.Handled = true;
            }
        }

        void OnPointerEnteredBackgroundStackPanel(object sender, MouseEventArgs args)
        {
            if (!IsReadOnly)
            {
                m_isPointerOver = true;

                if (m_backgroundStackPanel != null && MaxRating >= 1 && m_backgroundStackPanel.Children.Count > 0)
                {
                    var firstItem = m_backgroundStackPanel.Children[0];
                    var firstItemOffsetPoint = firstItem.TransformToVisual(m_backgroundStackPanel).Transform(new Point(0, 0));
                    m_firstItemOffset = firstItemOffsetPoint.X;
                }

                args.Handled = true;
            }
        }

        void OnPointerExitedBackgroundStackPanel(object sender, MouseEventArgs args)
        {
            PointerExitedImpl(args);
        }

        void PointerExitedImpl(MouseEventArgs args, bool resetScaleAnimation = true)
        {
            if (resetScaleAnimation)
            {
                m_isPointerOver = false;
            }

            if (!m_isPointerDown)
            {
                if (m_hasPointerCapture)
                {
                    m_backgroundStackPanel.ReleaseMouseCapture();
                    m_hasPointerCapture = false;
                }

                UpdateRatingItemsAppearance();
            }

            args.Handled = true;
        }

        void OnPointerPressedBackgroundStackPanel(object sender, MouseButtonEventArgs args)
        {
            if (!IsReadOnly)
            {
                m_isPointerDown = true;

                // We capture the pointer on pointer down because we want to support
                // the drag off the left side to clear the rating scenario.
                m_hasPointerCapture = m_backgroundStackPanel.CaptureMouse();
            }
        }

        void OnPointerReleasedBackgroundStackPanel(object sender, MouseButtonEventArgs args)
        {
            if (!IsReadOnly)
            {
                var point = args.GetPosition(m_backgroundStackPanel);
                var xPosition = point.X;

                double mousePercentage = xPosition / CalculateActualRatingWidth();
                SetRatingTo(Math.Ceiling(mousePercentage * MaxRating), true);
            }

            if (m_isPointerDown)
            {
                m_isPointerDown = false;
                UpdateRatingItemsAppearance();
            }

            if (m_hasPointerCapture)
            {
                m_backgroundStackPanel.ReleaseMouseCapture();
                m_hasPointerCapture = false;
            }
            Focus();
        }

        double CalculateTotalRatingControlWidth()
        {
            double totalWidth = CalculateActualRatingWidth();

            if (m_captionTextBlock != null)
            {
                var captionAsWinRT = (string)GetValue(CaptionProperty);

                if (captionAsWinRT.Length > 0)
                {
                    totalWidth += c_captionSpacing + m_captionTextBlock.ActualWidth;
                }
            }

            return totalWidth;
        }

        double CalculateStarCenter(int starIndex)
        {
            // TODO: replace hardcoding
            // MSFT #10030063
            // [real Rating Size * (starIndex + 0.5)] + (starIndex * itemSpacing)
            return (ActualRatingFontSize * (starIndex + 0.5)) + (starIndex * ItemSpacing);
        }

        double CalculateActualRatingWidth()
        {
            // TODO: replace hardcoding
            // MSFT #10030063
            // (max rating * rating size) + ((max rating - 1) * item spacing)
            return (MaxRating * ActualRatingFontSize) + ((MaxRating - 1) * ItemSpacing);
        }

        protected override void OnKeyDown(KeyEventArgs eventArgs)
        {
            if (eventArgs.Handled)
            {
                return;
            }

            if (!IsReadOnly)
            {
                bool handled = false;
                Key key = eventArgs.Key;

                double flowDirectionReverser = 1.0;

                if (FlowDirection == FlowDirection.RightToLeft)
                {
                    flowDirectionReverser *= -1.0;
                }

                var originalKey = eventArgs.Key;

                // Up down are right/left in keyboard only
                if (originalKey == Key.Up)
                {
                    key = Key.Right;
                    flowDirectionReverser = 1.0;
                }
                else if (originalKey == Key.Down)
                {
                    key = Key.Left;
                    flowDirectionReverser = 1.0;
                }

                switch (key)
                {
                    case Key.Left:
                        ChangeRatingBy(-1.0 * flowDirectionReverser, false);
                        handled = true;
                        break;
                    case Key.Right:
                        ChangeRatingBy(1.0 * flowDirectionReverser, false);
                        handled = true;
                        break;
                    case Key.Home:
                        SetRatingTo(0.0, false);
                        handled = true;
                        break;
                    case Key.End:
                        SetRatingTo(MaxRating, false);
                        handled = true;
                        break;
                    default:
                        break;
                }

                eventArgs.Handled = handled;
            }

            base.OnKeyDown(eventArgs);
        }

        void RecycleEvents()
        {
            var backgroundStackPanel = m_backgroundStackPanel;
            if (backgroundStackPanel != null)
            {
                backgroundStackPanel.LostMouseCapture -= OnPointerCaptureLostBackgroundStackPanel;
                backgroundStackPanel.MouseMove -= OnPointerMovedOverBackgroundStackPanel;
                backgroundStackPanel.MouseEnter -= OnPointerEnteredBackgroundStackPanel;
                backgroundStackPanel.MouseLeave -= OnPointerExitedBackgroundStackPanel;
                backgroundStackPanel.MouseDown -= OnPointerPressedBackgroundStackPanel;
                backgroundStackPanel.MouseUp -= OnPointerReleasedBackgroundStackPanel;
            }

            var captionTextBlock = m_captionTextBlock;
            if (captionTextBlock != null)
            {
                captionTextBlock.SizeChanged -= OnCaptionSizeChanged;
            }
        }

        // Private members
        StackPanel m_captionStackPanel;
        TextBlock m_captionTextBlock;

        StackPanelEx m_backgroundStackPanel;
        StackPanelEx m_foregroundStackPanel;

        TranslateTransform m_backgroundStackPanelTranslateTransform;
        TranslateTransform m_foregroundStackPanelTranslateTransform;

        bool m_isPointerOver = false;
        bool m_isPointerDown = false;
        bool m_hasPointerCapture = false;
        double m_mousePercentage = 0.0;
        double m_firstItemOffset = 0.0;

        RatingInfoType m_infoType = RatingInfoType.Font;

        bool m_resourcesLoaded = false;
        double m_fontSizeForRendering = c_defaultFontSizeForRendering;
        double m_itemSpacing = c_defaultItemSpacing;
        double m_captionTopMargin = c_defaultCaptionTopMargin;
        double m_scaledFontSizeForRendering = -1.0;
    }
}
