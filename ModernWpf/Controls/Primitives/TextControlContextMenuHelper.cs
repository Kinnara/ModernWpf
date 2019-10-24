using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace ModernWpf.Controls.Primitives
{
    public static class TextControlContextMenuHelper
    {
        #region IsEnabled

        public static bool GetIsEnabled(ContextMenu contextMenu)
        {
            return (bool)contextMenu.GetValue(IsEnabledProperty);
        }

        public static void SetIsEnabled(ContextMenu contextMenu, bool value)
        {
            contextMenu.SetValue(IsEnabledProperty, value);
        }

        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached(
                "IsEnabled",
                typeof(bool),
                typeof(TextControlContextMenuHelper),
                new PropertyMetadata(OnIsEnabledChanged));

        private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var contextMenu = (ContextMenu)d;

            if ((bool)e.NewValue)
            {
                contextMenu.Opened += OnOpened;
                contextMenu.Closed += OnClosed;
            }
            else
            {
                contextMenu.Opened -= OnOpened;
                contextMenu.Closed -= OnClosed;
                UnfreezeItemsVisibility(contextMenu);
                contextMenu.ClearValue(TokenProperty);
            }
        }

        #endregion

        #region Token

        private static Guid GetToken(ContextMenu contextMenu)
        {
            return (Guid)contextMenu.GetValue(TokenProperty);
        }

        private static void SetToken(ContextMenu contextMenu, Guid value)
        {
            contextMenu.SetValue(TokenProperty, value);
        }

        private static readonly DependencyProperty TokenProperty = DependencyProperty.RegisterAttached(
            "Token",
            typeof(Guid),
            typeof(TextControlContextMenuHelper),
            new PropertyMetadata(Guid.Empty));

        #endregion

        #region FreezeVisibilityStoryboard

        private static Storyboard GetFreezeVisibilityStoryboard(ContextMenu contextMenu)
        {
            return (Storyboard)contextMenu.GetValue(FreezeVisibilityStoryboardProperty);
        }

        private static void SetFreezeVisibilityStoryboard(ContextMenu contextMenu, Storyboard value)
        {
            contextMenu.SetValue(FreezeVisibilityStoryboardProperty, value);
        }

        private static readonly DependencyProperty FreezeVisibilityStoryboardProperty =
            DependencyProperty.RegisterAttached(
                "FreezeVisibilityStoryboard",
                typeof(Storyboard),
                typeof(TextControlContextMenuHelper));

        #endregion

        private static void OnOpened(object sender, RoutedEventArgs e)
        {
            var contextMenu = (ContextMenu)sender;

            UnfreezeItemsVisibility(contextMenu);

            var token = Guid.NewGuid();
            SetToken(contextMenu, token);

            contextMenu.Dispatcher.BeginInvoke(() =>
            {
                if (contextMenu.IsOpen && GetToken(contextMenu) == token)
                {
                    if (contextMenu.Items.OfType<MenuItem>().Any(x => x.IsEnabled))
                    {
                        FreezeItemsVisibility(contextMenu);
                    }
                    else
                    {
                        contextMenu.IsOpen = false;
                    }
                }
            }, DispatcherPriority.Background);
        }

        private static void OnClosed(object sender, RoutedEventArgs e)
        {
            UnfreezeItemsVisibility((ContextMenu)sender);
        }

        private static void FreezeItemsVisibility(ContextMenu contextMenu)
        {
            var storyboard = new Storyboard();
            foreach (var menuItem in contextMenu.Items.OfType<MenuItem>())
            {
                var animation = new ObjectAnimationUsingKeyFrames
                {
                    KeyFrames =
                    {
                        new DiscreteObjectKeyFrame(menuItem.Visibility)
                    }
                };
                Storyboard.SetTarget(animation, menuItem);
                Storyboard.SetTargetProperty(animation, new PropertyPath(UIElement.VisibilityProperty));
                storyboard.Children.Add(animation);
            }

            if (storyboard.Children.Count > 0)
            {
                SetFreezeVisibilityStoryboard(contextMenu, storyboard);
                storyboard.Begin();
            }
        }

        private static void UnfreezeItemsVisibility(ContextMenu contextMenu)
        {
            var freezeVisibilityStoryboard = GetFreezeVisibilityStoryboard(contextMenu);
            if (freezeVisibilityStoryboard != null)
            {
                freezeVisibilityStoryboard.Stop();
                contextMenu.ClearValue(FreezeVisibilityStoryboardProperty);
            }
        }
    }
}
