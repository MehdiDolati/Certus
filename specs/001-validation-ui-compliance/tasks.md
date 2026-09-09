---
description: "Task list for feature implementation: Validation UI Compliance"
---

# Tasks: Validation UI Compliance

**Input**: Design documents from `/specs/001-validation-ui-compliance/`

**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Tests**: Test tasks ARE included. This is a spec-driven, test-first project (Constitution Principle I & IV; research.md Decision 10). Component tests are authored from acceptance criteria (bUnit 1.35.x) **before** the restyle. Per AGENTS.md/constitution, component tests are best-effort and NOT run in CI — they verify this feature locally without changing CI coverage gates.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing. Each user story maps to exactly one component-test file per the spec §Test Mapping.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (US1, US2, US3, US4)
- Include exact file paths in descriptions

## Path Conventions

Single-project Blazor Server web application. All source changes are confined to the presentation layer `src/Certus.Dashboard/`; all verification lives in `tests/Certus.ComponentTests/`. No Domain/Application/Infrastructure code is modified (AC-016, NFR-005).

## Tech Stack (from plan.md)

- **Language/Runtime**: C# / .NET 10
- **UI**: Blazor Server (`Microsoft.NET.Sdk.Web`), MudBlazor 7.x
- **Testing**: bUnit **1.35.x** (pinned for MudBlazor 7 — do NOT upgrade to 2.7.x) + xUnit + FluentAssertions + Moq
- **Design tokens (canonical, from research.md)**: Primary `#3b82f6`, Secondary `#8b5cf6`, Success `#00ff88`, Warning `#f59e0b`, Error `#ff4466`, Surface `#1e293b`, TextSecondary `#94a3b8`, muted `#64748b`/`#475569`; card gradient `linear-gradient(135deg, #1e293b 0%, #0f172a 100%)`, card border `1px solid #334155`, card radius `16px`, icon tile `44px`/radius `12px`

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Test-project scaffolding and shared test infrastructure for the feature

- [ ] T001 Create the ComponentTests folder structure for this feature: `tests/Certus.ComponentTests/Navigation/` and `tests/Certus.ComponentTests/Validation/` (per plan.md §Project Structure)
- [ ] T002 [P] Verify `tests/Certus.ComponentTests/Certus.ComponentTests.csproj` references bUnit **1.35.x** (NOT 2.7.x), xUnit, FluentAssertions, and Moq; pin bUnit to `1.35.*` per AGENTS.md gotcha (MudBlazor 7 compatibility)
- [ ] T003 [P] Create a reusable bUnit test helper in `tests/Certus.ComponentTests/Validation/ValidationTestContextExtensions.cs` that registers MudBlazor services (`Services.AddMudServices()`), sets `JSInterop.Mode = Loose`, renders a `MudPopoverProvider`, and exposes a `Mock<IValidationWebService>` builder — mirroring the existing pattern in `tests/Certus.ComponentTests/ValidationFlowTests.cs`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Extract the shared presentation building blocks that the restyle stories (US2, US3, US4) rebuild upon (research.md Decision 2; contracts/presentation-components.md). These are the "same shared building blocks" that guarantee structural parity (FR-013, AC-019).

**⚠️ CRITICAL**: US2, US3, and US4 cannot be implemented until these components exist. (US1/navigation does not consume these components and is independent — see Dependencies.)

- [ ] T004 [P] Create `src/Certus.Dashboard/Components/Shared/PageHeader.razor` with parameters: `Title` (string, **Required**) — "Bold page title (`Typo.h4`, white, weight 700–800)"; `Description` (string?, optional, default `null`) — "Muted supporting line (`#64748b`). Hidden when null/empty."; `Icon` (string, **Required**) — Material icon inside the accent tile; `IconGradient` (string, optional, default `linear-gradient(135deg, #3b82f6, #8b5cf6)`); `Actions` (`RenderFragment?`, optional, default `null`) — right-aligned action slot. Validation rules: "`Title` and `Icon` must be non-empty." Invariant **PH-1**: "Emits no `<h1>` and no browser-default heading" (render via `MudText Typo="Typo.h4"`). Invariant **PH-2**: "44px tile, radius 12px, white bold title, muted description" (mirror `Components/Pages/PlatformDashboard.razor` header)
- [ ] T005 [P] Create `src/Certus.Dashboard/Components/Shared/DashboardCard.razor` with parameters: `ChildContent` (`RenderFragment`, **Required**) — card body; `Class` (string?, optional, default `null`) — pass-through spacing utilities; `Padding` (string?, optional, default `pa-6`). Invariant **DC-1**: renders canonical surface "gradient `#1e293b→#0f172a`, border `1px solid #334155`, radius `16px`". Invariant **DC-2**: "MUST NOT render a MudBlazor default light elevated surface (`MudPaper Elevation="2"` light look)" (mirror `Components/Shared/KpiCard.razor`)
- [ ] T006 [P] Create `src/Certus.Dashboard/Components/Shared/EmptyState.razor` with parameters: `Icon` (string, **Required**) — Material icon inside a muted tile; `Title` (string, **Required**) — "`Typo.h6`, `#94a3b8`"; `Description` (string?, optional, default `null`) — "`#64748b`/`#475569`"; `Action` (`RenderFragment?`, optional, default `null`). Invariant **ES-1**: "same empty-state composition as the reference pages ('No platform connections' / 'Portfolio not found')"

