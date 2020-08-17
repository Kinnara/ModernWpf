// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
            //AreOpenCloseAnimationsEnabled = false;

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

                    // TODO: WPF
                    switch (args.Action)
                    {
                        case NotifyCollectionChangedAction.Replace:
                            {
                                var element = (ICommandBarElement)args.NewItems[0];
                                var oldElement = (ICommandBarElement)args.OldItems[0];
                                var button = element as AppBarButton;
                                var toggleButton = element as AppBarToggleButton;

                                RevokeAndRemove(m_secondaryButtonClickRevokerByElementMap, oldElement);
                                RevokeAndRemove(m_secondaryToggleButtonCheckedRevokerByElementMap, oldElement);
                                RevokeAndRemove(m_secondaryToggleButtonUncheckedRevokerByElementMap, oldElement);

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
                                break;
                            }
                        case NotifyCollectionChangedAction.Add:
                            {
                                var element = (ICommandBarElement)args.NewItems[0];
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
                                break;
                            }
                        case NotifyCollectionChangedAction.Remove:
                            {
                                var element = (ICommandBarElement)args.OldItems[0];
                                RevokeAndRemove(m_secondaryButtonClickRevokerByElementMap, element);
                                RevokeAndRemove(m_secondaryToggleButtonCheckedRevokerByElementMap, element);
                                RevokeAndRemove(m_secondaryToggleButtonUncheckedRevokerByElementMap, element);
                                break;
                            }
                        case NotifyCollectionChangedAction.Move:
                            break;
                        case NotifyCollectionChangedAction.Reset:
                            SetSecondaryCommandsToCloseWhenExecuted();
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

                if (ShowMode == FlyoutShowMode.Standard)
                {
                    m_commandBar.IsOpen = true;
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

            Closing += delegate
            {
                var commandBar = m_commandBar;
                if (commandBar != null)
                {
                    if (!m_isClosingAfterCloseAnimation && commandBar.HasCloseAnimation())
                    {
                        //args.Cancel(true);

                        commandBar.PlayCloseAnimation(() =>
                        {
                            m_isClosingAfterCloseAnimation = true;
                            Hide();
                            m_isClosingAfterCloseAnimation = false;
                        });
                    }
                    // Close commandbar and thus other associated flyouts
                    commandBar.IsOpen = false;

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

        public ObservableCollection<ICommandBarElement> PrimaryCommands { get; }

        public ObservableCollection<ICommandBarElement> SecondaryCommands { get; }

        internal override PopupAnimation DesiredPopupAnimation => PopupAnimation.Fade;

        protected override Control CreatePresenter()
        {
            var commandBar = new CommandBarFlyoutCommandBar();

            commandBar.Opened += delegate
            {
                SetCurrentValue(ShowModeProperty, FlyoutShowMode.Standard);
            };

            SharedHelpers.CopyList(PrimaryCommands, commandBar.PrimaryCommands);
            SharedHelpers.CopyList(SecondaryCommands, commandBar.SecondaryCommands);

            SetSecondaryCommandsToCloseWhenExecuted();

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

            commandBar.SetOwningFlyout(this);

            m_commandBar = commandBar;
            return presenter;
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

        bool m_isClosingAfterCloseAnimation;
    }
}
