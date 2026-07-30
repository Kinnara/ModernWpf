# ModernWPF 1.0.0-preview.2

<!-- RELEASE-NOTES: DRAFT -->

`1.0.0-preview.2` is the active development version following the first
ModernWPF 1.x preview. This file will accumulate user-facing changes before
the version is tagged.

## Development baseline

- `1.0.0-preview.1` remains the immutable historical audit and migration
  comparison, not an API freeze across later previews.
- Current applicable WinUI API shape is authoritative for WinUI-derived
  controls. A deliberate breaking parity change must update the checked-in
  inventories and active package-validation baseline, add focused tests, and
  document consumer migration under `## Breaking changes`.
- New public CLR APIs and resource keys must be recorded in the checked-in
  inventories. Stable `1.0.0` will establish the SemVer compatibility baseline
  for subsequent 1.x releases.
- NuGet publication uses Trusted Publishing with a short-lived OIDC credential;
  the repository does not store a long-lived NuGet API key.

See [Migrating from ModernWPF 0.9.x](migrating-from-0.9.md) and the
[1.x public API contract](public-api-contract-1x.md) for the preview governance
and stable compatibility policy.

## WinUI 3 synchronization

- Reconciles every existing WinUI-derived control family through product
  `winui3/main` commit
  `eb75504a1978df0d37a3ad4574d6f72bf4d21583`, latest stable
  `winui3/release/2.3.1`, and WinUI Gallery commit
  `f4dc3eb367f4bcecac1793829d9a221e924e5bfb`. The complete disposition is in
  [the synchronization epoch record](winui3-sync-2026-07-29.md).
- Ports CommandBar's fractional-DPI compact-height threshold, WindowedPopup's
  pending XAML open lifecycle, and ItemsRepeater's ownerless recycling during
  nonvirtualizing source replacement.
- Adds regression guards for UniformGrid narrow-width layout and
  NavigationView's Alt+Space system-menu shortcut, and corrects the WinUI
  TitleBar drag-region API status to public V11.
- Adds a machine-readable stable/main/Gallery source manifest and a weekly,
  read-only drift report. New mapped or unmapped upstream changes require human
  review; the automation never ports, merges, commits, or advances a pin.

## Breaking changes

This synchronization epoch requires no public CLR or explicit public
resource-key change, so there is no consumer migration for the control fixes
above.