**Checkpoint**: Shared building blocks ready — US2/US3/US4 restyle work can now proceed in parallel.

---

## Phase 3: User Story 1 - Reach Data Validation from primary navigation (Priority: P1) 🎯 MVP

**Goal**: A single "Data Validation" entry in the primary navigation lets a user reach the section from any page, styled identically to the other sections, and it stays highlighted across the whole section (`/validation`, `/validation/compare`, `/validation/runs/{RunId}`). No new routes are introduced.

**Independent Test**: Render `NavMenu` (and route to each validation URL); confirm exactly one "Data Validation" nav link to `/validation`, click navigates to `/validation`, prefix-active highlight applies on all three section routes, and no run-history route/entry exists.

**Maps to**: FR-001, FR-002, FR-002a, FR-002b; contracts/navigation.md; AC-001, AC-002, AC-018

### Tests for User Story 1 ⚠️ (write FIRST, ensure they FAIL before implementation)

- [ ] T007 [P] [US1] Write `Navigation_HasDataValidationEntry_Should_Render_And_Navigate` in `tests/Certus.ComponentTests/Navigation/NavigationTests.cs` (AC-001, AC-002): assert exactly **one** "Data Validation" `MudNavLink` with `Href="/validation"`, a section icon, and the same `Style`/`ActiveStyle` as the Portfolios/Platform links; using `FakeNavigationManager` assert clicking navigates to `/validation` and the entry is active (prefix) on `/validation`, `/validation/compare`, and `/validation/runs/{RunId}`
- [ ] T008 [P] [US1] Write `Navigation_RunDetail_No_History_Route_Added` in `tests/Certus.ComponentTests/Navigation/NavigationTests.cs` (AC-018): assert no second top-level compare entry (INV-N1) and no run-history nav entry/route (INV-N2); only the three existing routes exist

### Implementation for User Story 1

- [ ] T009 [US1] Add a single "Data Validation" `MudNavLink` to `src/Certus.Dashboard/Components/Layout/NavMenu.razor`: `Href="/validation"`, `Match="NavLinkMatch.Prefix"`, `Icon="@Icons.Material.Filled.FactCheck"`, placed in the same `MudNavMenu` as the others, with `Style="color: #94a3b8; border-radius: 10px; margin-bottom: 4px;"` and `ActiveStyle="color: white; background: rgba(59, 130, 246, 0.12);"` identical to the existing Portfolios link. Exactly one entry; add no compare and no run-history entry (INV-N1, INV-N2, INV-N4)

**Checkpoint**: US1 fully functional and independently testable — the Data Validation section is reachable and highlighted from the primary nav. Shippable as MVP.

---

## Phase 4: User Story 2 - Consistent submission experience (Priority: P2)

**Goal**: The validation and compare submission pages are rebuilt on the shared building blocks — standard page header, dark gradient card surface, standard controls/busy state, inline error alerts, in-section compare/validate secondary action, and no-reload navigation on success — matching the rest of the dashboard.

**Independent Test**: Render `/validation` and `/validation/compare` with a mocked `IValidationWebService`; confirm both use `PageHeader` + `DashboardCard`, the submit busy state shows spinner + label with the button disabled (duplicate submit prevented), rejection/oversize errors render as `MudAlert Severity="Error"`, the compare/validate secondary action is present in-section, and a successful submission navigates to `/validation/runs/{id}` without a full reload.

