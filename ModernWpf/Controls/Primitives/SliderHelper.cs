using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;

namespace ModernWpf.Controls.Primitives
{
    public static class SliderHelper
    {
        #region VisualStateSettersEnabled

        public static readonly DependencyProperty VisualStateSettersEnabledProperty =
            DependencyProperty.RegisterAttached(
                "VisualStateSettersEnabled",
                typeof(bool),
                typeof(SliderHelper),
                new PropertyMetadata(false, OnVisualStateSettersEnabledChanged));

        public static bool GetVisualStateSettersEnabled(Slider slider)
        {
            return (bool)slider.GetValue(VisualStateSettersEnabledProperty);
        }

        public static void SetVisualStateSettersEnabled(Slider slider, bool value)
        {
            slider.SetValue(VisualStateSettersEnabledProperty, value);
        }

        private static void OnVisualStateSettersEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var slider = (Slider)d;
            if ((bool)e.NewValue)
            {
                GetOrCreateVisualStateTracker(slider).Attach();
            }
            else
            {
                GetVisualStateTracker(slider)?.Detach();
                slider.ClearValue(VisualStateTrackerProperty);
            }
        }

        #endregion

        private static readonly DependencyProperty VisualStateTrackerProperty =
            DependencyProperty.RegisterAttached(
                "VisualStateTracker",
                typeof(SliderVisualStateTracker),
                typeof(SliderHelper),
                new PropertyMetadata(null));

        private static SliderVisualStateTracker GetOrCreateVisualStateTracker(Slider slider)
        {
            var tracker = GetVisualStateTracker(slider);
            if (tracker == null)
            {
                tracker = new SliderVisualStateTracker(slider);
                slider.SetValue(VisualStateTrackerProperty, tracker);
            }

            return tracker;
        }

        private static SliderVisualStateTracker GetVisualStateTracker(Slider slider)
        {
            return (SliderVisualStateTracker)slider.GetValue(VisualStateTrackerProperty);
        }

        private sealed class SliderVisualStateTracker
        {
            private static readonly DependencyPropertyDescriptor OrientationPropertyDescriptor =
                DependencyPropertyDescriptor.FromProperty(Slider.OrientationProperty, typeof(Slider));

            private static readonly DependencyPropertyDescriptor TickPlacementPropertyDescriptor =
                DependencyPropertyDescriptor.FromProperty(Slider.TickPlacementProperty, typeof(Slider));

            private static readonly DependencyPropertyDescriptor ThumbIsDraggingPropertyDescriptor =
                DependencyPropertyDescriptor.FromProperty(Thumb.IsDraggingProperty, typeof(Thumb));

            public SliderVisualStateTracker(Slider slider)
            {
                _slider = slider;
            }

            public void Attach()
            {
                if (_isAttached)
                {
                    return;
                }

                _isAttached = true;
                _slider.Loaded += OnLoaded;
                _slider.Unloaded += OnUnloaded;
                _slider.IsEnabledChanged += OnSliderStateChanged;
                _slider.MouseEnter += OnInputStateChanged;
                _slider.MouseLeave += OnInputStateChanged;
                _slider.LostMouseCapture += OnInputStateChanged;
                _slider.AddHandler(UIElement.PreviewMouseLeftButtonDownEvent, new MouseButtonEventHandler(OnPreviewMouseLeftButtonDown), true);
                _slider.AddHandler(UIElement.PreviewMouseLeftButtonUpEvent, new MouseButtonEventHandler(OnPreviewMouseLeftButtonUp), true);
                OrientationPropertyDescriptor.AddValueChanged(_slider, OnTemplateStateChanged);
                TickPlacementPropertyDescriptor.AddValueChanged(_slider, OnTickPlacementChanged);

                AttachTemplateParts();
                UpdateTickBars();
                UpdateVisualStates(false);
            }

            public void Detach()
            {
                if (!_isAttached)
                {
                    return;
                }

                DetachTemplateParts();
                TickPlacementPropertyDescriptor.RemoveValueChanged(_slider, OnTickPlacementChanged);
                OrientationPropertyDescriptor.RemoveValueChanged(_slider, OnTemplateStateChanged);
                _slider.RemoveHandler(UIElement.PreviewMouseLeftButtonUpEvent, new MouseButtonEventHandler(OnPreviewMouseLeftButtonUp));
                _slider.RemoveHandler(UIElement.PreviewMouseLeftButtonDownEvent, new MouseButtonEventHandler(OnPreviewMouseLeftButtonDown));
                _slider.LostMouseCapture -= OnInputStateChanged;
                _slider.MouseLeave -= OnInputStateChanged;
                _slider.MouseEnter -= OnInputStateChanged;
                _slider.IsEnabledChanged -= OnSliderStateChanged;
                _slider.Unloaded -= OnUnloaded;
                _slider.Loaded -= OnLoaded;
                _isAttached = false;
            }

