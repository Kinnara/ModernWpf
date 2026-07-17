# PersonPicture WinUI 3 Source Audit

Source snapshot: `D:\repos\microsoft-ui-xaml`, `reference/winui3-current` / `c70471c511a0168b61dcca13af9556465f26b673`.

Current validation: 2026-07-17.

WinUI source files:

- `src\controls\dev\PersonPicture\PersonPicture.cpp`
- `src\controls\dev\PersonPicture\PersonPicture.xaml`
- `src\controls\dev\PersonPicture\PersonPicture.idl`
- `src\controls\dev\PersonPicture\PersonPictureAutomationPeer.cpp`
- `src\controls\dev\PersonPicture\PersonPictureTemplateSettings.h`
- `src\controls\dev\PersonPicture\InitialsGenerator.cpp`
- `src\controls\dev\PersonPicture\PersonPicture_themeresources.xaml`
- `src\controls\dev\PersonPicture\APITests\PersonPictureTests.cs`

ModernWpf files:

- `ModernWpf.Controls\PersonPicture\PersonPicture.cs`
- `ModernWpf.Controls\PersonPicture\PersonPicture.properties.cs`
- `ModernWpf.Controls\PersonPicture\PersonPicture.xaml`
- `ModernWpf.Controls\PersonPicture\PersonPictureAutomationPeer.cs`
- `ModernWpf.Controls\PersonPicture\PersonPictureTemplateSettings.cs`
- `ModernWpf.Controls\PersonPicture\InitialsGenerator.cs`
- `ModernWpf\ThemeResources\Light.xaml`
- `ModernWpf\ThemeResources\Dark.xaml`
- `ModernWpf\ThemeResources\HighContrast.xaml`
- `test\ModernWpf.WinUI.Tests\PersonPicture\PersonPictureApiTests.cs`

## Ported Source Behavior

- The old WPF-only placeholder path was removed from the template. There is no separate `PlaceholderIcon`, no template-local contact/people `StreamGeometry`, and no guessed translate offsets for the initials or badge text.
- The template now uses WinUI's single `InitialsTextBlock` placeholder model: `NoPhotoOrInitials` switches the text block to `SymbolThemeFontFamily` and `E77B`; `Group` switches it to `SymbolThemeFontFamily` and `E716`.
- The default style now exposes the WinUI chrome surface through `Foreground`, `Background`, `BorderBrush`, and `BorderThickness`, and the main ellipse binds to those templated-parent values.
- `PreferSmallImage` was added with the WinUI source default and property-change no-op behavior. It remains inert without the WinRT `Contact` surface.
- Existing source-shaped behavior remains covered: image/initials priority, `TemplateSettings.ActualInitials`, reusable `ActualImageBrush`, badge image/number/glyph precedence, pluralized automation names, badge text overrides, `InitialsGenerator`, and `PersonPictureAutomationPeer` reporting `Text` / `PersonPicture`.
- Theme resources match the WinUI 3 resource keys for light, dark, and high contrast.

## WPF Substitutions

- WinUI `Contact` and async `IRandomAccessStreamReference` profile-picture loading are still not exposed as WPF API. The internal contact fields remain unreachable, and `PreferSmallImage` is documented as inert until a WPF/WinRT contact bridge is intentionally added.
- WPF has no direct `AutomationProperties.AccessibilityView=Raw` equivalent in this template.
- WPF `TextBlock` does not expose WinUI `TextLineBounds=Tight` or `IsTextScaleFactorEnabled=False`; ModernWpf keeps the visible text behavior and documents those as text-platform gaps.
- WPF has no `x:DeferLoadStrategy=Lazy`; template parts are realized normally.
- WinUI binds `BorderThickness` into an ellipse `StrokeThickness` path that WPF types differently. ModernWpf keeps the control `BorderThickness` surface and binds the ellipse stroke to `BorderThickness.Left`.
- The keyed `DefaultPersonPictureStyle` is declared before the implicit style because WPF `StaticResource` lookup is order-sensitive.

## Verification

- `dotnet test .\test\ModernWpf.WinUI.Tests\ModernWpf.WinUI.Tests.csproj --filter FullyQualifiedName~PersonPicture --no-restore`
  - Passed 9/9.
- `dotnet build .\ModernWpf.Controls\ModernWpf.Controls.csproj --no-restore -m:1`
  - Passed with existing repository warnings.
- Installed WinUI 3 Gallery exact-size comparisons:
  - Light: `artifacts/visual-checks/20260717-082356-846-16316/report.md`, exact `96x96` avatar crops, primary delta `0.39`.
  - Dark: `artifacts/visual-checks/20260717-082417-249-7520/report.md`, exact `96x96` avatar crops, primary delta `0.35`.
  - `Run-GalleryVisualChecks.ps1` now enforces a strict `0.5` primary-crop threshold. The sample-specific WinUI crop searches the first example body for the rendered avatar because the installed Gallery does not expose a stable automation ID on the reference `PersonPicture` itself.
