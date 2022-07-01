using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Windows;
using System.Windows.Controls;

namespace ModernWpf.Controls
{
    public partial class PipsPager
    {
        #region MaxVisiblePips

        public int MaxVisiblePips
        {
            get => (int)GetValue(MaxVisiblePipsProperty);
            set => SetValue(MaxVisiblePipsProperty, value);
        }

        public static readonly DependencyProperty MaxVisiblePipsProperty =
            DependencyProperty.Register(
                nameof(MaxVisiblePips),
                typeof(int),
                typeof(PipsPager),
                new PropertyMetadata(5, OnMaxVisiblePipsPropertyChanged));

        private static void OnMaxVisiblePipsPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((PipsPager)sender).OnPropertyChanged(args);
        }

        #endregion

        #region NextButtonStyle

        public Style NextButtonStyle
        {
            get => (Style)GetValue(NextButtonStyleProperty);
            set => SetValue(NextButtonStyleProperty, value);
        }

        public static readonly DependencyProperty NextButtonStyleProperty =
            DependencyProperty.Register(
                nameof(NextButtonStyle),
                typeof(Style),
                typeof(PipsPager),
                new PropertyMetadata(OnNextButtonStylePropertyChanged));

        private static void OnNextButtonStylePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((PipsPager)sender).OnPropertyChanged(args);
        }

        #endregion

        #region NextButtonVisibility

        public PipsPagerButtonVisibility NextButtonVisibility
        {
            get => (PipsPagerButtonVisibility)GetValue(NextButtonVisibilityProperty);
            set => SetValue(NextButtonVisibilityProperty, value);
        }

        public static readonly DependencyProperty NextButtonVisibilityProperty =
            DependencyProperty.Register(
                nameof(NextButtonVisibility),
                typeof(PipsPagerButtonVisibility),
                typeof(PipsPager),
                new PropertyMetadata(PipsPagerButtonVisibility.Collapsed, OnNextButtonVisibilityPropertyChanged));

        private static void OnNextButtonVisibilityPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((PipsPager)sender).OnPropertyChanged(args);
        }

        #endregion

        #region NormalPipStyle

        public Style NormalPipStyle
        {
            get => (Style)GetValue(NormalPipStyleProperty);
            set => SetValue(NormalPipStyleProperty, value);
        }

        public static readonly DependencyProperty NormalPipStyleProperty =
            DependencyProperty.Register(
                nameof(NormalPipStyle),
                typeof(Style),
                typeof(PipsPager),
                new PropertyMetadata(OnNormalPipStylePropertyChanged));

        private static void OnNormalPipStylePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((PipsPager)sender).OnPropertyChanged(args);
        }

        #endregion

        #region NumberOfPages

        public int NumberOfPages
        {
            get => (int)GetValue(NumberOfPagesProperty);
            set => SetValue(NumberOfPagesProperty, value);
        }

        public static readonly DependencyProperty NumberOfPagesProperty =
            DependencyProperty.Register(
                nameof(NumberOfPages),
                typeof(int),
                typeof(PipsPager),
                new PropertyMetadata(-1, OnNumberOfPagesPropertyChanged));

        private static void OnNumberOfPagesPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((PipsPager)sender).OnPropertyChanged(args);
        }

        #endregion

        #region Orientation

        public Orientation Orientation
        {
            get => (Orientation)GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }

        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register(
                nameof(Orientation),
                typeof(Orientation),
                typeof(PipsPager),
                new PropertyMetadata(Orientation.Horizontal, OnOrientationPropertyChanged));

        private static void OnOrientationPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((PipsPager)sender).OnPropertyChanged(args);
        }

        #endregion

        #region PreviousButtonStyle

        public Style PreviousButtonStyle
        {
            get => (Style)GetValue(PreviousButtonStyleProperty);
            set => SetValue(PreviousButtonStyleProperty, value);
        }

        public static readonly DependencyProperty PreviousButtonStyleProperty =
            DependencyProperty.Register(
                nameof(PreviousButtonStyle),
                typeof(Style),
                typeof(PipsPager),
                new PropertyMetadata(OnPreviousButtonStylePropertyChanged));

        private static void OnPreviousButtonStylePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((PipsPager)sender).OnPropertyChanged(args);
        }

        #endregion

        #region PreviousButtonVisibility

        public PipsPagerButtonVisibility PreviousButtonVisibility
        {
            get => (PipsPagerButtonVisibility)GetValue(PreviousButtonVisibilityProperty);
            set => SetValue(PreviousButtonVisibilityProperty, value);
        }

        public static readonly DependencyProperty PreviousButtonVisibilityProperty =
            DependencyProperty.Register(
                nameof(PreviousButtonVisibility),
                typeof(PipsPagerButtonVisibility),
                typeof(PipsPager),
                new PropertyMetadata(PipsPagerButtonVisibility.Collapsed, OnPreviousButtonVisibilityPropertyChanged));

        private static void OnPreviousButtonVisibilityPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((PipsPager)sender).OnPropertyChanged(args);
        }

        #endregion

        #region SelectedPageIndex

        public int SelectedPageIndex
        {
            get => (int)GetValue(SelectedPageIndexProperty);
            set => SetValue(SelectedPageIndexProperty, value);
        }

        public static readonly DependencyProperty SelectedPageIndexProperty =
            DependencyProperty.Register(
                nameof(SelectedPageIndex),
                typeof(int),
                typeof(PipsPager),
                new PropertyMetadata(0, OnSelectedPageIndexPropertyChanged));

        private static void OnSelectedPageIndexPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((PipsPager)sender).OnPropertyChanged(args);
        }

        #endregion

        #region SelectedPipStyle

        public Style SelectedPipStyle
        {
            get => (Style)GetValue(SelectedPipStyleProperty);
            set => SetValue(SelectedPipStyleProperty, value);
        }

        public static readonly DependencyProperty SelectedPipStyleProperty =
            DependencyProperty.Register(
                nameof(SelectedPipStyle),
                typeof(Style),
                typeof(PipsPager),
                new PropertyMetadata(OnSelectedPipStylePropertyChanged));

        private static void OnSelectedPipStylePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((PipsPager)sender).OnPropertyChanged(args);
        }

        #endregion

        #region TemplateSettings

        private static readonly DependencyPropertyKey TemplateSettingsPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(TemplateSettings),
                typeof(PipsPagerTemplateSettings),
                typeof(PipsPager),
                null);

        public static readonly DependencyProperty TemplateSettingsProperty =
            TemplateSettingsPropertyKey.DependencyProperty;

        public PipsPagerTemplateSettings TemplateSettings
        {
            get => (PipsPagerTemplateSettings)GetValue(TemplateSettingsProperty);
        }

        #endregion

        public event TypedEventHandler<PipsPager, PipsPagerSelectedIndexChangedEventArgs> SelectedIndexChanged;
    }
}
