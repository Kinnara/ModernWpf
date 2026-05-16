# WinUI VisualState.Setters Audit

This tracks WinUI `VisualState.Setters` parity for ModernWpf 1.x. The primary
source is the local final WinUI 2.8.7 checkout:
`D:\repos\microsoft-ui-xaml-v2.8.7\dev`. The current WinUI 3 checkout at
`D:\repos\microsoft-ui-xaml\src\controls\dev` was also counted to identify
newer deltas, but the repo-wide sync matrix is still anchored on WinUI 2.8.7.

## Status Legend

- `Converted`: ModernWpf product XAML uses `VisualStateEx.Setters`.
- `Partial`: some setters are converted, but unsupported setter shapes remain
  represented by WPF storyboards/triggers/code.
- `Pending`: the WinUI setter blocks map to an existing ModernWpf template and
  still need conversion or an explicit compatibility decision.
- `Unsupported`: the setter target needs runtime support that does not exist yet.
- `Excluded`: the upstream file maps to a WinUI surface this repo does not carry.

## Current Conversions

| WinUI 2.8.7 source | Setter blocks | ModernWpf file | Status | Notes |
| --- | ---: | --- | --- | --- |
| `dev\ProgressBar\ProgressBar.xaml` | 4 | `ModernWpf\ProgressBar\ProgressBar.xaml` | Partial | Direct opacity setters for `Indeterminate`, `IndeterminateError`, and `IndeterminatePaused` are converted. The `UpdatingError` brush-color setter still uses a storyboard because nested target paths such as `(Shape.Fill).(SolidColorBrush.Color)` are not supported yet. |
| `dev\CommonStyles\AppBarSeparator_themeresources.xaml` | 2 | `ModernWpf.Controls\CommandBar\AppBarSeparator.xaml` | Converted | Compact and overflow layout states now use `VisualStateEx.Setters` instead of a `VisualStateGroupListener` trigger bridge. |
| `dev\CommonStyles\CalendarView_themeresources.xaml` | 2 | `ModernWpf\Styles\Calendar.xaml` | Converted | Calendar navigation button pointer-over/pressed border and foreground setters now use `VisualStateEx.Setters` instead of WPF template triggers. |
| `dev\CommonStyles\CommandBar_themeresources.xaml` | 3 | `ModernWpf.Controls\CommandBar\CommandBar.xaml` | Partial | The `DynamicOverflowEnabled` column-width setters now use `VisualStateEx.Setters`; the `CommandBarOverflowPresenter` full-width open-up/down states remain inactive in this WPF port and still need a compatibility decision. |
| `dev\InfoBadge\InfoBadge_themeresources.xaml` | 3 | `ModernWpf.Controls\InfoBadge\InfoBadge.xaml` | Converted | `Dot`, `Icon`, `FontIcon`, and `Value` state behavior now lives in `VisualStateEx.Setters`; code only chooses the state and creates the icon element. |
| `dev\PersonPicture\PersonPicture.xaml` | 5 | `ModernWpf.Controls\PersonPicture\PersonPicture.xaml` | Partial | Placeholder and badge visibility/opacity state effects now use `VisualStateEx.Setters`; the photo image-brush binding and badge image-brush object setter remain on WPF trigger/storyboard paths because binding-valued and named object-valued setters still need runtime work. |
| `dev\ProgressRing\ProgressRing.xaml` | 1 | `ModernWpf.Controls\ProgressRing\ProgressRing.xaml` | Partial | The inactive layout opacity setter is represented with `VisualStateEx.Setters`; WinUI's `LottiePlayer.(AutomationProperties.AccessibilityView)` setter has no WPF ellipse-animation equivalent. |
| `dev\RadioButtons\RadioButtons.xaml` | 1 | `ModernWpf.Controls\RadioButtons\RadioButtons.xaml` | Converted | Disabled header foreground now uses a dynamic-resource `VisualStateEx.Setters` entry instead of a WPF template trigger. |
| `dev\CommonStyles\ScrollBar_themeresources.xaml` | 8 | `ModernWpf\Styles\ScrollBar.xaml` | Partial | Existing WPF conscious expand/collapse states now use `VisualStateEx.Setters` for pointer-over chrome and panning thumb brushes. WinUI common/indicator-state setters are not represented by this WPF ScrollBar template yet. |
| `dev\CommonStyles\ToggleSwitch_themeresources.xaml` | 1 | `ModernWpf.Controls\ToggleSwitch\ToggleSwitch.xaml` | Converted | WinUI's pressed knob alignment setters are represented in ModernWpf's active `Dragging` state, because this WPF port routes thumb interaction through that state instead of `CommonStates.Pressed`. |
| `dev\TwoPaneView\TwoPaneView.xaml` | 5 | `ModernWpf.Controls\TwoPaneView\TwoPaneView.xaml` | Converted | Mode-state pane row/column and single-pane visibility setters now live in `VisualStateEx.Setters`; code still computes mode and pane lengths. |

