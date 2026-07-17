using System.Collections;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using ModernWpf.Controls.Primitives;

namespace ModernWpf.Controls
{
    internal interface IAppBarElement
    {
        void UpdateApplicationViewState();
    }

    internal interface IAppBarButtonElement : IAppBarElement
    {
        IconElement Icon { get; }

        string KeyboardAcceleratorTextOverride { get; }

        void SetDefaultLabelPosition(CommandBarDefaultLabelPosition defaultLabelPosition);

        bool GetHasBottomLabel();

        bool GetHasRightLabel();

        void SetOverflowStyleParams(bool hasIcons, bool hasToggleButtons, bool hasKeyboardAcceleratorText);

        void SetInputMode(AppBarButtonInputMode inputMode);

        double GetKeyboardAcceleratorTextDesiredWidth();

        void UpdateTemplateSettings(double maxKeyboardAcceleratorTextWidth);
    }

    internal enum AppBarButtonInputMode
    {
        Default,
        Touch,
        GameController
    }

    internal static partial class AppBarElementProperties
    {
        internal static readonly DependencyProperty IsInCommandBarFlyoutProperty =
            DependencyProperty.RegisterAttached(
                "IsInCommandBarFlyout",
                typeof(bool),
                typeof(AppBarElementProperties),
                new PropertyMetadata(false));

        internal static bool GetIsInCommandBarFlyout(DependencyObject element)
        {
            return (bool)element.GetValue(IsInCommandBarFlyoutProperty);
        }

        internal static void SetIsInCommandBarFlyout(DependencyObject element, bool value)
        {
            element.SetValue(IsInCommandBarFlyoutProperty, value);
        }

        static AppBarElementProperties()
        {
            InputGestureTextProperty = KeyboardAcceleratorTextOverrideProperty;
        }

        #region Icon

        private static void OnIconChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as IAppBarElement)?.UpdateApplicationViewState();
        }

        #endregion

        #region Label

        private static void OnLabelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as FrameworkElement)?.CoerceValue(FrameworkElement.ToolTipProperty);
            (d as IAppBarElement)?.UpdateApplicationViewState();
            CommandBar.OnCommandBarElementDependencyPropertyChanged(d);
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

        #region DefaultLabelPosition

        private static void OnDefaultLabelPositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is IAppBarButtonElement appBarButtonElement)
            {
                appBarButtonElement.SetDefaultLabelPosition((CommandBarDefaultLabelPosition)e.NewValue);
            }
            else
            {
                (d as IAppBarElement)?.UpdateApplicationViewState();
            }
        }

        #endregion

        #region LabelPosition

        private static void OnLabelPositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as IAppBarElement)?.UpdateApplicationViewState();
            CommandBar.OnCommandBarElementDependencyPropertyChanged(d);
        }

        #endregion

        #region IsCompact

        private static void OnIsCompactChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as IAppBarElement)?.UpdateApplicationViewState();
        }

        #endregion

        #region IsInOverflow

        internal static bool GetUseOverflowStyle(DependencyObject element)
        {
            return (bool)element.GetValue(UseOverflowStyleProperty);
        }

        internal static void SetUseOverflowStyle(DependencyObject element, bool value)
        {
            element.SetValue(UseOverflowStyleProperty, value);
        }

        private static void OnUseOverflowStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            d.SetValue(IsInOverflowPropertyKey, e.NewValue);
        }

        private static void OnIsInOverflowChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as IAppBarElement)?.UpdateApplicationViewState();
            UpdateShowKeyboardAcceleratorText(d as FrameworkElement);
            (d as FrameworkElement)?.CoerceValue(FrameworkElement.ToolTipProperty);
        }

        internal static void SetIsInOverflow(DependencyObject element, bool value)
        {
            SetUseOverflowStyle(element, value);
        }

        #endregion

        #region InputGestureText

        public static readonly DependencyProperty InputGestureTextProperty;

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
                             GetUseOverflowStyle(element);
                SetShowKeyboardAcceleratorText(element, value);
            }
        }

        #endregion

        internal static void UpdateOverflowStyleParams(
            IEnumerable commands,
            bool useOverflowStyle,
            AppBarButtonInputMode inputMode = AppBarButtonInputMode.Default)
        {
            if (!useOverflowStyle)
            {
                foreach (object command in commands)
                {
                    if (command is DependencyObject dependencyObject)
                    {
                        SetUseOverflowStyle(dependencyObject, false);
                    }

                    if (command is IAppBarButtonElement appBarElement)
                    {
                        appBarElement.SetOverflowStyleParams(false, false, false);
                        appBarElement.SetInputMode(AppBarButtonInputMode.Default);
                        appBarElement.UpdateTemplateSettings(0);
                    }
                }

                return;
            }

            bool hasAppBarToggleButtons = false;
            bool hasAppBarIcons = false;
            bool hasAppBarAcceleratorText = false;
            double maxAppBarKeyboardAcceleratorTextWidth = 0;

            foreach (object command in commands)
            {
                if (command is UIElement element && !element.IsVisible)
                {
                    continue;
                }

                if (command is IAppBarButtonElement appBarElement)
                {
                    if (command is AppBarToggleButton)
                    {
                        hasAppBarToggleButtons = true;
                    }

                    hasAppBarIcons = hasAppBarIcons || appBarElement.Icon != null;
                    hasAppBarAcceleratorText = hasAppBarAcceleratorText || !string.IsNullOrEmpty(appBarElement.KeyboardAcceleratorTextOverride);
                    maxAppBarKeyboardAcceleratorTextWidth = System.Math.Max(
                        maxAppBarKeyboardAcceleratorTextWidth,
                        appBarElement.GetKeyboardAcceleratorTextDesiredWidth());
                }
            }

            foreach (object command in commands)
            {
                if (command is DependencyObject dependencyObject)
                {
                    SetUseOverflowStyle(dependencyObject, useOverflowStyle);
                }

                if (command is IAppBarButtonElement appBarElement)
                {
                    appBarElement.SetOverflowStyleParams(
                        hasAppBarIcons,
                        hasAppBarToggleButtons,
                        hasAppBarAcceleratorText);
                    appBarElement.SetInputMode(inputMode);
                    appBarElement.UpdateTemplateSettings(maxAppBarKeyboardAcceleratorTextWidth);
                }
            }
        }

        internal static object CoerceToolTip(DependencyObject d, object baseValue)
        {
            var button = (ButtonBase)d;

            if (baseValue == null &&
                button.HasDefaultValue(FrameworkElement.ToolTipProperty) &&
                (bool)button.GetValue(HasInputGestureTextProperty) &&
                !GetUseOverflowStyle(button))
            {
                string label = (string)button.GetValue(LabelProperty);
                string inputGestureText = (string)button.GetValue(KeyboardAcceleratorTextOverrideProperty);
                return $"{label} ({inputGestureText})".Trim();
            }

            return baseValue;
        }
    }
}
