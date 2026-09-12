# Phase 0 Research: Validation UI Compliance

**Feature**: 001-validation-ui-compliance
**Date**: 2026-09-09
**Input**: [plan.md](./plan.md), [spec.md](./spec.md)

All three spec clarifications were resolved during Session 2026-09-09 (see spec.md §Clarifications), so **no NEEDS CLARIFICATION markers remain**. This document consolidates the design-token, component-reuse, and Blazor/MudBlazor best-practice research needed to rebuild the three validation pages with structural parity to the rest of the dashboard.

---

## Decision 1: Source of the shared visual design language

**Decision**: Treat `MainLayout.razor` (the `MudTheme`) plus `PlatformDashboard.razor`, `PortfolioDashboard.razor`, `PortfolioDetail.razor`, and `Shared/KpiCard.razor` as the canonical reference for the dashboard's design tokens and composition patterns. The validation pages must match these, not invent new styling.

**Rationale**: These are the "other primary pages" the spec repeatedly references (AC-003, AC-015). The tokens are already defined in one place (the theme) and applied consistently across the reference pages.

**Design tokens observed (canonical values to reuse):**

| Token | Value | Source |
|-------|-------|--------|
| Primary | `#3b82f6` | `MainLayout` PaletteDark |
| Secondary | `#8b5cf6` | `MainLayout` PaletteDark |
| Success | `#00ff88` | `MainLayout` PaletteDark |
| Warning | `#f59e0b` | `MainLayout` PaletteDark |
| Error | `#ff4466` | `MainLayout` PaletteDark |
| Background | `#0a0f1a` | `MainLayout` PaletteDark |
| Surface | `#1e293b` | `MainLayout` PaletteDark |
| TextPrimary | `#ffffff` | `MainLayout` PaletteDark |
| TextSecondary | `#94a3b8` | `MainLayout` PaletteDark |
| Muted / meta text | `#64748b`, `#475569` | reference pages |
| Card surface | `linear-gradient(135deg, #1e293b 0%, #0f172a 100%)` | `KpiCard`, `PlatformDashboard` |
| Card border | `1px solid #334155` | `KpiCard`, `PlatformDashboard` |
| Card radius | `16px` (cards), `12px` (tables/filters) | reference pages |
| Icon tile | `44px` square, `border-radius: 12px`, gradient bg, centered `MudIcon` | `PlatformDashboard` header |
| Font | Inter / Roboto / sans-serif | `MainLayout` Typography |

**Alternatives considered**:
- *Add a global CSS stylesheet in `app.css`*: Rejected — the dashboard styles pages via inline `Style` on MudBlazor components and the theme, not `app.css` (which only holds Blazor boilerplate). A new stylesheet would be a divergent, one-off mechanism (violates FR-013/BR-3).
- *Rely purely on MudBlazor default theme*: Rejected — the reference pages deliberately override with the dark gradient card treatment; matching only the default MudBlazor look would still look disconnected.

---

## Decision 2: Structural parity via extracted shared components

**Decision**: Extract three reusable presentation components into `src/Certus.Dashboard/Components/Shared/` and use them on all three validation pages (and optionally refactor reference pages later, out of scope here):
- `PageHeader` — the icon-tile + bold title + muted description block (matches `PlatformDashboard` header).
- `DashboardCard` — the dark gradient content surface (`ChildContent`), replacing `MudPaper Elevation="2"`.
- `EmptyState` — icon tile + heading + supporting text + optional action (matches the "No platform connections" / "Portfolio not found" pattern).

**Rationale**: The clarification (spec Q3 → FR-013, BR-3, AC-019) requires **structural parity**: pages "rebuilt on the same shared component library primitives," not "raw HTML with CSS imitations." Extracting shared components is the concrete mechanism that makes AC-019 verifiable (the validation page and a reference page render the *same* component), and it keeps the tokens in exactly one place.

**Alternatives considered**:
- *Copy the inline `Style` strings from reference pages onto the validation pages*: Rejected — this is precisely the "one-off lookalike markup" AC-019 forbids and is fragile/duplicative.
- *Build a full design-system library project*: Rejected — over-engineered for 3 pages; would add a project and cross-layer complexity the Constitution Check discourages (unjustified complexity).

---

