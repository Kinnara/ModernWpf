# ModernWPF 1.0 roadmap

ModernWPF 1.0 is being delivered as a sequence of source-audited previews.
Package download totals are informational: they count restores rather than
independent applications and are not a release-readiness gate. The project
instead relies on package execution, representative downstream builds, visual
evidence, and a time-boxed release-candidate soak.

## Milestones

| Version | Planned surface | Exit requirement |
| --- | --- | --- |
| `1.0.0-preview.2` | Complete the existing-control WinUI synchronization epoch, improve package and feedback surfaces, and add downstream compatibility canaries. | Classify the milestone's finite upstream cutoff, pass the complete release gate, and verify the published package on every supported target. |
| `1.0.0-preview.3` | Add source-audited `TimePicker` and `TwoPaneView` controls. | Document WPF adaptations, update public contracts and migration notes, add Gallery and focused test coverage, and pass the release gate. |
| `1.0.0-preview.4` | Add the WinUI-derived `TitleBar` control—distinct from the existing WPF `WindowTitleBar` chrome helpers—and WPF-native Mica/Desktop Acrylic window materials. | Cover supported-OS, unsupported-OS, disabled-composition, and High Contrast fallbacks as well as window activation and chrome behavior. |
| `1.0.0-preview.5` | Add the complete `TabView` control family. | Cover keyboard and automation behavior, close, overflow, reorder, and the documented WPF tear-out adaptation. |
| `1.0.0-preview.6` | Add `ItemContainer`, `LinedFlowLayout`, and adapted scrolling prerequisites. | Prove realization, recycling, selection, layout, accessibility, and supported-target behavior needed by `ItemsView`. |
| `1.0.0-preview.7` | Add the complete `ItemsView` control family. | Complete source, Gallery, interaction, automation, layout, theme, package, and migration coverage. |
| `1.0.0-rc.1` | Freeze the intended 1.0 CLR API and explicitly shipped resource-key surface. | Pass the full gate and downstream canaries, complete final visual/manual validation, and begin an unchanged 14-day soak. |
| `1.0.0` | Establish the stable SemVer boundary for the 1.x line. | Complete the RC soak with no release-blocking defect and no public-contract change. |

`PipsPager` is deferred to the 1.1 line. The milestones above are not
compressed or renumbered to compensate for that deferral.

## Preview rules

Current applicable WinUI source remains authoritative for WinUI-derived
controls during the preview series. Each feature preview must include:

- a pinned source audit and documented WPF adaptations;
- a Gallery page and focused behavior, input, automation, layout, and theme
  tests appropriate to the control;
- updated CLR and public resource-key inventories;
- explicit migration guidance for any intentional preview-era break;
- package verification, executable package-consumer smoke tests, and the
  complete serialized release gate.

After a preview is published, development advances to the next version and the
published preview becomes the active package-validation baseline. Preview
breaks are allowed only through the reviewed process in
[the public API contract](public-api-contract-1x.md).

## Release-candidate and stable rules

The accepted RC must have three green downstream canaries spanning .NET
Framework 4.7.2, .NET 8, and .NET 10, including evidence from at least two
documented 0.9-to-1.0 migrations. Existing package smoke coverage continues to
exercise both `FluentControlsResources` and `XamlControlsResources`.

The 14-day soak starts only after the RC package, Gallery visual/manual checks,
and downstream results are accepted. There must be no unresolved security,
data-loss, startup/crash, core-input, or equivalent P0/P1 release blocker. Any
CLR API or public resource-key change produces a new RC and restarts the soak.
Stable 1.0 must otherwise differ from the accepted RC only in version and
release documentation.

No download threshold, promotional campaign, private tester quota, automatic
downstream pull request, or 0.9 unlisting is part of the graduation policy.
