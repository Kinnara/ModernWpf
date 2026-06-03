// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;
using ModernWpf.Controls.Primitives;

namespace ModernWpf.Controls
{
    [ContentProperty(nameof(PrimaryCommands))]
    public class CommandBarFlyout : FlyoutBase
    {
        public CommandBarFlyout()
        {
            ShouldConstrainToRootBounds = false;
            AreOpenCloseAnimationsEnabled = false;

            PrimaryCommands = new ObservableCollection<ICommandBarElement>();
            SecondaryCommands = new ObservableCollection<ICommandBarElement>();

            PrimaryCommands.CollectionChanged += (object sender, NotifyCollectionChangedEventArgs args) =>
            {
                if (m_commandBar != null)
                {
                    SharedHelpers.ForwardCollectionChange((ObservableCollection<ICommandBarElement>)sender, m_commandBar.PrimaryCommands, args);
                }
            };

            SecondaryCommands.CollectionChanged += (object sender, NotifyCollectionChangedEventArgs args) =>
            {
                var commandBar = m_commandBar;
                if (commandBar != null)
                {
                    var source = (ObservableCollection<ICommandBarElement>)sender;
                    SharedHelpers.ForwardCollectionChange(source, commandBar.SecondaryCommands, args);

                    // We want to ensure that any interaction with secondary items causes the CommandBarFlyout
                    // to close, so we'll attach a Click handler to any buttons and Checked/Unchecked handlers
                    // to any toggle buttons that we get and close the flyout when they're invoked.
                    // The only exception is buttons with flyouts - in that case, clicking on the button
                    // will just open the flyout rather than executing an action, so we don't want that to
                    // do anything.
                    RoutedEventHandler closeFlyoutFunc = delegate { Hide(); };

                    switch (args.Action)
                    {
                        case NotifyCollectionChangedAction.Replace:
                            {
                                foreach (ICommandBarElement oldElement in args.OldItems)
                                {
                                    UnhookCommandBarElementDependencyPropertyChanges(oldElement);
                                    RevokeAndRemove(m_secondaryButtonClickRevokerByElementMap, oldElement);
                                    RevokeAndRemove(m_secondaryToggleButtonCheckedRevokerByElementMap, oldElement);
                                    RevokeAndRemove(m_secondaryToggleButtonUncheckedRevokerByElementMap, oldElement);
                                }

                                foreach (ICommandBarElement element in args.NewItems)
                                {
                                    HookCommandBarElementDependencyPropertyChanges(element);
                                    HookSecondaryCommandCloseHandlers(element, closeFlyoutFunc);
                                }
                                break;
                            }
                        case NotifyCollectionChangedAction.Add:
                            {
                                foreach (ICommandBarElement element in args.NewItems)
                                {
                                    HookCommandBarElementDependencyPropertyChanges(element);
                                    HookSecondaryCommandCloseHandlers(element, closeFlyoutFunc);
                                }
                                break;
                            }
                        case NotifyCollectionChangedAction.Remove:
                            {
                                foreach (ICommandBarElement element in args.OldItems)
                                {
                                    UnhookCommandBarElementDependencyPropertyChanges(element);
                                    RevokeAndRemove(m_secondaryButtonClickRevokerByElementMap, element);
                                    RevokeAndRemove(m_secondaryToggleButtonCheckedRevokerByElementMap, element);
                                    RevokeAndRemove(m_secondaryToggleButtonUncheckedRevokerByElementMap, element);
                                }
                                break;
                            }
                        case NotifyCollectionChangedAction.Move:
                            break;
                        case NotifyCollectionChangedAction.Reset:
                            SetSecondaryCommandsToCloseWhenExecuted();
                            HookAllCommandBarElementDependencyPropertyChanges();
                            break;
                        default:
                            Debug.Assert(false);
                            break;
                    }
                }
            };

            Opening += delegate
            {
                InternalPopup.SuppressFadeAnimation = true;

                if (m_commandBar is { } commandBar)
                {
                    if (ShowMode == FlyoutShowMode.Standard)
                    {
                        m_commandBar.IsOpen = true;
                    }

                    // When CommandBarFlyout is in AlwaysOpen state, don't show the overflow button
                    if (AlwaysExpanded)
                    {
                        SetCurrentValue(ShowModeProperty, FlyoutShowMode.Standard);
                        commandBar.IsOpen = true;
                        commandBar.OverflowButtonVisibility = CommandBarOverflowButtonVisibility.Collapsed;
                    }
                    else
                    {
                        commandBar.OverflowButtonVisibility = CommandBarOverflowButtonVisibility.Auto;
                    }
                }

                if (PrimaryCommands.Count > 0)
                {
                    AddDropShadow();
                }
            };

            Opened += delegate
            {
                if (m_commandBar != null)
                {
                    if (m_commandBar.HasOpenAnimation())
                    {
                        m_commandBar.PlayOpenAnimation();
                    }
                }
            };

            Closing += delegate (FlyoutBase sender, FlyoutBaseClosingEventArgs args)
            {
                var commandBar = m_commandBar;
                if (commandBar != null)
                {
                    RemoveDropShadow();

                    if (!m_isClosingAfterCloseAnimation && commandBar.HasCloseAnimation())
                    {
                        args.Cancel = true;

                        commandBar.PlayCloseAnimation(() =>
                        {
                            m_isClosingAfterCloseAnimation = true;
                            Hide();
                            m_isClosingAfterCloseAnimation = false;
                        });
                    }
                    else
                    {
                        // Close commandbar and thus other associated flyouts
                        commandBar.IsOpen = false;
                    }

                    //CommandBarFlyoutCommandBar.Closed will be called when
                    //clicking the more (...) button, we clear the translations
                    //here
                    commandBar.ClearShadow();
                }
            };

            Closed += delegate
            {
                if (m_commandBar != null)
                {
                    if (m_commandBar.IsOpen)
                    {
                        m_commandBar.IsOpen = false;
                    }
                }
            };
        }

