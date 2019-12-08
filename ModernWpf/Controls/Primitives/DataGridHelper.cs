using System.Collections.Specialized;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace ModernWpf.Controls.Primitives
{
    public static class DataGridHelper
    {
        #region IsEnabled

        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached(
                "IsEnabled",
                typeof(bool),
                typeof(DataGridHelper),
                new PropertyMetadata(false, OnIsEnabledChanged));

        public static bool GetIsEnabled(DataGrid dataGrid)
        {
            return (bool)dataGrid.GetValue(IsEnabledProperty);
        }

        public static void SetIsEnabled(DataGrid dataGrid, bool value)
        {
            dataGrid.SetValue(IsEnabledProperty, value);
        }

        private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var dataGrid = (DataGrid)d;
            if ((bool)e.NewValue)
            {
                dataGrid.LoadingRow += OnLoadingRow;
            }
            else
            {
                dataGrid.LoadingRow -= OnLoadingRow;
            }
        }

        #endregion

        private static void OnLoadingRow(object sender, DataGridRowEventArgs e)
        {
            Debug.Assert(sender is DataGrid);
            var row = e.Row;

            if (row.ReadLocalValue(DataGridRowHelper.AreRowDetailsFrozenInternalProperty) == DependencyProperty.UnsetValue)
            {
                row.SetBinding(DataGridRowHelper.AreRowDetailsFrozenInternalProperty,
                    new Binding { Path = new PropertyPath(DataGrid.AreRowDetailsFrozenProperty), Source = sender });
            }

            if (row.ReadLocalValue(DataGridRowHelper.HeadersVisibilityInternalProperty) == DependencyProperty.UnsetValue)
            {
                row.SetBinding(DataGridRowHelper.HeadersVisibilityInternalProperty,
                    new Binding { Path = new PropertyPath(DataGrid.HeadersVisibilityProperty), Source = sender });
            }
        }

        #region Column Styles

        #region TextColumnElementStyle

        public static readonly DependencyProperty TextColumnElementStyleProperty =
            DependencyProperty.RegisterAttached(
                "TextColumnElementStyle",
                typeof(Style),
                typeof(DataGridHelper));

        public static Style GetTextColumnElementStyle(DataGrid dataGrid)
        {
            return (Style)dataGrid.GetValue(TextColumnElementStyleProperty);
        }

        public static void SetTextColumnElementStyle(DataGrid dataGrid, Style value)
        {
            dataGrid.SetValue(TextColumnElementStyleProperty, value);
        }

        #endregion

        #region TextColumnEditingElementStyle

        public static readonly DependencyProperty TextColumnEditingElementStyleProperty =
            DependencyProperty.RegisterAttached(
                "TextColumnEditingElementStyle",
                typeof(Style),
                typeof(DataGridHelper));

        public static Style GetTextColumnEditingElementStyle(DataGrid dataGrid)
        {
            return (Style)dataGrid.GetValue(TextColumnEditingElementStyleProperty);
        }

        public static void SetTextColumnEditingElementStyle(DataGrid dataGrid, Style value)
        {
            dataGrid.SetValue(TextColumnEditingElementStyleProperty, value);
        }

        #endregion

        #region TextColumnFontSize

        public static readonly DependencyProperty TextColumnFontSizeProperty =
            DependencyProperty.RegisterAttached(
                "TextColumnFontSize",
                typeof(double),
                typeof(DataGridHelper),
                new PropertyMetadata(SystemFonts.MessageFontSize));

        public static double GetTextColumnFontSize(DataGrid dataGrid)
        {
            return (double)dataGrid.GetValue(TextColumnFontSizeProperty);
        }

        public static void SetTextColumnFontSize(DataGrid dataGrid, double value)
        {
            dataGrid.SetValue(TextColumnFontSizeProperty, value);
        }

        #endregion

        #region CheckBoxColumnElementStyle

        public static readonly DependencyProperty CheckBoxColumnElementStyleProperty =
            DependencyProperty.RegisterAttached(
                "CheckBoxColumnElementStyle",
                typeof(Style),
                typeof(DataGridHelper));

        public static Style GetCheckBoxColumnElementStyle(DataGrid dataGrid)
        {
            return (Style)dataGrid.GetValue(CheckBoxColumnElementStyleProperty);
        }

        public static void SetCheckBoxColumnElementStyle(DataGrid dataGrid, Style value)
        {
            dataGrid.SetValue(CheckBoxColumnElementStyleProperty, value);
        }

        #endregion

        #region CheckBoxColumnEditingElementStyle

        public static readonly DependencyProperty CheckBoxColumnEditingElementStyleProperty =
            DependencyProperty.RegisterAttached(
                "CheckBoxColumnEditingElementStyle",
                typeof(Style),
                typeof(DataGridHelper));

        public static Style GetCheckBoxColumnEditingElementStyle(DataGrid dataGrid)
        {
            return (Style)dataGrid.GetValue(CheckBoxColumnEditingElementStyleProperty);
        }

        public static void SetCheckBoxColumnEditingElementStyle(DataGrid dataGrid, Style value)
        {
            dataGrid.SetValue(CheckBoxColumnEditingElementStyleProperty, value);
        }

        #endregion

        #region ComboBoxColumnElementStyle

        public static readonly DependencyProperty ComboBoxColumnElementStyleProperty =
            DependencyProperty.RegisterAttached(
                "ComboBoxColumnElementStyle",
                typeof(Style),
                typeof(DataGridHelper));

        public static Style GetComboBoxColumnElementStyle(DataGrid dataGrid)
        {
            return (Style)dataGrid.GetValue(ComboBoxColumnElementStyleProperty);
        }

        public static void SetComboBoxColumnElementStyle(DataGrid dataGrid, Style value)
        {
            dataGrid.SetValue(ComboBoxColumnElementStyleProperty, value);
        }

        #endregion

        #region ComboBoxColumnEditingElementStyle

        public static readonly DependencyProperty ComboBoxColumnEditingElementStyleProperty =
            DependencyProperty.RegisterAttached(
                "ComboBoxColumnEditingElementStyle",
                typeof(Style),
                typeof(DataGridHelper));

        public static Style GetComboBoxColumnEditingElementStyle(DataGrid dataGrid)
        {
            return (Style)dataGrid.GetValue(ComboBoxColumnEditingElementStyleProperty);
        }

        public static void SetComboBoxColumnEditingElementStyle(DataGrid dataGrid, Style value)
        {
            dataGrid.SetValue(ComboBoxColumnEditingElementStyleProperty, value);
        }

        #endregion

        #region HyperlinkColumnElementStyle

        public static readonly DependencyProperty HyperlinkColumnElementStyleProperty =
            DependencyProperty.RegisterAttached(
                "HyperlinkColumnElementStyle",
                typeof(Style),
                typeof(DataGridHelper));

        public static Style GetHyperlinkColumnElementStyle(DataGrid dataGrid)
        {
            return (Style)dataGrid.GetValue(HyperlinkColumnElementStyleProperty);
        }

        public static void SetHyperlinkColumnElementStyle(DataGrid dataGrid, Style value)
        {
            dataGrid.SetValue(HyperlinkColumnElementStyleProperty, value);
        }

        #endregion

        #region HyperlinkColumnEditingElementStyle

        public static readonly DependencyProperty HyperlinkColumnEditingElementStyleProperty =
            DependencyProperty.RegisterAttached(
                "HyperlinkColumnEditingElementStyle",
                typeof(Style),
                typeof(DataGridHelper));

        public static Style GetHyperlinkColumnEditingElementStyle(DataGrid dataGrid)
        {
            return (Style)dataGrid.GetValue(HyperlinkColumnEditingElementStyleProperty);
        }

        public static void SetHyperlinkColumnEditingElementStyle(DataGrid dataGrid, Style value)
        {
            dataGrid.SetValue(HyperlinkColumnEditingElementStyleProperty, value);
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
                dataGrid.SetValue(ColumnStylesHelperProperty, new ColumnStylesHelper(dataGrid));
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
                new PropertyMetadata(OnColumnStylesHelperChanged));

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

        #endregion
    }
}
