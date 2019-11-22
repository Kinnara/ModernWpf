using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;

namespace ModernWpf.Controls
{
    [ContentProperty(nameof(PrimaryCommands))]
    public class CommandBar : ContentControl
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

        #region PrimaryCommands

        public ObservableCollection<ICommandBarElement> PrimaryCommands { get; }

        private void PrimaryCommands_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                UpdateOverflowModeForPrimaryCommands(e.NewItems.OfType<DependencyObject>());
            }
        }

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

        #endregion

        #region DefaultLabelPosition

        public static readonly DependencyProperty DefaultLabelPositionProperty =
            SimpleToolBar.DefaultLabelPositionProperty.AddOwner(typeof(CommandBar));

        public CommandBarDefaultLabelPosition DefaultLabelPosition
        {
            get => (CommandBarDefaultLabelPosition)GetValue(DefaultLabelPositionProperty);
            set => SetValue(DefaultLabelPositionProperty, value);
        }

        #endregion

        #region IsDynamicOverflowEnabled

        public static readonly DependencyProperty IsDynamicOverflowEnabledProperty =
            SimpleToolBar.IsDynamicOverflowEnabledProperty.AddOwner(typeof(CommandBar),
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
            bool isDynamicOverflowEnabled = IsDynamicOverflowEnabled;
            foreach (var item in items)
            {
                ToolBar.SetOverflowMode(item, isDynamicOverflowEnabled ? OverflowMode.AsNeeded : OverflowMode.Always);
            }
        }

        #endregion

        #region OverflowButtonVisibility

        public static readonly DependencyProperty OverflowButtonVisibilityProperty =
            SimpleToolBar.OverflowButtonVisibilityProperty.AddOwner(typeof(CommandBar));

        public CommandBarOverflowButtonVisibility OverflowButtonVisibility
        {
            get => (CommandBarOverflowButtonVisibility)GetValue(OverflowButtonVisibilityProperty);
            set => SetValue(OverflowButtonVisibilityProperty, value);
        }

        #endregion

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            m_toolBar = GetTemplateChild(ToolBarName) as SimpleToolBar;

            if (m_toolBar != null)
            {
                m_toolBar.ItemsSource = new CompositeCollection
                {
                    new CollectionContainer { Collection = PrimaryCommands },
                    new CollectionContainer { Collection = SecondaryCommands },
                };
            }
        }

        private SimpleToolBar m_toolBar;

        private const string ToolBarName = "PART_ToolBar";
    }
}
