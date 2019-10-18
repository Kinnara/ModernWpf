using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace ModernWpf.Controls.Primitives
{
    public class DataGridHelper
    {
        #region TextColumnElementStyle

        public static readonly DependencyProperty TextColumnElementStyleProperty =
            DependencyProperty.RegisterAttached(
                "TextColumnElementStyle",
                typeof(Style),
                typeof(DataGridHelper),
                new PropertyMetadata(default(Style), OnTextColumnElementStyleChanged));

        public static Style GetTextColumnElementStyle(DataGrid dataGrid)
        {
            return (Style)dataGrid.GetValue(TextColumnElementStyleProperty);
        }

        public static void SetTextColumnElementStyle(DataGrid dataGrid, Style value)
        {
            dataGrid.SetValue(TextColumnElementStyleProperty, value);
        }

        private static void OnTextColumnElementStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var dataGrid = (DataGrid)d;
            var oldValue = (Style)e.OldValue;
            var newValue = (Style)e.NewValue;
        }

        #endregion

        #region TextColumnEditingElementStyle

        public static readonly DependencyProperty TextColumnEditingElementStyleProperty =
            DependencyProperty.RegisterAttached(
                "TextColumnEditingElementStyle",
                typeof(Style),
                typeof(DataGridHelper),
                new PropertyMetadata(default(Style), OnTextColumnEditingElementStyleChanged));

        public static Style GetTextColumnEditingElementStyle(DataGrid dataGrid)
        {
            return (Style)dataGrid.GetValue(TextColumnEditingElementStyleProperty);
        }

        public static void SetTextColumnEditingElementStyle(DataGrid dataGrid, Style value)
        {
            dataGrid.SetValue(TextColumnEditingElementStyleProperty, value);
        }

        private static void OnTextColumnEditingElementStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var dataGrid = (DataGrid)d;
            var oldValue = (Style)e.OldValue;
            var newValue = (Style)e.NewValue;
        }

        #endregion

        #region TextColumnFontSize

        public static readonly DependencyProperty TextColumnFontSizeProperty =
            DependencyProperty.RegisterAttached(
                "TextColumnFontSize",
                typeof(double),
                typeof(DataGridHelper),
                new PropertyMetadata(SystemFonts.MessageFontSize, OnTextColumnFontSizeChanged));

        public static double GetTextColumnFontSize(DataGrid dataGrid)
        {
            return (double)dataGrid.GetValue(TextColumnFontSizeProperty);
        }

        public static void SetTextColumnFontSize(DataGrid dataGrid, double value)
        {
            dataGrid.SetValue(TextColumnFontSizeProperty, value);
        }

        private static void OnTextColumnFontSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var dataGrid = (DataGrid)d;
            var oldValue = (double)e.OldValue;
            var newValue = (double)e.NewValue;
        }

        #endregion

        #region CheckBoxColumnElementStyle

        public static readonly DependencyProperty CheckBoxColumnElementStyleProperty =
            DependencyProperty.RegisterAttached(
                "CheckBoxColumnElementStyle",
                typeof(Style),
                typeof(DataGridHelper),
                new PropertyMetadata(default(Style), OnCheckBoxColumnElementStyleChanged));

        public static Style GetCheckBoxColumnElementStyle(DataGrid dataGrid)
        {
            return (Style)dataGrid.GetValue(CheckBoxColumnElementStyleProperty);
        }

        public static void SetCheckBoxColumnElementStyle(DataGrid dataGrid, Style value)
        {
            dataGrid.SetValue(CheckBoxColumnElementStyleProperty, value);
        }

        private static void OnCheckBoxColumnElementStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var dataGrid = (DataGrid)d;
            var oldValue = (Style)e.OldValue;
            var newValue = (Style)e.NewValue;
        }

        #endregion

        #region CheckBoxColumnEditingElementStyle

        public static readonly DependencyProperty CheckBoxColumnEditingElementStyleProperty =
            DependencyProperty.RegisterAttached(
                "CheckBoxColumnEditingElementStyle",
                typeof(Style),
                typeof(DataGridHelper),
                new PropertyMetadata(default(Style), OnCheckBoxColumnEditingElementStyleChanged));

        public static Style GetCheckBoxColumnEditingElementStyle(DataGrid dataGrid)
        {
            return (Style)dataGrid.GetValue(CheckBoxColumnEditingElementStyleProperty);
        }

        public static void SetCheckBoxColumnEditingElementStyle(DataGrid dataGrid, Style value)
        {
            dataGrid.SetValue(CheckBoxColumnEditingElementStyleProperty, value);
        }

        private static void OnCheckBoxColumnEditingElementStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var dataGrid = (DataGrid)d;
            var oldValue = (Style)e.OldValue;
            var newValue = (Style)e.NewValue;
        }

        #endregion

        #region ComboBoxColumnElementStyle

        public static readonly DependencyProperty ComboBoxColumnElementStyleProperty =
            DependencyProperty.RegisterAttached(
                "ComboBoxColumnElementStyle",
                typeof(Style),
                typeof(DataGridHelper),
                new PropertyMetadata(default(Style), OnComboBoxColumnElementStyleChanged));

        public static Style GetComboBoxColumnElementStyle(DataGrid dataGrid)
        {
            return (Style)dataGrid.GetValue(ComboBoxColumnElementStyleProperty);
        }

        public static void SetComboBoxColumnElementStyle(DataGrid dataGrid, Style value)
        {
            dataGrid.SetValue(ComboBoxColumnElementStyleProperty, value);
        }

        private static void OnComboBoxColumnElementStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var dataGrid = (DataGrid)d;
            var oldValue = (Style)e.OldValue;
            var newValue = (Style)e.NewValue;
        }

        #endregion

        #region ComboBoxColumnEditingElementStyle

        public static readonly DependencyProperty ComboBoxColumnEditingElementStyleProperty =
            DependencyProperty.RegisterAttached(
                "ComboBoxColumnEditingElementStyle",
                typeof(Style),
                typeof(DataGridHelper),
                new PropertyMetadata(default(Style), OnComboBoxColumnEditingElementStyleChanged));

        public static Style GetComboBoxColumnEditingElementStyle(DataGrid dataGrid)
        {
            return (Style)dataGrid.GetValue(ComboBoxColumnEditingElementStyleProperty);
        }

        public static void SetComboBoxColumnEditingElementStyle(DataGrid dataGrid, Style value)
        {
            dataGrid.SetValue(ComboBoxColumnEditingElementStyleProperty, value);
        }

        private static void OnComboBoxColumnEditingElementStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var dataGrid = (DataGrid)d;
            var oldValue = (Style)e.OldValue;
            var newValue = (Style)e.NewValue;
        }

        #endregion

        #region HyperlinkColumnElementStyle

        public static readonly DependencyProperty HyperlinkColumnElementStyleProperty =
            DependencyProperty.RegisterAttached(
                "HyperlinkColumnElementStyle",
                typeof(Style),
                typeof(DataGridHelper),
                new PropertyMetadata(default(Style), OnHyperlinkColumnElementStyleChanged));

        public static Style GetHyperlinkColumnElementStyle(DataGrid dataGrid)
        {
            return (Style)dataGrid.GetValue(HyperlinkColumnElementStyleProperty);
        }

        public static void SetHyperlinkColumnElementStyle(DataGrid dataGrid, Style value)
        {
            dataGrid.SetValue(HyperlinkColumnElementStyleProperty, value);
        }

        private static void OnHyperlinkColumnElementStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var dataGrid = (DataGrid)d;
            var oldValue = (Style)e.OldValue;
            var newValue = (Style)e.NewValue;
        }

        #endregion

        #region HyperlinkColumnEditingElementStyle

        public static readonly DependencyProperty HyperlinkColumnEditingElementStyleProperty =
            DependencyProperty.RegisterAttached(
                "HyperlinkColumnEditingElementStyle",
                typeof(Style),
                typeof(DataGridHelper),
                new PropertyMetadata(default(Style), OnHyperlinkColumnEditingElementStyleChanged));

        public static Style GetHyperlinkColumnEditingElementStyle(DataGrid dataGrid)
        {
            return (Style)dataGrid.GetValue(HyperlinkColumnEditingElementStyleProperty);
        }

        public static void SetHyperlinkColumnEditingElementStyle(DataGrid dataGrid, Style value)
        {
            dataGrid.SetValue(HyperlinkColumnEditingElementStyleProperty, value);
        }

        private static void OnHyperlinkColumnEditingElementStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var dataGrid = (DataGrid)d;
            var oldValue = (Style)e.OldValue;
            var newValue = (Style)e.NewValue;
        }

        #endregion

        #region UseModernColumnStyles

        public static readonly DependencyProperty UseModernColumnStylesProperty =
            DependencyProperty.RegisterAttached(
                "UseModernColumnStyles",
                typeof(bool),
                typeof(DataGridHelper),
                new PropertyMetadata(OnUseModernColumnStylesChanged));

        public static bool GetUseModernColumnStyles(DataGrid dataGrid)
        {
            return (bool)dataGrid.GetValue(UseModernColumnStylesProperty);
        }

        public static void SetUseModernColumnStyles(DataGrid dataGrid, bool value)
        {
            dataGrid.SetValue(UseModernColumnStylesProperty, value);
        }

        private static void OnUseModernColumnStylesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var dataGrid = (DataGrid)d;
            if ((bool)e.NewValue)
            {
                SetColumnStylesHelper(dataGrid, new ColumnStylesHelper(dataGrid));
            }
            else
            {
                dataGrid.ClearValue(ColumnStylesHelperProperty);
            }
        }

        #endregion

        #region ColumnStylesHelper

        private static readonly DependencyProperty ColumnStylesHelperProperty =
            DependencyProperty.RegisterAttached(
                "ColumnStylesHelper",
                typeof(ColumnStylesHelper),
                typeof(DataGridHelper),
                new PropertyMetadata(default(ColumnStylesHelper), OnColumnStylesHelperChanged));

        private static ColumnStylesHelper GetColumnStylesHelper(DataGrid dataGrid)
        {
            return (ColumnStylesHelper)dataGrid.GetValue(ColumnStylesHelperProperty);
        }

        private static void SetColumnStylesHelper(DataGrid dataGrid, ColumnStylesHelper value)
        {
            dataGrid.SetValue(ColumnStylesHelperProperty, value);
        }

        private static void OnColumnStylesHelperChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is ColumnStylesHelper oldHelper)
            {
                oldHelper.Detach();
            }

            if (e.NewValue is ColumnStylesHelper newHelper)
            {
                newHelper.Attach();
            }
        }

        #endregion

        private class ColumnStylesHelper
        {
            private readonly DataGrid _dataGrid;

            public ColumnStylesHelper(DataGrid dataGrid)
            {
                _dataGrid = dataGrid;
            }

            public void Attach()
            {
                _dataGrid.Columns.CollectionChanged += OnColumnsCollectionChanged;

                foreach (var column in _dataGrid.Columns)
                {
                    BindColumnStyleProperties(column);
                }
            }

            public void Detach()
            {
                _dataGrid.Columns.CollectionChanged -= OnColumnsCollectionChanged;

                foreach (var column in _dataGrid.Columns)
                {
                    ClearColumnStyleProperties(column);
                }
            }

            private void OnColumnsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
            {
                if (e.NewItems != null)
                {
                    foreach (var item in e.NewItems)
                    {
                        BindColumnStyleProperties(item as DataGridColumn);
                    }
                }
            }

            private void BindColumnStyleProperties(DataGridColumn column)
            {
                if (column is DataGridTextColumn textColumn)
                {
                    Bind(textColumn, DataGridTextColumn.ElementStyleProperty, _dataGrid, TextColumnElementStyleProperty);
                    Bind(textColumn, DataGridTextColumn.EditingElementStyleProperty, _dataGrid, TextColumnEditingElementStyleProperty);
                    Bind(textColumn, DataGridTextColumn.FontSizeProperty, _dataGrid, TextColumnFontSizeProperty);
                }
                else if (column is DataGridCheckBoxColumn checkBoxColumn)
                {
                    Bind(checkBoxColumn, DataGridCheckBoxColumn.ElementStyleProperty, _dataGrid, CheckBoxColumnElementStyleProperty);
                    Bind(checkBoxColumn, DataGridCheckBoxColumn.EditingElementStyleProperty, _dataGrid, CheckBoxColumnEditingElementStyleProperty);
                }
                else if (column is DataGridComboBoxColumn comboBoxColumn)
                {
                    Bind(comboBoxColumn, DataGridComboBoxColumn.ElementStyleProperty, _dataGrid, ComboBoxColumnElementStyleProperty);
                    Bind(comboBoxColumn, DataGridComboBoxColumn.EditingElementStyleProperty, _dataGrid, ComboBoxColumnEditingElementStyleProperty);
                }
                else if (column is DataGridHyperlinkColumn hyperlinkColumn)
                {
                    Bind(hyperlinkColumn, DataGridHyperlinkColumn.ElementStyleProperty, _dataGrid, HyperlinkColumnElementStyleProperty);
                    Bind(hyperlinkColumn, DataGridHyperlinkColumn.EditingElementStyleProperty, _dataGrid, HyperlinkColumnEditingElementStyleProperty);
                }
            }

            private void ClearColumnStyleProperties(DataGridColumn column)
            {
                if (column is DataGridTextColumn textColumn)
                {
                    Clear(textColumn, DataGridTextColumn.ElementStyleProperty, _dataGrid, TextColumnElementStyleProperty);
                    Clear(textColumn, DataGridTextColumn.EditingElementStyleProperty, _dataGrid, TextColumnEditingElementStyleProperty);
                    Clear(textColumn, DataGridTextColumn.FontSizeProperty, _dataGrid, TextColumnFontSizeProperty);
                }
                else if (column is DataGridCheckBoxColumn checkBoxColumn)
                {
                    Clear(checkBoxColumn, DataGridCheckBoxColumn.ElementStyleProperty, _dataGrid, CheckBoxColumnElementStyleProperty);
                    Clear(checkBoxColumn, DataGridCheckBoxColumn.EditingElementStyleProperty, _dataGrid, CheckBoxColumnEditingElementStyleProperty);
                }
                else if (column is DataGridComboBoxColumn comboBoxColumn)
                {
                    Clear(comboBoxColumn, DataGridComboBoxColumn.ElementStyleProperty, _dataGrid, ComboBoxColumnElementStyleProperty);
                    Clear(comboBoxColumn, DataGridComboBoxColumn.EditingElementStyleProperty, _dataGrid, ComboBoxColumnEditingElementStyleProperty);
                }
                else if (column is DataGridHyperlinkColumn hyperlinkColumn)
                {
                    Clear(hyperlinkColumn, DataGridHyperlinkColumn.ElementStyleProperty, _dataGrid, HyperlinkColumnElementStyleProperty);
                    Clear(hyperlinkColumn, DataGridHyperlinkColumn.EditingElementStyleProperty, _dataGrid, HyperlinkColumnEditingElementStyleProperty);
                }
            }

            private static void Bind(
                DependencyObject target,
                DependencyProperty targetDP,
                DependencyObject source,
                DependencyProperty sourceDP)
            {
                if (target.ReadLocalValue(targetDP) == DependencyProperty.UnsetValue)
                {
                    BindingOperations.SetBinding(target, targetDP, new Binding { Path = new PropertyPath(sourceDP), Source = source });
                }
            }

            private static void Clear(
                DependencyObject target,
                DependencyProperty targetDP,
                DependencyObject source,
                DependencyProperty sourceDP)
            {
                var binding = BindingOperations.GetBinding(target, targetDP);
                if (binding != null && binding.Source == source)
                {
                    target.ClearValue(targetDP);
                }
            }
        }
    }
}
