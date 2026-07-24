# Retained legacy test retirements

The net48 `ModernWpfTestApp` suite remains in the release gate. Eleven
WinUI-era assertions are explicitly ignored because they validate obsolete
0.9 template details and have named modern replacements:

- AutoSuggestBox corner radius:
  `AutoSuggestBoxApiTests.VerifyAutoSuggestBoxCornerRadius`
- ComboBox popup and editable corner radii: current-style corner-radius and
  editable-template tests in `ComboBoxApiTests`
- NumberBox corner radius: current-style corner-radius tests in
  `NumberBoxApiTests`
- NavigationView item automation:
  `NavigationViewApiTests.VerifyNavigationItemUIAType`
- PersonPicture states:
  `PersonPictureApiTests.VerifyVSMStatesForPhotosAndInitials`
- Four CommandBarFlyout sizing cases: the four corresponding
  `CommandBarFlyoutApiTests.VerifyCommandBarSizing*` tests
- Dynamic-content FlowLayout geometry:
  `RepeaterLayoutTests.ValidateFlowLayoutWrapsItemsRepeaterChildren`, which
  uses deterministic item dimensions

Each ignored legacy test carries the replacement in its `Ignore` reason.
`tools/test/Assert-TestResults.ps1` fails the release gate if a skipped test has
no recorded reason.