**Maps to**: FR-002a, FR-003, FR-004, FR-005, FR-006, FR-011, FR-012; contracts/presentation-components.md (ValidationSubmit/ValidationCompare), status-presentation-map.md (U1–U5); AC-003, AC-004, AC-005, AC-006, AC-017, AC-020

### Tests for User Story 2 ⚠️ (write FIRST, ensure they FAIL before implementation)

- [ ] T010 [P] [US2] Write `ValidationSubmit_HeaderAndCard_Style_Matches_Dashboard` in `tests/Certus.ComponentTests/Validation/ValidationSubmitTests.cs` (AC-003, AC-004, AC-005): assert the page renders shared `PageHeader` + `DashboardCard`; on submit the button shows `MudProgressCircular Size="Small" Indeterminate` + label and is disabled (U2), preventing duplicate submission
- [ ] T011 [P] [US2] Write `ValidationSubmit_Rejection_Shows_Alert_Style` in `tests/Certus.ComponentTests/Validation/ValidationSubmitTests.cs` (AC-006): a `WebRunSubmission.Rejected` result and an oversize/upload error each render a `MudAlert Severity="Error"` (Code — Reason — Guidance) rather than plain text (U3, U4)
- [ ] T012 [P] [US2] Write `ValidationSubmit_CompareOption_Visible_Inside_Section` in `tests/Certus.ComponentTests/Validation/ValidationSubmitTests.cs` (AC-017): assert a secondary in-section action to `/validation/compare` is present and there is no separate top-level nav entry (INV-N1)
- [ ] T013 [P] [US2] Write `ValidationSubmit_Success_Navigates_Without_Reload` in `tests/Certus.ComponentTests/Validation/ValidationSubmitTests.cs` (AC-020): with `FakeNavigationManager`, a `WebRunSubmission.Accepted` result triggers `NavigateTo("/validation/runs/{id}", forceLoad: false)` (U5, INV-R4)

### Implementation for User Story 2

- [ ] T014 [US2] In `src/Certus.Dashboard/Components/Pages/ValidationSubmit.razor`, replace the `<h1>`/plain `<p>` header with the shared `PageHeader` (Title, Description, Icon) — no `<h1>` remains (FR-003, PH-1)
- [ ] T015 [US2] In `src/Certus.Dashboard/Components/Pages/ValidationSubmit.razor`, replace the `MudPaper Elevation="2"` form surface with the shared `DashboardCard`; keep file/timeframe/instrument/checkbox/button controls as standard MudBlazor controls with the same density/states (FR-004, FR-005, U1)
- [ ] T016 [US2] In `src/Certus.Dashboard/Components/Pages/ValidationSubmit.razor`, add the compare secondary action (via `PageHeader.Actions` or an in-card `MudButton`/link) navigating to `/validation/compare` (FR-002a, AC-017)
- [ ] T017 [US2] In `src/Certus.Dashboard/Components/Pages/ValidationSubmit.razor`, ensure the submit busy state uses `MudProgressCircular Size="Small" Indeterminate` + label with a disabled button and duplicate-submit prevention while `IsSubmitting == true` (FR-006, AC-005, U2)
- [ ] T018 [US2] In `src/Certus.Dashboard/Components/Pages/ValidationSubmit.razor`, ensure rejection and upload/oversize errors render via `MudAlert Severity="Error"` (Code — Reason — Guidance), with no plain-text strips remaining (FR-011, AC-006, U3, U4)
- [ ] T019 [US2] In `src/Certus.Dashboard/Components/Pages/ValidationSubmit.razor`, preserve `NavigationManager.NavigateTo($"/validation/runs/{id}", forceLoad: false)` on an accepted submission (FR-012, AC-020, U5, INV-R4)
- [ ] T020 [P] [US2] Apply the same rebuild to `src/Certus.Dashboard/Components/Pages/ValidationCompare.razor`: identical `PageHeader` + `DashboardCard` composition, standard controls, busy state, `MudAlert Severity="Error"` errors, no-reload success navigation, and a reciprocal secondary action back to `/validation` (FR-002a, FR-003, FR-004, FR-005, FR-006, FR-011, FR-012; AC-004)

**Checkpoint**: US1 and US2 both work independently — both submission pages match the dashboard and route without full reload.

---

## Phase 5: User Story 3 - Consistent run-detail results experience (Priority: P3)

**Goal**: The run-detail page renders every run state through the standard dashboard presentation: status banners (info/success/warning/error), quality-summary and scoring tables inside dashboard cards, secondary-styled export buttons, a loading skeleton, and an empty/unavailable state — with unchanged behavior and the existing sequential `GetStatusAsync` → `GetResultAsync` pattern preserved.