        public bool AlwaysExpanded { get; set; }

        public ObservableCollection<ICommandBarElement> PrimaryCommands { get; }

        public ObservableCollection<ICommandBarElement> SecondaryCommands { get; }

        internal override PopupAnimation DesiredPopupAnimation => PopupAnimation.Fade;

        protected override Control CreatePresenter()
        {
            var commandBar = new CommandBarFlyoutCommandBar();
            m_commandBar = commandBar;

            SharedHelpers.CopyList(PrimaryCommands, commandBar.PrimaryCommands);
            SharedHelpers.CopyList(SecondaryCommands, commandBar.SecondaryCommands);

            SetSecondaryCommandsToCloseWhenExecuted();
            HookAllCommandBarElementDependencyPropertyChanges();

            FlyoutPresenter presenter = new FlyoutPresenter
            {
                Background = null,
                Foreground = null,
                BorderBrush = null,
                MinWidth = 0,
                MaxWidth = double.PositiveInfinity,
                MinHeight = 0,
                MaxHeight = double.PositiveInfinity,
                BorderThickness = new Thickness(0),
                Padding = new Thickness(0),
                Content = commandBar,
                CornerRadius = new CornerRadius(0),
                IsDefaultShadowEnabled = false
            };

            m_presenter = presenter;

            commandBar.Opened += delegate
            {
                SetCurrentValue(ShowModeProperty, FlyoutShowMode.Standard);
            };
            commandBar.Opening += delegate
            {
                if (commandBar.HasSecondaryOpenCloseAnimations() && SecondaryCommands.Count > 0)
                {
                    RemoveDropShadow();
                }
            };
            commandBar.Closing += delegate
            {
                if (commandBar.HasSecondaryOpenCloseAnimations())
                {
                    RemoveDropShadow();
                }
            };

            commandBar.SetOwningFlyout(this);
            return presenter;
        }

        protected override bool ShouldRecreatePresenterAfterClose => true;

        protected override void OnPresenterReleased()
        {
            m_commandBar?.ReleaseCommandElements();
            m_commandBar = null;
            m_presenter = null;
        }

