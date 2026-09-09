# Contract: Navigation

**Feature**: 001-validation-ui-compliance
**Type**: UI navigation contract (`Components/Layout/NavMenu.razor`)

Adds a single primary-navigation entry for the Data Validation section and defines its active-state behavior.

## Nav entry contract

| Property | Required value | Requirement |
|----------|----------------|-------------|
| Count | Exactly **one** Data Validation entry | FR-001, FR-002a, AC-001 |
| Label | `Data Validation` | FR-001, AC-001 |
| Href | `/validation` | FR-001, AC-002 |
| Match | `NavLinkMatch.Prefix` | FR-002, FR-002b, AC-002 |
| Icon | Section-consistent Material icon (e.g., `Icons.Material.Filled.FactCheck`) | FR-002, AC-001 |
| Style / ActiveStyle | Same `Style`/`ActiveStyle` treatment as existing Portfolios/Platform `MudNavLink`s | FR-002, AC-001, AC-002 |
| Location | Same `MudNavMenu` as other primary sections | FR-002 |

## Active-state behavior

```
GIVEN the user is on a route R
WHEN R starts with "/validation"        → the entry renders with the selected/active style
WHEN R does NOT start with "/validation" → the entry renders with the default style
```

Because `NavLinkMatch.Prefix` is used, the entry is active on:
- `/validation` (submit)
- `/validation/compare` (compare)
- `/validation/runs/{RunId}` (run detail)

This satisfies FR-002b ("nav entry displays the highlighted state on run detail pages as well").

## Invariants

- **INV-N1**: No second top-level nav entry for compare (FR-002a, AC-017). Compare is reached via an in-section secondary action (see [presentation-components.md](./presentation-components.md)).
- **INV-N2**: No nav entry for a run-history page (none exists) (FR-002b, AC-018).
- **INV-N3**: Clicking the entry opens `/validation` without error and highlights it (AC-002).
- **INV-N4**: The entry is permanently visible from any page in the same nav area as the others (FR-002).

## Acceptance mapping

| Contract check | Acceptance Criteria | Test method (from spec Test Mapping) |
|----------------|---------------------|--------------------------------------|
| Single entry renders alongside others, styled identically, with icon | AC-001 | `Navigation_HasDataValidationEntry_Should_Render_And_Navigate` |
| Click navigates + active highlight applied | AC-002 | `Navigation_HasDataValidationEntry_Should_Render_And_Navigate` |
| No run-history route/entry added | AC-018 | `Navigation_RunDetail_No_History_Route_Added` |