**Independent Test**: Render `/validation/runs/{RunId}` with a mocked `IValidationWebService` across states; confirm Pending/Running → `MudAlert Severity="Info"` + progress + refresh, CompletedClean → Success banner + quality table, CompletedWithFindings → Warning banner + all counts, Failed → Error banner (code/reason/guidance), Unavailable → Warning/`EmptyState`; scoring renders in a dashboard table on the standard KPI typographic scale; exports render as secondary buttons.

**Maps to**: FR-003, FR-007, FR-008, FR-009, FR-010; contracts/status-presentation-map.md (S1–S9); AC-007, AC-008, AC-009, AC-010, AC-011, AC-012, AC-013

### Tests for User Story 3 ⚠️ (write FIRST, ensure they FAIL before implementation)

- [ ] T021 [P] [US3] Write `ValidationRunDetail_Status_Shows_Consistent_Style_Per_State` in `tests/Certus.ComponentTests/Validation/ValidationRunDetailTests.cs` (AC-007–AC-011): parametrize `WebRunStatus` → Pending/Running render `MudAlert Severity="Info"` + `MudProgressCircular` + Refresh `MudButton` (S2/S3, AC-010); `CompletedClean` → `MudAlert Severity="Success"` + quality table (S4, AC-007); `CompletedWithFindings` → `MudAlert Severity="Warning"` with all counts visible (S5, AC-008); `Failed` → `MudAlert Severity="Error"` with Code/Reason/Guidance (S6, AC-009); Unavailable → `MudAlert Severity="Warning"`/`EmptyState` (S7)
- [ ] T022 [P] [US3] Write `ValidationRunDetail_ExportButtons_Style_Matches_Dashboard` in `tests/Certus.ComponentTests/Validation/ValidationRunDetailTests.cs` (AC-012): when `View.AvailableExports.Count > 0`, one secondary `MudButton Variant="Outlined"` renders per `ReportRepresentation` (S9)
- [ ] T023 [P] [US3] Write `ValidationRunDetail_Scores_Render_On_Standard_Typography` in `tests/Certus.ComponentTests/Validation/ValidationRunDetailTests.cs` (AC-013, AC-011): the scoring section renders dataset average + per-dimension `Category`/`State`/`Score` rows in a dashboard-styled table, numbers on the standard KPI typographic scale (S8)

### Implementation for User Story 3

- [ ] T024 [US3] In `src/Certus.Dashboard/Components/Pages/ValidationRunDetail.razor`, add the shared `PageHeader` (remove any `<h1>`/plain header) (FR-003, PH-1)
- [ ] T025 [US3] In `src/Certus.Dashboard/Components/Pages/ValidationRunDetail.razor`, implement status banners per the status→presentation map, preserving state→severity mapping (SP-2): Pending/Running → `MudAlert Severity="Info"` + `MudProgressCircular` + Refresh button (S2/S3, AC-010); CompletedClean → `MudAlert Severity="Success"` (S4, AC-007); CompletedWithFindings → `MudAlert Severity="Warning"` (S5, AC-008); Failed → `MudAlert Severity="Error"` with `FatalDiagnostic` Code/Reason/Guidance (S6, AC-009). Keep the sequential `GetStatusAsync` → `GetResultAsync` awaits (NO `Task.WhenAll`)
- [ ] T026 [US3] In `src/Certus.Dashboard/Components/Pages/ValidationRunDetail.razor`, render the `DetailedSummary` counts (`MissingCandles`, `DuplicateRecords`, `InvalidOhlc`, `ClosedMarketRecords`, `TimeGaps`, `MalformedRows`) inside a `DashboardCard` using a dashboard-styled table (`MudTable`/`MudSimpleTable Hover Dense`, `#1e293b` bg, `#94a3b8` header) — all counts visible (FR-008, AC-011, SP-3, S4/S5)
- [ ] T027 [US3] In `src/Certus.Dashboard/Components/Pages/ValidationRunDetail.razor`, render the scoring section (dataset average + per-dimension `Category`/`State`/`Score` from `WebScoringSection`/`ScoreDisplay`) inside a `DashboardCard` + dashboard table; numeric values on the standard KPI typographic scale (FR-008, AC-013, S8)
- [ ] T028 [US3] In `src/Certus.Dashboard/Components/Pages/ValidationRunDetail.razor`, render available exports (`IReadOnlyList<ReportRepresentation>`) as one secondary `MudButton Variant="Outlined"` per representation; keep the existing JS-interop download behavior unchanged (FR-009, AC-012, S9)
- [ ] T029 [US3] In `src/Certus.Dashboard/Components/Pages/ValidationRunDetail.razor`, replace the bare `MudProgressLinear` initial-load indicator with `MudSkeleton Animation="Wave"` so no blank white region appears on first render (FR-010, AC-014, S1)
- [ ] T030 [US3] In `src/Certus.Dashboard/Components/Pages/ValidationRunDetail.razor`, render the Unavailable/unknown/expired/invalid-id state via `MudAlert Severity="Warning"` or the shared `EmptyState` (no crash); preserve `WebRunId.Parse` behavior (FR-007, S7, INV-R3)

