# PersonPicture WinUI 3 Source Audit

Current product snapshot: `D:\repos\microsoft-ui-xaml`, official
`microsoft/microsoft-ui-xaml` `winui3/main` commit
`de3e767333c2f0717a6a70cb22bd192ced5ad885`.

Current Gallery snapshot: official `microsoft/WinUI-Gallery` commit
`29f62479d5c046a0b854a5868e5a7cd484572d87`.

Parity refresh: 2026-07-18.

## Current Product Source Pins

| Current source | Git blob |
| --- | --- |
| `controls\dev\PersonPicture\PersonPicture.cpp` | `fa62c1631427dec8bd0c3e92b9bb4e9fac9fd067` |
| `controls\dev\PersonPicture\PersonPicture.h` | `0cfd9681cf281c930fddda4cf772bb97ac629d6a` |
| `controls\dev\PersonPicture\PersonPicture.idl` | `2a8c30e3824074adf4a7ca8238f307de8590283c` |
| `controls\dev\PersonPicture\PersonPicture.vcxitems` | `26f6466c3d2d54549c599f8e2278835d913111ba` |
| `controls\dev\PersonPicture\InitialsGenerator.cpp` | `7747fd1000a0db817a7ceba76c70b1b61199eb6f` |
| `controls\dev\PersonPicture\InitialsGenerator.h` | `b06f4d3b551bdcac851d840b5adc1ab0af052d40` |
| `controls\dev\PersonPicture\PersonPictureAutomationPeer.cpp` | `60e636281541e844a97827a563e5ed46b2cfc716` |
| `controls\dev\PersonPicture\PersonPictureAutomationPeer.h` | `f4f4f40d09efe35c352f981c0db90331215288c7` |
| `controls\dev\PersonPicture\PersonPictureAutomationPeer.idl` | `6ac3c164ca53ffb5b3eb24bb9eba09c3b6fc4692` |
| `controls\dev\PersonPicture\PersonPictureTemplateSettings.h` | `80fb805c112d99aa5106bf8af92074eb6e1bf934` |
| `controls\dev\PersonPicture\PersonPicture.xaml` | `1bd0a1c6f84ab4565c2b2c8fd10ba092e4ebb98c` |
| `controls\dev\PersonPicture\PersonPicture_themeresources.xaml` | `228efb45b2e22ae1d482304ecf9b2a6af1496011` |
| `controls\dev\Generated\PersonPicture.properties.cpp` | `f9dbcaf5193ff2226f370518c77b50c5c0777ea2` |
| `controls\dev\Generated\PersonPicture.properties.h` | `df2509468f2d1f2c3e4f030f34b3b7ee3a1baa5a` |
| `controls\dev\Generated\PersonPictureAutomationPeer.properties.cpp` | `cf7915ee5ae5085e0c9ef0279986a0d8071be245` |
| `controls\dev\Generated\PersonPictureTemplateSettings.properties.cpp` | `d9af29f5d531d2c963a0531ba8bf379cdb75a5e8` |
| `controls\dev\Generated\PersonPictureTemplateSettings.properties.h` | `5a6dab83e0b5c4db66028c622790247889782663` |
| `controls\dev\PersonPicture\APITests\PersonPictureTests.cs` | `e0e8b9aabb828e0d67c6ac7b6c2aae8db705e4af` |
| `controls\dev\PersonPicture\InteractionTests\PersonPictureTests.cs` | `f5918072ecb8c1ecc8fda46432ff403d0c127969` |
| `controls\test\MUXControlsTestApp\verification\PersonPicture.xml` | `4a68256600df687ad13177f6c62cd1b4fae26b8f` |

