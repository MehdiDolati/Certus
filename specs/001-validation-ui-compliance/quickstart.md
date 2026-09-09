# Quickstart: Validation UI Compliance

**Feature**: 001-validation-ui-compliance
**Purpose**: Runnable validation guide proving the Data Validation section is reachable from primary navigation and visually consistent with the rest of the dashboard — with no behavioral regression.

> This is a **validation/run guide**, not an implementation guide. Component/API details live in [contracts/](./contracts/) and [data-model.md](./data-model.md); implementation steps live in `tasks.md` (produced by `/speckit-tasks`).

## Prerequisites

- .NET 10 SDK
- Repo restored: `dotnet restore`
- (Optional) Docker for the containerized run: `docker compose up --build` → dashboard at `http://localhost:8080`

## Build

```bash
dotnet build --configuration Release --no-restore
```

## Automated verification

### 1. Behavioral regression gate (AC-016 / NFR-005) — MUST stay green

Run the CI-order suites; none of them should change because no Domain/Application/Infrastructure code is modified:

```bash
dotnet test tests/Certus.Domain.Tests/
dotnet test tests/Certus.Application.Tests/
dotnet test tests/Certus.Infrastructure.Tests/
dotnet test tests/Certus.IntegrationTests/
dotnet test tests/Certus.ArchitectureTests/
```

**Expected**: all pass, unchanged (confirms AC-016 — no behavioral regression).

### 2. UI compliance component tests (AC-001…AC-015, AC-017, AC-019, AC-020)

```bash
dotnet test tests/Certus.ComponentTests/
```

**Expected**: the new tests referenced in the spec Test Mapping pass, and the existing `ValidationFlowTests` still pass. Key methods (see spec §Test Mapping and [contracts/presentation-components.md](./contracts/presentation-components.md)):

| Area | Test method | Proves |
|------|-------------|--------|
| Navigation | `Navigation_HasDataValidationEntry_Should_Render_And_Navigate` | AC-001, AC-002 |
| Navigation | `Navigation_RunDetail_No_History_Route_Added` | AC-018 |
| Submit | `ValidationSubmit_HeaderAndCard_Style_Matches_Dashboard` | AC-003, AC-004, AC-005 |
| Submit | `ValidationSubmit_Rejection_Shows_Alert_Style` | AC-006 |
| Submit | `ValidationSubmit_CompareOption_Visible_Inside_Section` | AC-017 |
| Submit | `ValidationSubmit_Success_Navigates_Without_Reload` | AC-020 |
| Run detail | `ValidationRunDetail_Status_Shows_Consistent_Style_Per_State` | AC-007–AC-011 |
| Run detail | `ValidationRunDetail_ExportButtons_Style_Matches_Dashboard` | AC-012 |
| Run detail | `ValidationRunDetail_Scores_Render_On_Standard_Typography` | AC-013 |
| Pages | `ValidationPages_LoadingState_Style_Matches_Dashboard` | AC-014 |
| Pages | `ValidationPages_No_Unstyled_Headings_Or_Light_Surfaces` | AC-015 |
| Pages | `ValidationPages_Shares_Dashboard_Building_Blocks` | AC-019 |

> Note: Component tests are **not** run in CI (best-effort per AGENTS.md / constitution). Run them locally to validate this feature. bUnit is pinned to 1.35.x for MudBlazor 7 compatibility.

## Manual validation scenarios

Run the app:

```bash
dotnet run --project src/Certus.Dashboard
```

Then walk these scenarios (each maps to acceptance criteria):

1. **Navigation entry (AC-001, AC-002)**: From any page (e.g., `/portfolios`), confirm a single "Data Validation" entry appears in the left nav, styled like Portfolios/Platform with an icon. Click it → lands on `/validation` and the entry shows the active highlight.
2. **Section highlight persists (FR-002b, AC-002)**: Navigate to `/validation/compare` and to a run detail URL `/validation/runs/{id}`; the nav entry stays highlighted on both.
3. **Header + card parity (AC-003, AC-015)**: On `/validation`, confirm the page uses the accent icon-tile + bold title + muted description header and a dark gradient card surface (no plain `<h1>`, no light elevated card).
4. **Compare parity + secondary action (AC-004, AC-017)**: From `/validation`, use the in-section secondary action to reach `/validation/compare` (no separate nav entry). Confirm identical header/card composition.
5. **Submit busy state (AC-005)**: Select a valid CSV and submit; the button shows a spinner + label and is disabled (duplicate submit prevented).
6. **Rejection/upload error alert (AC-006)**: Submit an invalid/oversize file; the error renders as an inline alert (not plain text).
7. **No-reload navigation (AC-020)**: On a successful submission, you are taken to `/validation/runs/{id}` without a full page reload.
8. **Run states (AC-007–AC-010)**: View run detail pages that are clean (success banner + counts table), with findings (warning banner + counts), failed (error banner with code + guidance), and pending/running (info + progress + refresh button).
9. **Tables & scores (AC-011, AC-013)**: With scoring enabled, confirm the dimension table uses dashboard table density/spacing and numeric values use the standard KPI typographic scale.
10. **Export buttons (AC-012)**: On a completed run with exports, confirm secondary-styled download buttons matching other secondary actions.
11. **Loading skeleton (AC-014)**: On first load of a validation page, confirm a skeleton/progress treatment appears (no blank white region).
12. **Structural parity sweep (AC-019, AC-015)**: Compare all three validation pages against Portfolios/Platform — confirm the same shared building blocks and no browser-default headings or light surfaces remain.

## Success definition

- All CI-order suites pass unchanged (AC-016).
- All component tests in the Test Mapping pass.
- Manual scenarios 1–12 behave as described.
- Routes remain exactly `/validation`, `/validation/compare`, `/validation/runs/{RunId}` with no new route (AC-018).

## References

- Design decisions & tokens: [research.md](./research.md)
- Component/nav/route/state contracts: [contracts/](./contracts/)
- Presentation entities & read-only view state: [data-model.md](./data-model.md)
- Acceptance criteria & test mapping: [spec.md](./spec.md)
