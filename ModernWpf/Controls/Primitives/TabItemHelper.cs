using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace ModernWpf.Controls.Primitives
{
    public static class TabItemHelper
    {
        #region VisualStateSettersEnabled

        public static bool GetVisualStateSettersEnabled(TabItem tabItem)
        {
            return (bool)tabItem.GetValue(VisualStateSettersEnabledProperty);
        }

        public static void SetVisualStateSettersEnabled(TabItem tabItem, bool value)
        {
            tabItem.SetValue(VisualStateSettersEnabledProperty, value);
        }

        public static readonly DependencyProperty VisualStateSettersEnabledProperty =
            DependencyProperty.RegisterAttached(
                "VisualStateSettersEnabled",
                typeof(bool),
                typeof(TabItemHelper),
                new PropertyMetadata(false, OnVisualStateSettersEnabledChanged));

        private static void OnVisualStateSettersEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var tabItem = (TabItem)d;
            if ((bool)e.NewValue)
            {
                GetOrCreateVisualStateTracker(tabItem).Attach();
            }
            else
            {
                GetVisualStateTracker(tabItem)?.Detach();
            }
        }

        #endregion

        #region Icon

        public static object GetIcon(TabItem tabItem)
        {
            return tabItem.GetValue(IconProperty);
        }

        public static void SetIcon(TabItem tabItem, object value)
        {
            tabItem.SetValue(IconProperty, value);
        }

        public static readonly DependencyProperty IconProperty =
            DependencyProperty.RegisterAttached(
                "Icon",
                typeof(object),
                typeof(TabItemHelper));

        #endregion

        private static VisualStateTracker GetOrCreateVisualStateTracker(TabItem tabItem)
        {
            var tracker = GetVisualStateTracker(tabItem);
            if (tracker == null)
            {
                tracker = new VisualStateTracker(tabItem);
                tabItem.SetValue(VisualStateTrackerProperty, tracker);
            }

            return tracker;
        }

        private static VisualStateTracker GetVisualStateTracker(TabItem tabItem)
        {
            return (VisualStateTracker)tabItem.GetValue(VisualStateTrackerProperty);
        }

        private static readonly DependencyProperty VisualStateTrackerProperty =
            DependencyProperty.RegisterAttached(
                "VisualStateTracker",
                typeof(VisualStateTracker),
                typeof(TabItemHelper));

        private sealed class VisualStateTracker
        {
            private static readonly DependencyPropertyDescriptor IsSelectedPropertyDescriptor =
                DependencyPropertyDescriptor.FromProperty(TabItem.IsSelectedProperty, typeof(TabItem));

            private static readonly DependencyPropertyDescriptor IconPropertyDescriptor =
                DependencyPropertyDescriptor.FromProperty(IconProperty, typeof(TabItem));

            private static readonly DependencyPropertyDescriptor ForegroundPropertyDescriptor =
                DependencyPropertyDescriptor.FromProperty(Control.ForegroundProperty, typeof(TabItem));

            private static readonly DependencyPropertyDescriptor PressHelperIsPressedPropertyDescriptor =
                DependencyPropertyDescriptor.FromProperty(PressHelper.IsPressedProperty, typeof(Border));

            public VisualStateTracker(TabItem tabItem)
            {
                _tabItem = tabItem;
            }

            public void Attach()
            {
                if (_isAttached)
                {
                    return;
                }

                _isAttached = true;
                _tabItem.Loaded += OnLoaded;
                _tabItem.Unloaded += OnUnloaded;
                _tabItem.IsEnabledChanged += OnDependencyStateChanged;
                IsSelectedPropertyDescriptor.AddValueChanged(_tabItem, OnDependencyStateChanged);
                IconPropertyDescriptor.AddValueChanged(_tabItem, OnDependencyStateChanged);
                ForegroundPropertyDescriptor.AddValueChanged(_tabItem, OnDependencyStateChanged);

                AttachTemplateParts();
                UpdateVisualStates(false);
            }

            public void Detach()
            {
                if (!_isAttached)
                {
                    return;
                }

                DetachTemplateParts();
                ForegroundPropertyDescriptor.RemoveValueChanged(_tabItem, OnDependencyStateChanged);
                IconPropertyDescriptor.RemoveValueChanged(_tabItem, OnDependencyStateChanged);
                IsSelectedPropertyDescriptor.RemoveValueChanged(_tabItem, OnDependencyStateChanged);
                _tabItem.IsEnabledChanged -= OnDependencyStateChanged;
                _tabItem.Unloaded -= OnUnloaded;
                _tabItem.Loaded -= OnLoaded;
                _isAttached = false;
            }

            private void OnLoaded(object sender, RoutedEventArgs e)
            {
                AttachTemplateParts();
                UpdateVisualStates(false);
            }

            private void OnUnloaded(object sender, RoutedEventArgs e)
            {
                DetachTemplateParts();
            }

            private void AttachTemplateParts()
            {
                DetachTemplateParts();

                _tabItem.ApplyTemplate();
                _stateGroupsRoot = _tabItem.Template?.FindName("LayoutRoot", _tabItem) as FrameworkElement;
                _tabContainer = _tabItem.Template?.FindName("TabContainer", _tabItem) as Border;

                if (_tabContainer != null)
                {
                    _tabContainer.MouseEnter += OnInputStateChanged;
                    _tabContainer.MouseLeave += OnInputStateChanged;
                    _tabContainer.PreviewMouseLeftButtonDown += OnInputButtonStateChanged;
                    _tabContainer.PreviewMouseLeftButtonUp += OnInputButtonStateChanged;
                    _tabContainer.LostMouseCapture += OnInputStateChanged;
                    PressHelperIsPressedPropertyDescriptor.AddValueChanged(_tabContainer, OnDependencyStateChanged);
                }
            }

            private void DetachTemplateParts()
            {
                if (_tabContainer != null)
                {
                    PressHelperIsPressedPropertyDescriptor.RemoveValueChanged(_tabContainer, OnDependencyStateChanged);
                    _tabContainer.LostMouseCapture -= OnInputStateChanged;
                    _tabContainer.PreviewMouseLeftButtonUp -= OnInputButtonStateChanged;
                    _tabContainer.PreviewMouseLeftButtonDown -= OnInputButtonStateChanged;
                    _tabContainer.MouseLeave -= OnInputStateChanged;
                    _tabContainer.MouseEnter -= OnInputStateChanged;
                    _tabContainer = null;
                }

                _stateGroupsRoot = null;
            }

            private void OnDependencyStateChanged(object sender, EventArgs e)
            {
                ScheduleVisualStateUpdate();
            }

            private void OnDependencyStateChanged(object sender, DependencyPropertyChangedEventArgs e)
            {
                ScheduleVisualStateUpdate();
            }

            private void OnInputStateChanged(object sender, MouseEventArgs e)
            {
                ScheduleVisualStateUpdate();
            }

            private void OnInputButtonStateChanged(object sender, MouseButtonEventArgs e)
            {
                ScheduleVisualStateUpdate();
            }

            private void ScheduleVisualStateUpdate()
            {
                UpdateVisualStates(true);
                _tabItem.Dispatcher.BeginInvoke(
                    (Action)(() =>
                    {
                        if (_isAttached)
                        {
                            UpdateVisualStates(true);
                        }
                    }),
                    DispatcherPriority.Input);
            }

            private void UpdateVisualStates(bool useTransitions)
            {
                GoToState(GetCommonStateName(), useTransitions);
                GoToState(_tabItem.IsEnabled ? "Enabled" : "Disabled", useTransitions);
                GoToState(GetIcon(_tabItem) == null ? "NoIcon" : "Icon", useTransitions);
                GoToState(IsForegroundSet() ? "ForegroundSet" : "ForegroundNotSet", useTransitions);
            }

            private void GoToState(string stateName, bool useTransitions)
            {
                if (_stateGroupsRoot != null)
                {
                    VisualStateManager.GoToElementState(_stateGroupsRoot, stateName, useTransitions);
                }
                else
                {
                    VisualStateManager.GoToState(_tabItem, stateName, useTransitions);
                }
            }

            private string GetCommonStateName()
            {
                bool isSelected = _tabItem.IsSelected;
                bool isPressed = _tabContainer != null && PressHelper.GetIsPressed(_tabContainer);
                bool isPointerOver = _tabContainer?.IsMouseOver == true || _tabItem.IsMouseOver;

                if (isSelected)
                {
                    if (isPressed)
                    {
                        return "PressedSelected";
                    }

                    if (isPointerOver)
                    {
                        return "PointerOverSelected";
                    }

                    return "Selected";
                }

                if (!_tabItem.IsEnabled)
                {
                    return "Normal";
                }

                if (isPressed)
                {
                    return "Pressed";
                }

                if (isPointerOver)
                {
                    return "PointerOver";
                }

                return "Normal";
            }

            private bool IsForegroundSet()
            {
                return _tabItem.ReadLocalValue(Control.ForegroundProperty) != DependencyProperty.UnsetValue;
            }

            private readonly TabItem _tabItem;
            private bool _isAttached;
            private FrameworkElement _stateGroupsRoot;
            private Border _tabContainer;
        }
    }
}
