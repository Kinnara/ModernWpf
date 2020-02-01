// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows;

namespace ModernWpf.Controls.Primitives
{
    public sealed class CommandBarFlyoutCommandBarTemplateSettings : DependencyObject
    {
        internal CommandBarFlyoutCommandBarTemplateSettings()
        {
        }

        #region CloseAnimationEndPosition

        private static readonly DependencyPropertyKey CloseAnimationEndPositionPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(CloseAnimationEndPosition),
                typeof(double),
                typeof(CommandBarFlyoutCommandBarTemplateSettings),
                null);

        public static readonly DependencyProperty CloseAnimationEndPositionProperty =
            CloseAnimationEndPositionPropertyKey.DependencyProperty;

        public double CloseAnimationEndPosition
        {
            get => (double)GetValue(CloseAnimationEndPositionProperty);
            internal set => SetValue(CloseAnimationEndPositionPropertyKey, value);
        }

        #endregion

        #region ContentClipRect

        private static readonly DependencyPropertyKey ContentClipRectPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(ContentClipRect),
                typeof(Rect),
                typeof(CommandBarFlyoutCommandBarTemplateSettings),
                null);

        public static readonly DependencyProperty ContentClipRectProperty =
            ContentClipRectPropertyKey.DependencyProperty;

        public Rect ContentClipRect
        {
            get => (Rect)GetValue(ContentClipRectProperty);
            internal set => SetValue(ContentClipRectPropertyKey, value);
        }

        #endregion

        #region CurrentWidth

        private static readonly DependencyPropertyKey CurrentWidthPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(CurrentWidth),
                typeof(double),
                typeof(CommandBarFlyoutCommandBarTemplateSettings),
                null);

        public static readonly DependencyProperty CurrentWidthProperty =
            CurrentWidthPropertyKey.DependencyProperty;

        public double CurrentWidth
        {
            get => (double)GetValue(CurrentWidthProperty);
            internal set => SetValue(CurrentWidthPropertyKey, value);
        }

        #endregion

        #region ExpandDownAnimationEndPosition

        private static readonly DependencyPropertyKey ExpandDownAnimationEndPositionPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(ExpandDownAnimationEndPosition),
                typeof(double),
                typeof(CommandBarFlyoutCommandBarTemplateSettings),
                null);

        public static readonly DependencyProperty ExpandDownAnimationEndPositionProperty =
            ExpandDownAnimationEndPositionPropertyKey.DependencyProperty;

        public double ExpandDownAnimationEndPosition
        {
            get => (double)GetValue(ExpandDownAnimationEndPositionProperty);
            internal set => SetValue(ExpandDownAnimationEndPositionPropertyKey, value);
        }

        #endregion

        #region ExpandDownAnimationHoldPosition

        private static readonly DependencyPropertyKey ExpandDownAnimationHoldPositionPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(ExpandDownAnimationHoldPosition),
                typeof(double),
                typeof(CommandBarFlyoutCommandBarTemplateSettings),
                null);

        public static readonly DependencyProperty ExpandDownAnimationHoldPositionProperty =
            ExpandDownAnimationHoldPositionPropertyKey.DependencyProperty;

        public double ExpandDownAnimationHoldPosition
        {
            get => (double)GetValue(ExpandDownAnimationHoldPositionProperty);
            internal set => SetValue(ExpandDownAnimationHoldPositionPropertyKey, value);
        }

        #endregion

        #region ExpandDownAnimationStartPosition

        private static readonly DependencyPropertyKey ExpandDownAnimationStartPositionPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(ExpandDownAnimationStartPosition),
                typeof(double),
                typeof(CommandBarFlyoutCommandBarTemplateSettings),
                null);

        public static readonly DependencyProperty ExpandDownAnimationStartPositionProperty =
            ExpandDownAnimationStartPositionPropertyKey.DependencyProperty;

        public double ExpandDownAnimationStartPosition
        {
            get => (double)GetValue(ExpandDownAnimationStartPositionProperty);
            internal set => SetValue(ExpandDownAnimationStartPositionPropertyKey, value);
        }

        #endregion

        #region ExpandDownOverflowVerticalPosition

        private static readonly DependencyPropertyKey ExpandDownOverflowVerticalPositionPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(ExpandDownOverflowVerticalPosition),
                typeof(double),
                typeof(CommandBarFlyoutCommandBarTemplateSettings),
                null);

        public static readonly DependencyProperty ExpandDownOverflowVerticalPositionProperty =
            ExpandDownOverflowVerticalPositionPropertyKey.DependencyProperty;

        public double ExpandDownOverflowVerticalPosition
        {
            get => (double)GetValue(ExpandDownOverflowVerticalPositionProperty);
            internal set => SetValue(ExpandDownOverflowVerticalPositionPropertyKey, value);
        }

        #endregion

        #region ExpandedWidth

        private static readonly DependencyPropertyKey ExpandedWidthPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(ExpandedWidth),
                typeof(double),
                typeof(CommandBarFlyoutCommandBarTemplateSettings),
                null);

        public static readonly DependencyProperty ExpandedWidthProperty =
            ExpandedWidthPropertyKey.DependencyProperty;

        public double ExpandedWidth
        {
            get => (double)GetValue(ExpandedWidthProperty);
            internal set => SetValue(ExpandedWidthPropertyKey, value);
        }

        #endregion

        #region ExpandUpAnimationEndPosition

        private static readonly DependencyPropertyKey ExpandUpAnimationEndPositionPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(ExpandUpAnimationEndPosition),
                typeof(double),
                typeof(CommandBarFlyoutCommandBarTemplateSettings),
                null);

        public static readonly DependencyProperty ExpandUpAnimationEndPositionProperty =
            ExpandUpAnimationEndPositionPropertyKey.DependencyProperty;

        public double ExpandUpAnimationEndPosition
        {
            get => (double)GetValue(ExpandUpAnimationEndPositionProperty);
            internal set => SetValue(ExpandUpAnimationEndPositionPropertyKey, value);
        }

        #endregion

        #region ExpandUpAnimationHoldPosition

        private static readonly DependencyPropertyKey ExpandUpAnimationHoldPositionPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(ExpandUpAnimationHoldPosition),
                typeof(double),
                typeof(CommandBarFlyoutCommandBarTemplateSettings),
                null);

        public static readonly DependencyProperty ExpandUpAnimationHoldPositionProperty =
            ExpandUpAnimationHoldPositionPropertyKey.DependencyProperty;

        public double ExpandUpAnimationHoldPosition
        {
            get => (double)GetValue(ExpandUpAnimationHoldPositionProperty);
            internal set => SetValue(ExpandUpAnimationHoldPositionPropertyKey, value);
        }

        #endregion

        #region ExpandUpAnimationStartPosition

        private static readonly DependencyPropertyKey ExpandUpAnimationStartPositionPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(ExpandUpAnimationStartPosition),
                typeof(double),
                typeof(CommandBarFlyoutCommandBarTemplateSettings),
                null);

        public static readonly DependencyProperty ExpandUpAnimationStartPositionProperty =
            ExpandUpAnimationStartPositionPropertyKey.DependencyProperty;

        public double ExpandUpAnimationStartPosition
        {
            get => (double)GetValue(ExpandUpAnimationStartPositionProperty);
            internal set => SetValue(ExpandUpAnimationStartPositionPropertyKey, value);
        }

        #endregion

        #region ExpandUpOverflowVerticalPosition

        private static readonly DependencyPropertyKey ExpandUpOverflowVerticalPositionPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(ExpandUpOverflowVerticalPosition),
                typeof(double),
                typeof(CommandBarFlyoutCommandBarTemplateSettings),
                null);

        public static readonly DependencyProperty ExpandUpOverflowVerticalPositionProperty =
            ExpandUpOverflowVerticalPositionPropertyKey.DependencyProperty;

        public double ExpandUpOverflowVerticalPosition
        {
            get => (double)GetValue(ExpandUpOverflowVerticalPositionProperty);
            internal set => SetValue(ExpandUpOverflowVerticalPositionPropertyKey, value);
        }

        #endregion

        #region OpenAnimationEndPosition

        private static readonly DependencyPropertyKey OpenAnimationEndPositionPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(OpenAnimationEndPosition),
                typeof(double),
                typeof(CommandBarFlyoutCommandBarTemplateSettings),
                null);

        public static readonly DependencyProperty OpenAnimationEndPositionProperty =
            OpenAnimationEndPositionPropertyKey.DependencyProperty;

        public double OpenAnimationEndPosition
        {
            get => (double)GetValue(OpenAnimationEndPositionProperty);
            internal set => SetValue(OpenAnimationEndPositionPropertyKey, value);
        }

        #endregion

        #region OpenAnimationStartPosition

        private static readonly DependencyPropertyKey OpenAnimationStartPositionPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(OpenAnimationStartPosition),
                typeof(double),
                typeof(CommandBarFlyoutCommandBarTemplateSettings),
                null);

        public static readonly DependencyProperty OpenAnimationStartPositionProperty =
            OpenAnimationStartPositionPropertyKey.DependencyProperty;

        public double OpenAnimationStartPosition
        {
            get => (double)GetValue(OpenAnimationStartPositionProperty);
            internal set => SetValue(OpenAnimationStartPositionPropertyKey, value);
        }

        #endregion

        #region OverflowContentClipRect

        private static readonly DependencyPropertyKey OverflowContentClipRectPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(OverflowContentClipRect),
                typeof(Rect),
                typeof(CommandBarFlyoutCommandBarTemplateSettings),
                null);

        public static readonly DependencyProperty OverflowContentClipRectProperty =
            OverflowContentClipRectPropertyKey.DependencyProperty;

        public Rect OverflowContentClipRect
        {
            get => (Rect)GetValue(OverflowContentClipRectProperty);
            internal set => SetValue(OverflowContentClipRectPropertyKey, value);
        }

        #endregion

        #region WidthExpansionAnimationEndPosition

        private static readonly DependencyPropertyKey WidthExpansionAnimationEndPositionPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(WidthExpansionAnimationEndPosition),
                typeof(double),
                typeof(CommandBarFlyoutCommandBarTemplateSettings),
                null);

        public static readonly DependencyProperty WidthExpansionAnimationEndPositionProperty =
            WidthExpansionAnimationEndPositionPropertyKey.DependencyProperty;

        public double WidthExpansionAnimationEndPosition
        {
            get => (double)GetValue(WidthExpansionAnimationEndPositionProperty);
            internal set => SetValue(WidthExpansionAnimationEndPositionPropertyKey, value);
        }

        #endregion

        #region WidthExpansionAnimationStartPosition

        private static readonly DependencyPropertyKey WidthExpansionAnimationStartPositionPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(WidthExpansionAnimationStartPosition),
                typeof(double),
                typeof(CommandBarFlyoutCommandBarTemplateSettings),
                null);

        public static readonly DependencyProperty WidthExpansionAnimationStartPositionProperty =
            WidthExpansionAnimationStartPositionPropertyKey.DependencyProperty;

        public double WidthExpansionAnimationStartPosition
        {
            get => (double)GetValue(WidthExpansionAnimationStartPositionProperty);
            internal set => SetValue(WidthExpansionAnimationStartPositionPropertyKey, value);
        }

        #endregion

        #region WidthExpansionDelta

        private static readonly DependencyPropertyKey WidthExpansionDeltaPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(WidthExpansionDelta),
                typeof(double),
                typeof(CommandBarFlyoutCommandBarTemplateSettings),
                null);

        public static readonly DependencyProperty WidthExpansionDeltaProperty =
            WidthExpansionDeltaPropertyKey.DependencyProperty;

        public double WidthExpansionDelta
        {
            get => (double)GetValue(WidthExpansionDeltaProperty);
            internal set => SetValue(WidthExpansionDeltaPropertyKey, value);
        }

        #endregion

        #region WidthExpansionMoreButtonAnimationEndPosition

        private static readonly DependencyPropertyKey WidthExpansionMoreButtonAnimationEndPositionPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(WidthExpansionMoreButtonAnimationEndPosition),
                typeof(double),
                typeof(CommandBarFlyoutCommandBarTemplateSettings),
                null);

        public static readonly DependencyProperty WidthExpansionMoreButtonAnimationEndPositionProperty =
            WidthExpansionMoreButtonAnimationEndPositionPropertyKey.DependencyProperty;

        public double WidthExpansionMoreButtonAnimationEndPosition
        {
            get => (double)GetValue(WidthExpansionMoreButtonAnimationEndPositionProperty);
            internal set => SetValue(WidthExpansionMoreButtonAnimationEndPositionPropertyKey, value);
        }

        #endregion

        #region WidthExpansionMoreButtonAnimationStartPosition

        private static readonly DependencyPropertyKey WidthExpansionMoreButtonAnimationStartPositionPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(WidthExpansionMoreButtonAnimationStartPosition),
                typeof(double),
                typeof(CommandBarFlyoutCommandBarTemplateSettings),
                null);

        public static readonly DependencyProperty WidthExpansionMoreButtonAnimationStartPositionProperty =
            WidthExpansionMoreButtonAnimationStartPositionPropertyKey.DependencyProperty;

        public double WidthExpansionMoreButtonAnimationStartPosition
        {
            get => (double)GetValue(WidthExpansionMoreButtonAnimationStartPositionProperty);
            internal set => SetValue(WidthExpansionMoreButtonAnimationStartPositionPropertyKey, value);
        }

        #endregion
    }
}