            private void OnLoaded(object sender, RoutedEventArgs e)
            {
                AttachTemplateParts();
                UpdateTickBars();
                UpdateVisualStates(false);
            }

            private void OnUnloaded(object sender, RoutedEventArgs e)
            {
                DetachTemplateParts();
                _isPressed = false;
            }

            private void OnSliderStateChanged(object sender, DependencyPropertyChangedEventArgs e)
            {
                if (!_slider.IsEnabled)
                {
                    _isPressed = false;
                }

                ScheduleVisualStateUpdate();
            }

            private void OnTemplateStateChanged(object sender, EventArgs e)
            {
                AttachTemplateParts();
                UpdateTickBars();
                ScheduleTemplateRefresh();
            }

            private void OnTickPlacementChanged(object sender, EventArgs e)
            {
                UpdateTickBars();
            }

            private void OnInputStateChanged(object sender, RoutedEventArgs e)
            {
                if (Mouse.LeftButton != MouseButtonState.Pressed)
                {
                    _isPressed = false;
                }

                ScheduleVisualStateUpdate();
            }

            private void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
            {
                if (_slider.IsEnabled)
                {
                    _isPressed = true;
                    ScheduleVisualStateUpdate();
                }
            }

            private void OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
            {
                _isPressed = false;
                ScheduleVisualStateUpdate();
            }

            private void OnThumbDraggingChanged(object sender, EventArgs e)
            {
                ScheduleVisualStateUpdate();
            }

            private void AttachTemplateParts()
            {
                DetachTemplateParts();

                _thumb = GetTemplateChild<Thumb>(
                    _slider.Orientation == Orientation.Horizontal ? "HorizontalThumb" : "VerticalThumb");

                if (_thumb != null)
                {
                    ThumbIsDraggingPropertyDescriptor.AddValueChanged(_thumb, OnThumbDraggingChanged);
                }
            }

            private void DetachTemplateParts()
            {
                if (_thumb != null)
                {
                    ThumbIsDraggingPropertyDescriptor.RemoveValueChanged(_thumb, OnThumbDraggingChanged);
                    _thumb = null;
                }
            }

            private void ScheduleTemplateRefresh()
            {
                _slider.Dispatcher.BeginInvoke(
                    (Action)(() =>
                    {
                        AttachTemplateParts();
                        UpdateTickBars();
                        UpdateVisualStates(true);
                    }),
                    DispatcherPriority.Loaded);
            }

            private void ScheduleVisualStateUpdate()
            {
                UpdateVisualStates(true);
                _slider.Dispatcher.BeginInvoke(
                    (Action)(() => UpdateVisualStates(true)),
                    DispatcherPriority.Input);
            }

            private void UpdateVisualStates(bool useTransitions)
            {
                string stateName = GetCommonStateName();
                if (!VisualStateManager.GoToState(_slider, stateName, useTransitions) &&
                    _slider.GetTemplateRoot() is { } templateRoot)
                {
                    VisualStateManager.GoToElementState(templateRoot, stateName, useTransitions);
                }

                if (_thumb != null)
                {
                    VisualStateManager.GoToState(_thumb, stateName, useTransitions);
                }
            }

            private string GetCommonStateName()
            {
                if (!_slider.IsEnabled)
                {
                    return "Disabled";
                }

                if (_isPressed || _thumb?.IsDragging == true)
                {
                    return "Pressed";
                }

                if (_slider.IsMouseOver)
                {
                    return "PointerOver";
                }

                return "Normal";
            }

            private void UpdateTickBars()
            {
                Visibility topLeftVisibility = Visibility.Collapsed;
                Visibility bottomRightVisibility = Visibility.Collapsed;

                switch (_slider.TickPlacement)
                {
                    case TickPlacement.TopLeft:
                        topLeftVisibility = Visibility.Visible;
                        break;
                    case TickPlacement.BottomRight:
                        bottomRightVisibility = Visibility.Visible;
                        break;
                    case TickPlacement.Both:
                        topLeftVisibility = Visibility.Visible;
                        bottomRightVisibility = Visibility.Visible;
                        break;
                }

                SetTemplatePartVisibility("TopTickBar", topLeftVisibility);
                SetTemplatePartVisibility("LeftTickBar", topLeftVisibility);
                SetTemplatePartVisibility("BottomTickBar", bottomRightVisibility);
                SetTemplatePartVisibility("RightTickBar", bottomRightVisibility);
            }

            private void SetTemplatePartVisibility(string name, Visibility visibility)
            {
                if (GetTemplateChild<UIElement>(name) is UIElement element)
                {
                    element.Visibility = visibility;
                }
            }

            private T GetTemplateChild<T>(string name)
                where T : DependencyObject
            {
                return _slider.Template?.FindName(name, _slider) as T;
            }

            private readonly Slider _slider;
            private bool _isAttached;
            private bool _isPressed;
            private Thumb _thumb;
        }
    }
}
