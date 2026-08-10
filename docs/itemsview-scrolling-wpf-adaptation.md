# ItemsView scrolling prerequisites: WPF adaptation

Date: 2026-08-10

Preview 6 establishes the scrolling boundary required by Preview 7 ItemsView
without publishing partial `ScrollView` or `ScrollPresenter` shells.

## Pinned source boundary

The current source authority is microsoft-ui-xaml `winui3/main`
`23a73be03d194ea0ece97da71de98b6b53021b70`:

| Path | Blob |
| --- | --- |
| `controls/dev/ScrollView/ScrollView.idl` | `2848163f939b4725ac7b9279b5b228622b1db39a` |
| `controls/dev/Repeater/ItemsRepeaterScrollHost.cpp` | `fdd2b26086742f271e9e872ada409faa6befc800` |
| `controls/dev/Repeater/ItemsRepeaterScrollHost.h` | `c3fcaba0f931b2093ae129a2cf1131c68a5ac1d5` |
| `docs/design-notes/ItemsView_spec.md` | `541dc3aecaa4a2bde243c550d5bc2b63ea0f0b33` |

The reconciled stable cutoff is
`a97562621a1d1ea397a38a3f512c9eef99db52d8`; the WinUI Gallery cutoff is
`b78c440193aab788215888561e45adf72da848cb`.

ModernWPF already ships the source-shaped `IScrollController`, request event
arguments, correlation IDs, panning-information contract, and scrolling
options in `ModernWpf.Controls.Primitives`. It also ships
`ItemsRepeaterScrollHost`, which supplies visible and realization windows,
anchoring, caching, and recycling around a WPF `ScrollViewer`.

## Accepted WPF substitution

WPF has no InteractionTracker, expression animation property set, portable
content-island scroll presenter, or WinUI Composition animation object. A
public ModernWPF `ScrollView` or `ScrollPresenter` with those contracts absent
would be misleading and would freeze an incomplete API before stable 1.0.
Preview 6 therefore adds an internal `ItemsViewScrollHost :
System.Windows.Controls.ScrollViewer`; this is not a new ModernWPF public
scrolling control.

The host:

- publishes its minimum, maximum, offset, viewport, enabled, and scrollable
  state to an attached vertical `IScrollController`;
- accepts absolute, relative, and velocity requests, assigns non-sentinel
  correlation IDs, clamps offsets, and reports completion after the dispatcher
  applies the WPF scroll;
- maps velocity to one deterministic 60 Hz display-frame delta because WPF has
  no compositor velocity transaction;
- hides the native vertical bar only while an external controller is attached,
  preserves it when controllers are replaced, and restores the exact original
  visibility when detached; and
- composes inside `ItemsRepeaterScrollHost`, retaining real realization-window
  virtualization instead of measuring every item.

The animation-object and panning hooks remain controller contracts; the host
does not fabricate native composition objects. Preview 7 may expose its
source-named `ScrollView` result as a WPF `ScrollViewer` and retain the existing
public `IScrollController` type. That final disposition must be recorded in the
ItemsView audit alongside every other source type substitution.

Focused tests cover controller values, enabled/scrollable changes, absolute,
relative, NaN, velocity, clamping, correlation completion, controller
replacement, event detachment, and native-scrollbar restoration. Preview 7
adds end-to-end template, bring-into-view, keyboard, selection, and external
controller coverage.
