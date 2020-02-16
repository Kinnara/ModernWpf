using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using ModernWpf.Controls.Primitives;

namespace ModernWpf.Controls
{
    [ContentProperty(nameof(PrimaryCommands))]
    [TemplatePart(Name = ToolBarName, Type = typeof(CommandBarToolBar))]
    public class CommandBar : Control
    {
        static CommandBar()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CommandBar), new FrameworkPropertyMetadata(typeof(CommandBar)));
        }

        public CommandBar()
        {
            PrimaryCommands = new ObservableCollection<ICommandBarElement>();
            PrimaryCommands.CollectionChanged += PrimaryCommands_CollectionChanged;

            SecondaryCommands = new ObservableCollection<ICommandBarElement>();
            SecondaryCommands.CollectionChanged += SecondaryCommands_CollectionChanged;
        }

        #region Content

        public static readonly DependencyProperty ContentProperty =
            DependencyProperty.Register(
                nameof(Content),
                typeof(object),
                typeof(CommandBar));

        public object Content
        {
            get => GetValue(ContentProperty);
            set => SetValue(ContentProperty, value);
        }

        #endregion

        #region ContentTemplate

        public static readonly DependencyProperty ContentTemplateProperty =
            DependencyProperty.Register(
                nameof(ContentTemplate),
                typeof(DataTemplate),
                typeof(CommandBar));

        public DataTemplate ContentTemplate
        {
            get => (DataTemplate)GetValue(ContentTemplateProperty);
            set => SetValue(ContentTemplateProperty, value);
        }

        #endregion

        #region CornerRadius

        public static readonly DependencyProperty CornerRadiusProperty =
            ControlHelper.CornerRadiusProperty.AddOwner(typeof(CommandBar));

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        #endregion

        #region IsOpen

        public static readonly DependencyProperty IsOpenProperty =
            DependencyProperty.Register(
                nameof(IsOpen),
                typeof(bool),
                typeof(CommandBar),
                new FrameworkPropertyMetadata(
                    false,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public bool IsOpen
        {
            get => (bool)GetValue(IsOpenProperty);
            set => SetValue(IsOpenProperty, value);
        }

        #endregion

        #region PrimaryCommands

        public ObservableCollection<ICommandBarElement> PrimaryCommands { get; }

        private void PrimaryCommands_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                UpdateOverflowModeForPrimaryCommands(e.NewItems.OfType<DependencyObject>());
            }
        }

        private bool HasPrimaryCommands => m_toolBar != null && m_toolBar.HasPrimaryCommands;

        #endregion

        #region SecondaryCommands

        public ObservableCollection<ICommandBarElement> SecondaryCommands { get; }

        private void SecondaryCommands_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                UpdateOverflowModeForSecondaryCommands(e.NewItems.OfType<DependencyObject>());
            }
        }

        private bool HasSecondaryCommands => m_toolBar != null && m_toolBar.HasOverflowItems;

        #endregion

        #region CommandBarOverflowPresenterStyle

        public static readonly DependencyProperty CommandBarOverflowPresenterStyleProperty =
            DependencyProperty.Register(
                nameof(CommandBarOverflowPresenterStyle),
                typeof(Style),
                typeof(CommandBar),
                null);

        public Style CommandBarOverflowPresenterStyle
        {
            get => (Style)GetValue(CommandBarOverflowPresenterStyleProperty);
            set => SetValue(CommandBarOverflowPresenterStyleProperty, value);
        }

        #endregion

        #region DefaultLabelPosition

        public static readonly DependencyProperty DefaultLabelPositionProperty =
            CommandBarToolBar.DefaultLabelPositionProperty.AddOwner(typeof(CommandBar));

        public CommandBarDefaultLabelPosition DefaultLabelPosition
        {
            get => (CommandBarDefaultLabelPosition)GetValue(DefaultLabelPositionProperty);
            set => SetValue(DefaultLabelPositionProperty, value);
        }

        #endregion

        #region IsDynamicOverflowEnabled

        public static readonly DependencyProperty IsDynamicOverflowEnabledProperty =
            CommandBarToolBar.IsDynamicOverflowEnabledProperty.AddOwner(typeof(CommandBar),
                new FrameworkPropertyMetadata(OnIsDynamicOverflowEnabledChanged));

        public bool IsDynamicOverflowEnabled
        {
            get => (bool)GetValue(IsDynamicOverflowEnabledProperty);
            set => SetValue(IsDynamicOverflowEnabledProperty, value);
        }

        private static void OnIsDynamicOverflowEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((CommandBar)d).OnIsDynamicOverflowEnabledChanged();
        }

        private  void OnIsDynamicOverflowEnabledChanged()
        {
            UpdateOverflowModeForPrimaryCommands(PrimaryCommands.OfType<DependencyObject>());
            UpdateOverflowModeForSecondaryCommands(SecondaryCommands.OfType<DependencyObject>());
        }

        private void UpdateOverflowModeForPrimaryCommands(IEnumerable<DependencyObject> items)
        {
            bool isDynamicOverflowEnabled = IsDynamicOverflowEnabled;
            foreach (var item in items)
            {
                ToolBar.SetOverflowMode(item, isDynamicOverflowEnabled ? OverflowMode.AsNeeded : OverflowMode.Never);
            }
        }

        private void UpdateOverflowModeForSecondaryCommands(IEnumerable<DependencyObject> items)
        {
            foreach (var item in items)
            {
                ToolBar.SetOverflowMode(item, OverflowMode.Always);
            }
        }

        #endregion

        #region OverflowButtonVisibility

        public static readonly DependencyProperty OverflowButtonVisibilityProperty =
            CommandBarToolBar.OverflowButtonVisibilityProperty.AddOwner(typeof(CommandBar));

        public CommandBarOverflowButtonVisibility OverflowButtonVisibility
        {
            get => (CommandBarOverflowButtonVisibility)GetValue(OverflowButtonVisibilityProperty);
            set => SetValue(OverflowButtonVisibilityProperty, value);
        }

        #endregion

        public event EventHandler<object> Opened;

        public event EventHandler<object> Closed;

        public override void OnApplyTemplate()
        {
            if (m_toolBar != null)
            {
                m_toolBar.ClearValue(ItemsControl.ItemsSourceProperty);
                m_toolBar.OverflowOpened -= OnOverflowOpened;
                m_toolBar.OverflowClosed -= OnOverflowClosed;
                m_toolBar.HasPrimaryCommandsChanged -= OnHasPrimaryCommandsChanged;
                m_toolBar.HasOverflowItemsChanged -= OnHasOverflowItemsChanged;
            }

            base.OnApplyTemplate();

            m_toolBar = GetTemplateChild(ToolBarName) as CommandBarToolBar;

            if (m_toolBar != null)
            {
                m_toolBar.ItemsSource = new CompositeCollection
                {
                    new CollectionContainer { Collection = PrimaryCommands },
                    new CollectionContainer { Collection = SecondaryCommands },
                };
                m_toolBar.OverflowOpened += OnOverflowOpened;
                m_toolBar.OverflowClosed += OnOverflowClosed;
                m_toolBar.HasPrimaryCommandsChanged += OnHasPrimaryCommandsChanged;
                m_toolBar.HasOverflowItemsChanged += OnHasOverflowItemsChanged;
            }

            UpdateVisualState(false);
        }

        private void OnOverflowOpened(object sender, EventArgs e)
        {
            Opened?.Invoke(this, null);
        }

        private void OnOverflowClosed(object sender, EventArgs e)
        {
            Closed?.Invoke(this, null);
        }

        private void OnHasPrimaryCommandsChanged(object sender, EventArgs e)
        {
            UpdateVisualState();
        }

        private void OnHasOverflowItemsChanged(object sender, EventArgs e)
        {
            UpdateVisualState();
        }

        internal void UpdateVisualState(bool useTransitions = true)
        {
            if (m_toolBar != null)
            {
                string stateName;

                if (HasPrimaryCommands && HasSecondaryCommands)
                {
                    stateName = "BothCommands";
                }
                else if (HasSecondaryCommands)
                {
                    stateName = "SecondaryCommandsOnly";
                }
                else
                {
                    stateName = "PrimaryCommandsOnly";
                }

                VisualStateManager.GoToState(m_toolBar, stateName, useTransitions);
            }
        }

        private CommandBarToolBar m_toolBar;

        internal const string ToolBarName = "PART_ToolBar";
    }
}