The prior audit used product commit
`c70471c511a0168b61dcca13af9556465f26b673`. Rename-aware comparison to the
current snapshot shows every audited runtime, header, generated-property,
automation, template, resource, test, verification, and packaging file as a
byte-identical 100% rename. Commit
`8463f45162149de0ec3ad7df752596893fe3e13e` only removes the mirror's old
`src\` prefix; there are no PersonPicture changes after that move. Unlike some
other controls, the current tree has no separate PersonPicture perf2026 theme
dictionary, so the classic dictionary above remains the authoritative current
template.

## Current Gallery Source Pins

Current commit `29f62479d5c046a0b854a5868e5a7cd484572d87` carries the PersonPicture page
created by `14a4a1a2b8ddc527dc4a7d5f7e743d7c2bc97db7` (`Convert other samples`,
2026-05-22), with no later PersonPicture changes:

| Current Gallery source | Git blob |
| --- | --- |
| `WinUIGallery\Samples\PersonPicture\PersonPicturePage.xaml` | `d705d1c283d05cdf38ee710795924431706f91f6` |
| `WinUIGallery\Samples\PersonPicture\PersonPicturePage.xaml.cs` | `4a822bd76ff2be755f1247d68f63734a49860fe9` |
| `WinUIGallery\Samples\PersonPicture\PersonPictureSelectDifferentLooksPerson.txt` | `265630278d14a4598d1d146b9121bf6a7b301e4c` |
| `tests\WinUIGallery.UITests\Tests\PersonPicture.cs` | `129f1e913ef06ec42375259282a95b1c19c0052a` |

The current page contains one `Select different looks for the person picture.`
example and three profile-type choices. Profile Image uses Microsoft's
shoulder-tap payload, Display Name sets `Jane Doe`, and Initials sets `SB`; each
choice clears the two lower-priority values. ModernWpf reproduces the same
runtime, names, labels, source snippet/substitutions, and 24-DIP example/options
gap. It packages the official image locally so the sample remains deterministic
and uses an explicit 96-DIP runtime avatar while preserving the source's
`Height=300` display snippet.

## Ported Product Behavior

- Image selection follows source priority: explicit ProfilePicture, contact
  image when available, then initials/display-name initials, then the person or
  group placeholder glyph.
- `TemplateSettings.ActualInitials` and the reusable `ActualImageBrush` track
  the resolved content without reallocating an image brush for every update.
- The main control is forced circular from the smaller arranged dimension;
  initials font size and badge geometry derive from that actual size.
- Badge priority remains image, positive number, glyph, no badge. Numbers above
  99 render `99+`; glyph/image and number states use their source opacity,
  stroke, foreground, and size resources.
- Automation names retain source contact/display-name/initials fallback,
  localized singular/plural badge descriptions, and BadgeText overrides.
- `InitialsGenerator` preserves the source tokenization and two-character
  generation rules. `PreferSmallImage` retains the source default and no-op
  property-change path when no WinRT Contact bridge exists.

## Accessibility Parity

The current WinUI peer exposes class `PersonPicture` and Text control type,
without an additional control pattern. ModernWpf's `PersonPictureAutomationPeer`
matches that shape. The owner automation name is updated by the same resolved
identity and badge-information path exercised by focused tests. The template's
decorative ellipses/text remain implementation details rather than independent
interactive controls.

## Template and Resource Parity

- The default template retains `PersonPictureEllipse`, `InitialsTextBlock`,
  badge ellipses/brushes, `BadgeNumberTextBlock`, and `BadgeGlyphIcon`.
- Common states retain Photo, Initials, NoPhotoOrInitials, and Group. Badge
  states retain BadgeWithImageSource, BadgeWithoutImageSource, and NoBadge.
- The placeholder state uses `SymbolThemeFontFamily` with E77B; Group uses
  E716. The main ellipse binds the control's Foreground, Background,
  BorderBrush, and BorderThickness surface.
- Light, Dark, and HighContrast resources keep current WinUI brush aliases,
  one-DIP main stroke, two-DIP badge stroke, badge opacities, and sizing ratios.

## WPF Substitutions

- WinUI Contact and async `IRandomAccessStreamReference` profile-picture
  loading are not exposed as WPF API. Internal contact fields are unreachable;
  `PreferSmallImage` is therefore inert until a deliberate WPF/WinRT bridge is
  added.
- WPF has no direct `AutomationProperties.AccessibilityView=Raw`,
  `x:DeferLoadStrategy=Lazy`, `TextLineBounds=Tight`, or
  `IsTextScaleFactorEnabled=False` equivalents. The WPF template preserves the
  rendered and peer behavior without inventing public API.
- WPF Ellipse expects a scalar StrokeThickness, so the template maps the
  control's BorderThickness surface through `BorderThickness.Left`.
- `DefaultPersonPictureStyle` precedes the implicit style because WPF
  `StaticResource` lookup is order-sensitive.

## Regression Coverage

- `PersonPictureApiTests` covers defaults/properties, image/initials priority,
  reusable template settings, all badge branches and automation names, small
  dimensions, visual states, peer role, template parts, and current Light/Dark/
  HighContrast resources.
- `GalleryAutomationHookTests` pins the one current example, source snippet,
  official local image, exact 96-DIP runtime avatar, profile labels, and all
  three selection transitions.
- `WpfGallerySourceShapeTests` pins the image asset path, avatar-specific
  reference crop, strict `0.5` mean gate, and zero primary size tolerance.
- `PersonPictureSourceAuditTests` pins current product/Gallery commits and
  blobs, implementation/template/peer/Gallery shape, and final reports.

## Live Installed-Gallery Evidence

| Theme | Report | Reference | Crop sizes | Mean delta | Gate |
| --- | --- | --- | --- | ---: | --- |
| Light | `artifacts/visual-checks/20260718-132501-654-69848/report.md` | Fresh live installed Gallery and ModernWpf | `96x96` / `96x96` | `0.39` | `0.5`, size `0` |
| Dark | `artifacts/visual-checks/20260718-132425-300-73904/report.md` | Fresh live installed Gallery and ModernWpf | `96x96` / `96x96` | `0.35` | `0.5`, size `0` |

The sample-specific reference crop finds the rendered avatar in the first
example because the installed Gallery does not expose a stable automation ID
on that PersonPicture. Shape, crop dimensions, image framing, and antialiasing
align; the remaining sub-half-point delta is platform rasterization.

## Verification

- The refreshed product/source slice passes 10/10 on
  `net8.0-windows7.0`.
- Focused Gallery runtime/source-shape tests pass 4/4 on both
  `net8.0-windows7.0` and `net10.0-windows7.0`.
- The net462 Controls build succeeds warning-free with zero errors; Gallery
  tests rebuild the net8/net10 product outputs.
- Both final fully live comparisons pass the `0.5` mean gate with exact `96x96`
  crops and zero size tolerance.