## Decision 3: Navigation entry and active-state highlighting

**Decision**: Add a single `MudNavLink` to `NavMenu.razor` targeting `/validation` with `Match="NavLinkMatch.Prefix"`, an icon consistent with the section (e.g. `Icons.Material.Filled.FactCheck` / `VerifiedUser` / `Rule`), and the same `Style`/`ActiveStyle` pattern as the existing Portfolios/Platform links.

**Rationale**: `NavLinkMatch.Prefix` on `/validation` naturally highlights for `/validation`, `/validation/compare`, and `/validation/runs/{RunId}` — exactly the "highlight across the whole section including run detail" behavior required by FR-002, FR-002b, AC-002. This mirrors how the existing Portfolios link (`Match="NavLinkMatch.Prefix"`) already highlights on `/portfolios/{id}` detail pages. A single entry (no separate compare link) satisfies the Q1 clarification (FR-002a).

**Alternatives considered**:
- *`NavLinkMatch.All`*: Rejected — would only highlight the exact `/validation` URL, failing to highlight on compare/run-detail pages (fails FR-002b/AC-002).
- *Two nav entries (validation + compare)*: Rejected — contradicts the Q1 clarification (FR-002a); compare is a secondary in-section action.

---

## Decision 4: Compare reached as a secondary in-section action

**Decision**: On `ValidationSubmit`, add a secondary action (e.g., a `MudButton Variant="Text"`/link styled action inside or beside the submission card) that navigates to `/validation/compare`. On `ValidationCompare`, provide a reciprocal secondary action back to `/validation`.

**Rationale**: Satisfies FR-002a and AC-017 ("a secondary action inside the section switches to the compare submission page without requiring a separate navigation entry"). Uses the standard MudBlazor secondary/text button treatment for parity.

**Alternatives considered**:
- *Top-level nav entry for compare*: Rejected per Q1 clarification.
- *Tabs between validate/compare*: Viable but heavier; a secondary action is the minimal match to the clarified intent and avoids implying they are equal peers.

---

## Decision 5: Status → presentation mapping on the run detail page

**Decision**: Map each existing run state to an existing dashboard presentation pattern using `MudAlert` severities + shared components (full table in [contracts/status-presentation-map.md](./contracts/status-presentation-map.md)):

| Run state | Presentation | Building block |
|-----------|--------------|----------------|
| Loading (initial) | Skeleton / progress | `MudSkeleton` (as `PortfolioDetail`) / `MudProgressLinear` |
| Pending / Running | Informational + progress + refresh action | `MudAlert Severity="Info"` + `MudProgressCircular` + `MudButton` refresh |
| CompletedClean | Success banner + quality table | `MudAlert Severity="Success"` + `DashboardCard` + `MudTable`/`MudSimpleTable` |
| CompletedWithFindings | Warning banner + quality table | `MudAlert Severity="Warning"` + `DashboardCard` + table |
| Failed | Error banner (message, code, guidance) | `MudAlert Severity="Error"` |
| Unavailable | Warning/empty state | `MudAlert Severity="Warning"` or `EmptyState` |

**Rationale**: BR-3 requires every existing presentation state to map to an existing pattern — "no new one-off visuals." The current page already uses `MudAlert` severities and `MudSimpleTable`; this decision keeps the semantics identical while upgrading surfaces to the shared `DashboardCard` and standard table styling (FR-007, FR-008, AC-007…AC-011).

**Alternatives considered**:
- *Custom colored banners with inline styles*: Rejected — duplicates what `MudAlert` severities already provide and risks contrast drift (NFR-004).

---

## Decision 6: Table styling for quality summary and score dimensions

**Decision**: Render the quality-check counts and scoring-dimension rows using dashboard-consistent table styling. Prefer `MudTable` with the reference header/row treatment (as `PortfolioSummaryTable`: `#1e293b` background, `#94a3b8` header text, `Hover`, `Dense`); `MudSimpleTable Hover Dense` inside a `DashboardCard` is acceptable where the data is static, provided header/density/spacing match.

**Rationale**: FR-008/AC-011 require "same section headers, spacing, table density, and alternating/hover behavior" as other dashboard tables. The existing detail page already uses `MudSimpleTable Hover Dense`; wrapping it in the shared card and aligning header styling achieves parity with minimal behavioral risk.

