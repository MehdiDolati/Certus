# Validation UI Compliance

## Status
Draft

## Overview
The Data Validation workflow is currently presented with plain, unstyled page headings and generic elevated surfaces that look disconnected from the rest of the application, and it can only be reached by typing a URL. This feature makes the Data Validation area reachable from the application's main navigation and applies the same visual design language used across the other dashboard pages (consistent headings, card surfaces, result/status presentation, alert styling, and empty/loading states) **without changing any validation behavior or existing run URLs**.

## Clarifications

### Session 2026-09-09

- Q: How should the Data Validation section appear in the application's main navigation? → A: Single primary nav entry to /validation; compare reached as a secondary action inside the section.
- Q: How should users reach previously submitted runs from the Data Validation section? → A: No new run-history page; run detail remains reachable via the submission redirect, bookmarks, and shared links; the nav entry simply highlights on run detail pages.
- Q: How literally should the validation pages match the styling of the shared dashboard components? → A: Structural parity — validation pages are rebuilt on the same shared component library primitives used elsewhere in the dashboard (not raw HTML with CSS imitations), keeping behavior unchanged.

## Route

- `/validation` — validation submission page
- `/validation/compare` — benchmark comparison submission page
- `/validation/runs/{RunId}` — validation / comparison run result detail page

## Requirements

### Functional Requirements

- [ ] FR-001: The user can reach the Data Validation section from the application's primary navigation with a single click from any page in the application, via a single navigation entry that opens the validation submission page.
- [ ] FR-002: The single Data Validation navigation entry is permanently visible in the same navigation area as the other primary sections, uses the same label styling/icon treatment, and highlights when the user is inside the Data Validation section (validation, compare, or run detail pages).
- [ ] FR-002a: The compare page is reachable as a secondary action within the Data Validation section (the submission card provides the option to switch to benchmark comparison), not as a separate top-level navigation entry.
- [ ] FR-002b: No new run-history page or route is introduced; previously submitted runs remain reachable through the existing submission redirect, bookmarks, and shared links, and the Data Validation navigation entry displays the highlighted state on run detail pages as well.
- [ ] FR-003: Every Data Validation page presents its title in the same page-header composition used by the other dashboard pages (accent icon tile, prominent title, and supporting description).
- [ ] FR-004: The dataset submission form (validation and compare) is presented on surfaces that match the card design used across the platform: same background treatment, border, corner radius, inner spacing, and layering as the standard content cards.
- [ ] FR-005: File selection, timeframe/benchmark/instrument inputs, checkboxes, and action buttons inside the submission cards use the same visual presentation, density, and states as equivalent controls found elsewhere in the application.
- [ ] FR-006: In-progress submission (uploading/validating/comparing) is indicated using the same busy/processing visual treatment (with text + progress indicator) used by other primary actions in the application.
- [ ] FR-007: The run detail page shows run state (pending, running, complete-clean, complete-with-findings, failed, unavailable) through consistent result/status messaging styles used elsewhere: clean → success presentation, findings → warning presentation, failed/unavailable → error presentation, and pending/running → informational presentation with the same progress treatment.
- [ ] FR-008: Quality summary counts and scoring dimensions on the run detail page are displayed in structured content sections consistent with other data tables on the dashboard: same section headers, spacing, table density, and alternating/hover behavior.
- [ ] FR-009: The report download controls use the standard action-button presentation used for similar secondary actions elsewhere in the application.
- [ ] FR-010: Empty/loading states across the validation pages use the same skeleton/empty-state presentation (including empty-state icon treatment) as other dashboard pages.
- [ ] FR-011: Submission rejection and upload error alerts use the same inline alert presentation used across the application rather than plain text strips.
- [ ] FR-012: Successful submission navigates the user to the validation run results page using the same "primary call to action" flow and page transition as other cross-page navigation (no full page reload).
- [ ] FR-013: The visible presentation of the validation pages is achieved with the same shared presentation building blocks used by the other dashboard pages (page header block, card/surface elements, alert elements, table elements, progress/skeleton indicators) rather than purpose-built one-off markup styled to look similar; no new one-off visual elements are introduced.

### Non-Functional Requirements