**Checkpoint**: US1, US2, and US3 all work independently — all three validation pages match the dashboard presentation.

---

## Phase 6: User Story 4 - Structural parity & legacy-free sweep (Priority: P4)

**Goal**: Guarantee that all three validation pages share the same building blocks as the reference pages, contain no browser-default/legacy elements, and remain responsive with WCAG AA contrast — verified as a cross-page sweep.

**Independent Test**: Render all three validation pages and assert none contain `<h1>`, plain paragraph strips, or `MudPaper Elevation="2"` light surfaces; each renders the shared `PageHeader`/`DashboardCard` (and `EmptyState` where applicable); first render shows a skeleton/progress treatment (no blank region).

**Maps to**: FR-010, FR-013, NFR-001, NFR-002, NFR-003, NFR-004; contracts/presentation-components.md (G-1…G-5); AC-014, AC-015, AC-019

### Tests for User Story 4 ⚠️ (write FIRST, ensure they FAIL before implementation)

- [ ] T031 [P] [US4] Write `ValidationPages_LoadingState_Style_Matches_Dashboard` in `tests/Certus.ComponentTests/Validation/ValidationPagesTests.cs` (AC-014): on first render, a validation page shows the skeleton/progress treatment (e.g., `MudSkeleton`) with no blank white region (S1)
- [ ] T032 [P] [US4] Write `ValidationPages_No_Unstyled_Headings_Or_Light_Surfaces` in `tests/Certus.ComponentTests/Validation/ValidationPagesTests.cs` (AC-015): assert none of the three pages render an `<h1>`, plain paragraph text strips, or a `MudPaper Elevation="2"` light elevated surface (NFR-002, G-2)
- [ ] T033 [P] [US4] Write `ValidationPages_Shares_Dashboard_Building_Blocks` in `tests/Certus.ComponentTests/Validation/ValidationPagesTests.cs` (AC-019): assert each page renders the shared `PageHeader`/`DashboardCard`/`EmptyState` components (structural parity), not one-off lookalike markup (FR-013, G-1)

### Implementation for User Story 4

- [ ] T034 [US4] Cross-page audit and cleanup across `src/Certus.Dashboard/Components/Pages/ValidationSubmit.razor`, `ValidationCompare.razor`, and `ValidationRunDetail.razor`: remove any residual `<h1>`, plain paragraph strips, or default light elevated surfaces and replace with the shared building blocks (NFR-002, G-2, AC-015)
- [ ] T035 [US4] Verify responsive layout across the three validation pages: control rows align to the standard responsive grid/breakpoints with no horizontal overflow at supported widths (NFR-003, G-3)
- [ ] T036 [US4] Verify WCAG AA contrast parity across the three validation pages using the shared design tokens (no custom colors introduced) (NFR-004, G-4)

**Checkpoint**: All validation pages are structurally identical to the dashboard in composition and contain no legacy/un-themed elements.

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Behavioral-regression gates, full verification, and documentation

