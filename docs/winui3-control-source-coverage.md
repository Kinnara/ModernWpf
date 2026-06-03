# WinUI 3 Control Source Coverage

This audit covers every resource dictionary merged by
`ModernWpf.Controls\Themes\Generic.xaml`. It is the WinUI-derived control
counterpart to `docs\official-fluent-style-coverage.md`, which covers stock WPF
controls governed by official `PresentationFramework.Fluent` styles.

Rows in this file must remain one-to-one with the `ModernWpf.Controls` generic
resource inventory. Existing controls may be grouped by source audit evidence,
but each shipped resource dictionary still gets its own row so a new or renamed
control resource cannot bypass source-parity review.

## Status

- `WinUI 3 source-backed WPF port`: the resource belongs to an existing
  ModernWpf control mapped to local WinUI 3 source, with WPF substitutions
  documented in the linked audit.
- `WinUI 3 source-backed WPF family`: the resource is one entry in a larger
  source-backed control family documented by the linked audit.

## Generic Resource Inventory

| Generic resource | Status | Evidence |
| --- | --- | --- |
| `AnnotatedScrollBar/AnnotatedScrollBar.xaml` | WinUI 3 source-backed WPF port | `docs\annotatedscrollbar-winui3-source-audit.md` |
| `AutoSuggestBox/AutoSuggestBox.xaml` | WinUI 3 source-backed WPF port | `docs\autosuggestbox-winui3-source-audit.md` |
| `BreadcrumbBar/BreadcrumbBar.xaml` | WinUI 3 source-backed WPF port | `docs\breadcrumbbar-winui3-source-audit.md` |
| `ColorPicker/ColorPicker.xaml` | WinUI 3 source-backed WPF port | `docs\colorpicker-winui3-source-audit.md` |
| `ContentDialog/ContentDialog.xaml` | WinUI 3 source-backed WPF port | `docs\contentdialog-winui3-source-audit.md` |
| `CommandBar/AppBarButton.xaml` | WinUI 3 source-backed WPF family | `docs\appbarbutton-winui3-source-audit.md` |
| `CommandBar/AppBarToggleButton.xaml` | WinUI 3 source-backed WPF family | `docs\appbarbutton-winui3-source-audit.md` |
| `CommandBar/AppBarSeparator.xaml` | WinUI 3 source-backed WPF family | `docs\commandbar-winui3-source-audit.md` |
| `CommandBar/AppBarElementContainer.xaml` | WinUI 3 source-backed WPF family | `docs\commandbar-winui3-source-audit.md` |
| `CommandBar/CommandBar.xaml` | WinUI 3 source-backed WPF port | `docs\commandbar-winui3-source-audit.md` |
| `CommandBarFlyout/CommandBarFlyout.xaml` | WinUI 3 source-backed WPF port | `docs\commandbarflyout-winui3-source-audit.md` |
| `DropDownButton/DropDownButton.xaml` | WinUI 3 source-backed WPF port | `docs\dropdownbutton-winui3-source-audit.md` |
| `Flyout/FlyoutPresenter.xaml` | WinUI 3 source-backed WPF family | `docs\flyoutbase-winui3-source-audit.md` |
| `HyperlinkButton/HyperlinkButton.xaml` | WinUI 3 source-backed WPF port | `docs\hyperlinkbutton-winui3-source-audit.md` |
| `InfoBadge/InfoBadge.xaml` | WinUI 3 source-backed WPF port | `docs\infobadge-winui3-source-audit.md` |
| `InfoBar/InfoBar.xaml` | WinUI 3 source-backed WPF port | `docs\infobar-winui3-source-audit.md` |
| `ListView/ListView.xaml` | WinUI 3 source-backed WPF family | `docs\listview-winui3-source-audit.md` |
| `ListView/GridView.xaml` | WinUI 3 source-backed WPF family | `docs\listview-winui3-source-audit.md` |
| `MenuBar/MenuBar.xaml` | WinUI 3 source-backed WPF port | `docs\menubar-winui3-source-audit.md` |
| `MenuFlyout/MenuFlyout.xaml` | WinUI 3 source-backed WPF family | `docs\flyoutbase-winui3-source-audit.md` |
| `NavigationView/NavigationView.xaml` | WinUI 3 source-backed WPF port | `docs\navigationview-winui3-source-audit.md` |
| `NumberBox/NumberBox.xaml` | WinUI 3 source-backed WPF port | `docs\numberbox-winui3-source-audit.md` |
| `PagerControl/PagerControl.xaml` | WinUI 3 source-backed WPF port | `docs\pagercontrol-winui3-source-audit.md` |
| `PersonPicture/PersonPicture.xaml` | WinUI 3 source-backed WPF port | `docs\personpicture-winui3-source-audit.md` |
| `ProgressRing/ProgressRing.xaml` | WinUI 3 source-backed WPF port | `docs\progressring-winui3-source-audit.md` |
| `RatingControl/RatingControl.xaml` | WinUI 3 source-backed WPF port | `docs\ratingcontrol-winui3-source-audit.md` |
| `SelectorBar/SelectorBar.xaml` | WinUI 3 source-backed WPF port | `docs\selectorbar-winui3-source-audit.md` |
| `SplitButton/SplitButton.xaml` | WinUI 3 source-backed WPF family | `docs\splitbutton-winui3-source-audit.md` |
| `SplitView/SplitView.xaml` | WinUI 3 source-backed WPF port | `docs\splitview-winui3-source-audit.md` |
| `TeachingTip/TeachingTip.xaml` | WinUI 3 source-backed WPF port | `docs\teachingtip-winui3-source-audit.md` |
| `RadioButtons/RadioButtons.xaml` | WinUI 3 source-backed WPF port | `docs\radiobuttons-winui3-source-audit.md` |
| `RadioMenuItem/RadioMenuItem.xaml` | WinUI 3 source-backed WPF port | `docs\radiomenuflyoutitem-winui3-source-audit.md` |
| `ToggleSwitch/ToggleSwitch.xaml` | WinUI 3 source-backed WPF port | `docs\toggleswitch-winui3-source-audit.md` |
