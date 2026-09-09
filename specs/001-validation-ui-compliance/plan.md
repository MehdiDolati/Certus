# Implementation Plan: Validation UI Compliance

**Branch**: `001-validation-ui-compliance` | **Date**: 2026-09-09 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/001-validation-ui-compliance/spec.md`

**Note**: This template is filled in by the `/speckit-plan` command; its definition describes the execution workflow.

## Summary

Make the Data Validation workflow reachable from the application's primary navigation and align its three pages (`/validation`, `/validation/compare`, `/validation/runs/{RunId}`) with the dashboard's shared visual design language — **without changing any validation behavior or existing run URLs**.

Technical approach: The validation pages are Blazor Server Razor components that currently use un-themed primitives (`<h1>`, plain `<p>`, `MudPaper Elevation="2"` light surfaces). They will be rebuilt on the same shared presentation building blocks used by the other dashboard pages (page-header composition, dark gradient content cards, MudTable-based tables, MudAlert status treatments, MudProgress/MudSkeleton indicators) and the dark palette tokens defined in `MainLayout`'s `MudTheme`. A single "Data Validation" entry is added to `NavMenu` with prefix-based active highlighting across the section. To guarantee **structural parity** (not lookalike markup), reusable presentation components (`PageHeader`, `DashboardCard`, `EmptyState`) are extracted into `Components/Shared/` and consumed by the validation pages. No Domain/Application/Infrastructure code changes; all existing suites stay green (AC-016).

## Technical Context

**Language/Version**: C# / .NET 10

**Primary Dependencies**: Blazor Server (`Microsoft.NET.Sdk.Web`), MudBlazor 7.x, Plotly.Blazor 5.x (unused by this feature), `Validator.*` application/domain services (referenced, not modified)

**Storage**: N/A for this feature (presentation + navigation only). Existing validation run persistence (file-backed `AppData/data-validation/` via `Validator.Infrastructure`) is untouched.

**Testing**: bUnit 1.35.x + xUnit + FluentAssertions + Moq (component tests in `tests/Certus.ComponentTests`). Domain/Application/Infrastructure/Architecture suites must remain green.

**Target Platform**: Web — Blazor Server (Kestrel), containerized via Docker (`docker compose up`, dashboard at `http://localhost:8080`)

**Project Type**: Web application (single runnable project: `src/Certus.Dashboard`)

**Performance Goals**: No performance regression. Cross-page navigation on successful submission occurs without a full page reload (`NavigationManager.NavigateTo(..., forceLoad: false)`), matching the rest of the app.

**Constraints**:
- Existing routes MUST remain unchanged (`/validation`, `/validation/compare`, `/validation/runs/{RunId}`); no new routes/pages.
- No behavioral change to validation rules, scoring, or report semantics (NFR-005, BR-2).
- WCAG AA contrast parity with the rest of the app (NFR-004).
- Responsive with the same grid/breakpoint behavior; no horizontal overflow (NFR-003).
- Blazor Server: run-detail page shares a scoped `CertusDbContext`; awaits MUST remain sequential (no `Task.WhenAll`) — the existing sequential `GetStatusAsync` → `GetResultAsync` pattern is preserved.
- No new one-off visual markup; reuse shared building blocks (FR-013, BR-3, AC-019).

**Scale/Scope**: 3 pages restyled + 1 navigation entry added + up to 3 extracted shared presentation components. Confined entirely to the Dashboard (presentation) layer.

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

Gates derived from `.specify/memory/constitution.md` (v1.0.0):

| # | Principle / Constraint | Status | Notes |
|---|------------------------|--------|-------|
| I | Spec-Driven Development (NON-NEGOTIABLE) | ✅ PASS | Spec exists with AC-001…AC-020 and a Test Mapping table. Component tests will be written from the ACs before the restyle. |
| II | Clean Architecture & Dependency Rules (NON-NEGOTIABLE) | ✅ PASS | Changes confined to `Certus.Dashboard` (Razor pages + `Components/Shared` + `NavMenu`). No new cross-layer references; no bounded-context coupling. NetArchTest rules unaffected. |
| III | Research Before Trading | ➖ N/A | No trading strategy/hypothesis involved (UI presentation change). |
| IV | Test-First with 100% Coverage Gates | ✅ PASS | Domain/Application/Infrastructure code is untouched → their 100% coverage is preserved (AC-016, NFR-005). New verification is component-level (bUnit), authored test-first from ACs. Per constitution §Development Workflow (7) + AGENTS.md, component tests are best-effort and NOT run in CI; they do not add CI coverage obligations. |
| V | Progressive Autonomy & Transparency | ➖ N/A | No autonomy or black-box behavior introduced. |
| Arch | DI wiring via composition root | ✅ PASS | No new DI services required; extracted items are Razor components, not injected services. |
| Arch | Blazor sequential awaits on scoped DbContext | ✅ PASS | Existing sequential `GetStatusAsync` → `GetResultAsync` on run detail is retained; no `Task.WhenAll` introduced. |
| Arch | bUnit pinned to 1.35.x | ✅ PASS | `tests/Certus.ComponentTests` already references `bunit 1.35.*`. |
| Arch | Cross-platform path handling | ➖ N/A | No filesystem path manipulation in this feature. |