**Alternatives considered**:
- *Raw `<table>` HTML*: Rejected — un-themed, violates NFR-002/AC-015.

---

## Decision 7: In-progress / loading treatment

**Decision**: Reuse the existing in-flight pattern for submit buttons (`MudProgressCircular Size="Small" Indeterminate` + label + disabled button) — already present on the validation pages — and the `MudSkeleton Animation="Wave"` pattern (as `PortfolioDashboard`/`PortfolioDetail`) for initial page/data loading regions so no blank white area appears (FR-006, FR-010, AC-005, AC-014).

**Rationale**: These are the exact treatments used elsewhere; the submit-button busy state already matches, so only the initial-load skeleton needs to be added on the run detail page (it currently shows a bare `MudProgressLinear`).

**Alternatives considered**:
- *Spinner-only overlay*: Rejected — the dashboard convention is skeletons for content regions; using a different loader would break parity (AC-014).

---

## Decision 8: Alerts for rejection / upload errors

**Decision**: Keep using `MudAlert Severity="Error"` for submission rejections and oversize/upload errors (already in place), ensuring they render as inline alert components (not plain `<p>`/text strips) and sit within the themed layout (FR-011, AC-006).

**Rationale**: The current pages already use `MudAlert` for these — this requirement is largely satisfied; the plan verifies parity and ensures no plain-text error strips remain after the header/card rebuild.

---

## Decision 9: No-reload cross-page navigation on success

**Decision**: Retain `Navigation.NavigateTo($"/validation/runs/{id}", forceLoad: false)` on successful submission (already implemented on both submit pages).

**Rationale**: FR-012/AC-020 require navigation without a full page reload, matching cross-page navigation elsewhere. `forceLoad: false` is the Blazor mechanism and is already used; the plan only verifies it via a component test.

---

## Decision 10: Testing approach (test-first, component level)

**Decision**: Author bUnit component tests (xUnit + FluentAssertions + Moq, bUnit 1.35.x) for AC-001…AC-020 before/alongside the restyle, organized as in the plan's structure (Navigation, ValidationSubmit, ValidationRunDetail, ValidationPages). Mock `IValidationWebService` (as the existing `ValidationFlowTests` already does). Do not modify Domain/Application/Infrastructure; run those suites unchanged to confirm AC-016.

**Rationale**: Constitution Principle I/IV require tests derived from acceptance criteria. bUnit is already set up and pinned to 1.35.x for MudBlazor 7 (AGENTS.md gotcha). Component/E2E tests are best-effort and not run in CI, so they add verification without changing CI coverage gates.

**Testing notes / gotchas honored**:
- Register MudBlazor services in each `TestContext` (`Services.AddMudServices()`), as `ValidationFlowTests` does.
- Render `MudPopoverProvider` alongside pages that use popover-backed inputs (`MudSelect`).
- Use `FakeNavigationManager` to assert no-reload navigation (AC-020) and active-state expectations.
- Set `JSInterop.Mode = Loose` for pages invoking JS (e.g., report download).
- Assertions target structural parity: presence of the shared component/tokens (e.g., the page renders `PageHeader`/`DashboardCard`, no `<h1>`, no `Elevation="2"` light surface) rather than pixel comparison.

---

## Summary of resolved unknowns

| Unknown | Resolution |
|---------|-----------|
| Which pages/patterns are "canonical"? | MainLayout theme + Platform/Portfolio pages + KpiCard (Decision 1) |
| How to achieve structural parity without lookalike markup? | Extract `PageHeader`, `DashboardCard`, `EmptyState` shared components (Decision 2) |
| How does the nav entry highlight across the section? | Single `MudNavLink` to `/validation` with `NavLinkMatch.Prefix` (Decision 3) |
| How is compare reached? | Secondary in-section action, not a nav entry (Decision 4) |
| How do run states map to visuals? | `MudAlert` severities + shared card/table/skeleton (Decisions 5–8) |
| How is no-reload navigation done? | Existing `NavigateTo(..., forceLoad: false)` retained (Decision 9) |
| How is this verified? | Test-first bUnit component tests per AC; existing suites stay green (Decision 10) |

**All NEEDS CLARIFICATION resolved. Ready for Phase 1.**
