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
            AreOpenCloseAnimationsEnabled = true;

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
                    int index;
                    RoutedEventHandler closeFlyoutFunc = delegate { Hide(); };

                    // TODO
                    switch (args.Action)
                    {
                        case NotifyCollectionChangedAction.Replace:
                            {
                                index = args.NewStartingIndex;
                                var element = source[index];
                                var button = element as AppBarButton;
                                var toggleButton = element as AppBarToggleButton;

                                if (button != null && button.Flyout == null)
                                {
                                    m_secondaryButtonClickRevokerByIndexMap[index] = new RoutedEventHandlerRevoker(
                                        button, ButtonBase.ClickEvent, closeFlyoutFunc);
                                    m_secondaryToggleButtonCheckedRevokerByIndexMap.Remove(index);
                                    m_secondaryToggleButtonUncheckedRevokerByIndexMap.Remove(index);
                                }
                                else if (toggleButton != null)
                                {
                                    m_secondaryButtonClickRevokerByIndexMap.Remove(index);
                                    m_secondaryToggleButtonCheckedRevokerByIndexMap[index] = new RoutedEventHandlerRevoker(
                                        button, ToggleButton.CheckedEvent, closeFlyoutFunc);
                                    m_secondaryToggleButtonUncheckedRevokerByIndexMap[index] = new RoutedEventHandlerRevoker(
                                        button, ToggleButton.UncheckedEvent, closeFlyoutFunc);
                                }
                                else
                                {
                                    m_secondaryButtonClickRevokerByIndexMap.Remove(index);
                                    m_secondaryToggleButtonCheckedRevokerByIndexMap.Remove(index);
                                    m_secondaryToggleButtonUncheckedRevokerByIndexMap.Remove(index);
                                }
                                break;
                            }
                        case NotifyCollectionChangedAction.Add:
                            {
                                index = args.NewStartingIndex;
                                var element = source[index];
                                var button = element as AppBarButton;
                                var toggleButton = element as AppBarToggleButton;

                                if (button != null && button.Flyout == null)
                                {
                                    m_secondaryButtonClickRevokerByIndexMap[index] = new RoutedEventHandlerRevoker(
                                        button, ButtonBase.ClickEvent, closeFlyoutFunc);
                                }
                                else if (toggleButton != null)
                                {
                                    m_secondaryToggleButtonCheckedRevokerByIndexMap[index] = new RoutedEventHandlerRevoker(
                                        button, ToggleButton.CheckedEvent, closeFlyoutFunc);
                                    m_secondaryToggleButtonUncheckedRevokerByIndexMap[index] = new RoutedEventHandlerRevoker(
                                        button, ToggleButton.UncheckedEvent, closeFlyoutFunc);
                                }
                                break;
                            }
                        case NotifyCollectionChangedAction.Remove:
                            index = args.OldStartingIndex;
                            m_secondaryButtonClickRevokerByIndexMap.Remove(index);
                            m_secondaryToggleButtonCheckedRevokerByIndexMap.Remove(index);
                            m_secondaryToggleButtonUncheckedRevokerByIndexMap.Remove(index);
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
                m_commandBar.IsOpen = true;

                if (m_commandBar.HasOpenAnimation())
                {
                    InternalPopup.PopupAnimation = PopupAnimation.Fade;
                    InternalPopup.SuppressFadeAnimation = true;
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

        public ObservableCollection<ICommandBarElement> PrimaryCommands { get; }

        public ObservableCollection<ICommandBarElement> SecondaryCommands { get; }

        protected override Control CreatePresenter()
        {
            var commandBar = new CommandBarFlyoutCommandBar();

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

        void SetSecondaryCommandsToCloseWhenExecuted()
        {
            m_secondaryButtonClickRevokerByIndexMap.Clear();
            m_secondaryToggleButtonCheckedRevokerByIndexMap.Clear();
            m_secondaryToggleButtonUncheckedRevokerByIndexMap.Clear();

            RoutedEventHandler closeFlyoutFunc = delegate { Hide(); };

            for (int i = 0; i < SecondaryCommands.Count; i++)
            {
                var element = SecondaryCommands[i];
                var button = element as AppBarButton;
                var toggleButton = element as AppBarToggleButton;

                if (button != null)
                {
                    m_secondaryButtonClickRevokerByIndexMap[i] = new RoutedEventHandlerRevoker(
                        button, ButtonBase.ClickEvent, closeFlyoutFunc);
                }
                else if (toggleButton != null)
                {
                    m_secondaryToggleButtonCheckedRevokerByIndexMap[i] = new RoutedEventHandlerRevoker(
                        button, ToggleButton.CheckedEvent, closeFlyoutFunc);
                    m_secondaryToggleButtonUncheckedRevokerByIndexMap[i] = new RoutedEventHandlerRevoker(
                        button, ToggleButton.UncheckedEvent, closeFlyoutFunc);
                }
            }
        }

        CommandBarFlyoutCommandBar m_commandBar;

        Dictionary<int, RoutedEventHandlerRevoker> m_secondaryButtonClickRevokerByIndexMap = new Dictionary<int, RoutedEventHandlerRevoker>();
        Dictionary<int, RoutedEventHandlerRevoker> m_secondaryToggleButtonCheckedRevokerByIndexMap = new Dictionary<int, RoutedEventHandlerRevoker>();
        Dictionary<int, RoutedEventHandlerRevoker> m_secondaryToggleButtonUncheckedRevokerByIndexMap = new Dictionary<int, RoutedEventHandlerRevoker>();

        bool m_isClosingAfterCloseAnimation;
    }
}
