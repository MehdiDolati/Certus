# Phase 1 Data Model: Validation UI Compliance

**Feature**: 001-validation-ui-compliance
**Date**: 2026-09-09
**Input**: [plan.md](./plan.md), [research.md](./research.md)

## Scope note

This is a **presentation + navigation** feature. It introduces **no new domain entities, no persistence, and no changes to `Validator.*` data contracts**. The "entities" below are UI/presentation constructs: reusable component contracts, the navigation model, and the (read-only) run-view state the pages already consume. Behavioral data (run records, summaries, scores, diagnostics) is owned by the `Validator.*` application layer and is **rendered, never modified**.

---

## Presentation Components (new, in `Components/Shared/`)

### PageHeader

The standard page-header composition (icon tile + title + description) reused by all three validation pages.

| Parameter | Type | Required | Default | Notes |
|-----------|------|----------|---------|-------|
| `Title` | `string` | Yes | — | Bold page title (`Typo.h4`, white, weight 700–800). |
| `Description` | `string?` | No | `null` | Muted supporting line (`#64748b`). Hidden when null/empty. |
| `Icon` | `string` | Yes | — | Material icon rendered inside the accent tile. |
| `IconGradient` | `string` | No | primary→secondary gradient | Tile background gradient (defaults match dashboard). |
| `Actions` | `RenderFragment?` | No | `null` | Optional right-aligned action slot (e.g., secondary "Compare" action). |

**Validation rules**: `Title` and `Icon` must be non-empty. Renders no `<h1>` (uses `MudText Typo.h4`) so NFR-002/AC-015 hold.

**Relationships**: Consumed by `ValidationSubmit`, `ValidationCompare`, `ValidationRunDetail`. Mirrors the `PlatformDashboard` header composition.

---

### DashboardCard

The dark gradient content surface that replaces `MudPaper Elevation="2"` on the validation pages.

| Parameter | Type | Required | Default | Notes |
|-----------|------|----------|---------|-------|
| `ChildContent` | `RenderFragment` | Yes | — | Card body content. |
| `Class` | `string?` | No | `null` | Pass-through spacing utility classes (e.g., `pa-6 mb-4`). |
| `Padding` | `string?` | No | `pa-6` | Inner padding utility. |

**Validation rules**: Must render with the canonical surface tokens (gradient `#1e293b→#0f172a`, `1px solid #334155`, `border-radius: 16px`). Must NOT use MudBlazor default light elevation (`Elevation` > 0 light surface) — satisfies NFR-002/AC-015.

**Relationships**: Wraps the submission forms, the quality-summary section, the score section, and the download section.

---

### EmptyState

Icon-tile + heading + supporting text (+ optional action) for empty/unavailable/not-found regions.

| Parameter | Type | Required | Default | Notes |
|-----------|------|----------|---------|-------|
| `Icon` | `string` | Yes | — | Material icon inside a muted tile. |
| `Title` | `string` | Yes | — | Heading (`Typo.h6`, `#94a3b8`). |
| `Description` | `string?` | No | `null` | Supporting text (`#64748b`/`#475569`). |
| `Action` | `RenderFragment?` | No | `null` | Optional action button slot. |

**Relationships**: Used for the "unavailable run" state on the detail page (Decision 5) and any empty region. Mirrors the "No platform connections" / "Portfolio not found" patterns.

---

## Navigation Model (edit to `NavMenu.razor`)

### NavEntry: Data Validation

| Attribute | Value | Requirement |
|-----------|-------|-------------|
| Label | `Data Validation` | FR-001, AC-001 |
| Href | `/validation` | FR-001 |
| Match | `NavLinkMatch.Prefix` | FR-002/FR-002b/AC-002 (highlights across `/validation`, `/validation/compare`, `/validation/runs/{RunId}`) |
| Icon | Section-consistent Material icon (e.g., `FactCheck`) | FR-002, AC-001 |
| Style / ActiveStyle | Same pattern as existing Portfolios/Platform links | FR-002, AC-001, AC-002 |
| Placement | Same nav area as other primary sections | FR-002 |

**State transitions (active highlight)**:

```
route starts with "/validation"  → nav entry = ACTIVE (selected style)
otherwise                         → nav entry = INACTIVE (default style)
```

**Invariant**: Exactly **one** Data Validation nav entry exists; no compare entry, no run-history entry (FR-002a, FR-002b, AC-018).

---

## Read-only View State (existing — rendered, not modified)

These types are owned by `Validator.Application.Web` and are consumed by the pages exactly as today. Listed for completeness so tests know what to project.

### Run lifecycle state → presentation (see [contracts/status-presentation-map.md](./contracts/status-presentation-map.md))

`WebRunStatus` values the detail page renders:

| Value | Meaning | Presentation (unchanged semantics) |
|-------|---------|------------------------------------|
| `Pending` | Queued | Info + progress + refresh |
| `Running` | In progress | Info + progress + refresh |
| `CompletedClean` | Done, no findings | Success banner + quality table |
| `CompletedWithFindings` | Done, findings present | Warning banner + quality table |
| `Failed` | Terminal failure | Error banner (code + reason + guidance) |
| *(Unavailable result)* | Unknown/expired run | Warning / EmptyState |

### Quality summary (rendered in a dashboard-styled table — FR-008)

`DetailedSummary` fields projected as rows: `MissingCandles`, `DuplicateRecords`, `InvalidOhlc`, `ClosedMarketRecords`, `TimeGaps`, `MalformedRows`.

### Scoring section (rendered in a dashboard-styled table — FR-008, AC-013)

Dataset average + per-dimension rows (`Category`, `State`, `Score`) from `WebScoringSection` / `ScoreDisplay`. Numeric values render on the same typographic scale as other KPI/numeric values.

### Diagnostics (rendered in an error alert — FR-007, AC-009)

`FatalDiagnostic` fields: `Code`, `Reason`, `Guidance`.

### Available exports (rendered as secondary action buttons — FR-009, AC-012)

`IReadOnlyList<ReportRepresentation>` → one secondary-styled `MudButton` per representation.

**Data invariant (NFR-005 / BR-2 / AC-016)**: No field above is recomputed, reformatted beyond existing helpers, or altered. The feature only changes the **surfaces and composition** that display them.

---

## Entity relationship summary

```
NavMenu ──(1 entry, prefix match)──> /validation section
                                         │
        ┌────────────────────────────────┼─────────────────────────────┐
        ▼                                 ▼                             ▼
ValidationSubmit                 ValidationCompare              ValidationRunDetail
  uses PageHeader                  uses PageHeader                uses PageHeader
  uses DashboardCard               uses DashboardCard             uses DashboardCard (summary/score/exports)
  uses MudAlert (errors)           uses MudAlert (errors)         uses MudAlert (status: info/success/warning/error)
  secondary action → compare       secondary action → validate    uses EmptyState (unavailable)
  renders IValidationWebService    renders IValidationWebService   uses MudSkeleton (loading)
  submit → NavigateTo(forceLoad:false)                            renders DetailedSummary / WebScoringSection / FatalDiagnostic
```

No new fields, tables, migrations, or DTO changes are introduced by this feature.
