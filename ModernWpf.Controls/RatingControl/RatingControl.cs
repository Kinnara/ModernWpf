// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Diagnostics;
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
        Image,
        Path
    }

    public partial class RatingControl : Control
    {
        const double c_horizontalScaleAnimationCenterPoint = 0.5;
        const double c_verticalScaleAnimationCenterPoint = 0.8;
        static readonly Thickness c_focusVisualMargin = new Thickness(-8, -7, -8, 0);
        const int c_defaultRatingFontSizeForRendering = 32; // (32 = 2 * [default fontsize] -- because of double size rendering), remove when MSFT #10030063 is done
        const int c_defaultItemSpacing = 8;

        // 22 = 20(compensate for the -20 margin on StackPanel) + 2(magic number makes the text and star center-aligned)
        const double c_defaultCaptionTopMargin = 22;

        const double c_noValueSetSentinel = -1.0;

        static RatingControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RatingControl), new FrameworkPropertyMetadata(typeof(RatingControl)));
            FontFamilyProperty.OverrideMetadata(typeof(RatingControl), new FrameworkPropertyMetadata(OnFontFamilyPropertyChanged));
        }

        public RatingControl()
        {
        }

        double RenderingRatingFontSize => c_defaultRatingFontSizeForRendering;

        double ActualRatingFontSize => RenderingRatingFontSize / 2;

        double ItemSpacing => c_defaultItemSpacing;

        void UpdateCaptionMargins()
        {
            // We manually set margins to caption text to make it center-aligned with the stars
            // because star vertical center is 0.8 instead of the normal 0.5.
            // When text scale changes we need to update top margin to make the text follow start center.
            var captionTextBlock = m_captionTextBlock;
            if (captionTextBlock != null)
            {
                Thickness margin = captionTextBlock.Margin;
                margin.Top = c_defaultCaptionTopMargin - (ActualRatingFontSize * c_verticalScaleAnimationCenterPoint);

                captionTextBlock.Margin = margin;
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            RecycleEvents();

            if (GetTemplateChild("Caption") is TextBlock captionTextBlock)
            {
                m_captionTextBlock = captionTextBlock;
                captionTextBlock.SizeChanged += OnCaptionSizeChanged;
                UpdateCaptionMargins();
            }

            if (GetTemplateChild("RatingBackgroundStackPanel") is StackPanel backgroundStackPanel)
            {
                m_backgroundStackPanel = backgroundStackPanel;
                backgroundStackPanel.LostMouseCapture += OnPointerCaptureLostBackgroundStackPanel;
                backgroundStackPanel.MouseMove += OnPointerMovedOverBackgroundStackPanel;
                backgroundStackPanel.MouseEnter += OnPointerEnteredBackgroundStackPanel;
                backgroundStackPanel.MouseLeave += OnPointerExitedBackgroundStackPanel;
                backgroundStackPanel.MouseDown += OnPointerPressedBackgroundStackPanel;
                backgroundStackPanel.MouseUp += OnPointerReleasedBackgroundStackPanel;
            }

            m_foregroundStackPanel = GetTemplateChild("RatingForegroundStackPanel") as StackPanel;

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
            else if (IsItemInfoPresentAndPathInfo())
            {
                PopulateStackPanelWithItems("BackgroundPathDefaultTemplate", m_backgroundStackPanel, RatingControlStates.Unset);
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
            else if (IsItemInfoPresentAndPathInfo())
            {
                PopulateStackPanelWithItems("ForegroundPathDefaultTemplate", m_foregroundStackPanel, RatingControlStates.Set);
            }

            UpdateRatingItemsAppearance();
        }

        void ReRenderCaption()
        {
            var captionTextBlock = m_captionTextBlock;
            if (captionTextBlock != null)
            {
                ResetControlWidth();
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

                ResetControlWidth();
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
            transform.CenterX = c_defaultRatingFontSizeForRendering * c_horizontalScaleAnimationCenterPoint;
            transform.CenterY = c_defaultRatingFontSizeForRendering * c_verticalScaleAnimationCenterPoint;
        }

        void PopulateStackPanelWithItems(string templateName, StackPanel stackPanel, RatingControlStates state)
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
            else if (IsItemInfoPresentAndPathInfo())
            {
                if (ui is FontIconFallback pathControl)
                {
                    pathControl.Data = GetAppropriatePathData(type);
                }
            }
            else
            {
                Debug.Fail("Runtime error, ItemInfo property is null");
            }

        }

        void CustomizeStackPanel(StackPanel stackPanel, RatingControlStates state)
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
        bool IsItemInfoPresentAndPathInfo()
        {
            return m_infoType == RatingInfoType.Path;
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

        Geometry GetAppropriatePathData(RatingControlStates type)
        {
            if (!IsItemInfoPresentAndPathInfo())
            {
                Debug.Assert(false, "Runtime error, tried to retrieve a geometry when the ItemInfo is not a RatingItemPathInfo");
            }

            RatingItemPathInfo pathInfo = (RatingItemPathInfo)ItemInfo;

            switch (type)
            {
                case RatingControlStates.Disabled:
                    return GetNextGeometryIfNull(pathInfo.DisabledData, RatingControlStates.Set);
                case RatingControlStates.PointerOverSet:
                    return GetNextGeometryIfNull(pathInfo.PointerOverData, RatingControlStates.Set);
                case RatingControlStates.PointerOverPlaceholder:
                    return GetNextGeometryIfNull(pathInfo.PointerOverPlaceholderData, RatingControlStates.Placeholder);
                case RatingControlStates.Placeholder:
                    return GetNextGeometryIfNull(pathInfo.PlaceholderData, RatingControlStates.Set);
                case RatingControlStates.Unset:
                    return GetNextGeometryIfNull(pathInfo.UnsetData, RatingControlStates.Set);
                case RatingControlStates.Null:
                    return null;
                default:
                    return pathInfo.Data; // "Set" state
            }
        }

        Geometry GetNextGeometryIfNull(Geometry geometry, RatingControlStates fallbackType)
        {
            if (geometry == null)
            {
                if (fallbackType == RatingControlStates.Null)
                {
                    return null;
                }
                return GetAppropriatePathData(fallbackType);
            }
            return geometry;
        }

        void ResetControlWidth()
        {
            double newWidth = CalculateTotalRatingControlWidth();
            Width = newWidth;
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
            else if (ItemInfo is RatingItemPathInfo)
            {
                if (m_infoType != RatingInfoType.Path)
                {
                    m_infoType = RatingInfoType.Path;
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
            ResetControlWidth();
        }

        void OnPointerCaptureLostBackgroundStackPanel(object sender, MouseEventArgs args)
        {
            // We capture the pointer because we want to support the drag off the
            // left side to clear the rating scenario. However, this means that
            // when we simply click to set values - we get here, but we don't want
            // to reset the scaling on the stars underneath the pointer.
            PointerExitedImpl(args, false /* resetScaleAnimation */);
        }

        void OnPointerMovedOverBackgroundStackPanel(object sender, MouseEventArgs args)
        {
            if (!IsReadOnly)
            {
                var point = args.GetPosition(m_backgroundStackPanel);
                double xPosition = point.X;

                m_mousePercentage = xPosition / CalculateActualRatingWidth();

                UpdateRatingItemsAppearance();
                args.Handled = true;
            }
        }

        void OnPointerEnteredBackgroundStackPanel(object sender, MouseEventArgs args)
        {
            if (!IsReadOnly)
            {
                m_isPointerOver = true;
                args.Handled = true;
            }
        }

        void OnPointerExitedBackgroundStackPanel(object sender, MouseEventArgs args)
        {
            PointerExitedImpl(args);
        }

        void PointerExitedImpl(MouseEventArgs args, bool resetScaleAnimation = true)
        {
            var point = args.GetPosition(m_backgroundStackPanel);

            if (resetScaleAnimation)
            {
                m_isPointerOver = false;
            }

            if (!m_isPointerDown)
            {
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
                m_backgroundStackPanel.CaptureMouse();
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

            m_backgroundStackPanel.ReleaseMouseCapture();
            Focus();
        }

        double CalculateTotalRatingControlWidth()
        {
            double ratingStarsWidth = CalculateActualRatingWidth();
            var captionAsWinRT = (string)GetValue(CaptionProperty);
            double textSpacing = 0.0;

            if (captionAsWinRT.Length > 0)
            {
                textSpacing = ItemSpacing;
            }

            double captionWidth = 0.0;

            if (m_captionTextBlock != null)
            {
                captionWidth = m_captionTextBlock.ActualWidth;
            }

            return ratingStarsWidth + textSpacing + captionWidth;
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
        TextBlock m_captionTextBlock;

        StackPanel m_backgroundStackPanel;
        StackPanel m_foregroundStackPanel;

        bool m_isPointerOver = false;
        bool m_isPointerDown = false;
        double m_mousePercentage = 0.0;

        RatingInfoType m_infoType = RatingInfoType.Font;
    }
}