## Relevant Pending WinUI 2.8.7 Sources

| WinUI 2.8.7 source | Setter blocks | ModernWpf mapping | Status | Main blocker or next action |
| --- | ---: | --- | --- | --- |
| `dev\AutoSuggestBox\AutoSuggestBox_themeresources.xaml` | 2 | `ModernWpf\Styles\AutoSuggestBox.xaml` | Unsupported | WinUI setters only target `AnimatedIcon.State`; ModernWpf uses static icon presenter fallback and has no compatible `AnimatedIcon.State` surface yet. |
| `dev\Breadcrumb\BreadcrumbBar.xaml` | 16 | `ModernWpf.Controls\BreadcrumbBar\BreadcrumbBar.xaml` | Pending | Audit generated item/ellipsis state setters. |
| `dev\ColorPicker\ColorPicker.xaml` | 20 | `ModernWpf.Controls\ColorPicker\ColorPicker.xaml` | Pending | Audit slider/preview visibility and layout setters; skip WinUI-only spectrum rendering. |
| `dev\ColorPicker\ColorSpectrum.xaml` | 5 | `ModernWpf.Controls\ColorPicker\ColorPicker.xaml` | Pending | Map only the WPF-owned spectrum template pieces. |
| `dev\ComboBox\ComboBox_themeresources.xaml` | 10 | `ModernWpf\Styles\ComboBox.xaml` | Pending | Audit popup/open/disabled setter groups. |
| `dev\CommandBarFlyout\CommandBarFlyout_themeresources.xaml` | 41 | `ModernWpf.Controls\CommandBarFlyout\CommandBarFlyout.xaml` | Pending | Large visual-state matrix; convert direct layout/visibility setters after dynamic-resource support is settled. |
| `dev\CommonStyles\AppBarButton_themeresources.xaml` | 16 | `ModernWpf.Controls\CommandBar\AppBarButton.xaml` | Pending | Audit direct state setters; animated icon state remains unsupported. |
| `dev\CommonStyles\AppBarToggleButton_themeresources.xaml` | 18 | `ModernWpf.Controls\CommandBar\AppBarToggleButton.xaml` | Pending | Audit checked/common state setters; animated icon state remains unsupported. |
| `dev\CommonStyles\Button_themeresources.xaml` | 3 | `ModernWpf\Styles\Button.xaml`, `ModernWpf.Controls\HyperlinkButton\HyperlinkButton.xaml` | Unsupported | WinUI setters only target `AnimatedIcon.State`; ModernWpf button templates use static presenter/icon fallback and have no compatible `AnimatedIcon.State` surface yet. |
| `dev\CommonStyles\CalendarDatePicker_themeresources.xaml` | 1 | `ModernWpf\Styles\DatePicker.xaml` | Pending | Stock WPF mapping; decide whether a VisualStateEx port is meaningful. |
| `dev\CommonStyles\CheckBox_themeresources.xaml` | 12 | `ModernWpf\Styles\CheckBox.xaml` | Pending | Audit check glyph and pointer/pressed state setters. |
| `dev\CommonStyles\CommandBar_themeresources.xaml` | 3 | `ModernWpf.Controls\CommandBar\CommandBar.xaml` | Partial | See current conversions; remaining setter blocks are for disabled full-width overflow presenter placement states. |
| `dev\CommonStyles\ContentDialog_themeresources.xaml` | 15 | `ModernWpf.Controls\ContentDialog\ContentDialog.xaml` | Pending | Convert simple layout setters; preserve WPF popup/dialog adaptations. |
| `dev\CommonStyles\DatePicker_themeresources.xaml` | 1 | `ModernWpf\Styles\DatePicker.xaml` | Pending | Stock WPF DatePicker mapping. |
| `dev\CommonStyles\MenuFlyout_themeresources.xaml` | 42 | `ModernWpf.Controls\MenuFlyout\MenuFlyout.xaml`, `ModernWpf\Styles\MenuItem.xaml`, `ModernWpf.Controls\RadioMenuItem\RadioMenuItem.xaml` | Pending | Large menu state matrix; direct setters should move to `VisualStateEx` where ModernWpf owns the template. |
| `dev\CommonStyles\Pivot_themeresources.xaml` | 5 | `ModernWpf\Styles\Pivot.xaml` | Pending | Audit selected/header state setters. |
| `dev\DropDownButton\DropDownButton.xaml` | 3 | `ModernWpf.Controls\DropDownButton\DropDownButton.xaml` | Unsupported | WinUI setters target `AnimatedIcon.State`; ModernWpf currently uses `FontIconFallback`, so this needs an `AnimatedIcon.State` compatibility surface or a documented fallback. |
| `dev\Expander\Expander.xaml`, `dev\Expander\Expander_themeresources.xaml` | 15 | `ModernWpf\Styles\Expander.xaml` | Pending | WPF stock Expander mapping; audit direct chevron/content visibility setters. |
| `dev\InfoBar\InfoBar.xaml` | 8 | `ModernWpf.Controls\InfoBar\InfoBar.xaml` | Pending | Convert severity/icon/layout setters after checking dynamic-resource usage. |
| `dev\NavigationView\NavigationBackButton.xaml` | 2 | `ModernWpf\Styles\NavigationBackButton.xaml` | Unsupported | WinUI setters target `AnimatedIcon.State`; ModernWpf uses `FontIconFallback`, so a real port needs an `AnimatedIcon.State` compatibility surface or an explicit static-icon fallback decision. |
| `dev\NavigationView\NavigationView.xaml` | 21 | `ModernWpf.Controls\NavigationView\NavigationView.xaml` | Pending | Large item/header/overflow state surface; convert in small slices. |
| `dev\NavigationView\NavigationView_rs1_themeresources.xaml` | 71 | `ModernWpf\Styles\NavigationView.xaml` | Pending | Resource-style visual states need separate audit against current WPF style mappings. |
| `dev\NumberBox\NumberBox.xaml` | 7 | `ModernWpf.Controls\NumberBox\NumberBox.xaml` | Pending | Audit spin-button/delete-button state setters. |
| `dev\PagerControl\PagerControl.xaml` | 16 | `ModernWpf.Controls\PagerControl\PagerControl.xaml` | Pending | Audit number-panel/button state setters. |
| `dev\PipsPager\PipsPager.xaml` | 7 | `ModernWpf.Controls\PipsPager\PipsPager.xaml` | Pending | Audit selected/normal/overflow pip state setters. |
| `dev\RadioMenuFlyoutItem\RadioMenuFlyoutItem_themeresources.xaml` | 13 | `ModernWpf.Controls\RadioMenuItem\RadioMenuItem.xaml` | Pending | Map check glyph/placeholder setters to WPF menu item template. |
| `dev\RatingControl\RatingControl.xaml` | 6 | `ModernWpf.Controls\RatingControl\RatingControl.xaml` | Pending | Audit caption/placeholder state setters. |
| `dev\SplitButton\SplitButton.xaml`, `dev\SplitButton\SplitButton_themeresources.xaml` | 34 | `ModernWpf.Controls\SplitButton\SplitButton.xaml` | Pending | Convert direct state setters; animated icon state remains unsupported where present. |
| `dev\SplitView\SplitView_themeresources.xaml` | 4 | `ModernWpf.Controls\SplitView\SplitView.xaml` | Pending | Likely direct pane/light-dismiss setters. |
| `dev\SwipeControl\SwipeControl_themeresources.xaml` | 1 | `ModernWpf.Controls\SwipeControl\SwipeControl.xaml` | Pending | Audit WPF swipe action template mapping. |
| `dev\TabView\TabView.xaml` | 18 | `ModernWpf\Styles\TabControl.xaml` | Pending | WPF TabControl mapping; decide which visual states are meaningful. |
| `dev\TeachingTip\TeachingTip.xaml` | 29 | `ModernWpf.Controls\TeachingTip\TeachingTip.xaml` | Pending | Large template state surface; convert direct placement/layout setters in slices. |
| `dev\TitleBar\TitleBar.xaml`, `dev\TitleBar\TitleBar_themeresources.xaml` | 8 | `ModernWpf\TitleBar\TitleBarControl.xaml`, `ModernWpf\TitleBar\TitleBarButton.xaml` | Pending | WinUI title bar has newer experimental surface; map only WPF-owned chrome pieces. |
| `dev\TreeView\TreeViewItem.xaml` | 10 | `ModernWpf\Styles\TreeView.xaml` | Pending | WPF stock TreeView mapping; audit chevron/content/check state setters. |