        private void SetSecondaryCommandsToCloseWhenExecuted()
        {
            RevokeAndClear(m_secondaryButtonClickRevokerByElementMap);
            RevokeAndClear(m_secondaryToggleButtonCheckedRevokerByElementMap);
            RevokeAndClear(m_secondaryToggleButtonUncheckedRevokerByElementMap);

            RoutedEventHandler closeFlyoutFunc = delegate { Hide(); };

            for (int i = 0; i < SecondaryCommands.Count; i++)
            {
                var element = SecondaryCommands[i];
                var button = element as AppBarButton;
                var toggleButton = element as AppBarToggleButton;

                if (button != null && button.Flyout == null)
                {
                    m_secondaryButtonClickRevokerByElementMap[element] = new RoutedEventHandlerRevoker(
                        button, ButtonBase.ClickEvent, closeFlyoutFunc);
                }
                else if (toggleButton != null)
                {
                    m_secondaryToggleButtonCheckedRevokerByElementMap[element] = new RoutedEventHandlerRevoker(
                        toggleButton, ToggleButton.CheckedEvent, closeFlyoutFunc);
                    m_secondaryToggleButtonUncheckedRevokerByElementMap[element] = new RoutedEventHandlerRevoker(
                        toggleButton, ToggleButton.UncheckedEvent, closeFlyoutFunc);
                }
            }
        }

        private void HookSecondaryCommandCloseHandlers(
            ICommandBarElement element,
            RoutedEventHandler closeFlyoutFunc)
        {
            var button = element as AppBarButton;
            var toggleButton = element as AppBarToggleButton;

            if (button != null && button.Flyout == null)
            {
                m_secondaryButtonClickRevokerByElementMap[element] = new RoutedEventHandlerRevoker(
                    button, ButtonBase.ClickEvent, closeFlyoutFunc);
                RevokeAndRemove(m_secondaryToggleButtonCheckedRevokerByElementMap, element);
                RevokeAndRemove(m_secondaryToggleButtonUncheckedRevokerByElementMap, element);
            }
            else if (toggleButton != null)
            {
                RevokeAndRemove(m_secondaryButtonClickRevokerByElementMap, element);
                m_secondaryToggleButtonCheckedRevokerByElementMap[element] = new RoutedEventHandlerRevoker(
                    toggleButton, ToggleButton.CheckedEvent, closeFlyoutFunc);
                m_secondaryToggleButtonUncheckedRevokerByElementMap[element] = new RoutedEventHandlerRevoker(
                    toggleButton, ToggleButton.UncheckedEvent, closeFlyoutFunc);
            }
            else
            {
                RevokeAndRemove(m_secondaryButtonClickRevokerByElementMap, element);
                RevokeAndRemove(m_secondaryToggleButtonCheckedRevokerByElementMap, element);
                RevokeAndRemove(m_secondaryToggleButtonUncheckedRevokerByElementMap, element);
            }
        }

        internal FlyoutPresenter GetPresenter()
        {
            return m_presenter;
        }

        internal void AddDropShadow()
        {
            if (m_presenter != null)
            {
                m_presenter.IsDefaultShadowEnabled = true;
            }
        }

        internal void RemoveDropShadow()
        {
            if (m_presenter != null)
            {
                m_presenter.IsDefaultShadowEnabled = false;
            }
        }

        private void HookAllCommandBarElementDependencyPropertyChanges()
        {
            UnhookAllCommandBarElementDependencyPropertyChanges();

            foreach (var element in SecondaryCommands)
            {
                HookCommandBarElementDependencyPropertyChanges(element);
            }
        }

