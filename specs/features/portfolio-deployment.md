# Portfolio Deployment Spec

## Overview

This spec defines the "Portfolio Deployment" feature: the ability for a user to select a local folder containing compiled MT4 Expert Advisors (.ex4 files), deploy them to an MT4 terminal's Experts folder, create a Certus Portfolio entity, and automatically start monitoring the EA's output files for trade data.

## Requirements

### Functional Requirements

- [ ] FR-001: User can select a local folder containing .ex4 EA files via a folder picker dialog or manual path input
- [ ] FR-002: System validates the selected folder contains at least one .ex4 file (root folder only, not recursive)
- [ ] FR-003: User selects which MT4 connection to deploy the portfolio to (one connection at a time)
- [ ] FR-004: System creates a Certus Portfolio entity with name derived from folder name (user can override)
- [ ] FR-005: System creates StrategyDefinition entities for each .ex4 file found in the folder
- [ ] FR-006: System copies all .ex4 files from the source folder to the target MT4's MQL4/Experts/ directory
- [ ] FR-007: System auto-detects MT4 data folder from existing connection file path, with manual override option
- [ ] FR-008: System starts FileSystemWatcher on the target MT4's Files/Certus/ directory after deployment
- [ ] FR-009: System creates a PlatformConnection linking the Portfolio to the MT4 instance
- [ ] FR-010: System displays deployment status in the UI (copying, monitoring, error)
- [ ] FR-011: System prompts user for each conflicting EA (same filename exists in target) - skip or overwrite
- [ ] FR-012: System logs deployment activity (files copied, monitoring started, errors)
- [ ] FR-013: System auto-activates EAs after deployment (attaches to charts via management EA)

### Non-Functional Requirements

- [ ] NFR-001: File copy operations handle file locking gracefully (retry with backoff)
- [ ] NFR-002: Deployment completes within 30 seconds for typical portfolios (<50 EAs)
- [ ] NFR-003: FileSystemWatcher starts within 5 seconds of deployment completion
- [ ] NFR-004: All file operations use absolute paths with proper error handling

## Data Model

### PortfolioDeployment (New Entity)

```
PortfolioDeployment
  Id                   : Guid                    // Unique identifier
  PortfolioId          : Guid                    // Certus Portfolio reference
  ConnectionId         : Guid                    // Platform connection reference
  SourceFolderPath     : string                  // User-selected source folder
  TargetMT4Path        : string                  // MT4 Experts folder path
  Status               : DeploymentStatus        // Pending | Copying | Deployed | Monitoring | Error
  DeployedAt           : DateTime?               // When deployment completed
  MonitoringStartedAt  : DateTime?               // When monitoring began
  EACount              : int                     // Number of EAs deployed
  ErrorMessage         : string?                 // Error details if failed
```

### DeploymentStatus (New Enum)

```
DeploymentStatus
  Pending              // Waiting to start
  Copying              // Copying files to MT4
  Deployed             // Files copied, awaiting monitoring
  Monitoring           // Actively watching for EA output
  Error                // Deployment failed
  Stopped              // Monitoring manually stopped
```

### Updated PlatformConnection

```
PlatformConnection
  ... existing fields ...
  PortfolioDeploymentId : Guid?                  // Link to deployment (nullable)
```

## Business Rules

1. **BR-001**: A source folder must contain at least one .ex4 file to be deployable
2. **BR-002**: Only .ex4 files in the root of the selected folder are discovered (no recursive search)
3. **BR-003**: For each EA conflict (same filename in target), user is prompted to skip or overwrite
4. **BR-004**: Deployment creates a Portfolio entity with Status = Active
5. **BR-005**: Each .ex4 file becomes a StrategyDefinition with Status = Active
6. **BR-006**: MT4 Experts folder path is derived from connection's file path by replacing the trailing "Files/Certus/" with "MQL4/Experts/"
7. **BR-007**: If auto-detection fails, user must manually specify the MT4 data folder
8. **BR-008**: Monitoring starts automatically after successful file copy
9. **BR-009**: Deployment can be stopped (monitoring paused) but not deleted while active
10. **BR-010**: EAs are copied as-is; no compilation or modification occurs
11. **BR-011**: After deployment, system attempts to auto-activate EAs by writing a management script to MT4's Scripts/ folder
12. **BR-012**: Auto-activation uses a CertusManager.mq4 script that loads EAs on specified charts

