# Contract: Routes

**Feature**: 001-validation-ui-compliance
**Type**: UI route contract (Blazor Server page routes)

Existing validation routes MUST remain unchanged so bookmarks and shared links keep working (BR-1, FR-002b, BR-5, AC-018). This feature adds **no new routes**.

## Route inventory (after this feature)

| Route | Page component | Purpose | Change |
|-------|----------------|---------|--------|
| `/validation` | `ValidationSubmit.razor` | Validation dataset submission | Restyled only — route unchanged |
| `/validation/compare` | `ValidationCompare.razor` | Benchmark comparison submission | Restyled only — route unchanged |
| `/validation/runs/{RunId}` | `ValidationRunDetail.razor` | Run/comparison result detail | Restyled only — route unchanged |

## Invariants

- **INV-R1**: The `@page` directive on each of the three pages MUST remain exactly as today (`/validation`, `/validation/compare`, `/validation/runs/{RunId}`).
- **INV-R2**: No new `@page` route is introduced anywhere (no run-history route). Verified by AC-018.
- **INV-R3**: `{RunId}` continues to be passed to `ValidationRunDetail` as the `RunId` string parameter and parsed with `WebRunId.Parse` (unchanged behavior).
- **INV-R4**: Successful submission navigates to `/validation/runs/{RunId}` via `NavigationManager.NavigateTo(..., forceLoad: false)` (no full reload). Verified by AC-020.

## Acceptance mapping

| Contract check | Acceptance Criteria |
|----------------|---------------------|
| Only the three existing routes exist; no run-history route added | AC-018 |
| Run detail reachable via submission redirect / bookmark / shared link | FR-002b, BR-5 |
| Submission redirect is no-reload | AC-020 |
