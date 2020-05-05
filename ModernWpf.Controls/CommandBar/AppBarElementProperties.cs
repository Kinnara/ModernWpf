using System.Collections;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace ModernWpf.Controls
{
    internal interface IAppBarElement
    {
        void UpdateApplicationViewState();
        void ApplyApplicationViewState();
    }

    internal static class AppBarElementProperties
    {
        #region Icon

        public static readonly DependencyProperty IconProperty =
            DependencyProperty.RegisterAttached(
                "Icon",
                typeof(IconElement),
                typeof(AppBarElementProperties),
                new PropertyMetadata(OnIconChanged));

        private static void OnIconChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as IAppBarElement)?.UpdateApplicationViewState();
        }

        #endregion

        #region Label

        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.RegisterAttached(
                "Label",
                typeof(string),
                typeof(AppBarElementProperties),
                new PropertyMetadata(string.Empty, OnLabelChanged, CoerceLabel));

        private static void OnLabelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as FrameworkElement)?.CoerceValue(FrameworkElement.ToolTipProperty);
        }

        // Set the label to the command text if no label has been explicitly specified
        private static object CoerceLabel(DependencyObject d, object value)
        {
            ButtonBase button = (ButtonBase)d;
            RoutedUICommand uiCommand;

            // If no label has been set, use the command's text
            if (string.IsNullOrEmpty(value as string) && !button.HasNonDefaultValue(LabelProperty))
            {
                uiCommand = button.Command as RoutedUICommand;
                if (uiCommand != null)
                {
                    value = uiCommand.Text;
                }
                return value;
            }

            return value;
        }

        #endregion

        #region LabelPosition

        public static readonly DependencyProperty LabelPositionProperty =
            DependencyProperty.RegisterAttached(
                "LabelPosition",
                typeof(CommandBarLabelPosition),
                typeof(AppBarElementProperties),
                new PropertyMetadata(CommandBarLabelPosition.Default, OnLabelPositionChanged));

        private static void OnLabelPositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as IAppBarElement)?.UpdateApplicationViewState();
        }

        #endregion

        #region IsCompact

        public static readonly DependencyProperty IsCompactProperty =
            DependencyProperty.RegisterAttached(
                "IsCompact",
                typeof(bool),
                typeof(AppBarElementProperties),
                new PropertyMetadata(false, OnIsCompactChanged));

        private static void OnIsCompactChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as IAppBarElement)?.UpdateApplicationViewState();
        }

        #endregion

        #region IsInOverflow

        internal static readonly DependencyPropertyKey IsInOverflowPropertyKey =
            DependencyProperty.RegisterAttachedReadOnly(
                "IsInOverflow",
                typeof(bool),
                typeof(AppBarElementProperties),
                new PropertyMetadata(false, OnIsInOverflowChanged));

        public static readonly DependencyProperty IsInOverflowProperty =
            IsInOverflowPropertyKey.DependencyProperty;

        private static void OnIsInOverflowChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as IAppBarElement)?.UpdateApplicationViewState();
            UpdateShowKeyboardAcceleratorText(d as FrameworkElement);
            (d as FrameworkElement)?.CoerceValue(FrameworkElement.ToolTipProperty);
        }

        internal static void UpdateIsInOverflow(DependencyObject element)
        {
            bool value = ToolBar.GetIsOverflowItem(element) || ToolBar.GetOverflowMode(element) == OverflowMode.Always;
            element.SetValue(IsInOverflowPropertyKey, value);
        }

        #endregion

        #region ApplicationViewState

        internal static readonly DependencyPropertyKey ApplicationViewStatePropertyKey =
            DependencyProperty.RegisterAttachedReadOnly(
                "ApplicationViewState",
                typeof(AppBarElementApplicationViewState),
                typeof(AppBarElementProperties),
                new PropertyMetadata(
                    AppBarElementApplicationViewState.FullSize,
                    OnApplicationViewStateChanged));

        internal static readonly DependencyProperty ApplicationViewStateProperty =
            ApplicationViewStatePropertyKey.DependencyProperty;

        private static void OnApplicationViewStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as IAppBarElement)?.ApplyApplicationViewState();
        }

        #endregion

        #region InputGestureText

        public static readonly DependencyProperty InputGestureTextProperty =
            DependencyProperty.RegisterAttached(
                "InputGestureText",
                typeof(string),
                typeof(AppBarElementProperties),
                new PropertyMetadata(
                    string.Empty,
                    OnInputGestureTextChanged,
                    CoerceInputGestureText));

        private static void OnInputGestureTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            UpdateHasInputGestureText(d, (string)e.NewValue);
            (d as FrameworkElement)?.CoerceValue(FrameworkElement.ToolTipProperty);
        }

        // Gets the input gesture text from the command text if it hasn't been explicitly specified
        private static object CoerceInputGestureText(DependencyObject d, object value)
        {
            ButtonBase button = (ButtonBase)d;
            RoutedCommand routedCommand;

            if (string.IsNullOrEmpty((string)value) && !button.HasNonDefaultValue(InputGestureTextProperty)
                && (routedCommand = button.Command as RoutedCommand) != null)
            {
                InputGestureCollection col = routedCommand.InputGestures;
                if ((col != null) && (col.Count >= 1))
                {
                    // Search for the first key gesture
                    for (int i = 0; i < col.Count; i++)
                    {
                        KeyGesture keyGesture = ((IList)col)[i] as KeyGesture;
                        if (keyGesture != null)
                        {
                            return keyGesture.GetDisplayStringForCulture(CultureInfo.CurrentCulture);
                        }
                    }
                }
            }

            return value;
        }

        #endregion

        #region HasInputGestureText

        private static readonly DependencyPropertyKey HasInputGestureTextPropertyKey =
            DependencyProperty.RegisterAttachedReadOnly(
                "HasInputGestureText",
                typeof(bool),
                typeof(AppBarElementProperties),
                new PropertyMetadata(false, OnHasInputGestureTextChanged));

        public static readonly DependencyProperty HasInputGestureTextProperty =
            HasInputGestureTextPropertyKey.DependencyProperty;

        private static void OnHasInputGestureTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            UpdateShowKeyboardAcceleratorText(d as FrameworkElement);
        }

        private static void UpdateHasInputGestureText(DependencyObject element, string inputGestureText)
        {
            element.SetValue(HasInputGestureTextPropertyKey, !string.IsNullOrEmpty(inputGestureText));
        }

        #endregion

        #region ShowKeyboardAcceleratorText

        internal static readonly DependencyProperty ShowKeyboardAcceleratorTextProperty =
            DependencyProperty.RegisterAttached(
                "ShowKeyboardAcceleratorText",
                typeof(bool),
                typeof(AppBarElementProperties));

        internal static bool GetShowKeyboardAcceleratorText(DependencyObject element)
        {
            return (bool)element.GetValue(ShowKeyboardAcceleratorTextProperty);
        }

        private static void SetShowKeyboardAcceleratorText(DependencyObject element, bool value)
        {
            element.SetValue(ShowKeyboardAcceleratorTextProperty, value);
        }

        private static void UpdateShowKeyboardAcceleratorText(FrameworkElement element)
        {
            if (element != null)
            {
                bool value = (bool)element.GetValue(HasInputGestureTextProperty) &&
                             (bool)element.GetValue(IsInOverflowProperty);
                SetShowKeyboardAcceleratorText(element, value);
            }
        }

        #endregion

        internal static object CoerceToolTip(DependencyObject d, object baseValue)
        {
            var button = (ButtonBase)d;

            if (baseValue == null &&
                button.HasDefaultValue(FrameworkElement.ToolTipProperty) &&
                (bool)button.GetValue(HasInputGestureTextProperty) &&
                !(bool)button.GetValue(IsInOverflowProperty))
            {
                string label = (string)button.GetValue(LabelProperty);
                string inputGestureText = (string)button.GetValue(InputGestureTextProperty);
                return $"{label} ({inputGestureText})".Trim();
            }

            return baseValue;
        }
    }
}