- [ ] T037 [P] Behavioral regression gate (AC-016, NFR-005): run the CI-order suites — `dotnet test tests/Certus.Domain.Tests/`, `tests/Certus.Application.Tests/`, `tests/Certus.Infrastructure.Tests/`, `tests/Certus.IntegrationTests/`, `tests/Certus.ArchitectureTests/` — and confirm all pass unchanged
- [ ] T038 [P] Run `dotnet test tests/Certus.ComponentTests/` and confirm all new feature tests pass and the existing `tests/Certus.ComponentTests/ValidationFlowTests.cs` remains green
- [ ] T039 Execute the manual validation scenarios 1–12 in `specs/001-validation-ui-compliance/quickstart.md` against `dotnet run --project src/Certus.Dashboard`
- [ ] T040 [P] Mark satisfied requirement/AC checkboxes in `specs/001-validation-ui-compliance/spec.md` and record the outcome in `specs/session-log.md`
- [ ] T041 Final structural-parity review of the three validation pages against the reference pages (`src/Certus.Dashboard/Components/Pages/PlatformDashboard.razor`, `PortfolioDashboard.razor`, `Components/Shared/KpiCard.razor`) to confirm token/composition parity (G-1)

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on Setup — BLOCKS US2, US3, US4 (they consume the shared components). Does NOT block US1
- **User Stories (Phases 3–6)**: US2/US3/US4 depend on Foundational; US1 depends only on Setup
- **Polish (Phase 7)**: Depends on all targeted user stories being complete

### User Story Dependencies

- **US1 (P1) — Navigation**: Independent. Only edits `NavMenu.razor` + `NavigationTests.cs`; does not consume the shared components, so it can proceed immediately after Setup (even in parallel with Foundational)
- **US2 (P2) — Submission pages**: Depends on Foundational (PageHeader, DashboardCard). Independent of US1 and US3
- **US3 (P3) — Run detail**: Depends on Foundational (PageHeader, DashboardCard, EmptyState). Independent of US1 and US2
- **US4 (P4) — Parity sweep**: Verification story that sweeps all three pages; depends on US2 and US3 being implemented (and on the run-detail skeleton from T029). Independent of US1

### Within Each User Story

- Tests are written FIRST and must FAIL before implementation (test-first)
- Shared components (Foundational) before page rebuilds
- For US2/US3: same-file edits (e.g., all `ValidationSubmit.razor` tasks) run sequentially; cross-file tasks may parallelize
- Story complete and independently testable before moving to the next priority

### Parallel Opportunities

- Setup: T002, T003 in parallel
- Foundational: T004, T005, T006 in parallel (three different files)
- After Foundational: US1, US2, US3 can be worked in parallel by different developers (different files); US4 follows US2+US3
- All test-authoring tasks within a story marked [P] run in parallel (distinct assertions, appended to the same new file — coordinate final file assembly)
- T020 (ValidationCompare) parallels the ValidationSubmit tasks (different file)

---

## Parallel Example: Foundational + User Story 1

```bash
# Foundational shared components (different files — run together):
Task T004: "Create PageHeader.razor in src/Certus.Dashboard/Components/Shared/"
Task T005: "Create DashboardCard.razor in src/Certus.Dashboard/Components/Shared/"
Task T006: "Create EmptyState.razor in src/Certus.Dashboard/Components/Shared/"

# US1 navigation tests (write first, must fail) — can run alongside Foundational:
Task T007: "Navigation_HasDataValidationEntry_Should_Render_And_Navigate in tests/.../Navigation/NavigationTests.cs"
Task T008: "Navigation_RunDetail_No_History_Route_Added in tests/.../Navigation/NavigationTests.cs"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 3: User Story 1 (navigation) — does not require Foundational
3. **STOP and VALIDATE**: The Data Validation section is now reachable and highlighted from primary navigation
4. Deploy/demo if ready (discoverability MVP)

### Incremental Delivery

1. Setup → Foundational (shared components) ready
2. US1 (navigation) → test independently → demo (MVP: reachability)
3. US2 (submission pages) → test independently → demo (consistent submit/compare)
4. US3 (run detail) → test independently → demo (consistent results)
5. US4 (parity sweep) → confirms legacy-free structural parity across all pages
6. Polish → CI-order suites green (AC-016), quickstart scenarios pass

### Parallel Team Strategy

With multiple developers, after Setup + Foundational:

- Developer A: US1 (navigation) — can start during Foundational
- Developer B: US2 (submission pages)
- Developer C: US3 (run detail)
- Then: US4 parity sweep once US2 + US3 land

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps each task to its user story and its single component-test file (spec §Test Mapping)
- No Domain/Application/Infrastructure code is modified — AC-016/NFR-005 require those suites to stay green
- Blazor Server constraint: run-detail awaits stay sequential (`GetStatusAsync` → `GetResultAsync`); never `Task.WhenAll`
- bUnit stays pinned to 1.35.x for MudBlazor 7
- Routes are unchanged: `/validation`, `/validation/compare`, `/validation/runs/{RunId}` — no new route (AC-018)
- Verify each test fails before implementing; commit after each task or logical group
