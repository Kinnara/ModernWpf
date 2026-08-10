# ItemContainer current WinUI 3 source audit

Date: 2026-08-10

ModernWPF 1.0.0-preview.6 adds the WinUI-shaped `ItemContainer` required by
Preview 7 `ItemsView`. It is a separate control; it does not replace or restyle
WPF `ListBoxItem` or `ListViewItem`.

## Pinned source interval

| Source | Commit | Use |
| --- | --- | --- |
| microsoft-ui-xaml `winui3/main` | `23a73be03d194ea0ece97da71de98b6b53021b70` | Product, public IDL, template, resources, automation, API tests, interaction tests, and design cutoff. |
| microsoft-ui-xaml `winui3/release/2.3.1` | `a97562621a1d1ea397a38a3f512c9eef99db52d8` | Stable reconciliation. The relevant public ItemContainer surface remains aligned. |
| WinUI Gallery `main` | `b78c440193aab788215888561e45adf72da848cb` | Gallery cutoff. It has no standalone ItemContainer page, so current product TestUI and the ItemsView samples are the visual/sample authority. |

Exact microsoft-ui-xaml inputs:

| Path | Blob |
| --- | --- |
| `controls/dev/Generated/ItemContainer.properties.cpp` | `aff78431cf851edb806a28cab73af7a7c21513bd` |
| `controls/dev/Generated/ItemContainer.properties.h` | `714353785bb6da67db0ef4732fb995f8530ba033` |
| `controls/dev/ItemContainer/ItemContainer.cpp` | `4c3552d8ded36fa1ffa2884574f46ca58a7a48f8` |
| `controls/dev/ItemContainer/ItemContainer.h` | `6f4d61982b44a35d5c907e967d0c2c3a6fa1b60d` |
| `controls/dev/ItemContainer/ItemContainer.idl` | `89a7d29e365eb73d31fed61b81d373f6ba3ed8b2` |
| `controls/dev/ItemContainer/ItemContainer.xaml` | `369db4d085838fa2658ce2056f0091684d554adc` |
| `controls/dev/ItemContainer/ItemContainer_themeresources.xaml` | `b56c1a0b4abbe42b77c8cd74e47e42a2d7e8a2a6` |
| `controls/dev/ItemContainer/ItemContainerAutomationPeer.cpp` | `abf79f19728820b2d4db4649fcfd698500d939ba` |
| `controls/dev/ItemContainer/Strings/en-us/Resources.resw` | `d8430b5ee5ce7aff9dd6e60f849c59c869db6411` |
| `controls/dev/ItemContainer/APITests/ItemContainerTests.cs` | `6447df9e786f564c80263f111ff8e299336a8f77` |
| `controls/dev/ItemContainer/InteractionTests/ItemContainerTests.cs` | `5709f3b646b67e7ab1e1e7bad4710a836cc411d9` |
| `docs/design-notes/ItemsView-ItemContainer-overview.md` | `6ca6dd917930e850147fe553b8a4bf7855e551e5` |

## Public surface and WPF adaptations

The source declares an unsealed `ItemContainer : Control`, makes `Child` its
content property, and exposes `Child`, `IsSelected`, their dependency
properties, and the public automation peer. `CanUserSelect`, `CanUserInvoke`,
multi-select mode, invocation event, trigger enum, and invocation arguments are
ItemsView implementation details in the source and remain internal here.

WinUI inherits `CornerRadius` from its base `Control`; WPF `Control` does not.
ModernWPF therefore adds `CornerRadius` by owning WPF's
`Border.CornerRadiusProperty`, uses `ControlCornerRadius` as the style default,
and template-binds every rounded visual. This is the same WPF adaptation used
by other ModernWPF controls that expose WinUI's inherited property.

Pointer press/release and source double-tap map to WPF mouse capture and
double-click input. Enter and Space use ordinary WPF routed key input. The
standalone control displays selection but does not invent a public click or
selection event: Preview 7 ItemsView owns those policies.

## Template, theme, and accessibility

The template retains the common visual, selected outer visual, arbitrary child
presenter, non-interactive multi-select checkbox, focus visual, disabled state,
and selected/pointer/pressed combinations. WinUI visual states are represented
with ModernWPF's WPF visual-state setters.

The source ItemContainer keys map to existing Fluent semantic resources in the
global Light, Dark, and High Contrast theme dictionaries. Shared metrics live
in `ModernWpfControlsResources`, so dynamic resource lookup works for every
instantiated template and for the compact resource entry. Light and Dark
resolve through their respective semantic brushes; real OS High Contrast maps
the ItemContainer keys to WPF system colors. The control does not duplicate
theme palettes or alter stock WPF item containers.

`ItemContainerAutomationPeer` reports `ListItem`, derives its accessible name
from explicit automation metadata or child content, and falls back to the
localized `ItemContainer` resource. SelectionItem and Invoke patterns are
available only when ItemsView's internal policy enables them. Disabled pattern
operations follow WPF provider convention and throw
`ElementNotEnabledException`.

## Acceptance coverage

Focused tests cover defaults and XAML content, custom corner radii, child and
selection template state, pointer/double-click/keyboard triggers, conditional
automation patterns, accessible names, disabled providers, Light/Dark semantic
resources, High Contrast semantic mappings, and compact resources. The Gallery
page uses the real control with live selected, enabled, corner-radius, status,
focus, and automation state.