        private void HookCommandBarElementDependencyPropertyChanges(ICommandBarElement element)
        {
            if (m_commandBar == null)
            {
                return;
            }

            UnhookCommandBarElementDependencyPropertyChanges(element);

            if (element is AppBarButton button)
            {
                m_propertyChangedRevokersByElementMap[element] = new List<DependencyPropertyChangedRevoker>
                {
                    new(button, AppBarButton.IconProperty, OnCommandBarElementDependencyPropertyChanged),
                    new(button, AppBarButton.LabelProperty, OnCommandBarElementDependencyPropertyChanged),
                    new(button, AppBarButton.KeyboardAcceleratorTextOverrideProperty, OnCommandBarElementDependencyPropertyChanged)
                };
            }
            else if (element is AppBarToggleButton toggleButton)
            {
                m_propertyChangedRevokersByElementMap[element] = new List<DependencyPropertyChangedRevoker>
                {
                    new(toggleButton, AppBarToggleButton.IconProperty, OnCommandBarElementDependencyPropertyChanged),
                    new(toggleButton, AppBarToggleButton.LabelProperty, OnCommandBarElementDependencyPropertyChanged),
                    new(toggleButton, AppBarToggleButton.KeyboardAcceleratorTextOverrideProperty, OnCommandBarElementDependencyPropertyChanged)
                };
            }
        }

        private void UnhookCommandBarElementDependencyPropertyChanges(ICommandBarElement element)
        {
            if (m_propertyChangedRevokersByElementMap.TryGetValue(element, out var revokers))
            {
                foreach (var revoker in revokers)
                {
                    revoker.Revoke();
                }

                m_propertyChangedRevokersByElementMap.Remove(element);
            }
        }

        private void UnhookAllCommandBarElementDependencyPropertyChanges()
        {
            foreach (var revokers in m_propertyChangedRevokersByElementMap.Values)
            {
                foreach (var revoker in revokers)
                {
                    revoker.Revoke();
                }
            }

            m_propertyChangedRevokersByElementMap.Clear();
        }

        private void OnCommandBarElementDependencyPropertyChanged(object sender, EventArgs args)
        {
            m_commandBar?.OnCommandBarElementDependencyPropertyChanged();
        }

        private static void RevokeAndRemove(IDictionary<ICommandBarElement, RoutedEventHandlerRevoker> map, ICommandBarElement element)
        {
            if (map.TryGetValue(element, out var revoker))
            {
                revoker.Revoke();
                map.Remove(element);
            }
        }

        private static void RevokeAndClear(IDictionary<ICommandBarElement, RoutedEventHandlerRevoker> map)
        {
            foreach (var value in map.Values)
            {
                value.Revoke();
            }
            map.Clear();
        }

        CommandBarFlyoutCommandBar m_commandBar;

        Dictionary<ICommandBarElement, RoutedEventHandlerRevoker> m_secondaryButtonClickRevokerByElementMap =
            new Dictionary<ICommandBarElement, RoutedEventHandlerRevoker>();
        Dictionary<ICommandBarElement, RoutedEventHandlerRevoker> m_secondaryToggleButtonCheckedRevokerByElementMap =
            new Dictionary<ICommandBarElement, RoutedEventHandlerRevoker>();
        Dictionary<ICommandBarElement, RoutedEventHandlerRevoker> m_secondaryToggleButtonUncheckedRevokerByElementMap =
            new Dictionary<ICommandBarElement, RoutedEventHandlerRevoker>();
        Dictionary<ICommandBarElement, List<DependencyPropertyChangedRevoker>> m_propertyChangedRevokersByElementMap =
            new Dictionary<ICommandBarElement, List<DependencyPropertyChangedRevoker>>();

        FlyoutPresenter m_presenter;

        bool m_isClosingAfterCloseAnimation;

        private sealed class DependencyPropertyChangedRevoker
        {
            public DependencyPropertyChangedRevoker(
                DependencyObject source,
                DependencyProperty property,
                EventHandler handler)
            {
                _source = source;
                _handler = handler;
                _descriptor = DependencyPropertyDescriptor.FromProperty(property, source.GetType());
                _descriptor?.AddValueChanged(source, handler);
            }

            public void Revoke()
            {
                if (_descriptor != null && _source != null && _handler != null)
                {
                    _descriptor.RemoveValueChanged(_source, _handler);
                    _source = null;
                    _handler = null;
                    _descriptor = null;
                }
            }

            private DependencyObject _source;
            private EventHandler _handler;
            private DependencyPropertyDescriptor _descriptor;
        }
    }
}