## Acceptance Criteria

### Folder Selection & Validation
- [ ] AC-001: Given a user clicks "Select Portfolio Folder", when the folder picker opens, then the user can browse and select a local directory
- [ ] AC-002: Given a selected folder with 3 .ex4 files in root, when validated, then the system shows "3 Expert Advisors found" with file names
- [ ] AC-003: Given a selected folder with .ex4 files only in subfolders, when validated, then an error "No .ex4 files found in root folder" is displayed
- [ ] AC-004: Given a user types a manual path, when the path is valid and contains .ex4 files in root, then validation passes

### Connection Selection
- [ ] AC-005: Given multiple MT4 connections exist, when user opens connection selector, then all active connections are listed
- [ ] AC-006: Given a selected connection, when the system derives MT4 path, then it attempts auto-detection from the connection's file path
- [ ] AC-007: Given auto-detection fails, when user is prompted, then manual MT4 path input is available
- [ ] AC-008: Given a connection with an existing deployment, when selected, then a warning "This connection already has a deployment" is shown

### Deployment & File Conflicts
- [ ] AC-009: Given valid folder and connection, when user clicks "Deploy", then a Portfolio entity is created with the folder name
- [ ] AC-010: Given deployment starts, when copying files, then status shows "Copying X files to MT4..."
- [ ] AC-011: Given an EA with same filename exists in target, when prompted, then user can choose "Skip" or "Overwrite"
- [ ] AC-012: Given user chooses "Skip" for a conflicting EA, when copy continues, then that EA is skipped and logged
- [ ] AC-013: Given user chooses "Overwrite" for a conflicting EA, when copy continues, then the EA is overwritten
- [ ] AC-014: Given all .ex4 files copied, when copy completes, then status changes to "Deployed"
- [ ] AC-015: Given deployment completes, when monitoring starts, then FileSystemWatcher is attached to MT4's Files/Certus/ directory
- [ ] AC-016: Given a file copy fails due to locking, when retried, then system retries up to 3 times with exponential backoff
- [ ] AC-017: Given deployment fails, when error occurs, then status shows "Error" with error message

### Auto-Activation
- [ ] AC-018: Given deployment completes, when auto-activation runs, then a CertusManager.mq4 script is copied to MT4's Scripts/ folder
- [ ] AC-019: Given the management script is deployed, when MT4 runs it, then EAs are loaded on specified charts
- [ ] AC-020: Given auto-activation fails, when error occurs, then status shows "Deployed (activation pending)" with manual instructions

### Monitoring
- [ ] AC-021: Given monitoring is active, when portfolio_status.json changes, then Certus reads and imports the data
- [ ] AC-022: Given monitoring is active, when trades.json changes, then new trades are imported
- [ ] AC-023: Given monitoring is active, when displayed on Platform page, then deployment shows "Monitoring" status with last data timestamp

### Portfolio & Strategy Creation
- [ ] AC-024: Given deployment creates a Portfolio, when the Portfolio is created, then Status = Active and IsPlatformManaged = true
- [ ] AC-025: Given 5 .ex4 files in folder, when deployment creates Strategies, then 5 StrategyDefinition entities are created with matching names
- [ ] AC-026: Given StrategyDefinitions created, when viewed in Portfolio Detail, then each EA appears as a strategy

## API Contract