**Result**: PASS — no violations. Complexity Tracking table is intentionally empty.

### Post-Design Re-evaluation (after Phase 1)

Re-checked after generating `research.md`, `data-model.md`, `contracts/*`, and `quickstart.md`:

- **Principle II (Clean Architecture)**: The design adds only Razor presentation components (`PageHeader`, `DashboardCard`, `EmptyState`) in `Certus.Dashboard/Components/Shared/` and edits existing Dashboard pages + `NavMenu`. No new project, no new cross-layer or bounded-context reference — NetArchTest rules remain satisfied. ✅
- **Principle I & IV (Spec/Test-First)**: Contracts map every AC to a named component test method; `quickstart.md` gates on the unchanged CI-order suites (AC-016). ✅
- **Arch constraints**: Sequential-await pattern on the run-detail page is preserved (no `Task.WhenAll`); bUnit stays 1.35.x; no filesystem path handling introduced. ✅
- **Complexity**: No new complexity introduced; extracting 3 shared components reduces duplication rather than adding it.

**Post-design result**: PASS — no new violations; Complexity Tracking remains empty.


## Project Structure

### Documentation (this feature)

```text
specs/001-validation-ui-compliance/
├── plan.md              # This file (/speckit-plan command output)
├── research.md          # Phase 0 output (/speckit-plan command)
├── data-model.md        # Phase 1 output (/speckit-plan command)
├── quickstart.md        # Phase 1 output (/speckit-plan command)
├── contracts/           # Phase 1 output (/speckit-plan command)
│   ├── routes.md
│   ├── navigation.md
│   ├── presentation-components.md
│   └── status-presentation-map.md
├── checklists/
│   └── requirements.md  # Pre-existing spec quality checklist
├── spec.md              # Feature specification (input)
└── tasks.md             # Phase 2 output (/speckit-tasks command - NOT created by /speckit-plan)
```

### Source Code (repository root)

This is a Blazor Server web application. All changes are confined to the presentation layer in `src/Certus.Dashboard`, with verification in `tests/Certus.ComponentTests`.

```text
src/Certus.Dashboard/
├── Components/
│   ├── Layout/
│   │   └── NavMenu.razor                 # EDIT: add single "Data Validation" nav entry (prefix-active on /validation)
│   ├── Pages/
│   │   ├── ValidationSubmit.razor        # EDIT: rebuild on shared header/card/alert/progress building blocks
│   │   ├── ValidationCompare.razor       # EDIT: rebuild on shared header/card/alert/progress building blocks
│   │   └── ValidationRunDetail.razor     # EDIT: shared header + status alerts + dashboard tables + skeleton
│   └── Shared/
│       ├── PageHeader.razor              # NEW: reusable icon-tile + title + description header block
│       ├── DashboardCard.razor           # NEW: reusable dark gradient content surface (replaces Elevation="2")
│       └── EmptyState.razor              # NEW: reusable empty/unavailable state (icon tile + text [+ action])
└── (existing reference pages unchanged: PlatformDashboard.razor, PortfolioDashboard.razor, PortfolioDetail.razor, Shared/KpiCard.razor, Tables/*)

tests/Certus.ComponentTests/
├── Navigation/
│   └── NavigationTests.cs                # NEW: AC-001, AC-002, AC-018
├── Validation/
│   ├── ValidationSubmitTests.cs          # NEW: AC-003, AC-004, AC-005, AC-006, AC-017, AC-020
│   ├── ValidationRunDetailTests.cs       # NEW: AC-007…AC-013
│   └── ValidationPagesTests.cs           # NEW: AC-014, AC-015, AC-019
└── ValidationFlowTests.cs                # EXISTING: behavior smoke tests remain green
```

**Structure Decision**: Single-project web application. The feature touches only `src/Certus.Dashboard` (presentation) and `tests/Certus.ComponentTests` (verification). Shared presentation building blocks are extracted into `src/Certus.Dashboard/Components/Shared/` so the validation pages consume the *same* primitives as the rest of the dashboard, satisfying structural parity (FR-013 / AC-019) rather than producing lookalike one-off markup. No Domain/Application/Infrastructure projects are modified.

## Complexity Tracking

> No Constitution Check violations — this section is intentionally empty.

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| — | — | — |
