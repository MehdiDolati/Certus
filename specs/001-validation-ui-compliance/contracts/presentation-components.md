# Contract: Presentation Components

**Feature**: 001-validation-ui-compliance
**Type**: Blazor component contracts (`Components/Shared/`)

Defines the reusable presentation building blocks that give the validation pages **structural parity** with the rest of the dashboard (FR-013, BR-3, AC-019). Each validation page MUST compose these shared components rather than one-off lookalike markup.

---

## PageHeader

**Purpose**: Standard page-header block (accent icon tile + prominent title + muted description). Satisfies FR-003, AC-003, AC-004, AC-015.

**Parameters**:

| Name | Type | Required | Default | Notes |
|------|------|----------|---------|-------|
| `Title` | `string` | Yes | — | Rendered via `MudText Typo="Typo.h4"` (never `<h1>`). |
| `Description` | `string?` | No | `null` | Muted supporting line; omitted when null/empty. |
| `Icon` | `string` | Yes | — | Material icon inside the accent tile. |
| `IconGradient` | `string` | No | `linear-gradient(135deg, #3b82f6, #8b5cf6)` | Tile background. |
| `Actions` | `RenderFragment?` | No | `null` | Right-aligned action slot (e.g., compare secondary action). |

**Contract invariants**:
- **PH-1**: Emits no `<h1>` and no browser-default heading (NFR-002, AC-015).
- **PH-2**: Uses the same composition tokens as `PlatformDashboard` header (44px tile, radius 12px, white bold title, muted description).

---

## DashboardCard

**Purpose**: Dark gradient content surface replacing `MudPaper Elevation="2"`. Satisfies FR-004, AC-003, AC-004, AC-015, NFR-002.

**Parameters**:

| Name | Type | Required | Default | Notes |
|------|------|----------|---------|-------|
| `ChildContent` | `RenderFragment` | Yes | — | Card body. |
| `Class` | `string?` | No | `null` | Extra utility classes (spacing). |
| `Padding` | `string?` | No | `pa-6` | Inner padding utility. |

**Contract invariants**:
- **DC-1**: Renders the canonical surface: gradient `#1e293b→#0f172a`, border `1px solid #334155`, radius `16px`.
- **DC-2**: MUST NOT render a MudBlazor default light elevated surface (`MudPaper Elevation="2"` light look) (NFR-002, AC-015).

---

## EmptyState

**Purpose**: Empty / unavailable / not-found region. Satisfies FR-010 (empty-state icon treatment), supports the run "unavailable" state (Decision 5).

**Parameters**:

| Name | Type | Required | Default | Notes |
|------|------|----------|---------|-------|
| `Icon` | `string` | Yes | — | Material icon inside a muted tile. |
| `Title` | `string` | Yes | — | `Typo.h6`, `#94a3b8`. |
| `Description` | `string?` | No | `null` | `#64748b`/`#475569`. |
| `Action` | `RenderFragment?` | No | `null` | Optional action button. |

**Contract invariants**:
- **ES-1**: Uses the same empty-state composition as the reference pages ("No platform connections" / "Portfolio not found").

---

## Page composition contracts

Each page MUST use the shared building blocks for the states it renders.

### ValidationSubmit (`/validation`)

| Element | Building block | Requirement |
|---------|----------------|-------------|
| Header | `PageHeader` | FR-003, AC-003 |
| Submission form surface | `DashboardCard` | FR-004, AC-003 |
| File/timeframe/checkbox/button controls | Standard MudBlazor controls (same density/states) | FR-005 |
| Submit busy state | `MudProgressCircular` + label, disabled button | FR-006, AC-005 |
| Compare secondary action | `PageHeader.Actions` or in-card link/button to `/validation/compare` | FR-002a, AC-017 |
| Rejection / upload error | `MudAlert Severity="Error"` | FR-011, AC-006 |
| Success navigation | `NavigateTo(..., forceLoad:false)` | FR-012, AC-020 |

### ValidationCompare (`/validation/compare`)

| Element | Building block | Requirement |
|---------|----------------|-------------|
| Header | `PageHeader` (identical composition to submit) | FR-003, AC-004 |
| Form surface | `DashboardCard` | FR-004, AC-004 |
| Controls / busy / errors / success nav | Same as ValidationSubmit | FR-005, FR-006, FR-011, FR-012 |
| Reciprocal secondary action → `/validation` | `PageHeader.Actions` / in-card | FR-002a |

### ValidationRunDetail (`/validation/runs/{RunId}`)

| Element | Building block | Requirement |
|---------|----------------|-------------|
| Header | `PageHeader` | FR-003 |
| Status banners | `MudAlert` (info/success/warning/error) | FR-007, AC-007–AC-010 |
| Quality summary + score sections | `DashboardCard` + dashboard-styled table | FR-008, AC-011, AC-013 |
| Download controls | Secondary-styled `MudButton` | FR-009, AC-012 |
| Loading | `MudSkeleton` (no blank region) | FR-010, AC-014 |
| Unavailable | `MudAlert Warning` or `EmptyState` | FR-007 |

## Global invariants (all pages)

- **G-1 (structural parity)**: Every visible element is derived from the same shared building blocks / design tokens as the equivalent element elsewhere — no one-off lookalike markup (FR-013, NFR-001, AC-019).
- **G-2 (no legacy elements)**: No `<h1>`, no plain paragraph text strips, no default light elevated surfaces remain (NFR-002, AC-015).
- **G-3 (responsive)**: Same grid/breakpoint behavior; no horizontal overflow (NFR-003).
- **G-4 (contrast)**: WCAG AA parity via the shared tokens (NFR-004).
- **G-5 (behavior unchanged)**: No validation/scoring/report semantics change (NFR-005, BR-2).

## Acceptance mapping

| Component / composition | Acceptance Criteria | Test method |
|-------------------------|---------------------|-------------|
| PageHeader + DashboardCard on submit | AC-003 | `ValidationSubmit_HeaderAndCard_Style_Matches_Dashboard` |
| Identical header/card on compare | AC-004 | `ValidationSubmit_HeaderAndCard_Style_Matches_Dashboard` |
| Submit busy state + duplicate prevention | AC-005 | `ValidationSubmit_HeaderAndCard_Style_Matches_Dashboard` |
| Rejection alert style | AC-006 | `ValidationSubmit_Rejection_Shows_Alert_Style` |
| Status presentations per state | AC-007–AC-011 | `ValidationRunDetail_Status_Shows_Consistent_Style_Per_State` |
| Export buttons secondary style | AC-012 | `ValidationRunDetail_ExportButtons_Style_Matches_Dashboard` |
| Score typography | AC-013 | `ValidationRunDetail_Scores_Render_On_Standard_Typography` |
| Loading skeleton | AC-014 | `ValidationPages_LoadingState_Style_Matches_Dashboard` |
| No unstyled headings/light surfaces | AC-015 | `ValidationPages_No_Unstyled_Headings_Or_Light_Surfaces` |
| Compare secondary action visible | AC-017 | `ValidationSubmit_CompareOption_Visible_Inside_Section` |
| Shares dashboard building blocks | AC-019 | `ValidationPages_Shares_Dashboard_Building_Blocks` |
| Success navigates without reload | AC-020 | `ValidationSubmit_Success_Navigates_Without_Reload` |
