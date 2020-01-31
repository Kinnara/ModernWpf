// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Windows;
using System.Windows.Media;

namespace ModernWpf.Controls.Primitives
{
    /// <summary>
    /// Provides calculated values that can be referenced as **TemplatedParent** sources
    /// when defining templates for a ProgressBar control. Not intended for general use.
    /// </summary>
    public class ProgressBarTemplateSettings : DependencyObject
    {
        #region ClipRect

        private static readonly DependencyPropertyKey ClipRectPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(ClipRect),
                typeof(RectangleGeometry),
                typeof(ProgressBarTemplateSettings),
                null);

        public static readonly DependencyProperty ClipRectProperty =
            ClipRectPropertyKey.DependencyProperty;

        public RectangleGeometry ClipRect
        {
            get => (RectangleGeometry)GetValue(ClipRectProperty);
            internal set => SetValue(ClipRectPropertyKey, value);
        }

        #endregion

        #region ContainerAnimationEndPosition

        private static readonly DependencyPropertyKey ContainerAnimationEndPositionPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(ContainerAnimationEndPosition),
                typeof(double),
                typeof(ProgressBarTemplateSettings),
                null);

        public static readonly DependencyProperty ContainerAnimationEndPositionProperty =
            ContainerAnimationEndPositionPropertyKey.DependencyProperty;

        public double ContainerAnimationEndPosition
        {
            get => (double)GetValue(ContainerAnimationEndPositionProperty);
            internal set => SetValue(ContainerAnimationEndPositionPropertyKey, value);
        }

        #endregion

        #region ContainerAnimationEndPosition2

        private static readonly DependencyPropertyKey ContainerAnimationEndPosition2PropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(ContainerAnimationEndPosition2),
                typeof(double),
                typeof(ProgressBarTemplateSettings),
                null);

        public static readonly DependencyProperty ContainerAnimationEndPosition2Property =
            ContainerAnimationEndPosition2PropertyKey.DependencyProperty;

        public double ContainerAnimationEndPosition2
        {
            get => (double)GetValue(ContainerAnimationEndPosition2Property);
            internal set => SetValue(ContainerAnimationEndPosition2PropertyKey, value);
        }

        #endregion

        #region ContainerAnimationMidPosition

        private static readonly DependencyPropertyKey ContainerAnimationMidPositionPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(ContainerAnimationMidPosition),
                typeof(double),
                typeof(ProgressBarTemplateSettings),
                null);

        public static readonly DependencyProperty ContainerAnimationMidPositionProperty =
            ContainerAnimationMidPositionPropertyKey.DependencyProperty;

        public double ContainerAnimationMidPosition
        {
            get => (double)GetValue(ContainerAnimationMidPositionProperty);
            internal set => SetValue(ContainerAnimationMidPositionPropertyKey, value);
        }

        #endregion

        #region ContainerAnimationStartPosition

        private static readonly DependencyPropertyKey ContainerAnimationStartPositionPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(ContainerAnimationStartPosition),
                typeof(double),
                typeof(ProgressBarTemplateSettings),
                null);

        public static readonly DependencyProperty ContainerAnimationStartPositionProperty =
            ContainerAnimationStartPositionPropertyKey.DependencyProperty;

        public double ContainerAnimationStartPosition
        {
            get => (double)GetValue(ContainerAnimationStartPositionProperty);
            internal set => SetValue(ContainerAnimationStartPositionPropertyKey, value);
        }

        #endregion

        #region ContainerAnimationStartPosition2

        private static readonly DependencyPropertyKey ContainerAnimationStartPosition2PropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(ContainerAnimationStartPosition2),
                typeof(double),
                typeof(ProgressBarTemplateSettings),
                null);

        public static readonly DependencyProperty ContainerAnimationStartPosition2Property =
            ContainerAnimationStartPosition2PropertyKey.DependencyProperty;

        public double ContainerAnimationStartPosition2
        {
            get => (double)GetValue(ContainerAnimationStartPosition2Property);
            internal set => SetValue(ContainerAnimationStartPosition2PropertyKey, value);
        }

        #endregion

        #region IndicatorLengthDelta

        private static readonly DependencyPropertyKey IndicatorLengthDeltaPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(IndicatorLengthDelta),
                typeof(double),
                typeof(ProgressBarTemplateSettings),
                null);

        public static readonly DependencyProperty IndicatorLengthDeltaProperty =
            IndicatorLengthDeltaPropertyKey.DependencyProperty;

        public double IndicatorLengthDelta
        {
            get => (double)GetValue(IndicatorLengthDeltaProperty);
            internal set => SetValue(IndicatorLengthDeltaPropertyKey, value);
        }

        #endregion
    }
}
