namespace System.Windows.Controls
{
    public class GridEx : PanelEx<Grid>
    {
        public ColumnDefinitionCollection ColumnDefinitions => ItemsHost.ColumnDefinitions;
        public RowDefinitionCollection RowDefinitions => ItemsHost.RowDefinitions;
    }
}
