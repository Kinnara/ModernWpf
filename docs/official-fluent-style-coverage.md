# Official WPF Fluent Style Coverage

This file is the controlled alternative to copying the whole official WPF
Fluent `Styles` folder blindly. It tracks every source style file under:

`D:\repos\wpf\src\Microsoft.DotNet.Wpf\src\Themes\PresentationFramework.Fluent\Styles`

ModernWpf should use official WPF Fluent as the primary source for stock WPF
controls. Whole-file copying is allowed when the source shape fits the existing
ModernWpf resource surface. Otherwise the row must document whether the source
file is folded into an existing ModernWpf dictionary, substituted by a
ModernWpf-owned shell, or excluded because ModernWpf has no existing control or
style in scope.

## Status

- `Backported`: the official stock WPF style is represented by a ModernWpf
  style dictionary, usually with small compatibility substitutions such as
  `mscorlib` for older targets or `ControlHelper.CornerRadius`.
- `Folded`: the official source file is intentionally merged into another
  ModernWpf style dictionary to preserve the existing resource entry point.
- `Substituted`: the official source file cannot be copied wholesale because
  ModernWpf owns a compatible shell or platform bridge, but the compatible
  surface is source-mapped and tested.
- `Excluded`: ModernWpf has no existing matching control/style to port in this
  goal, so the source file is not copied.

## Source Inventory

