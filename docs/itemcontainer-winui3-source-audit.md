# ItemContainer current WinUI 3 source audit

Date: 2026-08-09

ModernWPF Preview 6 adds the WinUI-shaped `ItemContainer` used by the
Preview 7 `ItemsView`. It is distinct from WPF's generated
`ListBoxItem`/`ListViewItem` containers and does not replace their stock
Fluent styles.

## Pinned upstream snapshots

| Source | Commit | Purpose |
| --- | --- | --- |
| microsoft-ui-xaml `winui3/main` | `e1aa8f64df98d6229f6cd4074d59b654616254da` | Product, public IDL, template, resources, automation, API tests, and interaction cutoff. |
| microsoft-ui-xaml `winui3/release/2.3.1` | `a97562621a1d1ea397a38a3f512c9eef99db52d8` | Stable reconciliation; the public ItemContainer IDL is byte-identical to main. |
| WinUI Gallery `main` | `3669519356c67f1376152c33ed8ea45003a91f3a` | Gallery cutoff. No standalone ItemContainer page exists at this cutoff, so the product TestUI and ItemsView design note are the sample authority. |

The exact upstream inputs are:

| File under microsoft-ui-xaml | Blob |
| --- | --- |
| `controls/dev/Generated/ItemContainer.properties.cpp` | `aff78431cf851edb806a28cab73af7a7c21513bd` |
| `controls/dev/Generated/ItemContainer.properties.h` | `714353785bb6da67db0ef4732fb995f8530ba033` |
| `controls/dev/ItemContainer/ItemContainer.cpp` | `4c3552d8ded36fa1ffa2884574f46ca58a7a48f8` |
| `controls/dev/ItemContainer/ItemContainer.h` | `6f4d61982b44a35d5c907e967d0c2c3a6fa1b60d` |
| `controls/dev/ItemContainer/ItemContainer.idl` | `89a7d29e365eb73d31fed61b81d373f6ba3ed8b2` |
| `controls/dev/ItemContainer/ItemContainer.xaml` | `369db4d085838fa2658ce2056f0091684d554adc` |
| `controls/dev/ItemContainer/ItemContainer_perf2026.xaml` | `1e48448134a847a4babe5686107352ad4842c82b` |
| `controls/dev/ItemContainer/ItemContainer_themeresources.xaml` | `b56c1a0b4abbe42b77c8cd74e47e42a2d7e8a2a6` |
| `controls/dev/ItemContainer/ItemContainer_themeresources_perf2026.xaml` | `8f5ad66e0c5206be36a44beb9b14a9176a69956e` |
| `controls/dev/ItemContainer/ItemContainerAutomationPeer.cpp` | `abf79f19728820b2d4db4649fcfd698500d939ba` |
| `controls/dev/ItemContainer/ItemContainerAutomationPeer.h` | `d7cca0cd3f883518c2fed9a395a57a63ebb7a914` |
| `controls/dev/ItemContainer/ItemContainerInvokedEventArgs.cpp` | `f864e8a1d345862f0d72789de09f4ccc53a2023f` |
| `controls/dev/ItemContainer/ItemContainerInvokedEventArgs.h` | `bf4ad19df7cb19f6c89f484a41285237485337f1` |
| `controls/dev/ItemContainer/ItemContainerRevokers.h` | `301eccda40be724f362c5573fe2961ffbc75812a` |
| `controls/dev/ItemContainer/ItemContainerTrace.h` | `3cb75ff301de65ee77d0e42c8a5ed5aaf0a2dafb` |
| `controls/dev/ItemContainer/Strings/en-us/Resources.resw` | `d8430b5ee5ce7aff9dd6e60f849c59c869db6411` |
| `controls/dev/ItemContainer/APITests/ItemContainerTests.cs` | `6447df9e786f564c80263f111ff8e299336a8f77` |
| `controls/dev/ItemContainer/InteractionTests/ItemContainerTests.cs` | `5709f3b646b67e7ab1e1e7bad4710a836cc411d9` |
| `controls/dev/ItemContainer/TestUI/ItemContainerPage.xaml` | `7f05c09c5d7e7197cad7393f5f97c28159432997` |
| `controls/dev/ItemContainer/TestUI/ItemContainerPage.xaml.cs` | `62027656da6b38a764281b363c68212a98dc22bc` |
| `controls/dev/ItemContainer/TestUI/ItemContainerLayoutPage.xaml` | `09a1be59d17859d2a889f6272d2f95e65b083ad2` |
| `controls/dev/ItemContainer/TestUI/ItemContainerLayoutPage.xaml.cs` | `bf941937b957b7f8ac2209b6aa4373a82b7cdf19` |
| `docs/design-notes/ItemsView-ItemContainer-overview.md` | `6ca6dd917930e850147fe553b8a4bf7855e551e5` |

## Public surface and behavior

The source IDL declares an unsealed `ItemContainer : Control`, makes `Child`
its content property, and exposes only `Child`, `IsSelected`, and their
dependency properties. WinUI inherits `CornerRadius` from its `Control`; WPF
does not, so ModernWPF adds the equivalent `CornerRadius` dependency property
directly to preserve source-shaped markup.

The source `CanUserSelect`, `CanUserInvoke`, `MultiSelectMode`, invocation
event, trigger enum, and event arguments are ItemsView implementation
contracts marked internal in the IDL. They stay internal here. A standalone
ItemContainer therefore displays and automates the `IsSelected` state but
does not invent a public click event or toggle itself; Preview 7 ItemsView
owns pointer, keyboard, and selection-model policy.

The WPF input adaptation maps source pointer release/double-tap and Enter/Space
triggers to WPF mouse capture, double-click count, and routed key input. It
keeps the original-source and handled flow internal. Multiple-selection mode
shows a non-interactive WPF CheckBox visual, equivalent to the source
selection indicator.

## Accessibility

`ItemContainerAutomationPeer` remains public because the source peer is
public. It reports `ListItem`, class `ItemContainer`, and a child-derived name.
When neither markup nor the child supplies a name, both its name and localized
control type use the pinned `ItemContainerDefaultControlName` resource value
`ItemContainer`. It exposes SelectionItem only when the source effective
selection flags allow it, and Invoke only when ItemsView explicitly enables
invocation. Selection updates `IsSelected`; Invoke raises the internal item
invocation path. The peer walks ancestors for an `ISelectionProvider`, so
Preview 7 can supply the owning ItemsView selection container without a
second semantic item tree.

## Template and theme mapping

The WPF template preserves the source common visual, selected outer visual,
arbitrary child presenter, selection checkbox, focus visual, disabled
opacity, and selected/pointer/pressed combinations. WPF triggers replace
WinUI visual states; ordinary WPF mouse capture supplies the pressed state.

All 14 source theme brush keys and the seven source shared sizing/alignment
resources are declared. Light and Dark map to the corresponding Fluent
semantic brushes. High Contrast uses actual WPF system-color resources for
selection, borders, text contrast, and the unchecked checkbox background.
No stock WPF item-container style is modified.

## Gallery and acceptance coverage

The Gallery page uses the real `ModernWpf.Controls.ItemContainer`, arbitrary
child content, a live `IsSelected` option, and the WPF-owned corner-radius
adaptation. Focused tests cover defaults, XAML content parsing, template
parts, selected and multi-select visuals, source-gated automation patterns,
accessible names, and invocation. Theme and public-resource contract tests
cover Light, Dark, real High Contrast resources, and the checked-in inventory.