```csharp
public interface IPortfolioDeploymentService
{
    Task<ValidateFolderResult> ValidateFolderAsync(string folderPath);
    Task<List<string>> GetAvailableConnectionsAsync();
    Task<DeriveMT4PathResult> DeriveMT4PathAsync(Guid connectionId);
    Task<CheckConflictsResult> CheckConflictsAsync(string sourceFolder, string targetExpertsPath);
    Task<DeployPortfolioResult> DeployAsync(DeployPortfolioRequest request);
    Task<DeploymentStatusDto> GetDeploymentStatusAsync(Guid deploymentId);
    Task StopMonitoringAsync(Guid deploymentId);
    Task<List<PortfolioDeploymentDto>> GetDeploymentsAsync();
}

public record ValidateFolderResult
{
    public bool IsValid { get; init; }
    public List<string> EANames { get; init; } = new();
    public int EACount { get; init; }
    public string? ErrorMessage { get; init; }
}

public record DeriveMT4PathResult
{
    public bool Success { get; init; }
    public string? MT4DataPath { get; init; }
    public string? ExpertsPath { get; init; }
    public string? ErrorMessage { get; init; }
}

public record CheckConflictsResult
{
    public List<string> ConflictingFiles { get; init; } = new();
    public int TotalFiles { get; init; }
    public int NewFiles { get; init; }
}

public record DeployPortfolioRequest
{
    public string SourceFolderPath { get; init; } = string.Empty;
    public Guid ConnectionId { get; init; }
    public string? PortfolioName { get; init; } // Optional override
    public string? ManualMT4Path { get; init; } // Manual override if auto-detect fails
    public Dictionary<string, ConflictAction>? ConflictResolutions { get; init; } // Per-file resolution
}

public enum ConflictAction
{
    Skip,      // Skip this file
    Overwrite  // Overwrite existing file
}

public record DeployPortfolioResult
{
    public bool Success { get; init; }
    public Guid PortfolioId { get; init; }
    public Guid DeploymentId { get; init; }
    public int FilesCopied { get; init; }
    public int FilesSkipped { get; init; }
    public bool ActivationPending { get; init; } // true if auto-activation needs manual step
    public string? ErrorMessage { get; init; }
}
```

## UI Requirements

### DeployPortfolioDialog (New Component)

A MudDialog with the following steps:

1. **Step 1 - Folder Selection**:
   - "Select Portfolio Folder" button with MudIcon (FolderOpen)
   - Manual path input field
   - Validation result display (EA count, file names)
   - "Next" button (enabled when folder is valid)

2. **Step 2 - Connection Selection**:
   - Dropdown of available MT4 connections
   - Auto-detected MT4 path display with "Override" button
   - Manual path input (shown if override is clicked or auto-detect fails)
   - "Check Conflicts" button

3. **Step 3 - Conflict Resolution** (if conflicts exist):
   - Table showing conflicting EAs with columns: Filename, Source Size, Target Size, Modified Date
   - Per-file radio buttons: "Skip" | "Overwrite"
   - "Apply to All" dropdown: "Skip All" | "Overwrite All"
   - "Deploy" button

4. **Step 4 - Progress**:
   - Progress bar showing copy status
   - Status text: "Copying files...", "Activating EAs...", "Starting monitoring...", "Done!"
   - Success/error message
   - If activation pending: manual instructions link
   - "Close" button

### PlatformDashboard.razor Updates

- Add "Deploy Portfolio" button next to "Connect" button
- Show deployment list with status, EA count, and monitoring status
- Each deployment shows: Portfolio name, connection, status, EAs deployed, last data received
- "Stop" button for active deployments
- "Activation" badge if manual activation is needed

## Dependencies

- Domain: `Portfolio` (existing), `StrategyDefinition` (existing), `PlatformConnection` (existing)
- Domain: `PortfolioDeployment` (new entity), `DeploymentStatus` (new enum)
- Application: `IPortfolioDeploymentService` (new), `IPlatformService` (existing)
- Infrastructure: `FileSystemDeploymentService` (new), `FileCopyService` (new), `MT4ActivationService` (new)
- Dashboard: `DeployPortfolioDialog.razor` (new), `PlatformDashboard.razor` (updated)

## Test Mapping