- [ ] NFR-001: Visual consistency — every element visible on the three validation pages must be derived from the same design tokens (background surfaces, text colors, accent palette, border styles, radius, spacing) used by the rest of the application.
- [ ] NFR-002: No legacy/un-themed element (`<h1>`, plain paragraph text, or default light elevated surfaces) may remain on the three validation pages.
- [ ] NFR-003: The validation pages must remain responsive in the same layout grid/breakpoint behavior used by other pages with no horizontal overflow at the standard supported widths.
- [ ] NFR-004: All text on the validation pages must meet the same WCAG AA contrast requirements as the rest of the application.
- [ ] NFR-005: The styling/navigation change must not alter validation rule behavior, results content, or submission semantics; test coverage at infrastructure/application level must remain green.

## Business Rules

1. Existing run URLs (`/validation`, `/validation/compare`, `/validation/runs/{RunId}`) must remain unchanged so existing bookmarks and shared links keep working.
2. Validation behavior, scoring logic, and report semantics are out of scope; only presentation and application navigation are changed.
3. Every presentation state that already exists on the validation pages (loading, empty, pending, running, clean, findings, failed, rejected, error) must map to an existing presentation pattern from the rest of the application — no new one-off visuals may be invented. Visual parity is structural: the pages must be rebuilt using the same shared presentation building blocks used elsewhere in the dashboard.
4. The application's shared chrome (app bar, drawer/navigation, page background) is already applied; page content within the validation area is what must conform.
5. No new routes or pages are introduced by this feature; the run detail page remains reachable only through the existing submission flow, bookmarks, and shared links.

## Acceptance Criteria

- [ ] AC-001: Given the application is running and the main navigation is visible, when the user browses the primary navigation items, then a single "Data Validation" entry is visible alongside the other primary sections, styled identically to them, with a consistent icon.
- [ ] AC-002: Given the user is on any application page, when the user clicks the Data Validation navigation entry, then the validation submission page opens without error and the navigation entry displays the same selected/highlight treatment as the other selected entries.
- [ ] AC-003: Given the validation submission page, when visually compared with the platform's other primary pages, then the page header and content surfaces use the same composition and design tokens (title block, supporting text, card surfaces, controls).
- [ ] AC-004: Given the validation compare page, when visually compared with the validation submission page, then it shares the identical header composition and card/control presentation.
- [ ] AC-005: Given a user submits a valid CSV file, when the submission is in progress, then the submit button shows the same processing treatment as other in-flight primary actions (spinner + label) and duplicate submission is prevented.
- [ ] AC-006: Given a rejected submission or oversize upload, when the response returns, then the error is displayed with the same alert treatment used elsewhere (not as plain unstyled text).
- [ ] AC-007: Given a completed clean run, when the result detail page renders, then the completion banner uses the success presentation matching the rest of the application and the quality check counts are shown in a structured table consistent with the dashboard.
- [ ] AC-008: Given a completed run with findings, when the result detail page renders, then the completion banner/styling uses the warning presentation and all finding counts remain visible with dashboard-consistent spacing.
- [ ] AC-009: Given a failed run with a diagnostic code, when the result detail page renders, then the failure is shown with the same failure presentation style (message, code, guidance) used for errors elsewhere in the application.
- [ ] AC-010: Given a pending/running run, when the result detail page renders, then the informational treatment matches other informational/progress states in the application (progress indication + explanatory text + refresh action button).
- [ ] AC-011: Given the run detail page with scoring enabled, when the page renders, then the dimension table uses the same header/density/spacing as the standard dashboard tables.
- [ ] AC-012: Given the run detail page with available exports, when the user views the page, then download buttons use the same secondary-button style as equivalent actions on other pages.
- [ ] AC-013: Given two clean datasets, when quality scores are reviewed on the detail page, then each score renders on the same typographic scale as other KPI/numeric values in the application.
- [ ] AC-014: Given any validation page loading its data, when the page first renders, then loading feedback uses the same skeleton/progress treatment as other dashboard pages (no blank white region).
- [ ] AC-015: Given all three validation pages after the restyle, when they are compared against the platform pages (e.g., Portfolios, Platform), then no plain/browser-default headings or default light elevated card surfaces remain.
- [ ] AC-016: Given the existing automated test suite, when the suite is run, then all domain/application/infrastructure/architecture tests pass unchanged, confirming no behavioral regression from the visual alignment.
- [ ] AC-017: Given the validation submission page, when the user looks for the benchmark comparison option, then a secondary action inside the section switches to the compare submission page without requiring a separate navigation entry.
- [ ] AC-018: Given the set of application routes after this change, when the user browses the Data Validation section, then only the existing routes exist (`/validation`, `/validation/compare`, `/validation/runs/{RunId}`) and no new run-history route has been added.
- [ ] AC-019: Given the restyled validation pages, when their visible elements are compared with the other dashboard pages, then each element uses the same shared presentation building blocks as the equivalent element elsewhere (no one-off lookalike markup, no stray default-browser appearance).
- [ ] AC-020: Given a valid CSV submission, when processing completes successfully, then the user is automatically navigated to the run results page without a full page reload, matching the cross-page navigation behavior elsewhere in the application.

