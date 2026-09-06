# ModernWPF 1.0.0-rc.1

`1.0.0-rc.1` is the release-candidate milestone after Preview 7. It freezes
the intended 1.0 public CLR API and resource-key surface; it does not add
another feature preview or start the stable-release soak before acceptance.

## Compatibility and freeze

- `1.0.0-preview.7` is the active package-validation baseline.
- The accepted inventories contain 1,586 ModernWpf CLR entries, 3,225
  ModernWpf.Controls CLR entries, and 5,501 source-qualified resource entries.
  Promotion from Unshipped to Shipped preserves the combined contract.
- `1.0.0-preview.1` remains the historical audit and migration baseline.
- Stable `1.0.0` establishes the SemVer compatibility boundary for 1.x.

See [Migrating from ModernWPF 0.9.x](migrating-from-0.9.md) and the
[1.x public API contract](public-api-contract-1x.md).

## Fixes since Preview 7

- RadioButtons recycles factory-owned wrappers instead of accumulating visual
  children under a virtualizing template. Recycled wrappers clear selection,
  content, and template state; custom factories receive their recycle calls.
- CalendarDatePicker and AutoSuggestBox restore keyboard focus correctly when
  their popups close, including empty AutoSuggestBox result lists.
- RadioMenuItem group bookkeeping remains correct through unload/reload.
- Pack resource lookup is cached without retaining resource streams.
- Gallery title-bar close glyph alignment is corrected.
- Gallery Settings reflects the active theme when reopened, so returning to
  the system theme remains available after an explicit Light or Dark choice.
- CommandBar and CommandBarFlyout More-button icons follow the state-specific
  foreground, preserving contrast on Aquatic High Contrast highlight backgrounds.

## Validation and release status

The [RC 1 upstream cutoff](winui3-sync-2026-09-06-rc1.md) pins product stable,
product main, and Gallery sources and classifies all 424 changed paths.

The ItemsView transition-forwarding test uses a non-animating test provider
instead of invoking the abstract-by-convention animation behavior of the base
provider when OS animations are enabled. No product animation behavior changes.

Release acceptance requires the serialized build/test/package gate,
three consecutive complete WinUI runs from the final clean tip, all three
downstream canaries, and Light, Dark, and real OS High Contrast visual and
manual input checks on all supported targets.

CommandBar overflow-order tests now constrain the control's actual width,
independently of system window chrome and minimum HWND dimensions. This
removes High Contrast-specific test failures without changing overflow behavior.

The unchanged 14-day RC soak starts only after package publication and the
required visual/downstream evidence have been accepted. An accepted surface
change or a replacement RC restarts it; stable 1.0 cannot skip this interval.

## Breaking changes and migration

RC 1 makes no intentional breaking change to Preview 7 CLR APIs or public
resource keys. Preview 7 consumers only need to update the package version.
Targets remain `net462`, `net8.0-windows7.0`, and `net10.0-windows7.0`.
`PipsPager` remains deferred to 1.1.