## Excluded Or Non-Product Sources

| WinUI 2.8.7 source | Setter blocks | Reason |
| --- | ---: | --- |
| `dev\AnimatedIcon\TestUI\AnimatedIconPage.xaml` | 12 | TestUI sample only; `AnimatedIcon` itself is excluded from ModernWpf core. |
| `dev\CommonStyles\InkToolbar_themeresources.xaml` | 112 | InkToolbar is not carried as a ModernWpf control. |
| `dev\CommonStyles\MediaTransportControls_themeresources.xaml` | 11 | Media transport controls are not carried as ModernWpf controls. |
| `dev\CommonStyles\TimePicker_themeresources.xaml` | 1 | TimePicker is not carried as a ModernWpf control. |
| `dev\Materials\Reveal\*.xaml` and `dev\Materials\Reveal\TestUI\*.xaml` | 331 | WinUI compositor/reveal material system is excluded from the WPF port. |
| `dev\MenuBar\MenuBarItem.xaml` | 3 | ModernWpf maps menu behavior through WPF Menu/MenuItem/MenuFlyout surfaces, not a WinUI MenuBar control. |

## Runtime Gaps Blocking Full Conversion

- Nested target paths such as `Rect.(Shape.Fill).(SolidColorBrush.Color)`.
- Dynamic resource setters now work on `FrameworkElement` and
  `FrameworkContentElement` targets, but non-framework targets are unsupported.
- WinUI attached properties that do not exist in ModernWpf, especially
  `AnimatedIcon.State`.
- State setter timing versus transitions: controls with transition storyboards
  must be converted carefully so immediate setter application does not preempt
  transition animations.

## Validation Hooks

- `TemplateParityTests.ProductTemplatesDoNotContainRawWinUIVisualStateSetters`
  guards against raw `<VisualState.Setters>` syntax in WPF XAML.
- `TemplateParityTests.ProductTemplatesUseVisualStateExForConvertedStateSetters`
  guards converted product templates so they keep using `VisualStateEx.Setters`.