## Test Mapping

| Acceptance Criteria | Test File | Test Method |
|---------------------|-----------|-------------|
| AC-001, AC-002 | ComponentTests (Dashboard.Navigation) | Navigation_HasDataValidationEntry_Should_Render_And_Navigate |
| AC-003–AC-005 | ComponentTests (ValidationSubmit) | ValidationSubmit_HeaderAndCard_Style_Matches_Dashboard |
| AC-006 | ComponentTests (ValidationSubmit) | ValidationSubmit_Rejection_Shows_Alert_Style |
| AC-007–AC-011 | ComponentTests (ValidationRunDetail) | ValidationRunDetail_Status_Shows_Consistent_Style_Per_State |
| AC-012 | ComponentTests (ValidationRunDetail) | ValidationRunDetail_ExportButtons_Style_Matches_Dashboard |
| AC-013 | ComponentTests (ValidationRunDetail) | ValidationRunDetail_Scores_Render_On_Standard_Typography |
| AC-014 | ComponentTests (ValidationPages) | ValidationPages_LoadingState_Style_Matches_Dashboard |
| AC-015 | ComponentTests (ValidationPages) | ValidationPages_No_Unstyled_Headings_Or_Light_Surfaces |
| AC-016 | Domain/Application/Infrastructure test projects | — (existing suites must remain green) |
| AC-017 | ComponentTests (ValidationSubmit) | ValidationSubmit_CompareOption_Visible_Inside_Section |
| AC-018 | ComponentTests (Dashboard.Navigation) | Navigation_RunDetail_No_History_Route_Added |
| AC-019 | ComponentTests (ValidationPages) | ValidationPages_Shares_Dashboard_Building_Blocks |
| AC-020 | ComponentTests (ValidationSubmit) | ValidationSubmit_Success_Navigates_Without_Reload |

## UI Requirements

- **Navigation**: a single Data Validation primary-navigation entry with an icon consistent with the section (visible from any page, active state included on validation, compare, and run detail pages). No run-history route is added; past runs remain reachable via the existing submission flow, bookmarks, and shared links.
- **Page header**: each of the three pages uses the standard page-header block: rounded accent icon tile, bold page title, and muted supporting description on the same margins as the rest of the dashboard.
- **Submission cards**: the upload/compare forms sit on the shared card/surface elements with consistent corner radius, padding, and separators from the page background; control rows align to the standard responsive grid. Forms, alerts, tables, and progress indicators are all rendered via the same shared presentation building blocks used by the other dashboard pages.
- **Status & results**: quality summaries and score tables use standard dashboard table styling (structured headers, dense rows, consistent spacing); status outcomes use the standard success/warning/error/info alert treatments; in-flight work and initial loading use standard progress/skeleton treatments.
- **Guidance text on the dashboard: none** — Data Validation is a distinct section of the dashboard accessible from the shared navigation, not a separate standalone site.

## Dependencies

- Existing validation pages: `/validation`, `/validation/compare`, `/validation/runs/{RunId}`
- Shared dashboard page patterns (header, cards, tables, alerts, loading states) already established by the other primary dashboard sections
- `Validator.*` application/domain services (behavior) — referenced for the run lifecycle but **not modified by this feature**