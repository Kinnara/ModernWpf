// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ModernWpf.Controls.Primitives;

namespace ModernWpf.Controls
{
    public class NavigationViewItemBase : ContentControl, IControlProtected
    {
        static NavigationViewItemBase()
        {
            HorizontalContentAlignmentProperty.OverrideMetadata(
                typeof(NavigationViewItemBase),
                new FrameworkPropertyMetadata(HorizontalAlignment.Center));

            VerticalContentAlignmentProperty.OverrideMetadata(
                typeof(NavigationViewItemBase),
                new FrameworkPropertyMetadata(VerticalAlignment.Center));

            KeyboardNavigation.DirectionalNavigationProperty.OverrideMetadata(
                typeof(NavigationViewItemBase),
                new FrameworkPropertyMetadata(KeyboardNavigationMode.None));
        }

        internal NavigationViewItemBase()
        {
        }

        #region IsSelected

        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register(
                nameof(IsSelected),
                typeof(bool),
                typeof(NavigationViewItemBase),
                new PropertyMetadata(false, OnIsSelectedPropertyChanged));

        public bool IsSelected
        {
            get => (bool)GetValue(IsSelectedProperty);
            set => SetValue(IsSelectedProperty, value);
        }

        internal event DependencyPropertyChangedCallback IsSelectedChanged;

        private static void OnIsSelectedPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((NavigationViewItemBase)sender).OnIsSelectedPropertyChanged(args);
        }

        private void OnIsSelectedPropertyChanged(DependencyPropertyChangedEventArgs args)
        {
            OnPropertyChangedPrivate(args);
            IsSelectedChanged?.Invoke(this, args.Property);
        }

        #endregion

        #region UseSystemFocusVisuals

        public static readonly DependencyProperty UseSystemFocusVisualsProperty =
            FocusVisualHelper.UseSystemFocusVisualsProperty.AddOwner(typeof(NavigationViewItemBase));

        public bool UseSystemFocusVisuals
        {
            get => (bool)GetValue(UseSystemFocusVisualsProperty);
            set => SetValue(UseSystemFocusVisualsProperty, value);
        }

        #endregion

        internal NavigationViewRepeaterPosition Position
        {
            get => m_position;
            set
            {
                if (m_position != value)
                {
                    m_position = value;
                    OnNavigationViewItemBasePositionChanged();
                }
            }
        }

        private protected virtual void OnNavigationViewItemBasePositionChanged() { }

        internal NavigationView GetNavigationView()
        {
            if (m_navigationView != null && m_navigationView.TryGetTarget(out var target))
            {
                return target;
            }
            return null;
        }

        internal int Depth
        {
            get => m_depth;
            set
            {
                if (m_depth != value)
                {
                    m_depth = value;
                    OnNavigationViewItemBaseDepthChanged();
                }
            }
        }

        private protected virtual void OnNavigationViewItemBaseDepthChanged() { }

        private protected virtual void OnNavigationViewItemBaseIsSelectedChanged() { }

        internal SplitView GetSplitView()
        {
            SplitView splitView = null;
            var navigationView = GetNavigationView();
            if (navigationView != null)
            {
                splitView = navigationView.GetSplitView();
            }
            return splitView;
        }

        internal void SetNavigationViewParent(NavigationView navigationView)
        {
            m_navigationView = new WeakReference<NavigationView>(navigationView);
        }

        void OnPropertyChangedPrivate(DependencyPropertyChangedEventArgs args)
        {
            if (args.Property == IsSelectedProperty)
            {
                OnNavigationViewItemBaseIsSelectedChanged();
            }
        }

        DependencyObject IControlProtected.GetTemplateChild(string childName)
        {
            return GetTemplateChild(childName);
        }

        // TODO: Constant is a temporary measure. Potentially expose using TemplateSettings.
        internal const int c_itemIndentation = 25;

        internal bool IsTopLevelItem
        {
            get => m_isTopLevelItem;
            set => m_isTopLevelItem = value;
        }

        internal bool CreatedByNavigationViewItemsFactory
        {
            get => m_createdByNavigationViewItemsFactory;
            set => m_createdByNavigationViewItemsFactory = value;
        }

        private protected WeakReference<NavigationView> m_navigationView;

        NavigationViewRepeaterPosition m_position = NavigationViewRepeaterPosition.LeftNav;
        int m_depth = 0;
        bool m_isTopLevelItem = false;

        // Flag to keep track of whether this item was created by the custom internal NavigationViewItemsFactory.
        // This is required in order to achieve proper recycling
        bool m_createdByNavigationViewItemsFactory = false;
    }
}