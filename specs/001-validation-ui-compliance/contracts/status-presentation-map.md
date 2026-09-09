# Contract: Status → Presentation Map

**Feature**: 001-validation-ui-compliance
**Type**: State-to-UI mapping contract (`ValidationRunDetail.razor`)

Every existing run/presentation state MUST map to an **existing** dashboard presentation pattern — no new one-off visuals may be invented (BR-3, FR-007). Semantics are unchanged; only surfaces/composition are upgraded to the shared building blocks.

## Mapping table

| # | State (source) | Trigger | Presentation intent | Building block(s) | Requirements |
|---|----------------|---------|---------------------|-------------------|--------------|
| S1 | Initial loading | `IsLoading == true` (before first data) | Skeleton/progress, no blank white region | `MudSkeleton Animation="Wave"` (as PortfolioDetail) / `MudProgressLinear` | FR-010, AC-014 |
| S2 | Pending | `WebRunStatus.Pending` | Informational + progress + refresh action | `MudAlert Severity="Info"` + `MudProgressCircular` + `MudButton` (Refresh) | FR-007, AC-010 |
| S3 | Running | `WebRunStatus.Running` | Informational + progress + refresh action | same as S2 | FR-007, AC-010 |
| S4 | Complete — clean | `WebRunStatus.CompletedClean` | Success banner + quality summary table | `MudAlert Severity="Success"` + `DashboardCard` + dashboard table | FR-007, FR-008, AC-007, AC-011 |
| S5 | Complete — findings | `WebRunStatus.CompletedWithFindings` | Warning banner + quality summary table (all counts visible) | `MudAlert Severity="Warning"` + `DashboardCard` + dashboard table | FR-007, FR-008, AC-008, AC-011 |
| S6 | Failed | `WebRunStatus.Failed` | Error banner: message + diagnostic code + guidance | `MudAlert Severity="Error"` (Code, Reason, Guidance) | FR-007, AC-009 |
| S7 | Unavailable | `WebResultRetrieval.Unavailable` / unknown/expired run / invalid id | Error/warning informational (no crash) | `MudAlert Severity="Warning"` or `EmptyState` | FR-007 |
| S8 | Scoring present | `View.Scoring != null` | Score dimensions in dashboard-styled table; numbers on standard KPI typographic scale | `DashboardCard` + dashboard table (`ScoreDisplay` parity) | FR-008, AC-011, AC-013 |
| S9 | Exports available | `View.AvailableExports.Count > 0` | Secondary-styled action buttons (one per representation) | `MudButton Variant="Outlined"`/secondary | FR-009, AC-012 |

## Submission-page states (ValidationSubmit / ValidationCompare)

| # | State | Trigger | Presentation intent | Building block(s) | Requirements |
|---|-------|---------|---------------------|-------------------|--------------|
| U1 | Idle | no file / not submitting | Normal form in `DashboardCard` | `DashboardCard` + MudBlazor controls | FR-004, FR-005 |
| U2 | Submitting | `IsSubmitting == true` | Busy button (spinner + label), duplicate submission prevented | `MudProgressCircular Size="Small"` + disabled `MudButton` | FR-006, AC-005 |
| U3 | Rejected | `WebRunSubmission.Rejected` | Inline error alert (code — reason — guidance) | `MudAlert Severity="Error"` | FR-011, AC-006 |
| U4 | Upload error / oversize | exception / size > limit | Inline error alert | `MudAlert Severity="Error"` | FR-011, AC-006 |
| U5 | Accepted | `WebRunSubmission.Accepted` | No-reload navigation to run detail | `NavigateTo($"/validation/runs/{id}", forceLoad:false)` | FR-012, AC-020 |

## Invariants

- **SP-1**: Each state maps to exactly one existing presentation pattern; no bespoke banner/table/loader is introduced (BR-3, FR-013, AC-019).
- **SP-2**: State→severity mapping is preserved from current behavior: clean→Success, findings→Warning, failed/unavailable→Error/Warning, pending/running→Info.
- **SP-3**: All counts remain visible with dashboard-consistent spacing in findings state (AC-008).
- **SP-4**: No state alters underlying run data; it is projected read-only (NFR-005, BR-2, AC-016).

## Acceptance mapping

| States | Acceptance Criteria | Test method |
|--------|---------------------|-------------|
| S4, S8 | AC-007 (clean success + structured counts table) | `ValidationRunDetail_Status_Shows_Consistent_Style_Per_State` |
| S5 | AC-008 (findings warning + counts visible) | `ValidationRunDetail_Status_Shows_Consistent_Style_Per_State` |
| S6 | AC-009 (failure message/code/guidance) | `ValidationRunDetail_Status_Shows_Consistent_Style_Per_State` |
| S2, S3 | AC-010 (pending/running info + progress + refresh) | `ValidationRunDetail_Status_Shows_Consistent_Style_Per_State` |
| S8 | AC-011, AC-013 (table density + numeric typography) | `ValidationRunDetail_Scores_Render_On_Standard_Typography` |
| S9 | AC-012 (secondary export buttons) | `ValidationRunDetail_ExportButtons_Style_Matches_Dashboard` |
| S1 | AC-014 (loading skeleton) | `ValidationPages_LoadingState_Style_Matches_Dashboard` |
| U2 | AC-005 (busy + duplicate prevention) | `ValidationSubmit_HeaderAndCard_Style_Matches_Dashboard` |
| U3, U4 | AC-006 (error alert) | `ValidationSubmit_Rejection_Shows_Alert_Style` |
| U5 | AC-020 (no-reload navigation) | `ValidationSubmit_Success_Navigates_Without_Reload` |
