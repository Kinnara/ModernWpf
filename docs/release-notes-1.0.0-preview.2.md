# ModernWPF 1.0.0-preview.2

<!-- RELEASE-NOTES: DRAFT -->

`1.0.0-preview.2` is the active development version following the first
ModernWPF 1.x preview. This file will accumulate user-facing changes before
the version is tagged.

## Development baseline

- `1.0.0-preview.1` remains the immutable forward-compatibility and package
  validation baseline.
- New public CLR APIs and resource keys must be recorded in the checked-in
  unshipped inventories.
- NuGet publication uses Trusted Publishing with a short-lived OIDC credential;
  the repository does not store a long-lived NuGet API key.

See [Migrating from ModernWPF 0.9.x](migrating-from-0.9.md) and the
[1.x public API contract](public-api-contract-1x.md) for the compatibility
boundary.
