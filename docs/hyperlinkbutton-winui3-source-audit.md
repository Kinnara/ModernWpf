# HyperlinkButton current WinUI source audit

Audit date: 2026-07-19

## Authorities and bounded history

The product authority is `microsoft/microsoft-ui-xaml` `main` at commit
`de3e767333c2f0717a6a70cb22bd192ced5ad885`. This supersedes the prior audit
pin `3cae15f071f1ab8565f9a7592dbf27f04bafe651`. Every current object was read
from the pinned commit rather than inferred from the older upstream worktree:

| Current product object | Blob |
| --- | --- |
| `dxaml/xcp/dxaml/lib/HyperLinkButton_Partial.cpp` | `c0d59563ffb684a8f492715bb66c7bfa89a68313` |
| `HyperLinkButton_Partial.h` | `4760194f6724e7335963ad60fa69e356ccc9c9a6` |
| `HyperlinkButtonAutomationPeer_Partial.cpp` | `cc561812c862c252ab41c5ce5a4a47d11024f563` |
| `HyperlinkButtonAutomationPeer_Partial.h` | `a120c5fb943ae7623a56cc738cb00f0bb3b8cf2b` |
| `winrtgeneratedclasses/HyperlinkButton.g.cpp` | `08c4ff39ff0d1e3f185dc87a0a7d5388b47eaab4` |
| `dxaml/xcp/tools/XCPTypesAutoGen/XamlOM/Model/Microsoft.UI.Xaml.Controls.cs` | `ad1199a7ff9c253e38c4fb922accbe0afffbf432` |
| `controls/dev/CommonStyles/HyperlinkButton_themeresources.xaml` | `93b5efd391803a229e63e55c315e5675fef4362e` |
| `HyperlinkButton_themeresources_perf2026.xaml` | `3861ddde4574c2519b0e4f64d296db5d2dd2b5d5` |
| `dxaml/test/native/external/controls/hyperlinkbutton/HyperlinkButtonIntegrationTests.cpp` | `b26959664f83a1a088954b21ad8f834411077f33` |
| `HyperlinkButtonIntegrationTests.h` | `819170cc934226436706355221698507c04d8dba` |

Runtime, header, generated API, XamlOM, automation peer, classic theme, and
native integration blobs are byte-identical to previous mirror baseline
`c70471c511a0168b61dcca13af9556465f26b673`. Bounded later history adds and
packages an equivalent performance dictionary, then moves the repository:

- `49b4d5326b4deba8c036e63a7e676715a5de4f3a` creates the perf2026 template.
  Its only changes are **nine zero-duration** object assignments—Foreground,
  Background, and BorderBrush in PointerOver, Pressed, and Disabled—expressed
  as equivalent `VisualState.Setters`.
- mirror commits `5e04eeb82cdab8f66d5d98f066c8914cd6b00b51`
  and `51d82696da7f65c69e6479420a879a8600817401` remove and restore generated perf
  content without altering the control contract.
- `8463f45162149de0ec3ad7df752596893fe3e13e` only moves the mirrored tree from
  `src/` to the repository root.

ModernWpf already represents all nine assignments with `VisualStateEx.Setters`.
No current product behavior, API, accessibility, or classic visual change
justifies a new runtime/template patch.

## Product mapping

| Current WinUI contract | ModernWpf mapping |
| --- | --- |
| Public API exposes `NavigateUri` and no WPF-only `TargetName`. | A control-owned `NavigateUri` dependency property; the former WPF `Hyperlink` logical child and `TargetName` surface remain deleted. |
| Click raises Invoke automation, executes the ordinary button click path, then launches a non-null URI. | `OnClick` raises `InvokePatternOnInvoked`, calls `base.OnClick`, then uses desktop `Process.Start` for the URI. |
| The peer reports Hyperlink / `Hyperlink`, exposes Invoke, and rejects disabled invocation. | `HyperlinkButtonAutomationPeer` returns `AutomationControlType.Hyperlink`, class `Hyperlink`, implements `IInvokeProvider`, and throws `ElementNotEnabledException` when disabled. |
| The template uses `ButtonPadding` and current PointerOver/Pressed/Disabled resources. | `DefaultHyperlinkButtonStyle`, `ContentPresenterEx`, and `VisualStateEx.Setters` consume the current Light/Dark/High Contrast aliases. |
| Current control text renders in a 32-DIP hit target. | The retained template renders the Gallery label at exact `157x32` geometry in both apps. |

## Current Gallery mapping

The live authority is `microsoft/WinUI-Gallery` `main` at commit
`29f62479d5c046a0b854a5868e5a7cd484572d87`:

| Gallery object | Blob |
| --- | --- |
| `WinUIGallery/Samples/HyperlinkButton/HyperlinkButtonPage.xaml` | `89d2864e4545f894f6c80b0e2d41017112f348af` |
| `HyperlinkButtonPage.xaml.cs` | `cab77b96249fd2fd7c05958efaa8abf234fd5de9` |
| `HyperlinkButtonNavigate.txt` | `a28ff06e13d6fc028692829546039be7c71efd7c` |
| `HyperlinkButtonClick.txt` | `5449d629c81cffb74e7b922e74cacf95577f365a` |

Conversion commit `14a4a1a2b8ddc527dc4a7d5f7e743d7c2bc97db7`
moves the sample to its current folder without changing either example.

ModernWpf loads both marker-delimited source files and displays only their
`xaml` sections. The first example preserves `Control1`, `Microsoft home page`,
the Microsoft URI, and `Disable hyperlink button`; checking the option disables
the live control and unchecking restores it. The second preserves `Control2`,
live text `Go to ToggleButton`, displayed snippet content `ToggleButton`, and
real in-app navigation to the ToggleButton page.

Gallery regressions pin both headers/snippets, live content/URI and names,
disable/enable behavior, Hyperlink roles/class/names, Invoke providers,
disabled Invoke rejection, CheckBox/Toggle semantics, and Invoke-driven route
navigation.

## WPF substitutions

- WinUI launches through `Launcher::TryInvokeLauncher`; WPF desktop uses
  `Process.Start(..., UseShellExecute=true)`.
- WPF has no direct template-level equivalent for WinUI's Raw accessibility
  annotation; the peer contract remains Hyperlink/Invoke.
- Native underline/backplate text is represented by `ContentPresenterEx`
  rather than a WPF `Hyperlink` child.
- WinUI setters are represented by repo-standard `VisualStateEx.Setters`.
- Gallery compiled `x:Bind` for `IsEnabled` is represented by equivalent WPF
  CheckBox checked/unchecked callbacks.

## Pixel and verification evidence

The committed installed-Gallery harness requires official `Control1`, a
primary crop, mean delta at most `1.6`, and zero size difference. Current
failure-on-difference evidence passes exact `157x32` geometry:

- Light `artifacts/visual-checks/20260718-080523-554-71612/report.md` at `1.48`;
- Dark `artifacts/visual-checks/20260718-080545-679-80452/report.md` at `1.47`.

Focused product API/automation/source coverage passes 5/5 on net8. Focused
Gallery sample/source/gate coverage passes 3/3 on net8 and net10. The current
Gallery build evidence covers `net462`, `net8.0-windows7.0`, and
`net10.0-windows7.0` with zero errors. The historical milestone record is row
8.33; the current-product follow-up is row 8.72.
