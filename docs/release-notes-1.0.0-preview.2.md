# ModernWPF 1.0.0-preview.2

`1.0.0-preview.2` completes the existing-control WinUI synchronization
milestone and improves package discovery, migration guidance, samples,
feedback, and downstream compatibility validation for the actively maintained
ModernWPF 1.x line.

## Preview compatibility

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

## Package and adoption experience

- Expands the NuGet package page with the ModernWPF icon, a Gallery screenshot,
  an exact preview install command, supported-target guidance, recommended and
  legacy resource-entry examples, migration guidance, and a structured preview
  feedback route.
- Replaces the Gallery's obsolete wiki-based control reference with the current
  repository documentation, fixes its Getting started link, and directs bug
  reports to a versioned Preview bug form.
- Updates checked-in samples to teach `FluentControlsResources` and adds a
  small multi-target package consumer that the release gate can rebuild from
  the actual candidate `.nupkg`.
- Adds isolated, manually dispatched downstream canaries for pinned .NET
  Framework 4.7.2, .NET 8, and .NET 10 applications. Candidate restores use a
  local package feed and do not add Preview 2 downloads on nuget.org; unchanged
  0.9.x baseline restores can still add historical traffic. See
  [Downstream compatibility canaries](downstream-canaries.md).
- A [successful Preview 2 master candidate run](https://github.com/Kinnara/ModernWpf/actions/runs/31148484772)
  at commit `c5b4806a622cdee0b62b7df0c0c493548816a579` classified all three
  pinned canaries as `migrated`: each unchanged 0.9.x baseline and its
  documented, minimally migrated 1.x candidate built successfully for
  BilibiliLiveRecordDownLoader on .NET 10, BililiveRecorder on .NET Framework
  4.7.2, and OpenKh on .NET 8. Tagged-release validation runs separately.

## Road to stable 1.0

- Records the fixed Preview 3 through Preview 7 feature sequence, the
  API-frozen release candidate, representative downstream evidence, and the
  unchanged 14-day RC soak in the [ModernWPF 1.0 roadmap](roadmap-1.0.md).
- Download counts remain informational rather than a stable-release gate.
  `PipsPager` is deferred to 1.1, while the remaining planned 1.0 control
  families keep their reviewed preview milestones.

## WinUI 3 synchronization

- Reconciles every existing WinUI-derived control family through product
  `winui3/main` commit
  `d5bdbb190cdba0b7f1baec4b3981208a9685a360`, latest stable
  `winui3/release/2.3.1`, and WinUI Gallery commit
  `3669519356c67f1376152c33ed8ea45003a91f3a`. The original synchronization
  epoch remains recorded in [the July synchronization record](winui3-sync-2026-07-29.md),
  and the complete final Preview 2 cutoff is in
  [the August milestone disposition](winui3-sync-2026-08-06.md).
- Ports CommandBar's fractional-DPI compact-height threshold, WindowedPopup's
  pending XAML open lifecycle, and ItemsRepeater's ownerless recycling during
  nonvirtualizing source replacement.
- Adds regression guards for UniformGrid narrow-width layout and
  NavigationView's Alt+Space system-menu shortcut, and corrects the WinUI
  TitleBar drag-region API status used by the future Preview 4 source audit to
  public V11.
- Adds a machine-readable stable/main/Gallery source manifest and a weekly,
  read-only drift report. New mapped or unmapped upstream changes require human
  review; the automation never ports, merges, commits, or advances a pin. The
  report also fails closed when GitHub's compare response reaches its 300-file
  cap rather than presenting a partial path prefix as a complete inventory.

## Breaking changes

This synchronization epoch requires no public CLR or explicit public
resource-key change, so there is no consumer migration for the control fixes
above.

## Known preview limitations

- This release is not the stable 1.x compatibility boundary. Intentional,
  source-audited API or public resource-key corrections may still occur before
  the RC freeze and will carry explicit migration guidance.
- Native ModernWPF `TimePicker`, `TwoPaneView`, the future WinUI-derived
  `TitleBar` control (distinct from today's `WindowTitleBar` and
  `WindowTitleBarControl` chrome helpers), `TabView`,
  `ItemContainer`, `LinedFlowLayout`, and `ItemsView` control families are
  scheduled for later 1.0 previews rather than included in Preview 2.
- Window materials necessarily use WPF/Windows fallbacks when composition,
  transparency, or the required Windows feature is unavailable; the dedicated
  material work is scheduled for Preview 4.
