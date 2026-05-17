# HyperlinkButton WinUI 3 Source Audit

Source snapshot: `D:\repos\microsoft-ui-xaml`, `reference/winui3-current` / `c70471c511a0168b61dcca13af9556465f26b673`.

WinUI source files:

- `src\dxaml\xcp\dxaml\lib\HyperLinkButton_Partial.cpp`
- `src\dxaml\xcp\dxaml\lib\HyperLinkButton_Partial.h`
- `src\dxaml\xcp\dxaml\lib\HyperlinkButtonAutomationPeer_Partial.cpp`
- `src\dxaml\xcp\dxaml\lib\HyperlinkButtonAutomationPeer_Partial.h`
- `src\dxaml\xcp\dxaml\lib\winrtgeneratedclasses\HyperlinkButton.g.cpp`
- `src\dxaml\xcp\dxaml\lib\winrtgeneratedclasses\HyperlinkButtonAutomationPeer.g.cpp`
- `src\dxaml\xcp\tools\XCPTypesAutoGen\XamlOM\Model\Microsoft.UI.Xaml.Controls.cs`
- `src\controls\dev\CommonStyles\HyperlinkButton_themeresources.xaml`
- `src\dxaml\test\native\external\controls\hyperlinkbutton\HyperlinkButtonIntegrationTests.cpp`

ModernWpf files:

- `ModernWpf.Controls\HyperlinkButton\HyperlinkButton.cs`
- `ModernWpf.Controls\HyperlinkButton\HyperlinkButton.xaml`
- `ModernWpf.Controls\HyperlinkButton\HyperlinkButtonAutomationPeer.cs`
- `ModernWpf\ThemeResources\Light.xaml`
- `ModernWpf\ThemeResources\Dark.xaml`
- `ModernWpf\ThemeResources\HighContrast.xaml`
- `test\ModernWpf.WinUI.Tests\HyperlinkButton\HyperlinkButtonApiTests.cs`
- `test\ModernWpf.WinUI.Tests\LayoutCompatibility\LayoutCompatibilityApiTests.cs`

## Ported Source Behavior

- The old WPF `Hyperlink` logical child and WPF-only `TargetName` property were removed. WinUI 3 XamlOM exposes only `NavigateUri` on `HyperlinkButton`.
- `NavigateUri` is now a HyperlinkButton dependency property instead of an added owner of WPF `Hyperlink.NavigateUriProperty`.
- Click handling now follows the source order: raise the invoke automation event when listeners exist, run the base `ButtonBase` click path, then launch `NavigateUri` if it is set.
- The automation peer remains source-shaped: it exposes `Invoke`, reports class name `Hyperlink`, reports control type `Hyperlink`, rejects disabled invoke, and invokes through the owner button path.
- The template now uses source `ButtonPadding` instead of the old local `HyperlinkButtonPadding`.
- Pointer-over, pressed, and disabled foreground/background/border values now live in source-shaped visual states via `VisualStateEx.Setters` instead of WPF `ControlTemplate.Triggers`.
- Light and Dark `HyperlinkButtonForegroundPointerOver` now map to source `AccentTextFillColorSecondaryBrush`.

## WPF Substitutions

- WinUI launches `NavigateUri` through `Launcher::TryInvokeLauncher`; ModernWpf uses `Process.Start(..., UseShellExecute=true)` as the WPF desktop substitute.
- WPF has no direct `AutomationProperties.AccessibilityView=Raw` equivalent on the template content presenter.
- WinUI's generated default text underline and backplate foreground override are native text-rendering behaviors. ModernWpf keeps the existing `ContentPresenterEx` text surface and documents this as a WPF text substitute rather than keeping the old WPF `Hyperlink` child.
- WinUI storyboards are represented by `VisualStateEx.Setters`, matching the repo-wide WPF substitute for WinUI `VisualState.Setters`.

## Verification

Focused tests cover source API surface/defaults, removal of WPF-only `TargetName`, source `ButtonPadding`, visual-state setter ownership and application, source pointer-over theme resource mapping, and automation peer invoke behavior without launching a URI.

- `dotnet test .\test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --filter "FullyQualifiedName~HyperlinkButton" --no-restore`
  - Passed 4/4.
- `dotnet build .\ModernWpf.Controls\ModernWpf.Controls.csproj --no-restore -m:1`
  - Passed with existing warnings.
- `rg -n "TargetNameProperty|public string TargetName|m_hyperlink|ControlTemplate.Triggers|HyperlinkButtonPadding" .\ModernWpf.Controls\HyperlinkButton .\ModernWpf\ThemeResources .\test\ModernWpf.WinUI.Tests\HyperlinkButton`
  - Only the intentional test assertion for removed `TargetNameProperty` remains.
- `git diff --check`
  - Passed.