| Official style | ModernWpf artifact | Status | Evidence |
| --- | --- | --- | --- |
| `Button.xaml` | `ModernWpf\Styles\Button.xaml` | Backported | `docs\button-wpf-fluent-source-audit.md` |
| `Calendar.xaml` | `ModernWpf\Styles\Calendar.xaml` | Backported | `docs\calendar-datepicker-wpf-fluent-source-audit.md` |
| `CheckBox.xaml` | `ModernWpf\Styles\CheckBox.xaml` | Backported | `docs\checkbox-wpf-fluent-source-audit.md` |
| `CollectionViewGroup.xaml` | `ModernWpf\Styles\GroupItem.xaml` | Folded | `docs\groupitem-wpf-fluent-source-audit.md` |
| `ComboBox.xaml` | `ModernWpf\Styles\ComboBox.xaml` | Backported | `docs\combobox-wpf-fluent-source-audit.md` |
| `ContentControl.xaml` | `ModernWpf\Styles\ContentControl.xaml` | Backported | `docs\foundation-navigation-wpf-fluent-source-audit.md` |
| `ContextMenu.xaml` | `ModernWpf\Styles\ContextMenu.xaml` | Backported | `docs\menu-family-wpf-fluent-source-audit.md` |
| `DataGrid.xaml` | `ModernWpf\Styles\DataGrid.xaml` | Backported | `docs\datagrid-wpf-fluent-source-audit.md` |
| `DatePicker.xaml` | `ModernWpf\Styles\DatePicker.xaml` | Backported | `docs\calendar-datepicker-wpf-fluent-source-audit.md` |
| `DocumentViewer.xaml` | None | Excluded | ModernWpf has no existing DocumentViewer style/control in this goal. |
| `Expander.xaml` | `ModernWpf\Styles\Expander.xaml` | Backported | `docs\expander-wpf-fluent-source-audit.md` |
| `Frame.xaml` | `ModernWpf\Styles\Frame.xaml` | Backported | `docs\foundation-navigation-wpf-fluent-source-audit.md` |
| `GridSplitter.xaml` | `ModernWpf\Styles\GridSplitter.xaml` | Backported | `docs\gridsplitter-wpf-fluent-source-audit.md` |
| `GridView.xaml` | `ModernWpf\Styles\GridView.xaml` | Backported | `docs\listbox-listview-wpf-fluent-source-audit.md` |
| `GroupBox.xaml` | `ModernWpf\Styles\GroupBox.xaml` | Backported | `docs\groupbox-wpf-fluent-source-audit.md` |
| `GroupItem.xaml` | `ModernWpf\Styles\GroupItem.xaml` | Backported | `docs\groupitem-wpf-fluent-source-audit.md` |
| `HeaderedContentControl.xaml` | `ModernWpf\Styles\HeaderedContentControl.xaml` | Backported | `docs\foundation-navigation-wpf-fluent-source-audit.md` |
| `Hyperlink.xaml` | `ModernWpf\Styles\Hyperlink.xaml` | Backported | `docs\hyperlink-wpf-fluent-source-audit.md` |
| `ItemsControl.xaml` | `ModernWpf\Styles\ItemsControl.xaml` | Backported | `docs\foundation-navigation-wpf-fluent-source-audit.md` |
| `Label.xaml` | `ModernWpf\Styles\Label.xaml` | Backported | `docs\label-wpf-fluent-source-audit.md` |
| `ListBox.xaml` | `ModernWpf\Styles\ListBox.xaml` | Backported | `docs\listbox-listview-wpf-fluent-source-audit.md` |
| `ListBoxItem.xaml` | `ModernWpf\Styles\ListBoxItem.xaml` | Backported | `docs\listbox-listview-wpf-fluent-source-audit.md` |
| `ListView.xaml` | `ModernWpf\Styles\ListView.xaml` | Backported | `docs\listbox-listview-wpf-fluent-source-audit.md` |
| `ListViewItem.xaml` | `ModernWpf\Styles\ListViewItem.xaml` | Backported | `docs\listbox-listview-wpf-fluent-source-audit.md` |
| `Menu.xaml` | `ModernWpf\Styles\Menu.xaml` | Backported | `docs\menu-family-wpf-fluent-source-audit.md` |
| `MenuItem.xaml` | `ModernWpf\Styles\MenuItem.xaml` | Backported | `docs\menu-family-wpf-fluent-source-audit.md` |
| `NavigationWindow.xaml` | `ModernWpf\Styles\NavigationWindow.xaml` | Backported | `docs\foundation-navigation-wpf-fluent-source-audit.md` |
| `Page.xaml` | `ModernWpf\Styles\Page.xaml` | Backported | `docs\foundation-navigation-wpf-fluent-source-audit.md` |
| `PasswordBox.xaml` | `ModernWpf\Styles\PasswordBox.xaml` | Backported | `docs\textbox-passwordbox-wpf-fluent-source-audit.md` |
| `ProgressBar.xaml` | `ModernWpf\Styles\ProgressBar.xaml` | Backported | `docs\stock-progressbar-wpf-fluent-source-audit.md` |
| `RadioButton.xaml` | `ModernWpf\Styles\RadioButton.xaml` | Backported | `docs\radiobutton-wpf-fluent-source-audit.md` |
| `RepeatButton.xaml` | `ModernWpf\Styles\RepeatButton.xaml` | Backported | `docs\repeatbutton-wpf-fluent-source-audit.md` |
| `ResizeGrip.xaml` | `ModernWpf\Styles\ResizeGrip.xaml` | Backported | `docs\resizegrip-wpf-fluent-source-audit.md` |
| `RichTextBox.xaml` | `ModernWpf\Styles\RichTextBox.xaml` | Backported | `docs\richtextbox-wpf-fluent-source-audit.md` |
| `ScrollBar.xaml` | `ModernWpf\Styles\ScrollBar.xaml` | Backported | `docs\scrollbar-wpf-fluent-source-audit.md` |
| `ScrollViewer.xaml` | `ModernWpf\Styles\ScrollViewer.xaml` | Backported | `docs\scrollviewer-wpf-fluent-source-audit.md` |
| `Separator.xaml` | `ModernWpf\Styles\Separator.xaml` | Backported | `docs\toolbar-family-wpf-fluent-source-audit.md` |
| `Slider.xaml` | `ModernWpf\Styles\Slider.xaml` | Backported | `docs\slider-wpf-fluent-source-audit.md` |
| `StatusBar.xaml` | `ModernWpf\Styles\StatusBar.xaml` | Backported | `docs\statusbar-wpf-fluent-source-audit.md` |
| `StatusBarItem.xaml` | `ModernWpf\Styles\StatusBar.xaml` | Folded | `docs\statusbar-wpf-fluent-source-audit.md` |
| `TabControl.xaml` | `ModernWpf\Styles\TabControl.xaml` | Backported | `docs\tabcontrol-wpf-fluent-source-audit.md` |
| `TextBlock.xaml` | `ModernWpf\Styles\TextStyles.xaml` | Folded | `docs\foundation-navigation-wpf-fluent-source-audit.md` |
| `TextBox.xaml` | `ModernWpf\Styles\TextBox.xaml` | Backported | `docs\textbox-passwordbox-wpf-fluent-source-audit.md` |
| `Thumb.xaml` | `ModernWpf\Styles\Thumb.xaml` | Backported | `docs\toolbar-family-wpf-fluent-source-audit.md` |
| `ToggleButton.xaml` | `ModernWpf\Styles\ToggleButton.xaml` | Backported | `docs\togglebutton-wpf-fluent-source-audit.md` |
| `ToolBar.xaml` | `ModernWpf\Styles\ToolBar.xaml` | Backported | `docs\toolbar-family-wpf-fluent-source-audit.md` |
| `ToolTip.xaml` | `ModernWpf\Styles\ToolTip.xaml` | Backported | `docs\tooltip-wpf-fluent-source-audit.md` |
| `TreeView.xaml` | `ModernWpf\Styles\TreeView.xaml` | Backported | `docs\treeview-wpf-fluent-source-audit.md` |
| `TreeViewItem.xaml` | `ModernWpf\Styles\TreeViewItem.xaml` | Backported | `docs\treeview-wpf-fluent-source-audit.md` |
| `UserControl.xaml` | `ModernWpf\Styles\UserControl.xaml` | Backported | `docs\foundation-navigation-wpf-fluent-source-audit.md` |
| `Window.xaml` | `ModernWpf\Styles\Window.xaml` | Substituted | `docs\window-wpf-fluent-source-audit.md` |
