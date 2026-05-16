# WinUI 3 Source Parity

ModernWpf parity work now targets WinUI 3 source behavior for existing controls only.

Source of truth: `D:\repos\microsoft-ui-xaml`

Local source snapshot used for the first round:

```text
c70471c511a0168b61dcca13af9556465f26b673
reference/winui3-current
```

The checkout was 12 commits behind `origin/winui3/main` when this note was created; refresh the snapshot before making claims about the latest WinUI 3 behavior.

## Rules

- Prefer WinUI 3 implementation behavior over old ModernWpf behavior.
- Do not add new controls in this phase.
- Do not preserve old ModernWpf behavior when it conflicts with source-backed WinUI 3 behavior.
- Port source behavior directly where WPF has an equivalent model.
- Where WinUI depends on compositor, popup island, WinRT automation, gamepad, or platform-only services, document the WPF substitution and add tests around the substitute behavior.
- Keep WinUI 2.8.7 notes only as historical/resource-reference material unless a slice explicitly needs that baseline.

## Active Source Parity Matrix

| Area | WinUI 3 source files | ModernWpf files | Status | Evidence |
| --- | --- | --- | --- | --- |
| ToggleSwitch | `src\dxaml\xcp\dxaml\lib\ToggleSwitch_Partial.cpp` | `ModernWpf.Controls\ToggleSwitch\ToggleSwitch.cs`, `test\ModernWpf.WinUI.Tests\ToggleSwitch\ToggleSwitchApiTests.cs` | Source-backed WPF port | Drag semantics now match WinUI threshold behavior: deltas accumulate, short drags do not toggle, and transient drag translation returns to visual-state ownership. WPF keeps a VSM/storyboard handoff substitute because WPF animations can hold `TranslateTransform.X`. |
| CommandBarFlyout | `src\controls\dev\CommandBarFlyout\CommandBarFlyout.cpp`, `src\controls\dev\CommandBarFlyout\CommandBarFlyoutCommandBar.cpp`, `src\controls\dev\CommandBarFlyout\CommandBarFlyout_themeresources.xaml` | `ModernWpf.Controls\CommandBarFlyout\CommandBarFlyout.cs`, `ModernWpf.Controls\CommandBarFlyout\CommandBarFlyoutCommandBar.cs`, `ModernWpf.Controls\CommandBarFlyout\CommandBarFlyout.xaml`, `test\ModernWpf.WinUI.Tests\CommandBarFlyout\CommandBarFlyoutApiTests.cs` | Source-backed WPF port in progress | Deleted the old `CommandBarFlyoutToolBar` WPF `ToolBar` host and moved flyout command behavior onto `CommandBarFlyoutCommandBar`, matching WinUI's implementation shape. Constructor defaults, `AlwaysExpanded` show mode, secondary command property-change sizing refresh, template setting calculation, size-change visual-state refresh, command collection mirroring, and available/combined visual states now have focused tests. WPF keeps a narrow deferred sizing refresh because `TemplateBinding` text measurement lags the dependency-property callback; remaining platform-only gaps include compositor/system backdrop, WinRT automation `MenuOpened/MenuClosed`, and exact WinUI popup `ActualPlacement` behavior. |