| Acceptance Criteria | Test File | Test Method |
|---------------------|-----------|-------------|
| AC-001 | FolderPickerTests.cs | FolderPicker_Should_Open_Dialog |
| AC-002 | FolderValidationTests.cs | ValidateFolder_Should_Find_Ex4_In_Root |
| AC-003 | FolderValidationTests.cs | ValidateFolder_Should_Reject_No_Ex4_In_Root |
| AC-004 | FolderValidationTests.cs | ValidateFolder_Should_Accept_Manual_Path |
| AC-005 | ConnectionSelectorTests.cs | GetConnections_Should_Return_Active |
| AC-006 | MT4PathDerivationTests.cs | DerivePath_Should_AutoDetect_From_Connection |
| AC-007 | MT4PathDerivationTests.cs | DerivePath_Should_Fallback_To_Manual |
| AC-008 | ConflictDetectionTests.cs | CheckConflicts_Should_Warn_Existing_Deployment |
| AC-009 | DeploymentTests.cs | Deploy_Should_Create_Portfolio_Entity |
| AC-010 | DeploymentTests.cs | Deploy_Should_Show_Copying_Status |
| AC-011 | ConflictResolutionTests.cs | ConflictResolution_Should_Prompt_User |
| AC-012 | ConflictResolutionTests.cs | ConflictResolution_Skip_Should_Skip_File |
| AC-013 | ConflictResolutionTests.cs | ConflictResolution_Overwrite_Should_Overwrite |
| AC-014 | DeploymentTests.cs | Deploy_Should_Show_Deployed_Status |
| AC-015 | MonitoringTests.cs | Deploy_Should_Start_FileSystemWatcher |
| AC-016 | FileCopyTests.cs | Copy_Should_Retry_On_Lock |
| AC-017 | DeploymentTests.cs | Deploy_Should_Show_Error_On_Failure |
| AC-018 | ActivationTests.cs | Activation_Should_Copy_Manager_Script |
| AC-019 | ActivationTests.cs | Activation_Should_Generate_Config |
| AC-020 | ActivationTests.cs | Activation_Should_Handle_Failure_Gracefully |
| AC-021 | MonitoringTests.cs | FileSystemWatcher_Should_Import_Portfolio_Status |
| AC-022 | MonitoringTests.cs | FileSystemWatcher_Should_Import_Trades |
| AC-023 | PlatformDashboardTests.cs | Dashboard_Should_Show_Monitoring_Status |
| AC-024 | DeploymentTests.cs | Deploy_Should_Create_Active_Portfolio |
| AC-025 | DeploymentTests.cs | Deploy_Should_Create_Strategy_Definitions |
| AC-026 | PortfolioDetailTests.cs | Detail_Should_Show_EAs_As_Strategies |

## Implementation Notes

### MT4 Path Derivation Logic

Given a connection's file path like:
```
C:\Users\Mehdi\AppData\Roaming\MetaQuotes\Terminal\ABC123\Certus\portfolio_status.json
```

Derive MT4 Experts path:
```
C:\Users\Mehdi\AppData\Roaming\MetaQuotes\Terminal\ABC123\MQL4\Experts\
```

Logic: Take the connection file path, find the terminal hash directory (the directory containing "Certus" folder), then construct "MQL4/Experts/" path.

### File Copy Strategy

1. Read all .ex4 files from source folder root only (not recursive)
2. For each file, check if it already exists in target
3. If conflict exists, use the resolution from ConflictResolutions dictionary
4. Copy using File.Copy with overwrite based on resolution
5. Handle file locking with retry logic (3 attempts, exponential backoff)
6. Log each file copy operation (copied, skipped, overwritten)

### Auto-Activation Strategy

MT4 does not provide a direct API for attaching EAs to charts programmatically. The auto-activation approach uses a management script:

1. **CertusManager.mq4**: A MetaTrader script that:
   - Reads a configuration file (`certus_activation.json`) from MT4's Files/Certus/
   - Loops through the EA list in the config
   - For each EA, opens a new chart and attaches the EA with specified parameters
   - Handles errors gracefully (logs failures, continues with next EA)

2. **Activation Config**: Generated by Certus during deployment:
```json
{
  "eas": [
    {
      "name": "MomentumEA",
      "symbol": "EURUSD",
      "timeframe": "H1",
      "parameters": {}
    }
  ]
}
```

3. **Deployment Flow**:
   - Copy .ex4 files to MQL4/Experts/
   - Copy CertusManager.mq4 to MQL4/Scripts/
   - Generate certus_activation.json in Files/Certus/
   - User runs CertusManager script in MT4 (or it auto-runs if allowed)

4. **Fallback**: If auto-activation fails, show manual instructions:
   - "Open MT4 terminal"
   - "Navigate to Navigator > Scripts"
   - "Run CertusManager script"
   - "EAs will be attached to configured charts"

### Monitoring Setup

After deployment:
1. Construct the Certus output directory path: `<MT4_Data>\Files\Certus\`
2. Create FileSystemWatcher on that directory
3. Filter for `portfolio_status.json` and `trades.json`
4. Subscribe to Changed events
5. On change, trigger data import via existing Mt4Adapter/PlatformService
